using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using FluentAssertions;
using FsCheck;
using Microsoft.AspNetCore.TestHost;
using Moq;
using RefactoringLegacyCode.Tests.Asserters;
using RefactoringLegacyCode.Tests.Shared;

namespace RefactoringLegacyCode.Tests
{
    public class OrderControllerTests
    {
        [Test]
        public async Task ShouldProcessOrder()
        {
            using var server = new InMemoryServer();

            server.DateTimeProvider.Setup(mock => mock.Now).Returns(new DateTime(2025, 5, 6, 8, 13, 14));

            var response = await server.Client().PostAsync("/api/order/1/process", null);

            await HttpResponseAsserter.AssertThat(response).HasStatusCode(HttpStatusCode.OK);
            await HttpResponseAsserter.AssertThat(response).HasJsonInBody(new
            {
                orderId = 1,
                totalCost = 94.95,
                estimatedDeliveryDate = new DateTime(2025, 5, 11, 8, 13, 14),
                deliveryType = "Express",
            });

            var stockLevel = server.GetStockLevel(100);
            stockLevel.Should().Be(5);

            var orderState = server.GetOrderState(1);
            orderState.Should().Be("Processed");
        }


        [Test]
        public async Task ShouldSaveOrderProcessingInXml()
        {
            using var server = new InMemoryServer();

            server.DateTimeProvider.Setup(mock => mock.Now).Returns(new DateTime(2025, 5, 6, 8, 13, 14));

            var response = await server.Client().PostAsync("/api/order/1/process", null);

            var xmlPath = Path.Combine(Environment.CurrentDirectory, "Order_1.xml");

            await VerifyFile(xmlPath);

        }

        [TestCase(18, 11, "Express", 150)]
        [TestCase(17, 11, "Express", 120)]
        [TestCase(18, 11, "SameDay", 160)]
        [TestCase(11, 12, "SameDay", 180)]
        [TestCase(11, 11, "SameDay", 180)]
        [TestCase(11, 51, "Standard", 100)]
        [TestCase(11, 50, "Standard", 80)]
        [TestCase(18, 10, "Express", 70)]
        [TestCase(18, 9, "Express", 70)]
        [TestCase(11, 9, "Express", 60)]
        [TestCase(11, 4, "Express", 50)]
        [TestCase(11, 9, "SameDay", 90)]
        [TestCase(12, 9, "SameDay", 110)]
        [TestCase(18, 9, "Standard", 40)]
        [TestCase(11, 9, "Standard", 20)]
        public async Task CalculatePriority(int hour, int quantity, string deliveryType, int expectedPriorityResult)
        {
            var orderDate = new DateTime(2025, 5, 6, hour, 18, 14);
            var priority = OrderController.CalculatePriority(orderDate, quantity, deliveryType);
            priority.Should().Be(expectedPriorityResult);
        }

        public static Arbitrary<int> Hours()
        {
            return Arb.From(Gen.Choose(1, 23));
        }

        public static Arbitrary<int> Quantities()
        {
            return Arb.From(Gen.Choose(1, 100));
        }

        public static Arbitrary<string> DeliveryTypes()
        {
            return Arb.From(Gen.Elements("Express", "SameDay", "Standard"));
        }

       [Test]
        public void HigherQuantity_shouldLeadHigherPrirorityWithSameType()
        {
            Prop.ForAll(DeliveryTypes(), Hours(), Quantities(), (deliveryType, hour, quantity) =>
            {
                var orderDate = new DateTime(2025, 5, 6, hour, 18, 14);
                var priority = OrderController.CalculatePriority(orderDate, quantity, deliveryType);

                var higherQuantity = quantity + 20;
                var higherPriority = OrderController.CalculatePriority(orderDate, higherQuantity, deliveryType);

                return higherPriority >= priority;
            }).VerboseCheckThrowOnFailure();
        }

        [Test]
        public void PriorityOrderShouldBe_SameDay_Then_Express_Then_Standard()
        {
            Prop.ForAll(Hours(), Quantities(), (hour, quantity) =>
            {
                var orderDate = new DateTime(2025, 5, 6, hour, 18, 14);

                var sameDayPrio = OrderController.CalculatePriority(orderDate, quantity, "SameDay");
                var expressPrio = OrderController.CalculatePriority(orderDate, quantity, "Express");
                var standardPrio = OrderController.CalculatePriority(orderDate, quantity, "Standard");

                return sameDayPrio >= expressPrio && expressPrio >= standardPrio;

            }).VerboseCheckThrowOnFailure();
        }

        [Test]
        public async Task SendEmail_shouldSendExpectedResult()
        {
            //Arrange
            using var server = new InMemoryServer();

            string calledPath = null;
            StringContent capturedContent = null;

            server.EmailSender.Setup(sender => sender.SendEmailAsync(It.IsAny<string>(), It.IsAny<StringContent>()))
                .Callback<string, StringContent>((path, content) =>
                {
                    calledPath = path;
                    capturedContent = content;
                });

            var response = await server.Client().PostAsync("api/order/1/process", null);
            await HttpResponseAsserter.AssertThat(response).HasStatusCode(HttpStatusCode.OK);

            var actualEmailPayloadJson = await capturedContent.ReadAsStringAsync();
            var actualEmailPayload = JsonSerializer.Deserialize<Dictionary<string, string>>(actualEmailPayloadJson);

            calledPath.Should().Be("https://email.myservice.com/send");
            actualEmailPayload["to"].Should().Be("customer@example.com");
            actualEmailPayload["subject"].Should().Be($"Order Confirmation - Order #1");
            actualEmailPayload["body"].Should().Be($"Dear Customer,\n\nThank you for your order #1. Your order has been processed and will be delivered soon.\n\nBest Regards,\nWarehouse Team");
            capturedContent.Headers.ContentType.MediaType.Should().Be("application/json");
            capturedContent.Headers.ContentType.CharSet.Should().Be("utf-8");
        }

        [Test]
        public async Task ShouldFailForInsufficientStock()
        {

            using var server = new InMemoryServer();

            server.DateTimeProvider.Setup(mock => mock.Now).Returns(new DateTime(2025, 5, 6, 8, 13, 14));

            server.InsertProduct(200, 10, 100m);
            server.InsertOrder(12, 200, 11);

            var response = await server.Client().PostAsync("/api/order/12/process", null);

            await HttpResponseAsserter.AssertThat(response).HasStatusCode(HttpStatusCode.BadRequest);
            await HttpResponseAsserter.AssertThat(response).HasTextInBody("Insufficient stock to process the order.");
        }

        [Test]
        public async Task ShouldReturnInternalServerErrorWhenUnexpectedExceptionHappen()
        {
            using var server = new InMemoryServer();

            server.EmailSender.Setup(mock => mock.SendEmailAsync(It.IsAny<string>(), It.IsAny<StringContent>()))
                .Throws(new Exception("Unexpected error"));

            var response = await server.Client().PostAsync("/api/order/1/process", null);

            await HttpResponseAsserter.AssertThat(response).HasStatusCode(HttpStatusCode.InternalServerError);
            await HttpResponseAsserter.AssertThat(response).HasTextInBody("Internal server error: Unexpected error");

        }

        [Test]
        public async Task ShouldProcessAllStock()
        {

            using var server = new InMemoryServer();

            server.DateTimeProvider.Setup(mock => mock.Now).Returns(new DateTime(2025, 5, 6, 8, 13, 14));

            server.InsertProduct(200, 10, 100m);
            server.InsertOrder(12, 200, 10);

            var response = await server.Client().PostAsync("/api/order/12/process", null);

            await HttpResponseAsserter.AssertThat(response).HasStatusCode(HttpStatusCode.OK);
            var stockLevel = server.GetStockLevel(200);
            stockLevel.Should().Be(0);
        }

        [Test]
        public async Task ShouldProcessOrderWithHighQuantity()
        {
            using var server = new InMemoryServer();

            server.DateTimeProvider.Setup(mock => mock.Now).Returns(new DateTime(2025, 5, 6, 8, 13, 14));

            server.InsertProduct(200, 10, 100m);
            server.InsertOrder(12, 200, 6);

            var response = await server.Client().PostAsync("/api/order/12/process", null);

            await HttpResponseAsserter.AssertThat(response).HasStatusCode(HttpStatusCode.OK);
            await HttpResponseAsserter.AssertThat(response).HasJsonInBody(new
            {
                orderId = 12,
                totalCost = 600,
                estimatedDeliveryDate = new DateTime(2025, 5, 8, 8, 13, 14),
                deliveryType = "Express",
            });
        }

        [Test]
        public async Task ShouldReturnBadRequestWhenOrderNotFound()
        {
            using var server = new InMemoryServer();

            server.DateTimeProvider.Setup(mock => mock.Now).Returns(new DateTime(2025, 5, 6, 8, 13, 14));

            var response = await server.Client().PostAsync("/api/order/99/process", null);

            await HttpResponseAsserter.AssertThat(response).HasStatusCode(HttpStatusCode.BadRequest);
            await HttpResponseAsserter.AssertThat(response).HasTextInBody("Order not found.");
        }
    }
}
