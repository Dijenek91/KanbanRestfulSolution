using KanbanRestService.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Moq;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace KanbanTests.Unit.Controllers
{
    [TestFixture]
    internal class AuthControllerTests
    {
        private const string ValidUsername = "admin";
        private const string ValidPassword = "admin";
        private const string SecretKey = "TEST_KEY_1234567890123456789012345";
        private const string Issuer = "unit-tests";
        private const string Audience = "unit-tests-audience";

        private IConfiguration _configuration = null!;
        private IHostEnvironment _environment = null!; // not used in controller but required by constructor

        [SetUp]
        public void SetUp()
        {
            var inMemorySettings = new Dictionary<string, string?>
            {
                { "SuperSecretJwtKey", SecretKey },
                { "Issuer", Issuer },
                { "Audience", Audience }
            };

            _configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings!)
                .Build();

            var mockEnv = new Mock<IHostEnvironment>();
            mockEnv.SetupGet(e => e.EnvironmentName).Returns("UnitTest");
            _environment = mockEnv.Object;
        }

        private AuthController CreateController() => new AuthController(_configuration, _environment);

        [Test]
        public void Login_Returns_Unauthorized_When_Credentials_Are_Invalid()
        {
            var controller = CreateController();

            var badRequest = new AuthController.LoginRequest
            {
                Username = "not-admin",
                Password = "wrong"
            };

            var result = controller.Login(badRequest);

            Assert.That(result, Is.InstanceOf<UnauthorizedResult>());
        }

        [Test]
        public void Login_Returns_Ok_With_Token_When_Credentials_Are_Valid()
        {
            var controller = CreateController();

            var request = new AuthController.LoginRequest
            {
                Username = ValidUsername,
                Password = ValidPassword
            };

            var result = controller.Login(request);

            Assert.That(result, Is.InstanceOf<OkObjectResult>());

            var ok = (OkObjectResult)result;
            Assert.That(ok.Value, Is.Not.Null);

            // token is returned inside an anonymous object: new { token = jwt }
            var tokenProp = ok.Value!.GetType().GetProperty("token");
            Assert.That(tokenProp, Is.Not.Null, "Response object does not contain 'token' property");

            var token = tokenProp!.GetValue(ok.Value) as string;
            Assert.That(token, Is.Not.Null.And.Not.Empty, "Token is null or not a string");
        }

        [Test]
        public void Token_Is_Valid_Jwt_And_Contains_Expected_Claims()
        {
            var controller = CreateController();

            var request = new AuthController.LoginRequest
            {
                Username = ValidUsername,
                Password = ValidPassword
            };

            var result = controller.Login(request);
            var ok = (OkObjectResult)result;
            Assert.That(ok.Value, Is.Not.Null);

            var tokenProp = ok.Value!.GetType().GetProperty("token");
            Assert.That(tokenProp, Is.Not.Null);

            var token = tokenProp!.GetValue(ok.Value) as string;
            Assert.That(token, Is.Not.Null.And.Not.Empty);

            var handler = new JwtSecurityTokenHandler();
            var keyBytes = Encoding.UTF8.GetBytes(SecretKey);

            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = Issuer,
                ValidateAudience = true,
                ValidAudience = Audience,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(keyBytes),
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };

            SecurityToken validatedToken;
            var principal = handler.ValidateToken(token!, validationParameters, out validatedToken);

            Assert.That(principal, Is.Not.Null);
            Assert.That(principal.Identity?.IsAuthenticated, Is.True, "Principal should be authenticated");
            Assert.That(principal.Identity?.Name, Is.EqualTo(ValidUsername));

            var nameClaim = principal.FindFirst(ClaimTypes.Name);
            Assert.That(nameClaim, Is.Not.Null);
            Assert.That(nameClaim!.Value, Is.EqualTo(ValidUsername));

            var roleClaim = principal.FindFirst(ClaimTypes.Role);
            Assert.That(roleClaim, Is.Not.Null);
            Assert.That(roleClaim!.Value, Is.EqualTo("Admin"));

            Assert.That(validatedToken, Is.InstanceOf<JwtSecurityToken>());
            var jwtToken = (JwtSecurityToken)validatedToken;
            Assert.That(jwtToken.Issuer, Is.EqualTo(Issuer));
            CollectionAssert.Contains(jwtToken.Audiences, Audience);

            // token expiry should be in the future (controller sets +1 hour)
            Assert.That(jwtToken.ValidTo, Is.GreaterThan(DateTime.UtcNow));
        }
    }
}
