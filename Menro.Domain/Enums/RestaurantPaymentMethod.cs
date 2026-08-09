// Domain/Enums/RestaurantPaymentMethod.cs
namespace Menro.Domain.Enums
{
    public enum RestaurantPaymentMethod
    {
        PayAtCounterBeforeServing = 0, // پرداخت پای صندوق پیش از سرو غذا
        PayAfterServing = 1,           // پرداخت پس از سرو غذا
        BankGateway = 2                // درگاه بانکی — غیرفعال، به‌زودی
    }
}