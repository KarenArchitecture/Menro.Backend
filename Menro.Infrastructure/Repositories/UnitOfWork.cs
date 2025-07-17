using Menro.Domain.Interfaces;
using Menro.Infrastructure.Data;

namespace Menro.Infrastructure.Repositories
{
    public class UnitOfWork : IUnitOfWork, IDisposable
    {

        /*
        ✅ مزایای نسخه جدید:
        ساختار قبلی کامل حفظ شده
        ریپازیتوری‌ها فقط وقتی ساخته می‌شن که نیاز باشه
        هیچ تزریق تکراری یا اضافه نداری
        آماده‌ی استفاده برای همون سرویس‌هایی که قبلاً نوشتی
        */
        private readonly MenroDbContext _context;

        // private fields for lazy initialization
        private IUserRepository _user;
        private IFoodRepository _food;
        private IFoodCategoryRepository _foodCategory;
        private IRestaurantRepository _restaurant;
        private IRestaurantCategoryRepository _restaurantCategory;
        private ISubscriptionRepository _subscription;
        private ISubscriptionPlanRepository _subscriptionPlan;
        private IOtpRepository _otp;

        // public properties with lazy instantiation
        public IUserRepository User => _user ??= new UserRepository(_context);
        public IFoodRepository Food => _food ??= new FoodRepository(_context);
        public IFoodCategoryRepository FoodCategory => _foodCategory ??= new FoodCategoryRepository(_context);
        public IRestaurantRepository Restaurant => _restaurant ??= new RestaurantRepository(_context);
        public IRestaurantCategoryRepository RestaurantCategory => _restaurantCategory ??= new RestaurantCategoryRepository(_context);
        public ISubscriptionRepository Subscription => _subscription ??= new SubscriptionRepository(_context);
        public ISubscriptionPlanRepository SubscriptionPlan => _subscriptionPlan ??= new SubscriptionPlanRepository(_context);
        public IOtpRepository Otp => _otp ??= new OtpRepository(_context);

        // constructor
        public UnitOfWork(MenroDbContext context)
        {
            _context = context;
        }

        // commit/save
        public async Task<int> SaveAsync()
        {
            return await _context.SaveChangesAsync();
        }

        // IDisposable implementation
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
