using System;
using System.Threading.Tasks;
using Bidzy.API.Controllers.Auction;
using Bidzy.API.Controllers.Auth;
using Bidzy.API.DTOs;
using Bidzy.API.DTOs.user;
using Bidzy.Application.Repository.User;
using Bidzy.Application.Services.Admin;
using Bidzy.Application.Services.Auth;
using Bidzy.Application.Services.Email;
using Bidzy.Application.Services.Image;
using Bidzy.Domain.Entities;
using FakeItEasy;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Xunit;

namespace Bidzy.Test.API.Controller
{
    public class AuthControllerTests
    {
        private readonly IUserRepository _userRepository;
        private readonly IAuthService _authService;
        private readonly IEmailJobService _emailJobService;
        private readonly IImageService _imageService;
        private readonly IMemoryCache _memoryCache;
        private readonly IAdminService _adminService;
        private readonly IAdminDashboardHubService _adminDashboardHubService;
        private readonly Bidzy.API.Controllers.Auth.AuthController controller;

        public AuthControllerTests()
        {
            _userRepository = A.Fake<IUserRepository>();
            _authService = A.Fake<IAuthService>();
            _emailJobService = A.Fake<IEmailJobService>();
            _imageService = A.Fake<IImageService>();
            _memoryCache = A.Fake<IMemoryCache>();
            _adminDashboardHubService = A.Fake<AdminDashboardHubService>();
            _adminService = A.Fake<IAdminService>();
            controller = new AuthController(
            _userRepository,
            _authService,
            _emailJobService,
            _memoryCache,
            _imageService,
            _adminDashboardHubService,
            _adminService
        );

        }

        [Fact]
        public async Task Login_ShouldReturnToken_WhenCredentialsAreValid()
        {
            var user = new User
            {
                Id = Guid.NewGuid(),
                Email = "user@example.com",
                PasswordHash = PasswordHasher.Hash("password123"),
                Role = Domain.Enum.UserRole.Bidder,
            };

            var loginDto = new LoginDto { Email = user.Email, Password = "password123" };

            A.CallTo(() => _userRepository.GetUserByEmailAsync(user.Email)).Returns(user);
            A.CallTo(() => _authService.GenerateJwtToken(user.Id, user.Email, user.Role.ToString())).Returns("mocked-token");

            var result = await controller.Login(loginDto);

            var okResult = Assert.IsType<OkObjectResult>(result);
            var json = System.Text.Json.JsonSerializer.Serialize(okResult.Value);
            var response = System.Text.Json.JsonSerializer.Deserialize<TokenResponse>(json);
            Assert.Equal("mocked-token", response.token);
        }

        [Fact]
        public async Task Login_ShouldReturnUnauthorized_WhenEmailNotFound()
        {
            var loginDto = new LoginDto { Email = "unknown@example.com", Password = "password123" };
            A.CallTo(() => _userRepository.GetUserByEmailAsync(loginDto.Email)).Returns((User)null);

            var result = await controller.Login(loginDto);

            Assert.IsType<UnauthorizedObjectResult>(result);
        }

        [Fact]
        public async Task Login_ShouldReturnUnauthorized_WhenPasswordIsInvalid()
        {
            var user = new User
            {
                Id = Guid.NewGuid(),
                Email = "user@example.com",
                PasswordHash = PasswordHasher.Hash("correctpass"),
                Role = Domain.Enum.UserRole.Bidder
            };

            var loginDto = new LoginDto { Email = user.Email, Password = "wrongpass" };

            A.CallTo(() => _userRepository.GetUserByEmailAsync(user.Email)).Returns(user);

            var result = await controller.Login(loginDto);

            Assert.IsType<UnauthorizedObjectResult>(result);
        }

        [Fact]
        public async Task AddUser_ShouldReturnBadRequest_WhenEmailNotValidated()
        {
            var userAddDto = new UserAddDto { Email = "user@example.com", Password = "pass123", file = null };
            var cache = new Cache(_memoryCache);
            object value = null;

            A.CallTo(() => _memoryCache.TryGetValue("user@example.com", out value)).Returns(false);

            var result = await controller.AddUser(userAddDto);

            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task ForgotPassword_ShouldSendOtp_WhenEmailExists()
        {
            var emailDto = new ExitEmailDto { Email = "user@example.com",Action = "send" };
            var user = new User { Email = emailDto.Email };

            A.CallTo(() => _userRepository.GetUserByEmailAsync(emailDto.Email)).Returns(user);
            A.CallTo(() => _emailJobService.SendOTP(A<string>._, emailDto.Email)).Returns(Task.CompletedTask);

            var result = await controller.ForgotPassword(emailDto);

            var okResult = Assert.IsType<OkObjectResult>(result);
            var json = System.Text.Json.JsonSerializer.Serialize(okResult.Value);
            var response = System.Text.Json.JsonSerializer.Deserialize<OtpResponse>(json);
            Assert.True(response.success);
        }

        [Fact]
        public async Task ForgotPassword_ShouldReturnBadRequest_WhenEmailNotFound()
        {
            var emailDto = new ExitEmailDto { Email = "unknown@example.com" };
            A.CallTo(() => _userRepository.GetUserByEmailAsync(emailDto.Email)).Returns((User)null);

            var result = await controller.ForgotPassword(emailDto);

            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task ResetPassword_ShouldReturnBadRequest_WhenOtpIsInvalid()
        {
            var dto = new ResetPasswordDto
            {
                Email = "user@example.com",
                OTP = "123456",
                NewPassword = "newpass",
                ConfirmPassword = "newpass"
            };
            object value = null;

            A.CallTo(() => _memoryCache.TryGetValue(dto.Email, out value)).Returns(false);

            var result = await controller.ResetPassword(dto);

            var badResult = Assert.IsType<BadRequestObjectResult>(result);
            var json = System.Text.Json.JsonSerializer.Serialize(badResult.Value);
            var response = System.Text.Json.JsonSerializer.Deserialize<OtpResponse>(json);
            Assert.False(response.success);
        }

        [Fact]
        public async Task ResetPassword_ShouldReturnBadRequest_WhenPasswordsDoNotMatch()
        {
            var dto = new ResetPasswordDto
            {
                Email = "user@example.com",
                OTP = "123456",
                NewPassword = "newpass",
                ConfirmPassword = "wrongpass"
            };
            object value = null;

            A.CallTo(() => _memoryCache.TryGetValue(dto.Email, out value)).Returns(true);

            var result = await controller.ResetPassword(dto);

            var badResult = Assert.IsType<BadRequestObjectResult>(result);
            var json = System.Text.Json.JsonSerializer.Serialize(badResult.Value);
            var response = System.Text.Json.JsonSerializer.Deserialize<OtpResponse>(json); // use OtpResponse for simplicity
            Assert.False(response.success);
        }

        [Fact]
        public async Task CheckExitEmail_ShouldReturnExistsFlag()
        {
            var dto = new ExitEmailDto { Email = "user@example.com" };
            A.CallTo(() => _userRepository.IsExistByUserEmailAsync(dto.Email)).Returns(true);

            var result = await controller.CheckExitEmail(dto);

            var okResult = Assert.IsType<OkObjectResult>(result);
            var json = System.Text.Json.JsonSerializer.Serialize(okResult.Value);
            var response = System.Text.Json.JsonSerializer.Deserialize<ExistsResponse>(json);
            Assert.True(response.exists);
        }

        [Fact]
        public async Task CheckOtp_ShouldSendOtp_WhenActionIsSend()
        {
            var dto = new ExitEmailDto { Email = "user@example.com", Action = "send" };
            A.CallTo(() => _emailJobService.SendOTP(A<string>._, dto.Email)).Returns(Task.CompletedTask);

            var result = await controller.CheckOtp(dto);

            var okResult = Assert.IsType<OkObjectResult>(result);
            var json = System.Text.Json.JsonSerializer.Serialize(okResult.Value);
            var response = System.Text.Json.JsonSerializer.Deserialize<OtpResponse>(json);
            Assert.True(response.success);
        }

        [Fact]
        public async Task CheckOtp_ShouldReturnSuccess_WhenOtpIsValid()
        {
            var dto = new ExitEmailDto { Email = "user@example.com", OTP = "123456", Action = "check" };
            object value = dto.OTP;

            A.CallTo(() => _memoryCache.TryGetValue(dto.Email, out value)).Returns(true);

            var result = await controller.CheckOtp(dto);

            var okResult = Assert.IsType<OkObjectResult>(result);
            var json = System.Text.Json.JsonSerializer.Serialize(okResult.Value);
            var response = System.Text.Json.JsonSerializer.Deserialize<OtpResponse>(json);
            Assert.True(response.success);
        }

        [Fact]
        public async Task CheckOtp_ShouldReturnBadRequest_WhenOtpIsInvalid()
        {
            // Arrange
            var dto = new ExitEmailDto
            {
                Email = "user@example.com",
                OTP = "wrong-otp"
            };

            // Simulate cache not containing the correct OTP
            object cachedOtp = "123456"; // actual stored OTP
            A.CallTo(() => _memoryCache.TryGetValue(dto.Email, out cachedOtp)).Returns(true);


            // Act
            var result = await controller.CheckOtp(dto);

            // Assert
            var badResult = Assert.IsType<BadRequestObjectResult>(result);
            var json = System.Text.Json.JsonSerializer.Serialize(badResult.Value);
            var response = System.Text.Json.JsonSerializer.Deserialize<OtpResponse>(json);
            Assert.False(response.success);
        }
        private class ExistsResponse
        {
            public bool exists { get; set; }
        }
        private class OtpResponse
        {
            public bool success { get; set; }
        }
        private class ResetPasswordResponse
        {
            public bool success { get; set; }
            public string message { get; set; }
        }
        private class TokenResponse
        {
            public string token { get; set; }
        }
    }
}