using KanbanIntegrationTests.CustomTestSupportItems;
using KanbanModel.DTOs.ResponseDTOs;
using KanbanModel.ModelClasses;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Json;
using System.Security.Claims;

namespace KanbanIntegrationTests
{
    [TestFixture]
    [Category("Integration")]
    internal class AuthControllerIntegrationTests
    {
        private WebAppFactoryCustom _factory = null!;
        private HttpClient _client = null!;

        [OneTimeSetUp]
        public async Task OneTimeSetUp()
        {
            _factory = new WebAppFactoryCustom();
            _client = _factory.CreateClient();
        }

        [OneTimeTearDown]
        public async Task OneTimeTearDown()
        {
            _client.Dispose();
            _factory.Dispose();
        }
        private class LoginResponse
        {
            public string Token { get; set; } = string.Empty;
        }

        //IMPORTANT: test cases for accessing the Task Controller without tokens, invalid tokens, and valid tokens are in TaskControllerIntegrationTests.cs

        [Test]        
        public async Task LoginValid_GetToken()
        {
            var loginRequest = new
            {
                Username = "admin",
                Password = "admin"
            };

            // Act
            var postResponse = await _client.PostAsJsonAsync("api/auth/login", loginRequest);
            var postResponseResult = await postResponse.Content.ReadFromJsonAsync<LoginResponse>();

            // Decode the JWT token to verify its contents
            var handler = new JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(postResponseResult.Token);

            // Set the token in the Authorization header for subsequent requests
            _client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", postResponseResult!.Token);

            var getResponse = await _client.GetAsync("/api/tasks");            
            var taskReponse = await getResponse.Content.ReadFromJsonAsync<PagedResultKanbanTasksResponse<KanbanTaskResponse>>();

            postResponse.EnsureSuccessStatusCode();
            Assert.That(postResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(postResponseResult, Is.Not.Null);
            Assert.That(postResponseResult!.Token, Is.Not.Null.And.Not.Empty);
            Assert.That(jwt.Claims.Any(c => c.Type == JwtRegisteredClaimNames.UniqueName && c.Value == "admin"), Is.True);
            Assert.That(jwt.Claims.Any(c => c.Type == "role" && c.Value == "Admin"), Is.True);
            Assert.That(getResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(taskReponse.TotalCount, Is.EqualTo(0));
        }

        [Test]
        public async Task LoginInvalid_GetToken()
        {
            var loginRequest = new
            {
                Username = "INVALID USERNAME",
                Password = "INVALID PASSWORD"
            };

            var postResponse = await _client.PostAsJsonAsync("api/auth/login", loginRequest);

            Assert.That(postResponse.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
        }
    }
}
