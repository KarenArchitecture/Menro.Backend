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
        IFoodCategoryRepository FoodCategory { get;}
        IRestaurantRepository Restaurant { get;}
        IRestaurantCategoryRepository RestaurantCategory { get;}
        ISubscriptionRepository Subscription { get;}
        ISubscriptionPlanRepository SubscriptionPlan { get;}

        Task<int> SaveChangesAsync();
    }
}
