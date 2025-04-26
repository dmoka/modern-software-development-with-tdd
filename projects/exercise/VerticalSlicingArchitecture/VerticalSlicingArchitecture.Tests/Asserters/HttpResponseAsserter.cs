﻿using System.Net;
using FluentAssertions;
using Newtonsoft.Json;
using VerticalSlicingArchitecture.Database;

namespace VerticalSlicingArchitecture.Tests.Asserters
{
    public class HttpResponseAsserter : AbstractAsserter<HttpResponseMessage, HttpResponseAsserter>
    {

        public static HttpResponseAsserter AssertThat(HttpResponseMessage httpResponseMessage)
        {
            return new HttpResponseAsserter(httpResponseMessage);
        }

        public HttpResponseAsserter(HttpResponseMessage actual) : base(actual)
        {
        }

        public HttpResponseAsserter HasFailedStatus()
        {
            Actual.IsSuccessStatusCode.Should().BeFalse();

            return this;
        }

        public HttpResponseAsserter HasLocation(string headerLocationAsString)
        {
            var locationHeader = Actual.Headers.Location;
            locationHeader.Should().NotBeNull();
            locationHeader.ToString().Should().Be(headerLocationAsString);

            return this;
        }

        public async Task<HttpResponseAsserter> HasStatusCode(HttpStatusCode statusCode)
        {
            var bodyContent = await Actual.Content.ReadAsStringAsync();
            Actual.StatusCode.Should().Be(statusCode, bodyContent);

            return this;
        }

        public async Task<HttpResponseAsserter> HasEmptyJsonBody()
        {
            var content = await Actual.Content.ReadAsStringAsync();
            content.Should().BeEmpty();

            return this;
        }

        public async Task<HttpResponseAsserter> HasJsonInBody(string expectedJson)
        {
            var content = await Actual.Content.ReadAsStringAsync();
            JsonAsserter.AssertThat(content)
                .IsEqualTo(expectedJson);

            return this;
        }

        public async Task<HttpResponseAsserter> HasTextInBody(string expectedText)
        {
            var content = await Actual.Content.ReadAsStringAsync();

            content.Should().Be(expectedText);

            return this;
        }

        public async Task<HttpResponseAsserter> HasJsonInBody(dynamic expectedJson)
        {
            JsonAsserter.AssertThat(await Actual.Content.ReadAsStringAsync())
                .IsEqualTo(JsonConvert.SerializeObject(expectedJson));

            return this;
        }
		
		        public async Task<HttpResponseAsserter> HasValueBody<T>(T expectedText)
        {
            var responseBody = await Actual.Content.ReadAsStringAsync();
            var id = JsonSerializer.Deserialize<T>(responseBody);

            id.Should().Be(expectedText);

            return this;
        }

        public async Task<HttpResponseAsserter> HasJsonArrayInBody(string expectedJson)
        {
            JsonAsserter.AssertThat(await Actual.Content.ReadAsStringAsync())
                .IsEqualToArray(expectedJson);

            return this;
        }

        public async Task<HttpResponseAsserter> HasJsonArrayInBody(dynamic expectedJson)
        {
            JsonAsserter.AssertThat(await Actual.Content.ReadAsStringAsync())
                .IsEqualToArray(JsonConvert.SerializeObject(expectedJson));

            return this;
        }

        public async Task<HttpResponseAsserter> HasEmptyJsonArrayInBody()
        {
            JsonAsserter.AssertThat(await Actual.Content.ReadAsStringAsync())
                .IsEqualToArray(JsonConvert.SerializeObject(new dynamic[] { }));

            return this;
        }
    }
}
