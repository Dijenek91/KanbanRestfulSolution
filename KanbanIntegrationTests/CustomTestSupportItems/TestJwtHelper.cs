using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;

namespace KanbanIntegrationTests.CustomTestSupportItems
{
    public class TestJwtHelper
    {
        public static string GenerateToken()
        {
            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes("TEST_KEY_1234567890123456789012345"));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: "MyKanbanTaskApp",
                audience: "MyKanbanTaskApp",
                claims: new[]
                {
                    new Claim(ClaimTypes.Name, "admin"),
                    new Claim(ClaimTypes.Role, "admin")
                },
                expires: DateTime.UtcNow.AddHours(1),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public static async Task<string> GetJwtTokenAsync(HttpClient client)
        {
            var loginRequest = new
            {
                Username = "admin",
                Password = "admin"
            };

            var response = client.PostAsJsonAsync("api/auth/login", loginRequest);
            
            response.Result.EnsureSuccessStatusCode();

            var result = await response.Result.Content.ReadFromJsonAsync<LoginResponse>();

            return result!.Token;            
        }

        private class LoginResponse
        {
            public string Token { get; set; } = string.Empty;
        }
    }
}
