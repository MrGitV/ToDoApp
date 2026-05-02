using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;

namespace Tests
{
    // Integration test to cover Program.cs and middleware configuration
    public class ProgramIntegrationTests(WebApplicationFactory<Program> factory) : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory = factory;

        [Fact]
        public async Task App_Starts_And_RedirectsUnauthorizedUsersToLogin()
        {
            // Act
            var client = _factory.CreateClient(new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false // Stop redirect to see the 302 Status Code
            });
            var response = await client.GetAsync("/");

            // Assert
            // Because User is not authenticated, they should be redirected to Login page
            Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
            Assert.Contains("/Account/Login", response.Headers.Location?.ToString());
        }
    }
}
