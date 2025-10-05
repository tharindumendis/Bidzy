using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bidzy.API.Controllers;
using Bidzy.API.DTOs.userDtos;
using Bidzy.Application.Repository.Interfaces;
using Bidzy.Application.Services;
using Bidzy.Application.Services.Auth;
using Bidzy.Application.Services.Email;
using Bidzy.Domain.Enties;
using FakeItEasy;
using Microsoft.Extensions.Caching.Memory;

namespace Bidzy.Test.API.Controller
{
    public class AuthControllerTests
    {
        private readonly IUserRepository _userRepository;
        private readonly IAuthService _authService;
        private readonly IEmailJobService _emailJobService;
        private readonly IImageService _imageService;

        public AuthControllerTests()
        {
            _userRepository = A.Fake<IUserRepository>();
            _authService = A.Fake<IAuthService>();
            _emailJobService = A.Fake<IEmailJobService>();
            _imageService = A.Fake<IImageService>();
        }
        [Fact]
        public void Constructor_ShouldInitializeDependencies()
        {
            // Arrange
            var memoryCache = A.Fake<IMemoryCache>();
            var cache = new Cache(memoryCache);
            String hashedPassword = "$2a$11$eC1kixZRhj0K0BL0Pksg6.PIq5n7h3H.K7557UJANKTqoplfcxvKW"; // Hashed version of "mypass12"
            User user = new User
            {
                Id = Guid.NewGuid(),
                Email = "tharindumendis@gmail.com",
                PasswordHash = hashedPassword,
                Role = Domain.Enum.UserRole.Admin,
                CreatedAt = DateTime.UtcNow
            };
            LoginDto loginObj = new LoginDto
            {
                Email = "tharindumendis@gmail.com",
                Password = "mypass12"
            };
            A.CallTo(() => _authService.GenerateJwtToken(user.Id, user.Email, user.Role.ToString())).Returns("mocked-jwt-token");
            A.CallTo(() => _userRepository.GetUserByEmailAsync(loginObj.Email)).Returns(user);

            // Act
            var controller = new AuthController(_userRepository, _authService, _emailJobService, memoryCache, _imageService);
            var result = controller.Login(loginObj);
            

            // Assert
            Assert.NotNull(controller);
            Assert.NotNull(result);

        }

    }
}
