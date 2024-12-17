using System.IO;
using System.Net;
using System.Net.NetworkInformation;
using System.Text.Json;
using FluentAssertions;
using FsCheck;
using Moq;
using RefactoringLegacyCode.Tests.Asserters;
using RefactoringLegacyCode.Tests.Shared;

namespace RefactoringLegacyCode.Tests;

public class OrderControllerTests
{
    [Test]
    public async Task ProcessOrderShouldReturnProcessedOrderDetails()
    {
        using var testServer = new InMemoryServer();

        testServer.DateTimeProviderMock.Setup(mock => mock.Now).Returns(new DateTime(2024, 12, 17, 8, 0, 0));
        var response = await testServer.Client().PostAsync("api/order/1/process", null);


        await HttpResponseAsserter.AssertThat(response).HasStatusCode(HttpStatusCode.OK);

        await HttpResponseAsserter.AssertThat(response).HasJsonInBody(new
        {
            orderId = 1,
            totalCost = 94.95,
            estimatedDeliveryDate = new DateTime(2024, 12, 22, 8, 0, 0),
            deliveryType = "Express"
        });
    }

    [Test]
    public async Task SendEmail_ShouldSendExpectedRequest()
    {
        // Arrange
        var testServer = new InMemoryServer();

        string calledPath = null;
        StringContent capturedContent = null;

        testServer.EmailSenderMock.Setup(sender => sender.SendEmailAsync(It.IsAny<string>(), It.IsAny<StringContent>()))
            .Callback<string, StringContent>((path, content) =>
            {
                calledPath = path;
                capturedContent = content;
            });

        // Act  
        var response = await testServer.Client().PostAsync("api/order/1/process", null);

        // Assert
        await HttpResponseAsserter.AssertThat(response).HasStatusCode(HttpStatusCode.OK);

        var actualEmailPayloadJson = await capturedContent.ReadAsStringAsync();
        var actualEmailPayload = JsonSerializer.Deserialize<Dictionary<string, string>>(actualEmailPayloadJson);

        calledPath.Should().Be("https://api.emailservice.com/send");
        actualEmailPayload["to"].Should().Be("customer@example.com");
        actualEmailPayload["subject"].Should().Be($"Order Confirmation - Order #1");
        actualEmailPayload["body"].Should().Be($"Dear Customer,\n\nThank you for your order #1. Your order has been processed and will be delivered soon.\n\nBest Regards,\nWarehouse Team");
        capturedContent.Headers.ContentType.MediaType.Should().Be("application/json");
        capturedContent.Headers.ContentType.CharSet.Should().Be("utf-8");
    }

    [Test]
    public async Task ShouldSaveOrderProcessingInXml()
    {
        using var testServer = new InMemoryServer();

        testServer.DateTimeProviderMock.Setup(mock => mock.Now).Returns(new DateTime(2024, 12, 17, 8, 0, 0));
        var response = await testServer.Client().PostAsync("api/order/1/process", null);

        var xmlPath = Path.Combine(Environment.CurrentDirectory, $"Order_1.xml");
        
        await VerifyFile(xmlPath);
    }

    [TestCase("Express", 18, 10, 70)]
    [TestCase("Express", 18, 11, 150)]
    [TestCase("Express", 17, 11, 120)]
    [TestCase("SameDay", 11, 11, 180)]
    [TestCase("SameDay", 13, 11, 160)]
    [TestCase("SameDay", 12, 11, 160)]
    [TestCase("SameDay", 13, 11, 160)]
    [TestCase("Standard", 13, 51, 100)]
    [TestCase("Standard", 13, 50, 80)]
    [TestCase("Express", 18, 9, 70)]
    [TestCase("Express", 17, 9, 60)]
    [TestCase("Express", 17, 5, 50)]
    [TestCase("SameDay", 11, 9, 90)]
    [TestCase("SameDay", 12, 9, 110)]
    [TestCase("Standard", 12, 9, 20)]
    [TestCase("Standard", 18, 9, 40)]
    public void CharacterizePriorityCalculation(string deliverType, int hour, int quantity, int expectedPriority)
    {
        var dateTime = new DateTime(2024, 12, 17, hour, 0, 0);
        var orderDetails = new OrderDetails()
        {
            DeliveryType = deliverType,
            Quantity = quantity
        };
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

    public static Arbitrary<string> DeliveryTypes()
    {
        return Arb.From(Gen.Elements("Express", "SameDay", "Standard"));
    }

    [Test]
    public void HigherQuantity_shouldLeadHigherPriority()
    {
        Prop.ForAll(DeliveryTypes(), Hours(), Quantities(), (deliveryType, hour, quantity) =>
        {
            var dateTime = new DateTime(2024, 12, 17, hour, 0, 0);
            var orderDetails = new OrderDetails()
            {
                DeliveryType = deliveryType,
                Quantity = quantity
            };
            var priority = OrderController.CalculatePriority(orderDetails, dateTime);

            orderDetails = new OrderDetails()
            {
                DeliveryType = deliveryType,
                Quantity = quantity + 1
            };
            var higherPriority = OrderController.CalculatePriority(orderDetails, dateTime);

            return higherPriority >= priority;

        }).VerboseCheckThrowOnFailure();
    }

    [Test]
    public void PriorityOrderShouldBe_SameDay_Then_Express_Then_Standard()
    {
        Configuration.Default.MaxNbOfTest = 100;

        Prop.ForAll(
            Quantities(),
            Hours(),
            (quantity, hour) =>
            {
                var dateTime = new DateTime(2024, 12, 17, hour, 0, 0);

                var orderDetails = new OrderDetails
                {
                    Quantity = quantity,
                    DeliveryType = "SameDay"
                };
                var sameDayPriority = OrderController.CalculatePriority(orderDetails, dateTime);

                orderDetails = new OrderDetails
                {
                    Quantity = quantity,
                    DeliveryType = "Express"
                };
                var expressPriority = OrderController.CalculatePriority(orderDetails, dateTime);

                orderDetails = new OrderDetails
                {
                    Quantity = quantity,
                    DeliveryType = "Standard"
                };
                var standardPriority = OrderController.CalculatePriority(orderDetails, dateTime);


                return sameDayPriority > expressPriority && expressPriority > standardPriority;
            }
        ).VerboseCheckThrowOnFailure();
    }

    [Test]
    public async Task SuccessfullProcessingShouldMarkOrderProcessed()
    {
        //Arrange
        var testServer = new InMemoryServer();
        testServer.DateTimeProviderMock.Setup(provider => provider.Now).Returns(new DateTime(2024, 11, 7, 10, 10, 10));

        var state = testServer.GetOrderState(1);
        state.Should().Be("New");

        //Act
        var response = await testServer.Client().PostAsync("api/order/1/process", null);

        //Assert
        var newState = testServer.GetOrderState(1);
        newState.Should().Be("Processed");
    }

    [Test]
    public async Task ProcessingShouldBadRequestWhenNonExistingOrderPassed()
    {
        //Arrange
        var testServer = new InMemoryServer();
        testServer.DateTimeProviderMock.Setup(provider => provider.Now).Returns(new DateTime(2024, 11, 7, 10, 10, 10));

        var state = testServer.GetOrderState(1);
        state.Should().Be("New");

        //Act
        var response = await testServer.Client().PostAsync("api/order/99/process", null);

        //Assert
        await HttpResponseAsserter.AssertThat(response).HasStatusCode(HttpStatusCode.BadRequest);
        await HttpResponseAsserter.AssertThat(response).HasTextInBody("Order not found.");
    }

    [Test]
    public async Task ShouldProcessOrderWhenUsedAllStockAviable()
    {
        using var testServer = new InMemoryServer();

        testServer.InsertOrder(2, 200, 5);
        testServer.InsertProduct(200, 5, 20);
        testServer.DateTimeProviderMock.Setup(mock => mock.Now).Returns(new DateTime(2024, 12, 17, 8, 0, 0));

        var response = await testServer.Client().PostAsync("api/order/2/process", null);

        await HttpResponseAsserter.AssertThat(response).HasStatusCode(HttpStatusCode.OK);

        var stockLevel = testServer.GetStockLevel(200);
        stockLevel.Should().Be(0);
    }

    [Test]
    public async Task ShouldFailWhenStockNotAvaiable()
    {
        using var testServer = new InMemoryServer();

        testServer.InsertOrder(2, 200, 5);
        testServer.InsertProduct(200, 4, 20);
        testServer.DateTimeProviderMock.Setup(mock => mock.Now).Returns(new DateTime(2024, 12, 17, 8, 0, 0));

        var response = await testServer.Client().PostAsync("api/order/2/process", null);

        await HttpResponseAsserter.AssertThat(response).HasStatusCode(HttpStatusCode.BadRequest);
        await HttpResponseAsserter.AssertThat(response).HasTextInBody("Insufficient stock to process the order.");
    }

    [Test]
    public async Task ShouldReturnDifferentDeliverDateWhenQuantityIsBiggerThanFive()
    {
        using var testServer = new InMemoryServer();

        testServer.InsertOrder(2, 200, 6);
        testServer.InsertProduct(200, 6, 20);
        testServer.DateTimeProviderMock.Setup(mock => mock.Now).Returns(new DateTime(2024, 12, 17, 8, 0, 0));

        var response = await testServer.Client().PostAsync("api/order/2/process", null);

        await HttpResponseAsserter.AssertThat(response).HasStatusCode(HttpStatusCode.OK);

        await HttpResponseAsserter.AssertThat(response).HasJsonInBody(new
        {
            orderId = 2,
            totalCost = 120,
            estimatedDeliveryDate = new DateTime(2024, 12, 19, 8, 0, 0),
            deliveryType = "Express"
        });
    }

}