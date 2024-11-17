using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using FluentAssertions;
using FsCheck;
using Moq;
using Moq.Protected;
using NUnit.Framework;
using RefactoringLegacyCode.Tests.Asserters;
using RefactoringLegacyCode.Tests.Shared;
using VerifyNUnit;

namespace RefactoringLegacyCode.Tests
{
    public class OrderControllerTests
    {
        [Test]
        public async Task asd()
        {
            //Arrange
            var testServer = new InMemoryServer();

            testServer.DateTimeProvider().Setup(provider => provider.Now).Returns(new DateTime(2024, 11, 7, 10, 10, 10));

            //Act
            var response = await testServer.Client().PostAsync("api/order/1/process", null);

            //Assert
            var filePath = Path.Combine(Environment.CurrentDirectory, "Order_1.xml");
            await Verifier.VerifyFile(filePath);
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
    }
}
