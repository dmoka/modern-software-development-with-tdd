using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using FsCheck;
using RefactoringLegacyCode.Tests.Asserters;
using RefactoringLegacyCode.Tests.Shared;

namespace RefactoringLegacyCode.Tests
{
    public class OrderControllerTests
    {
        [Test]
        public async Task OrderShouldBeProcessed()
        {
            using var server = new InMemoryServer();

            server.DateTimeProviderMock.Setup(mock => mock.Now).Returns(new DateTime(2025, 5, 9, 3, 31, 20));

            var response = await server.Client().PostAsync("api/order/1/process", null);

            await HttpResponseAsserter.AssertThat(response).HasStatusCode(HttpStatusCode.OK);
            await HttpResponseAsserter.AssertThat(response).HasJsonInBody(new
            {
                orderId = 1,
                totalCost = 94.95,
                estimatedDeliveryDate = new DateTime(2025, 5, 14, 3, 31, 20),
                deliveryType = "Express",
            });
        }

        [Test]
        public async Task OrderProcessingShouldBeLoggedInXml()
        {
            using var server = new InMemoryServer();

            server.DateTimeProviderMock.Setup(mock => mock.Now).Returns(new DateTime(2025, 5, 9, 3, 31, 20));

            var response = await server.Client().PostAsync("api/order/1/process", null);

            var xmlPath = Path.Combine(Environment.CurrentDirectory, $"Order_1.xml");

            await VerifyFile(xmlPath);
        }

        [TestCase(3, 10, "Express", 60)]
        [TestCase(18, 11, "Express", 150)]
        [TestCase(14, 11, "Express", 120)]
        //TODO: Add more test cases
        public async Task CharacterizePriorityCalculation(int  hour, int quantity, string deliveryType, int expectedPriority)
        {
            var orderDetails = new OrderDetails()
            {
                Quantity = quantity,
                DeliveryType = deliveryType
            };
            var dateTime = new DateTime(2025, 5, 9, hour, 31, 20);
            var priority = OrderController.CalculatePriority(orderDetails, dateTime);

            priority.Should().Be(expectedPriority);
        }

        public static Arbitrary<int> Hours()
        {
            return Arb.From(Gen.Choose(1, 23));
        }

        public static Arbitrary<int> Quantities()
        {
            return Arb.From(Gen.Choose(1, 100));
        }

        [Test]
        public void SameDayShouldBeTheHighestPrioThenExpressThenStandard()
        {
            Prop.ForAll(Hours(), Quantities(), (hour, quantity) =>
            {
                var sameDayOrder = new OrderDetails()
                {
                    Quantity = quantity,
                    DeliveryType = "SameDay"
                };
                var sameDayPriority =
                    OrderController.CalculatePriority(sameDayOrder, new DateTime(2025, 5, 9, hour, 31, 20));

                var expressOrder = new OrderDetails()
                {
                    Quantity = quantity,
                    DeliveryType = "Express"
                };
                var expressPriority =
                    OrderController.CalculatePriority(expressOrder, new DateTime(2025, 5, 9, hour, 31, 20));

                var standardOrder = new OrderDetails()
                {
                    Quantity = quantity,
                    DeliveryType = "Standard"
                };

                var standardPriority =
                    OrderController.CalculatePriority(standardOrder, new DateTime(2025, 5, 9, hour, 31, 20));

                return sameDayPriority >= expressPriority && expressPriority >= standardPriority;
            }).VerboseCheckThrowOnFailure();
        }

        //TODO: The more the quantity the higher the priority for the same delivery type
    }
}
