using InnoShop.Services.AuthAPI.Data;
using InnoShop.Services.AuthAPI.Models;
using InnoShop.Services.AuthAPI.Models.DTO;
using InnoShop.Services.AuthAPI.Models.PasswordModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Org.BouncyCastle.Crypto.Macs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace InnoShop.TestAuthAPI.IntegrationTests
{
    public class AuthAPIControllerIntegrationTests
    {
        private readonly AuthAPIWebApplicationFactory _applicationFactory;
        private readonly HttpClient _httpClient;

        public AuthAPIControllerIntegrationTests()
        {
            _applicationFactory = new AuthAPIWebApplicationFactory();
            _httpClient = _applicationFactory.CreateClient();
        }

        [Fact]
        public async Task Register_CorrectCredentials_ReturnsOk()
        {
            // Arrange
            var requestDTO = new RegistrationRequestDTO
            {
                Email = "testrrrmail@gmail.com",
                Name = "Peet",
                PhoneNumber = "1234567",
                Password = "Admin123*"
            };

            // Act
            var response = await _httpClient.PostAsJsonAsync("/api/auth/register", requestDTO);

            // Assert
            response.EnsureSuccessStatusCode();
            var responseDTO = await response.Content.ReadFromJsonAsync<ResponseDTO>();
            Assert.NotNull(responseDTO);
            Assert.True(responseDTO.IsSuccess);
        }

        [Fact]
        public async Task Register_ExistingEmail_ReturnsBadRequest()
        {
            // Arrange
            var existingEmail = "test2mail@gmail.com";

            var initialRequestDTO = new RegistrationRequestDTO
            {
                Email = existingEmail,
                Name = "Peet2",
                PhoneNumber = "1234567",
                Password = "Admin123*"
            };

            var initialResponse = await _httpClient.PostAsJsonAsync("/api/auth/register", initialRequestDTO);
            initialResponse.EnsureSuccessStatusCode(); // successful

            var duplicateRequestDTO = new RegistrationRequestDTO
            {
                Email = existingEmail,
                Name = "John",
                PhoneNumber = "7654321",
                Password = "Admin123*"
            };

            // Act
            var response = await _httpClient.PostAsJsonAsync("/api/auth/register", duplicateRequestDTO);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

            var responseDTO = await response.Content.ReadFromJsonAsync<ResponseDTO>();
            Assert.False(responseDTO.IsSuccess);
            Assert.Contains($"Username '{existingEmail}' is already taken.", responseDTO.Message);
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

            //Act
            var response = await _httpClient.PostAsJsonAsync("/api/auth/register", registrationRequest);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            var responseDTO = await response.Content.ReadFromJsonAsync<ResponseDTO>();
            Assert.False(responseDTO.IsSuccess);
            Assert.Equal("Passwords must be at least 6 characters.", responseDTO.Message);
        }

        [Fact]
        public async Task ConfirmEmail_InvalidUserId_ReturnsBadRequest()
        {
            // Arrange
            string invalidUserId = "invalid-user-id";

            // Act
            var response = await _httpClient.GetAsync($"/api/auth/ConfirmEmail?userId={invalidUserId}");

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            var content = await response.Content.ReadAsStringAsync();
            Assert.False(string.IsNullOrEmpty(content)); // Ensure there is a response body
            Assert.NotEqual("Email is confirmed", content); // Ensure the response body is not the success message
        }

        // Helper method to get the userId from the in-memory or test database
        private string GetUserIdByEmailAsync(string email)
        {
            // Use the factory's Services to create a scope
            var scope = _applicationFactory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var user = dbContext.Users.FirstOrDefault(x => x.Email == email);
            return user?.Id ?? throw new Exception("User not found");
        }

        [Fact]
        public async Task ConfirmEmail_ValidUserId_ReturnsOk()
        {
            // Arrange
            var registrationDTO = new RegistrationRequestDTO
            {
                Email = "testmail@gmail.com",
                Name = "Peet",
                PhoneNumber = "1234567",
                Password = "Admin123*"
            };

            var registrationResponse = await _httpClient.PostAsJsonAsync("/api/auth/register", registrationDTO);
            registrationResponse.EnsureSuccessStatusCode();

            // Retrieve userId from the test database
            var userId = GetUserIdByEmailAsync("testmail@gmail.com");

            // Act
            var response = await _httpClient.GetAsync($"/api/auth/ConfirmEmail?userId={userId}");

            // Assert
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            Assert.Equal("Email is confirmed", content);
        }

        [Fact]
        public async Task Login_CorrectCredentials_ReturnsOk()
        {
            // Arrange
            var registrationDTO = new RegistrationRequestDTO
            {
                Email = "testmail@gmail.com",
                Name = "Peet",
                PhoneNumber = "1234567",
                Password = "Admin123*"
            };

            var registrationResponse = await _httpClient.PostAsJsonAsync("/api/auth/register", registrationDTO);
            registrationResponse.EnsureSuccessStatusCode();
            
            //Confirming email
            var userId = GetUserIdByEmailAsync("testmail@gmail.com");
            var confirmResponse = await _httpClient.GetAsync($"/api/auth/ConfirmEmail?userId={userId}");
            confirmResponse.EnsureSuccessStatusCode();

            var loginDTO = new LoginRequestDTO
            {
                UserName = "testmail@gmail.com",
                Password = "Admin123*"
            };

            // Act
            var response = await _httpClient.PostAsJsonAsync("/api/auth/login", loginDTO);

            // Assert
            response.EnsureSuccessStatusCode();
        }

        [Fact]
        public async Task Login_InvalidCredentials_ReturnsBadRequest()
        {
            // Arrange
            var registrationDTO = new RegistrationRequestDTO
            {
                Email = "testmail@gmail.com",
                Name = "Peet",
                PhoneNumber = "1234567",
                Password = "Admin123*"
            };

            var registrationResponse = await _httpClient.PostAsJsonAsync("/api/auth/register", registrationDTO);
            registrationResponse.EnsureSuccessStatusCode();

            //Confirming email
            var userId = GetUserIdByEmailAsync("testmail@gmail.com");
            var confirmResponse = await _httpClient.GetAsync($"/api/auth/ConfirmEmail?userId={userId}");
            confirmResponse.EnsureSuccessStatusCode();

            var loginDTO = new LoginRequestDTO
            {
                UserName = "testmail@gmail.com",
                Password = "Admin123-"
            };

            // Act
            var response = await _httpClient.PostAsJsonAsync("/api/auth/login", loginDTO);

            // Assert
            Assert.Equal(response.StatusCode, HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task SaveNewPassword_InvalidResetCode_ReturnsBadRequest()
        {
            //Arrange
            var registrationDTO = new RegistrationRequestDTO
            {
                Email = "testmail@gmail.com",
                Name = "Peet",
                PhoneNumber = "1234567",
                Password = "Admin123*"
            };

            var registrationResponse = await _httpClient.PostAsJsonAsync("/api/auth/register", registrationDTO);
            registrationResponse.EnsureSuccessStatusCode();

            //Confirming email
            var userId = GetUserIdByEmailAsync("testmail@gmail.com");
            var confirmResponse = await _httpClient.GetAsync($"/api/auth/ConfirmEmail?userId={userId}");
            confirmResponse.EnsureSuccessStatusCode();

            var resetPasswordModel = new ResetPasswordViewModel
            {
                Email = "testmail@gmail.com",
                Password = "Admin123*",
                ConfirmPassword = "Admin123*",
                Code = "InvalidToken"
            };

            // Act
            var response = await _httpClient.PostAsJsonAsync("/api/auth/SaveNewPassword", resetPasswordModel);

            // Assert
            Assert.Equal(response.StatusCode, HttpStatusCode.BadRequest);
            var responseDTO = await response.Content.ReadFromJsonAsync<ResponseDTO>();

            Assert.NotNull(responseDTO);
            Assert.False(responseDTO.IsSuccess);
            Assert.Equal("Error saving password", responseDTO.Message);
        }

        [Fact]
        public async Task SaveNewPassword_ValidCode_ReturnsOk()
        {
            //Arrange
            var email = "testmail@gmail.com";
            var password = "Admin123*";
            var resetToken = "";

            var registrationDTO = new RegistrationRequestDTO
            {
                Email = email,
                Name = "Peet",
                PhoneNumber = "1234567",
                Password = password
            };

            var registrationResponse = await _httpClient.PostAsJsonAsync("/api/auth/register", registrationDTO);
            registrationResponse.EnsureSuccessStatusCode();

            //Confirming email
            var userId = GetUserIdByEmailAsync("testmail@gmail.com");
            var confirmResponse = await _httpClient.GetAsync($"/api/auth/ConfirmEmail?userId={userId}");
            confirmResponse.EnsureSuccessStatusCode();

            // Create a test user
            using (var scope = _applicationFactory.Services.CreateScope())
            {
                var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
                var user = await userManager.FindByEmailAsync(email);
                Assert.NotNull(user); // Ensure the user exists
                resetToken = await userManager.GeneratePasswordResetTokenAsync(user);
            }

            var resetPasswordModel = new ResetPasswordViewModel
            {
                Email = "testmail@gmail.com",
                Password = "Admin123*",
                ConfirmPassword = "Admin123*",
                Code = resetToken
            };

            // Act
            var response = await _httpClient.PostAsJsonAsync("/api/auth/SaveNewPassword", resetPasswordModel);

            // Assert
            response.EnsureSuccessStatusCode();
            var responseDTO = await response.Content.ReadFromJsonAsync<ResponseDTO>();

            Assert.NotNull(responseDTO);
            Assert.True(responseDTO.IsSuccess);
            Assert.Equal("Password successfully reset", responseDTO.Message);
        }

        [Fact]
        public async Task GetUser_InvalidId_ReturnsNotFound()
        {
            // Arrange
            var invalidUserId = "nonexistent-user-id";

            // Act
            var response = await _httpClient.GetAsync($"/api/users/{invalidUserId}");

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task GetUser_ValidId_ReturnsOk()
        {
            // Arrange
            var userEmail = "testuser@example.com";
            var userPassword = "Password123!";
            string userId;

            // Create a test user or fetch an existing one
            using (var scope = _applicationFactory.Services.CreateScope())
            {
                var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
                var user = await userManager.FindByEmailAsync(userEmail);

                if (user == null)
                {
                    user = new ApplicationUser { UserName = userEmail, Email = userEmail,Name="Me" };
                    var createUserResult = await userManager.CreateAsync(user, userPassword);
                    Assert.True(createUserResult.Succeeded);
                }

                userId = user.Id;
            }

            // Act
            var response = await _httpClient.GetAsync($"/api/auth/{userId}");

            // Assert
            response.EnsureSuccessStatusCode(); // Asserts that the status code is 200 OK
            var userResponse = await response.Content.ReadFromJsonAsync<IdentityUser>();
            Assert.NotNull(userResponse);
            Assert.Equal(userId, userResponse.Id);
            Assert.Equal(userEmail, userResponse.Email);
        }

        [Fact]
        public async Task UpdateUser_ValidData_ReturnsOk()
        {
            // Arrange
            var userEmail = "testuser@example.com";
            var userPassword = "Password123!";
            string userId;

            // Create a test user or fetch an existing one
            using (var scope = _applicationFactory.Services.CreateScope())
            {
                var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
                var user = await userManager.FindByEmailAsync(userEmail);

                if (user == null)
                {
                    user = new ApplicationUser { UserName = userEmail, Email = userEmail,Name="NewName" };
                    var createUserResult = await userManager.CreateAsync(user, userPassword);
                    Assert.True(createUserResult.Succeeded); // Ensure the user is created
                }

                userId = user.Id; // Get the ID of the created or fetched user
            }

            // Prepare update user DTO
            var updateUserDTO = new UpdateUserDTO
            {
                Email = "Newemail@example.com",
                Name = "NewName",
                PhoneNumber = "lalala"
            };

            // Act
            var response = await _httpClient.PutAsJsonAsync($"/api/auth/{userId}", updateUserDTO);

            // Assert
            response.EnsureSuccessStatusCode(); // Asserts that the status code is 200 OK

            var responseMessage = await response.Content.ReadAsStringAsync();
            Assert.Equal("User successfully updated", responseMessage); // Adjust the message based on your API response

            // Verify the update
            using (var scope = _applicationFactory.Services.CreateScope())
            {
                var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
                var updatedUser = await userManager.FindByIdAsync(userId);

                Assert.NotNull(updatedUser);
                Assert.Equal(updateUserDTO.Email, updatedUser.Email);
            }
        }

        [Fact]
        public async Task UpdateUser_InvalidData_ReturnsBadRequest()
        {
            // Arrange
            var invalidUserId = "nonexistent-user-id";
            var updateUserDTO = new UpdateUserDTO
            {
                Email = "invalidemail@example.com",
                Name="NewName",
                PhoneNumber="lalala"
            };

            // Act
            var response = await _httpClient.PutAsJsonAsync($"/api/auth/{invalidUserId}", updateUserDTO);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            var responseMessage = await response.Content.ReadAsStringAsync();
            Assert.Equal("User not found", responseMessage); // Adjust the message based on your API response
        }

        [Fact]
        public async Task DeleteUser_InvalidId_ReturnsBadRequest()
        {
            //Arrange
            var invalidId = "invalid-id";

            //Act
            var response = await _httpClient.DeleteAsync($"/api/auth/{invalidId}");

            //Assert
            Assert.Equal(HttpStatusCode.BadRequest,response.StatusCode);
            var message=await response.Content.ReadAsStringAsync();
            Assert.Equal("User doesn't exist", message);

        }


        [Fact]
        public async Task DeleteUser_ValidId_ReturnsOk()
        {
            // Arrange
            var userEmail = "testuser@example.com";
            var userPassword = "Password123!";
            string userId;

            // Create a test user or fetch an existing one
            using (var scope = _applicationFactory.Services.CreateScope())
            {
                var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
                var user = await userManager.FindByEmailAsync(userEmail);

                if (user == null)
                {
                    user = new ApplicationUser { UserName = userEmail, Email = userEmail, Name = "NewName" };
                    var createUserResult = await userManager.CreateAsync(user, userPassword);
                    Assert.True(createUserResult.Succeeded); // Ensure the user is created
                }

                userId = user.Id; // Get the ID of the created or fetched user
            }

            var response = await _httpClient.DeleteAsync($"/api/auth/{userId}");

            //Assert
            response.EnsureSuccessStatusCode();
            var message = await response.Content.ReadAsStringAsync();
            Assert.Equal("Successfully deleted", message);

        }
    }
}
