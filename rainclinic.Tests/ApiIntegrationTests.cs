using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using Newtonsoft.Json;
using Xunit;

namespace rainclinic.Tests
{
    public class ApiIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;

        public ApiIntegrationTests(WebApplicationFactory<Program> factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task ProtectedEndpoint_Unauthorized_WithoutToken()
        {
            var response = await _client.GetAsync("/api/example");

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task ProtectedEndpoint_Authorized_WithToken()
        {
            var loginData = new
            {
                Email = "admin@rainclinic.blog",
                Password = "Rainclinic123!",
                RememberMe = false
            };

            var content = new StringContent(JsonConvert.SerializeObject(loginData), Encoding.UTF8, "application/json");
            var tokenResponse = await _client.PostAsync("/api/token", content);
            tokenResponse.EnsureSuccessStatusCode();

            var tokenResult = JsonConvert.DeserializeObject<TokenResult>(await tokenResponse.Content.ReadAsStringAsync());
            var token = tokenResult.Token;

            _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            
            var response = await _client.GetAsync("/api/example");

            
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }
    }

    public class TokenResult
    {
        public string Token { get; set; }
        public System.DateTime Expiration { get; set; }
    }
}
