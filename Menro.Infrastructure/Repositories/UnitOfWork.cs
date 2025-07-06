using Menro.Domain.Interfaces;
using Menro.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Menro.Infrastructure.Repositories
{
    public class UnitOfWork : IUnitOfWork, IDisposable
    {
        private readonly MenroDbContext _context;

        public IUserRepository User { get; private set; }
        public IFoodRepository Food { get; private set; }
        public IFoodCategoryRepository FoodCategory { get; private set; }
        public IRestaurantRepository Restaurant { get; private set; }
        public IRestaurantCategoryRepository RestaurantCategory { get; private set; }
        public ISubscriptionRepository Subscription { get; private set; }
        public ISubscriptionPlanRepository SubscriptionPlan { get; private set; }
        public IOtpRepository Otp { get; private set; }

        public UnitOfWork(MenroDbContext context)
        {
            _context = context;
            User = new UserRepository(context);
            Food = new FoodRepository(context);
            FoodCategory = new FoodCategoryRepository(context);
            Restaurant = new RestaurantRepository(context);
            RestaurantCategory = new RestaurantCategoryRepository(context);
            Subscription = new SubscriptionRepository(context);
            SubscriptionPlan = new SubscriptionPlanRepository(context);
            Otp = new OtpRepository(context);

        }

        public async Task SaveAsync()
        {
            await _context.SaveChangesAsync();
        }

        // اضافه کردن IDisposable برای آزادسازی context
        private bool disposed = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    _context.Dispose();
                }
                disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
