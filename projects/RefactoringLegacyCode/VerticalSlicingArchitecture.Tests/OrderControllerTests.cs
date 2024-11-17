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
        public async Task asd()
        {
            var testServer = new InMemoryServer();
            
            var client = testServer.CreateClient();

            var response = await client.PostAsync("api/order/1/process", null);

            await HttpResponseAsserter.AssertThat(response).HasStatusCode(HttpStatusCode.InternalServerError);
            await HttpResponseAsserter.AssertThat(response).HasTextInBody("dsa");
        }
    }
}
