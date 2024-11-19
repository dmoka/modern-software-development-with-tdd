using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using FluentAssertions;
using FsCheck;
using Moq;
using NUnit.Framework;
using RefactoringLegacyCode.Features;
using RefactoringLegacyCode.Tests.Asserters;
using RefactoringLegacyCode.Tests.Shared;
using VerifyNUnit;

//TODO:
//add shared folder
//rename orderId to id in OrderDetails
//unit price is hardcoded in xml
//why http client is still hanging? We prolly left there
namespace RefactoringLegacyCode.Tests.Features
{
    public class ProcessOrderTests
    {
        [Test]
        public async Task XmlFileSnapShotTest()
        {
            //Arrange
            var testServer = new InMemoryServer();

            testServer.DateTimeProvider().Setup(provider => provider.Now).Returns(new DateTime(2024, 11, 7, 10, 10, 10));

            //Act
            var response = await testServer.Client().PostAsync("api/order/1/process", null);

            //Assert
            var filePath = Path.Combine(Environment.CurrentDirectory, "Order_1.xml");
            await VerifyFile(filePath);
        }

        [Test]
        public async Task SendEmail_ShouldSendExpectedRequest()
        {
            // Arrange
            var testServer = new InMemoryServer();

            StringContent capturedContent = null;

            testServer.EmailSender().Setup(sender => sender.SendEmail(It.IsAny<StringContent>()))
                .Callback<StringContent>(content => capturedContent = content);

            // Act  
            var response = await testServer.Client().PostAsync("api/order/1/process", null);

            // Assert
            await HttpResponseAsserter.AssertThat(response).HasStatusCode(HttpStatusCode.OK);

            var actualEmailPayloadJson = await capturedContent.ReadAsStringAsync();
            var actualEmailPayload = JsonSerializer.Deserialize<Dictionary<string, string>>(actualEmailPayloadJson);

            actualEmailPayload["to"].Should().Be("customer@example.com");
            actualEmailPayload["subject"].Should().Be($"Order Confirmation - Order #1");
            actualEmailPayload["body"].Should().Be($"Dear Customer,\n\nThank you for your order #1. Your order has been processed and will be delivered soon.\n\nBest Regards,\nWarehouse Team");
            capturedContent.Headers.ContentType.MediaType.Should().Be("application/json");
            capturedContent.Headers.ContentType.CharSet.Should().Be("utf-8");
        }

        [TestCase(10, DeliveryType.Express, 10, 60)]
        [TestCase(11, DeliveryType.Express, 10, 120)]
        [TestCase(11, DeliveryType.Express, 18, 150)]
        [TestCase(11, DeliveryType.Express, 19, 150)]
        [TestCase(11, DeliveryType.SameDay, 10, 180)]
        [TestCase(11, DeliveryType.SameDay, 14, 160)]
        [TestCase(11, DeliveryType.SameDay, 12, 160)]
        [TestCase(11, DeliveryType.Standard, 12, 80)]
        [TestCase(50, DeliveryType.Standard, 12, 80)]
        [TestCase(51, DeliveryType.Standard, 12, 100)]
        [TestCase(1, DeliveryType.Express, 10, 50)]
        [TestCase(5, DeliveryType.Express, 10, 50)]
        [TestCase(6, DeliveryType.Express, 10, 60)]
        [TestCase(6, DeliveryType.Express, 18, 70)]
        [TestCase(1, DeliveryType.SameDay, 10, 90)]
        [TestCase(1, DeliveryType.SameDay, 14, 110)]
        [TestCase(1, DeliveryType.Standard, 10, 20)]
        [TestCase(1, DeliveryType.Standard, 18, 40)]
        [TestCase(1, DeliveryType.Standard, 19, 40)]
        [TestCase(1, DeliveryType.SameDay, 12, 110)]

        public void TestPriorityCalculation(int quantity, DeliveryType deliveryType, int hour, int expectedPriority)
        {
            var orderDetails = new OrderDetails
            {
                Quantity = quantity,
                DeliveryType = deliveryType
            };

            var mockedDateTimeProvider = new Mock<IDateTimeProvider>();
            mockedDateTimeProvider.Setup(provider => provider.Now).Returns(new DateTime(2024, 11, 7, hour, 10, 10));

            var priority = PriorityCalculator.CalculatePriority(mockedDateTimeProvider.Object, orderDetails);

            priority.Should().Be(expectedPriority);
        }

        public static Arbitrary<DeliveryType> DeliveryTypes()
        {
            var deliveryTypes = new[] { DeliveryType.Standard, DeliveryType.Express, DeliveryType.SameDay };

            return Arb.From(Gen.Elements(deliveryTypes));
        }

        public static Arbitrary<int> QuantityBetween1And100()
        {
            return Arb.From(Gen.Choose(1, 100));
        }

        public static Arbitrary<int> Hours()
        {
            return Arb.From(Gen.Choose(1, 23));
        }

        [Test]
        public void HigherQuantity_ShouldIncreasePriority()
        {
            Configuration.Default.MaxNbOfTest = 100;

            Prop.ForAll(
                DeliveryTypes(),
                QuantityBetween1And100(),
                Hours(),
                (deliveryType, quantity, hour) =>
                {
                    var mockedDateTimeProvider = new Mock<IDateTimeProvider>();
                    mockedDateTimeProvider.Setup(provider => provider.Now).Returns(new DateTime(2024, 11, 7, hour, 10, 10));

                    var lowerQuantityPriority = PriorityCalculator.CalculatePriority(mockedDateTimeProvider.Object, new OrderDetails
                    {
                        Quantity = quantity,
                        DeliveryType = deliveryType
                    });

                    var higherQuantityPriority = PriorityCalculator.CalculatePriority(mockedDateTimeProvider.Object, new OrderDetails
                    {
                        Quantity = quantity + 1,
                        DeliveryType = deliveryType
                    });

                    return lowerQuantityPriority <= higherQuantityPriority;
                }
            ).VerboseCheckThrowOnFailure();
        }


        [Test]
        public void PriorityOrderShouldBeSameDayThenExpressThenStandard()
        {
            Configuration.Default.MaxNbOfTest = 100;

            Prop.ForAll(
                QuantityBetween1And100(),
                Hours(),
                (quantity, hour) =>
                {
                    var mockedDateTimeProvider = new Mock<IDateTimeProvider>();
                    mockedDateTimeProvider.Setup(provider => provider.Now).Returns(new DateTime(2024, 11, 7, hour, 10, 10));

                    var sameDayPriority = PriorityCalculator.CalculatePriority(mockedDateTimeProvider.Object, new OrderDetails
                    {
                        Quantity = quantity,
                        DeliveryType = DeliveryType.SameDay
                    });

                    var expressPriority = PriorityCalculator.CalculatePriority(mockedDateTimeProvider.Object, new OrderDetails
                    {
                        Quantity = quantity,
                        DeliveryType = DeliveryType.Express
                    });

                    var standardPriority = PriorityCalculator.CalculatePriority(mockedDateTimeProvider.Object, new OrderDetails
                    {
                        Quantity = quantity,
                        DeliveryType = DeliveryType.Standard
                    });

                    return sameDayPriority > expressPriority && expressPriority > standardPriority;
                }
            ).VerboseCheckThrowOnFailure();
        }

        [Test]
        public async Task SuccessfullProcessingShouldMarkOrderProcessed()
        {
            //Arrange
            var testServer = new InMemoryServer();
            testServer.DateTimeProvider().Setup(provider => provider.Now).Returns(new DateTime(2024, 11, 7, 10, 10, 10));

            var state = testServer.GetOrderState(1);
            state.Should().Be(Status.New);

            //Act
            var response = await testServer.Client().PostAsync("api/order/1/process", null);

            //Assert
            var newState = testServer.GetOrderState(1);
            newState.Should().Be(Status.Processed);
        }

        [Test]
        public async Task ProcessingShouldReturnOrderProcessDetails()
        {
            //Arrange
            var testServer = new InMemoryServer();
            testServer.DateTimeProvider().Setup(provider => provider.Now).Returns(new DateTime(2024, 11, 7, 10, 10, 10));

            var state = testServer.GetOrderState(1);
            state.Should().Be(Status.New);

            //Act
            var response = await testServer.Client().PostAsync("api/order/1/process", null);

            //Assert
            await HttpResponseAsserter.AssertThat(response).HasStatusCode(HttpStatusCode.OK);
            await HttpResponseAsserter.AssertThat(response).HasJsonInBody(new
            {
                orderId = 1,
                totalCost = 94.95,
                estimatedDeliveryDate = new DateTime(2024, 11, 12, 10, 10, 10),
                deliveryType = "Express"
            });
        }

        [Test]
        public async Task ProcessingShouldBadRequestWhenNonExistingOrderPassed()
        {
            //Arrange
            var testServer = new InMemoryServer();
            testServer.DateTimeProvider().Setup(provider => provider.Now).Returns(new DateTime(2024, 11, 7, 10, 10, 10));

            var state = testServer.GetOrderState(1);
            state.Should().Be(Status.New);

            //Act
            var response = await testServer.Client().PostAsync("api/order/99/process", null);

            //Assert
            await HttpResponseAsserter.AssertThat(response).HasStatusCode(HttpStatusCode.BadRequest);
            await HttpResponseAsserter.AssertThat(response).HasTextInBody("Order not found.");
        }

        [Test]
        public async Task ProcessingShouldFail_whenInsufficientStockForOrder()
        {
            //Arrange
            var testServer = new InMemoryServer();
            testServer.DateTimeProvider().Setup(provider => provider.Now).Returns(new DateTime(2024, 11, 7, 10, 10, 10));

            var productId = 200;
            testServer.InsertOrder(2, productId, 11);
            testServer.InsertProduct(productId, 10, 18.99m);

            //Act
            var response = await testServer.Client().PostAsync("api/order/2/process", null);

            //Assert
            await HttpResponseAsserter.AssertThat(response).HasStatusCode(HttpStatusCode.BadRequest);
            await HttpResponseAsserter.AssertThat(response).HasTextInBody("Insufficient stock to process the order.");
        }

        [Test]
        public async Task ProcessingShouldDecreaseStockLevel()
        {
            //Arrange
            var testServer = new InMemoryServer();
            testServer.DateTimeProvider().Setup(provider => provider.Now).Returns(new DateTime(2024, 11, 7, 10, 10, 10));

            var state = testServer.GetOrderState(1);
            state.Should().Be(Status.New);

            //Act
            var response = await testServer.Client().PostAsync("api/order/1/process", null);

            //Assert
            var stockLevel = testServer.GetStockLevel(100);
            stockLevel.Should().Be(5);
        }

        [Test]
        public async Task DeliverDateShouldBeLowerWithHigQuanttiy()
        {
            //Arrange
            var testServer = new InMemoryServer();
            testServer.DateTimeProvider().Setup(provider => provider.Now).Returns(new DateTime(2024, 11, 7, 10, 10, 10));

            var productId = 200;
            testServer.InsertOrder(2, productId, 11);
            testServer.InsertProduct(productId, 100, 18.99m);

            //Act
            var response = await testServer.Client().PostAsync("api/order/2/process", null);

            //Assert
            await HttpResponseAsserter.AssertThat(response).HasStatusCode(HttpStatusCode.OK);
            await HttpResponseAsserter.AssertThat(response).HasJsonInBody(new
            {
                orderId = 2,
                totalCost = 208.89,
                estimatedDeliveryDate = new DateTime(2024, 11, 9, 10, 10, 10),
                deliveryType = "Express"
            });
        }


        [Test]
        public async Task WhenEmailSenderHasIssues_thenReturnsInternalServerErro()
        {
            // Arrange
            var testServer = new InMemoryServer();

            StringContent capturedContent = null;

            testServer.EmailSender().Setup(sender => sender.SendEmail(It.IsAny<StringContent>())).Throws(new ApplicationException("Something bad happened when sending email"));

            // Act  
            var response = await testServer.Client().PostAsync("api/order/1/process", null);

            // Assert
            await HttpResponseAsserter.AssertThat(response).HasStatusCode(HttpStatusCode.InternalServerError);
            await HttpResponseAsserter.AssertThat(response).HasTextInBody("Internal server error: Something bad happened when sending email");
        }
    }
}
