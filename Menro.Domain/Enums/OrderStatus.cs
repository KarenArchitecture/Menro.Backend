using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Menro.Domain.Enums
{
    public enum OrderStatus
    {
        Pending = 0,    // سفارش ثبت شده ولی پرداخت نشده
        Paid = 1,       // پرداخت انجام شده
        Cancelled = 2,  // سفارش لغو شده
        Completed = 3,  // سفارش تحویل شده
        // می‌تونی موارد دیگه مثل Refund, Failed و ... اضافه کنی
    }
}
