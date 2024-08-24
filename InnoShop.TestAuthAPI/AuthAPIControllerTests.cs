using Xunit;
using Moq;
using InnoShop.Services.AuthAPI.Controllers;
using InnoShop.Services.AuthAPI.Models;
using InnoShop.Services.AuthAPI.Service.IService;
using InnoShop.Services.AuthAPI.Models.DTO;
using InnoShop.Services.AuthAPI.Models.PasswordModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace InnoShop.TestAuthAPI
{
    public class AuthAPIControllerTests
    {
        private readonly Mock<IAuthService> _authServiceMock;
        private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
        private readonly AuthAPIController _controller;

        public AuthAPIControllerTests()
        {
            _authServiceMock = new Mock<IAuthService>();
            _userManagerMock = new Mock<UserManager<ApplicationUser>>(
                Mock.Of<IUserStore<ApplicationUser>>(), null, null, null, null, null, null, null, null
            );

            _controller = new AuthAPIController(_authServiceMock.Object, _userManagerMock.Object);
        }


        [Fact]
        public async Task Register_CorrectData_ReturnsOk()
        {
            // Arrange
            var registrationRequest = new RegistrationRequestDTO
            {
                Name = "test1",
                Email = "test1mail@gmail.com",
                PhoneNumber = "1234456",
                Password = "Admin123*"
            };

            _authServiceMock.Setup(s => s.Register(It.IsAny<RegistrationRequestDTO>())).ReturnsAsync(string.Empty);

            //Act
            var result = await _controller.Register(registrationRequest);

            //Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.True(((ResponseDTO)okResult.Value).IsSuccess);
        }


        [Fact]
        public async Task Register_ShortPassword_ReturnsBadRequest()
        {
            // Arrange
            var weakPassword = "78";
            var registrationRequest = new RegistrationRequestDTO
            {
                Name = "test2",
                Email = "test2mail@gmail.com",
                PhoneNumber = "234456",
                Password = weakPassword
            };

            _authServiceMock.Setup(s => s.Register(It.Is<RegistrationRequestDTO>(r => r.Password == weakPassword)))
                            .ReturnsAsync("Passwords must be at least 6 characters.");

            // Act
            var result = await _controller.Register(registrationRequest);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var response = (ResponseDTO)badRequestResult.Value;
            Assert.False(response.IsSuccess);
            Assert.Equal("Passwords must be at least 6 characters.", response.Message);
        }


        [Fact]
        public async Task Register_ExistingEmail_ReturnsBadRequest()
        {
            // Arrange
            var existingEmail = "existingemail@gmail.com";
            var registrationRequest = new RegistrationRequestDTO
            {
                Name = "test3",
                Email = existingEmail,
                PhoneNumber = "334456",
                Password = "2-Password"
            };

            _authServiceMock.Setup(s => s.Register(It.IsAny<RegistrationRequestDTO>()))
                            .ReturnsAsync($"Username '{existingEmail}' is already taken.");

            // Act
            var result = await _controller.Register(registrationRequest);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var response = (ResponseDTO)badRequestResult.Value;
            Assert.False(response.IsSuccess);
            Assert.Equal($"Username '{existingEmail}' is already taken.", response.Message);
        }

        [Fact]
        public async Task Login_CorrectCredentials_ReturnsOk()
        {
            //Arrange
            var loginRequestDto = new LoginRequestDTO
            {
                UserName = "testmail@gmail.com",
                Password = "Admin123*"
            };

            var expectedUser = new UserDTO
            {
                Id = "1",
                Email = "testmail@gmail.com",
                PhoneNumber = "56477374",
                Name = "User"
            };

            var expectedResponse = new LoginResponseDTO
            {
                User = expectedUser,
                Token = "Some generated token"
            };

            _authServiceMock.Setup(s => s.Login(It.IsAny<LoginRequestDTO>()))
                            .ReturnsAsync(expectedResponse);

            //Act
            var result=await _controller.Login(loginRequestDto);

            //Assert
            var okResult=Assert.IsType<OkObjectResult>(result);
            var response= Assert.IsType<ResponseDTO>(okResult.Value);
            Assert.True(response.IsSuccess);

            var loginResponse = Assert.IsType<LoginResponseDTO>(response.Result);
            Assert.Equal(expectedResponse.User.Email, loginResponse.User.Email);
            Assert.Equal(expectedResponse.Token, loginResponse.Token);
        }

        [Fact]
        public async Task Login_UserDoesntExist_ReturnsBadRequest()
        {
            // Arrange
            var loginRequest = new LoginRequestDTO
            {
                UserName = "nonexistentuser@gmail.com",
                Password = "AnyPassword"
            };

            var loginResponse = new LoginResponseDTO
            {
                User = null,
                Token = ""
            };

            _authServiceMock.Setup(s => s.Login(It.IsAny<LoginRequestDTO>()))
                .ReturnsAsync(loginResponse);

            // Act
            var result = await _controller.Login(loginRequest);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var response = Assert.IsType<ResponseDTO>(badRequestResult.Value);
            Assert.False(response.IsSuccess);
            Assert.Equal("Credentials are incorrect or account is not confirmed.", response.Message);
        }

        [Fact]
        public async Task ConfirmEmail_Correct_ReturnsOk()
        {
            //Arrange
            string userId = "someExistingId";
            _authServiceMock.Setup(s=>s.ConfirmAccount(It.IsAny<string>()))
                .ReturnsAsync(string.Empty);

            //Act
            var result=await _controller.ConfirmEmail(userId);

            //Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, okResult.StatusCode);

        }

        [Fact]
        public async Task ForgotPassword_CorrectModel_ReturnsOk()
        {
            // Arrange
            var forgotPasswordModel = new ForgotPasswordViewModel
            {
                Email = "test@example.com"
            };

            _authServiceMock.Setup(s => s.ForgotPassword(It.IsAny<ForgotPasswordViewModel>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _controller.ForgotPassword(forgotPasswordModel);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<ResponseDTO>(okResult.Value);
            Assert.True(response.IsSuccess);
            Assert.Equal("Email sent", response.Message);
        }

        [Fact]
        public async Task ForgotPassword_InvalidModel_ReturnsBadRequest()
        {
            // Arrange
            var forgotPasswordModel = new ForgotPasswordViewModel
            {
                Email = "invalid-email"
            };

            _controller.ModelState.AddModelError("Email", "Invalid email format");

            // Act
            var result = await _controller.ForgotPassword(forgotPasswordModel);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var response = Assert.IsType<ResponseDTO>(badRequestResult.Value);
            Assert.False(response.IsSuccess);
            Assert.Equal("An error occured", response.Message);
        }

        [Fact]
        public async Task ResetPassword_InvalidUser_ReturnsBadRequest()
        {
            string userId = "invalidId";
            _authServiceMock.Setup(s => s.ResetPassword(It.IsAny<string>()))
                .ReturnsAsync("Error");

            //Act
            var result = await _controller.ResetPassword(userId);

            //Assert
            var badResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(400, badResult.StatusCode);

        }

        [Fact]
        public async Task ResetPassword_ValidUser_ReturnsBadRequest()
        {
            // Arrange
            string userId = "existingUserId";
            string resetCode = "resetCode123";
            _authServiceMock.Setup(s => s.ResetPassword(It.IsAny<string>()))
                .ReturnsAsync(resetCode);

            // Act
            var result = await _controller.ResetPassword(userId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(resetCode, okResult.Value);

        }

        [Fact]
        public async Task SaveNewPassword_PasswordSavedSuccessfully_ReturnsOk()
        {
            // Arrange
            var model = new ResetPasswordViewModel
            {
                Email = "test@example.com",
                Password = "StrongPassword123",
                ConfirmPassword = "StrongPassword123",
                Code = "ResetCode123"
            };

            var response = new ResponseDTO { IsSuccess = true };

            _authServiceMock.Setup(s => s.SavePassword(It.IsAny<ResetPasswordViewModel>()))
                .ReturnsAsync(response);

            // Act
            var result = await _controller.SaveNewPassword(model);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var responseObject = Assert.IsType<ResponseDTO>(okResult.Value);
            Assert.True(responseObject.IsSuccess);
        }

        [Fact]
        public async Task SaveNewPassword_SavePasswordFailed_ReturnsBadRequest()
        {
            // Arrange
            var model = new ResetPasswordViewModel
            {
                Email = "test@example.com",
                Password = "StrongPassword123",
                ConfirmPassword = "StrongPassword123",
                Code = "ResetCode123"
            };

            var response = new ResponseDTO { IsSuccess = false, Message = "Error saving password" };

            _authServiceMock.Setup(s => s.SavePassword(It.IsAny<ResetPasswordViewModel>()))
                .ReturnsAsync(response);

            // Act
            var result = await _controller.SaveNewPassword(model);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var responseObject = Assert.IsType<ResponseDTO>(badRequestResult.Value);
            Assert.False(responseObject.IsSuccess);
            Assert.Equal("Error saving password", responseObject.Message);
        }

        [Fact]
        public async Task GetUser_UserExists_ReturnsOk()
        {
            // Arrange
            string userId = "existingUserId";
            var user = new ApplicationUser { Id = userId, UserName = "testuser" };

            _userManagerMock.Setup(um => um.FindByIdAsync(userId)).ReturnsAsync(user);

            // Act
            var result = await _controller.GetUser(userId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedUser = Assert.IsType<ApplicationUser>(okResult.Value);
            Assert.Equal(userId, returnedUser.Id);
            Assert.Equal("testuser", returnedUser.UserName);
        }

        [Fact]
        public async Task GetUser_UserDoesNotExist_ReturnsNotFound()
        {
            // Arrange
            string userId = "nonExistingUserId";

            _userManagerMock.Setup(um => um.FindByIdAsync(userId)).ReturnsAsync((ApplicationUser)null);

            // Act
            var result = await _controller.GetUser(userId);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task UpdateUser_UpdateSuccessful_ReturnsOk()
        {
            // Arrange
            string userId = "existingUserId";
            var model = new UpdateUserDTO
            {
                Name = "Updated Name",
                Email = "updatedemail@example.com"
            };

            var response = new ResponseDTO { IsSuccess = true, Message = "User successfully updated" };

            _authServiceMock.Setup(s => s.UpdateUserAsync(userId, model))
                .ReturnsAsync(response);

            // Act
            var result = await _controller.UpdateUser(userId, model);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal("User successfully updated", okResult.Value);
        }

        [Fact]
        public async Task UpdateUser_UpdateFails_ReturnsBadRequest()
        {
            // Arrange
            string userId = "existingUserId";
            var model = new UpdateUserDTO
            {
                Name = "Updated Name",
                Email = "updatedemail@example.com"
            };

            var response = new ResponseDTO { IsSuccess = false, Message = "Email is already in use" };

            _authServiceMock.Setup(s => s.UpdateUserAsync(userId, model))
                .ReturnsAsync(response);

            // Act
            var result = await _controller.UpdateUser(userId, model);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Email is already in use", badRequestResult.Value);
        }

        [Fact]
        public async Task DeleteUser_DeleteSuccessful_ReturnsOk()
        {
            // Arrange
            string userId = "existingUserId";
            var response = new ResponseDTO 
            { 
                IsSuccess = true, 
                Message = "Successfully deleted"
            };

            _authServiceMock.Setup(s => s.DeleteUserAsync(userId))
                .ReturnsAsync(response);

            // Act
            var result = await _controller.DeleteUser(userId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal("Successfully deleted", okResult.Value);
        }

        [Fact]
        public async Task DeleteUser_DeleteFails_ReturnsBadRequest()
        {
            // Arrange
            string userId = "existingUserId";
            var response = new ResponseDTO 
            { 
                IsSuccess = false, 
                Message = "Failed to delete user" 
            };

            _authServiceMock.Setup(s => s.DeleteUserAsync(userId))
                .ReturnsAsync(response);

            // Act
            var result = await _controller.DeleteUser(userId);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Failed to delete user", badRequestResult.Value);
        }
    }
}