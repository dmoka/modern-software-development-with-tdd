namespace RefactoringLegacyCode.Features
{
    public class PriorityCalculator
    {
        public static int CalculatePriority(IDateTimeProvider timeProvider, OrderDetails orderDetails)
        {
            var now = timeProvider.Now;
            var isMorning = now.Hour < 12;
            var isEvening = now.Hour >= 18;

            if (orderDetails.Quantity > 10)
            {
                return CalculateLargeOrderPriority(orderDetails, isEvening, isMorning);
            }
            else
            {
                return CalculateSmallOrderPriority(orderDetails, isEvening, isMorning, now);
            }
        }

        private static int CalculateSmallOrderPriority(OrderDetails orderDetails, bool isEvening, bool isMorning,
            DateTime now)
        {

            return orderDetails.DeliveryType switch
            {
                DeliveryType.Express => orderDetails.Quantity > 5 ? isEvening ? 70 : 60 : 50,
                DeliveryType.SameDay => isMorning ? 90 : 110,
                DeliveryType.Standard => now.Hour >= 18 ? 40 : 20,
                _ => 0
            };
        }

        private static int CalculateLargeOrderPriority(OrderDetails orderDetails, bool isEvening, bool isMorning)
        {
            return orderDetails.DeliveryType switch
            {
                DeliveryType.Express => isEvening ? 150 : 120,
                DeliveryType.SameDay => isMorning ? 180 : 160,
                DeliveryType.Standard => orderDetails.Quantity > 50 ? 100 : 80,
                _ => 0
            };
        }
    }
}
