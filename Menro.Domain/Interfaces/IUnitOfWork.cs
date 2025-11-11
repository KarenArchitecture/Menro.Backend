using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Menro.Domain.Interfaces
{
    public interface IUnitOfWork
    {
        IUserRepository User { get; }
        IFoodRepository Food { get;}
        ICustomFoodCategoryRepository FoodCategory { get;}
        IRestaurantRepository Restaurant { get;}
        IRestaurantCategoryRepository RestaurantCategory { get;}
        ISubscriptionRepository Subscription { get;}
        ISubscriptionPlanRepository SubscriptionPlan { get;}
        IOtpRepository Otp { get;}
        IOrderRepository Order { get;}
        IOrderItemRepository OrderItem { get;}
        IRefreshTokenRepository RefreshToken { get;}
        Task<int> SaveChangesAsync();
    }
}
