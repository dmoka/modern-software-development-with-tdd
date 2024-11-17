using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using FluentAssertions;
using FsCheck;
using Moq;
using Moq.Protected;
using RefactoringLegacyCode.Tests.Asserters;
using RefactoringLegacyCode.Tests.Shared;

namespace RefactoringLegacyCode.Tests
{
    public class OrderControllerTests
    {
        [Test]
        public async Task asd()
        {
            var testServer = new InMemoryServer();
            
            var client = testServer.CreateClient();

            var response = await client.PostAsync("api/order/1/process", null);

           // await HttpResponseAsserter.AssertThat(response).HasStatusCode(HttpStatusCode.InternalServerError);
           // await HttpResponseAsserter.AssertThat(response).HasTextInBody("dsa");
        }

        [Test]
        public async Task SendEmail_ShouldSendExpectedRequest()
        {
            // Arrange
            var testServer = new InMemoryServer();

            var client = testServer.CreateClient();

            StringContent capturedContent = null;

            // Setting up the mock to capture the StringContent passed to SendEmail
            testServer.EmailSender().Setup(sender => sender.SendEmail(It.IsAny<StringContent>()))
                .Callback<StringContent>(content => capturedContent = content);

            var response = await client.PostAsync("api/order/1/process", null);

            // Act  
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
