namespace Menro.Domain.Enums
{
    public enum OrderStatus
    {
        Pending = 0,    // سفارش ثبت شده ولی پرداخت نشده
        Confirmed = 1,  // تایید شده
        Cancelled = 2,  // سفارش لغو شده
        Delivered = 3,  //تحویل داده شده
        Paid = 4,       // پرداخت انجام شده
        Completed = 5  // سفارش تحویل شده
    }
}
