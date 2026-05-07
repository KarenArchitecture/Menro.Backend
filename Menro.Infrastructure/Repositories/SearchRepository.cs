using Menro.Domain.Contracts;
using Menro.Domain.Enums;
using Menro.Domain.Interfaces;
using Menro.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Menro.Infrastructure.Repositories
{
    public class SearchRepository : ISearchRepository
    {
        private readonly MenroDbContext _context;

        public SearchRepository(MenroDbContext context)
        {
            _context = context;
        }

        private static bool ComputeIsOpen(TimeSpan? open, TimeSpan? close, TimeSpan now)
        {
            if (open == null || close == null) return false;

            var o = open.Value;
            var c = close.Value;

            // normal (08:00 -> 22:00)
            if (o <= c) return now >= o && now <= c;

            // overnight (20:00 -> 04:00)
            return now >= o || now <= c;
        }

        public async Task<List<SearchHit>> SearchAsync(string term, int take)
        {
            term = (term ?? "").Trim();
            if (term.Length < 2) return new List<SearchHit>();

            take = Math.Clamp(take, 1, 50);

            static string EscapeLike(string s)
                => s.Replace("[", "[[]").Replace("%", "[%]").Replace("_", "[_]");

            var esc = EscapeLike(term);
            var likeAny = $"%{esc}%";
            var likeStart = $"{esc}%";

            var nowUtc = DateTime.UtcNow;
            var nowLocal = DateTime.Now.TimeOfDay;

            var restaurants = await _context.Restaurants
                .AsNoTracking()
                .Where(r =>
                    !r.IsDeleted &&
                    r.IsActive &&
                    r.Status == RestaurantStatus.Approved &&
                    (EF.Functions.Like(r.Name, likeAny) || EF.Functions.Like(r.Slug, likeAny))
                )
                .Select(r => new SearchHit
                {
                    Type = SearchHitType.Restaurant,
                    Id = r.Id,
                    Title = r.Name,
                    Subtitle = r.Address,

                    ImageFileName = r.BannerImageUrl,
                    LogoImageUrl = r.LogoImageUrl,

                    RestaurantId = r.Id,
                    RestaurantSlug = r.Slug,

                    Category = r.RestaurantCategory.Name,
                    OpenTime = r.OpenTime,
                    CloseTime = r.CloseTime,

                    Discount =
                        _context.RestaurantDiscounts
                            .Where(d => d.RestaurantId == r.Id && d.StartDate <= nowUtc && d.EndDate >= nowUtc)
                            .Select(d => (int?)d.Percent)
                            .Max() ?? 0,

                    Rating =
                        _context.RestaurantRatings
                            .Where(rr => rr.RestaurantId == r.Id)
                            .Select(rr => (double?)rr.Score)
                            .Average() ?? 0,

                    Voters =
                        _context.RestaurantRatings
                            .Count(rr => rr.RestaurantId == r.Id),

                    Rank = EF.Functions.Like(r.Name, likeStart) ? 2 : 1
                })
                .OrderByDescending(x => x.Rank)
                .ThenBy(x => x.Title)
                .Take(take)
                .ToListAsync();

            foreach (var r in restaurants)
                r.IsOpen = ComputeIsOpen(r.OpenTime, r.CloseTime, nowLocal);

            var foods = await _context.Foods
                .AsNoTracking()
                .Where(f => !f.IsDeleted && f.IsAvailable)
                .Join(
                    _context.Restaurants.Where(r => !r.IsDeleted && r.IsActive && r.Status == RestaurantStatus.Approved),
                    f => f.RestaurantId,
                    r => r.Id,
                    (f, r) => new { f, r }
                )
                .Where(x => EF.Functions.Like(x.f.Name, likeAny))
                .Select(x => new SearchHit
                {
                    Type = SearchHitType.Food,
                    Id = x.f.Id,
                    Title = x.f.Name,

                    // ✅ for FoodCard restaurantName
                    Subtitle = x.r.Name,

                    ImageFileName = x.f.ImageUrl,

                    RestaurantId = x.f.RestaurantId,
                    RestaurantSlug = x.r.Slug,

                    Rating =
                        _context.FoodRatings
                            .Where(fr => fr.FoodId == x.f.Id)
                            .Select(fr => (double?)fr.Score)
                            .Average() ?? 0,

                    Voters =
                        _context.FoodRatings
                            .Count(fr => fr.FoodId == x.f.Id),

                    Rank = EF.Functions.Like(x.f.Name, likeStart) ? 2 : 1
                })
                .OrderByDescending(x => x.Rank)
                .ThenBy(x => x.Title)
                .Take(take)
                .ToListAsync();

            return restaurants
                .Concat(foods)
                .OrderByDescending(x => x.Rank)
                .ThenBy(x => x.Title)
                .Take(take)
                .ToList();
        }
    }
}
