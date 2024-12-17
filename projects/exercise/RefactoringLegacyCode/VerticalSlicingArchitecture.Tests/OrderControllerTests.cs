using System.Net;
using FluentAssertions;
using FsCheck;
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
    public async Task ShouldSaveOrderProcessingInXml()
    {
        using var testServer = new InMemoryServer();

        testServer.DateTimeProviderMock.Setup(mock => mock.Now).Returns(new DateTime(2024, 12, 17, 8, 0, 0));
        var response = await testServer.Client().PostAsync("api/order/1/process", null);

        var xmlPath = Path.Combine(Environment.CurrentDirectory, $"Order_1.xml");
        
        await VerifyFile(xmlPath);
    }

    [TestCase("Express", 18, 11, 150)]
    [TestCase("Express", 17, 11, 120)]
    [TestCase("SameDay", 11, 11, 180)]
    [TestCase("SameDay", 13, 11, 160)]
    [TestCase("SameDay", 13, 11, 160)]
    [TestCase("Standard", 13, 51, 100)]
    [TestCase("Standard", 13, 50, 80)]

    //TODO: add more characterization test to figure out how it works
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
        //TODO: exercise
    }
}