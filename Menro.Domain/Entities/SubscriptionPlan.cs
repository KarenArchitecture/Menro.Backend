using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Menro.Domain.Entities
{
    public class SubscriptionPlan
    {
        [Key]
        public int Id { get; set; }

        [Display(Name = "نام اشتراک")]
        [MaxLength(50)]
        [Required]
        public string Name { get; set; }

        [Required]
        [Display(Name = "مدت اعتبار")]
        public int DurationDays { get; set; }

        [Required]
        [Display(Name = "قیمت")]
        public int Price { get; set; }

        [Required]
        [Display(Name = "مشخصات")]
        public string Features { get; set; }

        public ICollection<Subscription> Subscriptions { get; set; }
    }
}
