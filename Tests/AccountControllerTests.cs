using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Moq;
using Moq.Protected;
using System.Net;
using ToDoApp.Controllers;
using ToDoApp.Models.ViewModels;

namespace Tests
{
    public class AccountControllerTests
    {
        private readonly Mock<IHttpClientFactory> _httpClientFactoryMock = new();
        private readonly Mock<IConfiguration> _configMock = new();
        private readonly Mock<IAuthenticationService> _authServiceMock = new();
        private readonly AccountController _controller;

        public AccountControllerTests()
        {
            _configMock.Setup(c => c["AuthApiUrl"]).Returns("http://localhost");
            var tokenValidationParams = new TokenValidationParameters();

            // Mock required MVC services to prevent InvalidOperationException
            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(_ => _.GetService(typeof(IAuthenticationService))).Returns(_authServiceMock.Object);
            serviceProviderMock.Setup(_ => _.GetService(typeof(IUrlHelperFactory))).Returns(new Mock<IUrlHelperFactory>().Object);
            serviceProviderMock.Setup(_ => _.GetService(typeof(ITempDataDictionaryFactory))).Returns(new Mock<ITempDataDictionaryFactory>().Object);

            var httpContext = new DefaultHttpContext { RequestServices = serviceProviderMock.Object };

            _controller = new AccountController(_httpClientFactoryMock.Object, _configMock.Object, tokenValidationParams)
            {
                ControllerContext = new ControllerContext { HttpContext = httpContext },
                TempData = new TempDataDictionary(httpContext, Mock.Of<ITempDataProvider>()) // Initialize TempData directly
            };
        }

        // Tests successful GET request for Login page
        [Fact]
        public void Login_Get_ReturnsView()
        {
            var result = _controller.Login() as ViewResult;
            Assert.NotNull(result);
        }

        // Tests if failed API authorization returns view with an error
        [Fact]
        public async Task LoginAsync_ApiFails_ReturnsViewWithError()
        {
            var handlerMock = new Mock<HttpMessageHandler>();
            handlerMock.Protected()
               .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
               .ReturnsAsync(new HttpResponseMessage { StatusCode = HttpStatusCode.Unauthorized });

            var client = new HttpClient(handlerMock.Object);
            _httpClientFactoryMock.Setup(f => f.CreateClient(It.IsAny<string>())).Returns(client);

            var model = new LoginViewModel { Username = "user", Password = "123" };
            var result = await _controller.LoginAsync(model) as ViewResult;

            Assert.NotNull(result);
            Assert.False(_controller.ModelState.IsValid);
        }

        // Tests if Logout calls SignOutAsync and redirects
        [Fact]
        public async Task Logout_RedirectsToLogin()
        {
            var result = await _controller.LogoutAsync() as RedirectToActionResult;

            Assert.Equal("Login", result?.ActionName);
            Assert.Equal("Account", result?.ControllerName);
            _authServiceMock.Verify(a => a.SignOutAsync(It.IsAny<HttpContext>(), It.IsAny<string>(), It.IsAny<AuthenticationProperties>()), Times.Once);
        }

        // Tests AccessDenied page
        [Fact]
        public void AccessDenied_ReturnsView()
        {
            var result = _controller.AccessDenied() as ViewResult;
            Assert.NotNull(result);
        }
    }
}
