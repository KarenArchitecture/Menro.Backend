using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Menro.Domain.Entities
{
    public class SubscriptionPlan
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int DurationDays { get; set; }
        public int Price { get; set; }
        public string Features { get; set; }

        public ICollection<Subscription> Subscriptions { get; set; }
    }
}
