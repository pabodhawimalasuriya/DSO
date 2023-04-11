//// Auth - Capgemini
//// pabodha.wimalasuriya@capgemini.com

using DSO.Services.Implementation;
using DSO.Shared.Constant;
using DSO.Shared.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using MojoAuth.NET;
using MojoAuth.NET.Core;
using Moq;
using Shared.Models;

namespace Tests.Services
{
    public class EmailOtpServiceTest
    {
        // Initialize the EmailOtpService class.
        private readonly EmailOtpService emailOtpService;

        // Initialize the Appsettings Configuration.
        private readonly Mock<IConfiguration> configurations;

        // Initialize the MemoryCache.
        private readonly Mock<IMemoryCache> memoryCache;

        public EmailOtpServiceTest()
        {
            var expectedValue = 1;
            // Set up the Appsettings Configurations.
            this.configurations = new Mock<IConfiguration>();
            this.configurations.SetupGet(x => x[It.Is<string>(s => s == "MojoAuth:APIKey")]).Returns("test-89e4637c-ffc0-45fe-9923-6a8951d37f95");
            this.configurations.SetupGet(x => x[It.Is<string>(s => s == "MojoAuth:SecretKey")]).Returns("cghq7e9vu52c70c7jl00.ETRAG47D7yZH3UbRgutzdC");

            // Set up the MemoryCache.
            this.memoryCache = new Mock<IMemoryCache>();

            // Create an instance of the controller class and use across the Test class.
            this.emailOtpService = new EmailOtpService(configurations.Object, memoryCache.Object);
        }

        /// <summary>
        /// Generating the OTP and Sending the Email.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task SendEmailOTPTest_ValidEmail_ReturnsValidResponse()
        {
            // Arrange
            var model = new SendEmailOtpRequestModel
            {
                Email = "pabodha.capgemini@gmail.com"
            };

            // Act
            var result = await this.emailOtpService.SendEmailOTP(model);

            // Assert
            Assert.NotNull(result);
            var returnResponse = Assert.IsType<Response<EmailOtpResponse>>(result);
            Assert.NotNull(returnResponse.Result.StateId);
        }

        /// <summary>
        /// Validating the OTP using the StateId and the OTP.
        /// </summary>
        //[Fact]
        public async Task CheckOTPTest_ValidStateIdOTP_ValidRetries_ReturnUser()
        {
            // Arrange
            var stateId = "642648c639e8ef5cd12a496d";
            var otpTries = 1;
            var model = new VerifyEmailOtpRequestModel
            {
                StateId = stateId,
                OTP = "913866"
            };

            var otpKey = string.Format(Constants.OTPKey, model.StateId);
            var userKey = string.Format(Constants.UserKey, model.StateId);
            //this.memoryCache.Setup(m => m.TryGetValue(It.IsAny<object>(), out otpTries)).Returns(false);
            //this.memoryCache.Setup(x => x.TryGetValue(It.IsAny<object>(), out otpTries));
            //this.memoryCache.Setup(x => x.TryGetValue(otpKey, out otpTries));
            //this.memoryCache.Setup(x => x.Get(It.IsAny<string>())).Returns(new User
            //{
            //    UserId = stateId,
            //    CreatedAt = string.Empty,
            //    Identifier = string.Empty,
            //    Issuer = string.Empty,
            //    UpdatedAt = string.Empty
            //});

            // Act
            var result = await this.emailOtpService.CheckOTP(model);

            // Assert
            Assert.NotNull(result);
            var responseResult = Assert.IsType<VerifyEmailOtpResponseModel>(result);
            Assert.NotNull(responseResult.User);
        }
    }
}