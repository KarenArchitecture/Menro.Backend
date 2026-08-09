using Menro.Application.Features.Orders.DTOs;
using Menro.Domain.Interfaces;
using Menro.Application.Common.Interfaces;
using Menro.Application.Common.Media;
using Menro.Application.Features.Orders.Services.Interfaces;
using Menro.Domain.Enums;

namespace Menro.Application.Features.Orders.Services.Implementations
{
    public class AdminOrderService : IAdminOrderService
    {
        #region DI
        private readonly IOrderRepository _orderRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IGlobalDateTimeService _dateTimeService;
        private readonly IMediaStorageProvider _mediaStorage;

        public AdminOrderService(
            IOrderRepository orderRepository,
            IUnitOfWork unitOfWork,
            IGlobalDateTimeService dateTimeService,
            IMediaStorageProvider mediaStorage)
        {
            _orderRepository = orderRepository;
            _unitOfWork = unitOfWork;
            _dateTimeService = dateTimeService;
            _mediaStorage = mediaStorage;
        }
        #endregion

        // The two payment methods walk the same statuses in a different
        // order — pay-at-counter collects payment BEFORE delivery, so
        // "Paid" and "Delivered" swap places relative to pay-after-serving.
        // BankGateway isn't selectable yet, so it falls back to the
        // pay-after-serving sequence.
        private static readonly Dictionary<RestaurantPaymentMethod, OrderStatus[]> StatusSequences = new()
        {
            [RestaurantPaymentMethod.PayAtCounterBeforeServing] = new[]
            {
                OrderStatus.Pending, OrderStatus.Confirmed, OrderStatus.Paid, OrderStatus.Delivered, OrderStatus.Completed
            },
            [RestaurantPaymentMethod.PayAfterServing] = new[]
            {
                OrderStatus.Pending, OrderStatus.Confirmed, OrderStatus.Delivered, OrderStatus.Paid, OrderStatus.Completed
            },
            [RestaurantPaymentMethod.BankGateway] = new[]
            {
                OrderStatus.Pending, OrderStatus.Confirmed, OrderStatus.Delivered, OrderStatus.Paid, OrderStatus.Completed
            },
        };

        private OrderStatus[] GetSequence(RestaurantPaymentMethod? method)
            => StatusSequences.TryGetValue(method ?? RestaurantPaymentMethod.PayAfterServing, out var seq)
                ? seq
                : StatusSequences[RestaurantPaymentMethod.PayAfterServing];

        /* dashboard stats */
        public async Task<int> GetTotalRevenueAsync(int? restaurantId = null)
        {
            return await _orderRepository.GetTotalRevenueAsync(restaurantId);
        }

        public async Task<List<MonthlySalesDto>> GetMonthlySalesRawAsync(int? restaurantId = null)
        {
            var persianYear = _dateTimeService.GetPersianYear(DateTime.UtcNow);
            DateTime startOfYear = _dateTimeService.ConvertToGregorian(persianYear, 1, 1);
            DateTime startOfNextYear = _dateTimeService.ConvertToGregorian(persianYear + 1, 1, 1);

            var orders = await _orderRepository.GetCompletedOrdersAsync(restaurantId, startOfYear, startOfNextYear);
            var grouped = orders
                .GroupBy(o => _dateTimeService.GetPersianMonth(o.CreatedAt))
                .Select(g => new MonthlySalesDto
                {
                    Month = g.Key,
                    MonthName = _dateTimeService.GetPersianMonthName(g.First().CreatedAt),
                    TotalAmount = g.Sum(x => x.TotalPrice)
                })
                .ToList();

            var result = Enumerable.Range(1, 12)
                .GroupJoin(grouped, m => m, x => x.Month, (m, g) =>
                    g.FirstOrDefault() ?? new MonthlySalesDto
                    {
                        Month = m,
                        MonthName = _dateTimeService.GetPersianMonthName(
                            _dateTimeService.ConvertToGregorian(persianYear, m, 1)
                        ),
                        TotalAmount = 0
                    })
                .OrderBy(x => x.Month)
                .ToList();

            return result;
        }

        public async Task<int> GetRecentOrdersCountAsync(int? restaurantId = null, int daysBack = 0)
        {
            DateTime since = daysBack == 0 ? DateTime.UtcNow.Date : DateTime.UtcNow.AddDays(-daysBack);
            return await _orderRepository.GetRecentOrdersCountAsync(restaurantId, since);
        }

        public async Task<int> GetRecentOrdersRevenueAsync(int? restaurantId = null, int daysBack = 0)
        {
            DateTime since = daysBack == 0 ? DateTime.UtcNow.Date : DateTime.UtcNow.AddDays(-daysBack);
            return await _orderRepository.GetRecentOrdersRevenueAsync(restaurantId, since);
        }

        /* order management */
        public async Task<List<AdminOrderListItemDto>> GetActiveOrdersAsync(int restaurantId)
        {
            var restaurant = await _unitOfWork.Restaurant.GetByIdAsync(restaurantId);
            var paymentMethod = restaurant?.PaymentMethod.ToString() ?? "";

            var orders = await _orderRepository.GetActiveOrdersAsync(restaurantId);
            return orders.Select(o => new AdminOrderListItemDto
            {
                Id = o.Id,
                RestaurantOrderNumber = o.RestaurantOrderNumber,
                InvoiceNumber = o.InvoiceNumber,
                PaymentMethod = paymentMethod,
                TableLabel = o.TableLabel,
                TotalPrice = o.TotalPrice,
                Status = o.Status,
                CreatedAt = o.CreatedAt
            }).ToList();
        }

        public async Task<List<AdminOrderListItemDto>> GetOrderHistoryAsync(int restaurantId)
        {
            var restaurant = await _unitOfWork.Restaurant.GetByIdAsync(restaurantId);
            var paymentMethod = restaurant?.PaymentMethod.ToString() ?? "";

            var orders = await _orderRepository.GetOrderHistoryAsync(restaurantId);
            return orders.Select(o => new AdminOrderListItemDto
            {
                Id = o.Id,
                RestaurantOrderNumber = o.RestaurantOrderNumber,
                InvoiceNumber = o.InvoiceNumber,
                PaymentMethod = paymentMethod,
                TableLabel = o.TableLabel,
                TotalPrice = o.TotalPrice,
                Status = o.Status,
                CreatedAt = o.CreatedAt
            }).ToList();
        }

        public async Task<AdminOrderDetailsDto?> GetOrderDetailsAsync(int restaurantId, int orderId)
        {
            var order = await _orderRepository.GetOrderDetailsAsync(restaurantId, orderId);
            if (order == null) return null;

            var restaurant = await _unitOfWork.Restaurant.GetByIdAsync(restaurantId);

            return new AdminOrderDetailsDto
            {
                Id = order.Id,
                RestaurantOrderNumber = order.RestaurantOrderNumber,
                InvoiceNumber = order.InvoiceNumber,
                PaymentMethod = restaurant?.PaymentMethod.ToString() ?? "",
                TableLabel = order.TableLabel,
                Status = order.Status,
                CreatedAt = new DateTimeOffset(DateTime.SpecifyKind(order.CreatedAt, DateTimeKind.Utc)),
                TotalPrice = order.TotalPrice,
                Items = order.OrderItems.Select(oi => new AdminOrderItemDto
                {
                    Id = oi.Id,
                    Name = oi.TitleSnapshot,
                    Qty = oi.Quantity,
                    Price = oi.UnitPrice,
                    ImageUrl = _mediaStorage.GetUrl(MediaCategory.RestaurantFoodImage, oi.Food.ImageUrl, oi.FoodId.ToString(), MediaVariant.Resized),
                    Addons = oi.Extras.Select(ex => new AdminOrderItemAddonDto
                    {
                        Name = ex.FoodAddon.Name
                    }).ToList()
                }).ToList()
            };
        }

        public async Task<List<AdminOrderListItemDto>> SearchOrdersAsync(int restaurantId, string query)
        {
            if (string.IsNullOrWhiteSpace(query) || query.Trim().Length < 2)
                return new List<AdminOrderListItemDto>();

            var restaurant = await _unitOfWork.Restaurant.GetByIdAsync(restaurantId);
            var paymentMethod = restaurant?.PaymentMethod.ToString() ?? "";

            var orders = await _orderRepository.SearchOrdersByInvoiceAsync(restaurantId, query.Trim(), 8);
            return orders.Select(o => new AdminOrderListItemDto
            {
                Id = o.Id,
                RestaurantOrderNumber = o.RestaurantOrderNumber,
                InvoiceNumber = o.InvoiceNumber,
                PaymentMethod = paymentMethod,
                TableLabel = o.TableLabel,
                TotalPrice = o.TotalPrice,
                Status = o.Status,
                CreatedAt = o.CreatedAt
            }).ToList();
        }

        // manage order status
        public async Task<OrderStatus?> AdvanceStatusAsync(int restaurantId, int orderId)
        {
            var order = await _orderRepository.GetForUpdateAsync(restaurantId, orderId);
            if (order == null) return null;

            if (order.Status == OrderStatus.Completed || order.Status == OrderStatus.Cancelled)
                throw new InvalidOperationException("این سفارش قابل تغییر نیست.");

            var restaurant = await _unitOfWork.Restaurant.GetByIdAsync(restaurantId);
            var sequence = GetSequence(restaurant?.PaymentMethod);

            var currentIndex = Array.IndexOf(sequence, order.Status);
            if (currentIndex >= 0 && currentIndex < sequence.Length - 1)
            {
                order.Status = sequence[currentIndex + 1];
            }

            await _orderRepository.SaveChangesAsync();
            return order.Status;
        }

        public async Task<OrderStatus?> CancelOrderAsync(int restaurantId, int orderId)
        {
            var order = await _orderRepository.GetForUpdateAsync(restaurantId, orderId);
            if (order == null) return null;

            if (order.Status == OrderStatus.Completed || order.Status == OrderStatus.Cancelled)
                throw new InvalidOperationException("این سفارش قابل لغو نیست.");

            order.Status = OrderStatus.Cancelled;
            await _orderRepository.SaveChangesAsync();
            return order.Status;
        }
    }
}