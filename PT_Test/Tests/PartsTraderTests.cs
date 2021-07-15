using System.Net.Http;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace PT_Test.Tests
{
    public class PartsTraderTests : IClassFixture<WebApplicationFactory<PT_Test.Startup>>
    {
        private readonly HttpClient _httpClient;

        public PartsTraderTests(WebApplicationFactory<PT_Test.Startup> factory)
        {
            _httpClient = factory.CreateClient();
        }

        /// <summary>
        /// Entries without invalid part numbers should not fail.
        /// </summary>
        [Theory]
        [InlineData("[\"1111-Invoice\", \"2222-Invoice\"]")]
        [InlineData("[\"2222-Invoice\", \"2222-Invoice\"]")]
        [InlineData("[\"2222-Invoice\", \"3333-Invoice\", \"3333-Invoice\"]")]
        public async void SubmitParts_Test_Pass(string request_str)
        {
            var request_content = new StringContent(request_str, System.Text.Encoding.UTF8, "application/json");

            using (var response = await _httpClient.PostAsync("api/CheckCompatibility", request_content))
            {
                Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
                Assert.NotEqual("{}", await response.Content.ReadAsStringAsync());
            }
        }

        /// <summary>
        /// The presence of invalid data will result in an error message.
        /// </summary>
        [Theory]
        [InlineData("[]")]  // no data
        [InlineData("[0]")]  // invalid parameter
        [InlineData("[\"123-ABC\"]")]
        [InlineData("[\"123-abcdef\"]")]
        [InlineData("[\"12345-abc\", \"1234-test\"]")]
        [InlineData("[\"12345-ABCDEF\", \"1234-test\"]")]
        public async void SubmitParts_Test_Fail(string request_str)
        {
            var request_content = new StringContent(request_str, System.Text.Encoding.UTF8, "application/json");

            using (var response = await _httpClient.PostAsync("api/CheckCompatibility", request_content))
            {
                Assert.NotEqual(System.Net.HttpStatusCode.OK, response.StatusCode);
            }
        }

        /// <summary>
        /// Fully-excluded entries should result in an empty dataset.
        /// </summary>
        [Theory]
        [InlineData("[\"1111-Invoice\"]")]
        [InlineData("[\"1111-Invoice\", \"1111-Invoice\"]")]
        [InlineData("[\"1111-Invoice\", \"1234-abcd\"]")]
        [InlineData("[\"1111-Invoice\", \"1234-abcd\", \"9999-charge\"]")]
        public async void SubmitParts_Test_Exclusion(string request_str)
        {
            var request_content = new StringContent(request_str, System.Text.Encoding.UTF8, "application/json");

            using (var response = await _httpClient.PostAsync("api/CheckCompatibility", request_content))
            {
                Assert.Equal("{}", await response.Content.ReadAsStringAsync());
            }
        }
    }
}
