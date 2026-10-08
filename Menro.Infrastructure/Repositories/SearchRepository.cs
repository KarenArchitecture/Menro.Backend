using Menro.Domain.Contracts;
using Menro.Domain.Enums;
using Menro.Domain.Interfaces;
using Menro.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

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
            if (open == null || close == null)
                return false;

            var o = open.Value;
            var c = close.Value;

            // normal (08:00 -> 22:00)
            if (o <= c)
                return now >= o && now <= c;

            // overnight (20:00 -> 04:00)
            return now >= o || now <= c;
        }

        private static string ToPersianLetters(string s)
            => s.Replace('ي', 'ی').Replace('ى', 'ی').Replace('ك', 'ک');

        private static string ToArabicLetters(string s)
            => s.Replace('ی', 'ي').Replace('ک', 'ك');

        public async Task<List<SearchHit>> SearchAsync(string term, int take)
        {
            term = (term ?? "").Trim();

            if (term.Length < 2)
                return new List<SearchHit>();

            // take = max results PER TYPE (restaurants and foods are capped separately)
            take = Math.Clamp(take, 1, 50);

            static string EscapeLike(string s)
                => s.Replace("[", "[[]")
                    .Replace("%", "[%]")
                    .Replace("_", "[_]");

            var persian = ToPersianLetters(term);
            var arabic = ToArabicLetters(persian);

            var likeAny = $"%{EscapeLike(persian)}%";
            var likeAnyAr = $"%{EscapeLike(arabic)}%";
            var likeStart = $"{EscapeLike(persian)}%";
            var likeStartAr = $"{EscapeLike(arabic)}%";

            var nowUtc = DateTime.UtcNow;
            var nowLocal = DateTime.Now.TimeOfDay;

            var restaurants = await _context.Restaurants
                .AsNoTracking()
                .Where(r =>
                    !r.IsDeleted &&
                    r.IsActive &&
                    r.Status == RestaurantStatus.Approved &&
                    (
                        EF.Functions.Like(r.Name, likeAny) ||
                        EF.Functions.Like(r.Name, likeAnyAr) ||
                        EF.Functions.Like(r.Slug, likeAny)
                    )
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

                    Discount = _context.Discounts
                        .Where(d =>
                            d.RestaurantId == r.Id &&
                            d.IsActive &&
                            d.Scope == DiscountScope.Restaurant &&
                            d.ValueType == DiscountValueType.Percent &&
                            d.StartDate <= nowUtc &&
                            d.EndDate >= nowUtc
                        )
                        .Select(d => (int?)d.Value)
                        .Max() ?? 0,

                    Rating = _context.RestaurantRatings
                        .Where(rr => rr.RestaurantId == r.Id)
                        .Select(rr => (double?)rr.Score)
                        .Average() ?? 0,

                    Voters = _context.RestaurantRatings
                        .Count(rr => rr.RestaurantId == r.Id),

                    Rank = (EF.Functions.Like(r.Name, likeStart) ||
                            EF.Functions.Like(r.Name, likeStartAr))
                        ? 2
                        : 1
                })
                .OrderByDescending(x => x.Rank)
                .ThenBy(x => x.Title)
                .Take(take)
                .ToListAsync();

            foreach (var r in restaurants)
            {
                r.IsOpen = ComputeIsOpen(
                    r.OpenTime,
                    r.CloseTime,
                    nowLocal
                );
            }

            var foods = await _context.Foods
                .AsNoTracking()
                .Where(f =>
                    !f.IsDeleted &&
                    f.IsAvailable
                )
                .Join(
                    _context.Restaurants.Where(r =>
                        !r.IsDeleted &&
                        r.IsActive &&
                        r.Status == RestaurantStatus.Approved
                    ),
                    f => f.RestaurantId,
                    r => r.Id,
                    (f, r) => new { f, r }
                )
                .Where(x =>
                    EF.Functions.Like(x.f.Name, likeAny) ||
                    EF.Functions.Like(x.f.Name, likeAnyAr))
                .Select(x => new SearchHit
                {
                    Type = SearchHitType.Food,

                    Id = x.f.Id,

                    Title = x.f.Name,

                    Subtitle = x.r.Name,

                    ImageFileName = x.f.ImageUrl,

                    RestaurantId = x.f.RestaurantId,

                    RestaurantSlug = x.r.Slug,

                    Rating = _context.FoodRatings
                        .Where(fr => fr.FoodId == x.f.Id)
                        .Select(fr => (double?)fr.Score)
                        .Average() ?? 0,

                    Voters = _context.FoodRatings
                        .Count(fr => fr.FoodId == x.f.Id),

                    Rank = (EF.Functions.Like(x.f.Name, likeStart) ||
                            EF.Functions.Like(x.f.Name, likeStartAr))
                        ? 2
                        : 1
                })
                .OrderByDescending(x => x.Rank)
                .ThenBy(x => x.Title)
                .Take(take)
                .ToListAsync();

            // No final Take(): restaurants and foods are each capped at `take`,
            // so one type can never push the other out of the results.
            return restaurants
                .Concat(foods)
                .OrderByDescending(x => x.Rank)
                .ThenBy(x => x.Title)
                .ToList();
        }

        private static string EscapeLikeTerm(string s)
            => s.Replace("[", "[[]").Replace("%", "[%]").Replace("_", "[_]");

        public async Task<(List<SearchHit> Items, bool HasMore)> SearchPagedAsync(
            string term, SearchHitType type, int skip, int take, int? globalCategoryId = null)
        {
            term = (term ?? "").Trim();
            if (term.Length < 2)
                return (new List<SearchHit>(), false);

            take = Math.Clamp(take, 1, 50);
            skip = Math.Max(skip, 0);

            var esc = EscapeLikeTerm(term);
            var likeAny = $"%{esc}%";
            var likeStart = $"{esc}%";

            List<SearchHit> rows;

            if (type == SearchHitType.Restaurant)
            {
                var nowUtc = DateTime.UtcNow;
                var nowLocal = DateTime.Now.TimeOfDay;

                var q = _context.Restaurants
                    .AsNoTracking()
                    .Where(r =>
                        !r.IsDeleted &&
                        r.IsActive &&
                        r.Status == RestaurantStatus.Approved &&
                        (EF.Functions.Like(r.Name, likeAny) ||
                         EF.Functions.Like(r.Slug, likeAny)));

                if (globalCategoryId.HasValue)
                {
                    var gid = globalCategoryId.Value;
                    q = q.Where(r => _context.Foods.Any(f =>
                        f.RestaurantId == r.Id &&
                        !f.IsDeleted &&
                        f.IsAvailable &&
                        f.CustomFoodCategory != null &&
                        f.CustomFoodCategory.GlobalCategoryId == gid));
                }

                rows = await q
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
                        Discount = _context.Discounts
                            .Where(d =>
                                d.RestaurantId == r.Id &&
                                d.IsActive &&
                                d.Scope == DiscountScope.Restaurant &&
                                d.ValueType == DiscountValueType.Percent &&
                                d.StartDate <= nowUtc &&
                                d.EndDate >= nowUtc)
                            .Select(d => (int?)d.Value)
                            .Max() ?? 0,
                        Rating = _context.RestaurantRatings
                            .Where(rr => rr.RestaurantId == r.Id)
                            .Select(rr => (double?)rr.Score)
                            .Average() ?? 0,
                        Voters = _context.RestaurantRatings
                            .Count(rr => rr.RestaurantId == r.Id),
                        Rank = EF.Functions.Like(r.Name, likeStart) ? 2 : 1
                    })
                    .OrderByDescending(x => x.Rank)
                    .ThenBy(x => x.Title)
                    .ThenBy(x => x.Id)
                    .Skip(skip)
                    .Take(take + 1)
                    .ToListAsync();

                foreach (var r in rows)
                    r.IsOpen = ComputeIsOpen(r.OpenTime, r.CloseTime, nowLocal);
            }
            else
            {
                var q = _context.Foods
                    .AsNoTracking()
                    .Where(f => !f.IsDeleted && f.IsAvailable)
                    .Join(
                        _context.Restaurants.Where(r =>
                            !r.IsDeleted &&
                            r.IsActive &&
                            r.Status == RestaurantStatus.Approved),
                        f => f.RestaurantId,
                        r => r.Id,
                        (f, r) => new { f, r });

                if (globalCategoryId.HasValue)
                {
                    var gid = globalCategoryId.Value;
                    q = q.Where(x =>
                        x.f.CustomFoodCategory != null &&
                        x.f.CustomFoodCategory.GlobalCategoryId == gid &&
                        (EF.Functions.Like(x.f.Name, likeAny) ||
                         EF.Functions.Like(x.r.Name, likeAny)));
                }
                else
                {
                    q = q.Where(x => EF.Functions.Like(x.f.Name, likeAny));
                }

                rows = await q
                    .Select(x => new SearchHit
                    {
                        Type = SearchHitType.Food,
                        Id = x.f.Id,
                        Title = x.f.Name,
                        Subtitle = x.r.Name,
                        ImageFileName = x.f.ImageUrl,
                        RestaurantId = x.f.RestaurantId,
                        RestaurantSlug = x.r.Slug,
                        Rating = _context.FoodRatings
                            .Where(fr => fr.FoodId == x.f.Id)
                            .Select(fr => (double?)fr.Score)
                            .Average() ?? 0,
                        Voters = _context.FoodRatings
                            .Count(fr => fr.FoodId == x.f.Id),
                        Rank = EF.Functions.Like(x.f.Name, likeStart) ? 2 : 1
                    })
                    .OrderByDescending(x => x.Rank)
                    .ThenBy(x => x.Title)
                    .ThenBy(x => x.Id)
                    .Skip(skip)
                    .Take(take + 1)
                    .ToListAsync();
            }

            var hasMore = rows.Count > take;
            if (hasMore) rows.RemoveAt(rows.Count - 1);

            return (rows, hasMore);
        }
    }
}