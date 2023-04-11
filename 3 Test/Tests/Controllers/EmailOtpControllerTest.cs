using System.Net;
using DSO.Controllers;
using DSO.Shared.Models;
using DSO.Services.Contract;
using Microsoft.AspNetCore.Mvc;
using MojoAuth.NET.Core;
using MojoAuth.NET;
using Moq;
using Shared.Models;
using MojoAuth.NET.Http;

namespace Tests.Controllers;

public class EmailOtpControllerTest
{
    // Initialize the EmailOTPController class.
    private readonly EmailOtpController emailOTPController;

    // Initialize the EmailOtpService.
    private readonly Mock<IEmailOtpService> emailOtpService;

    public EmailOtpControllerTest()
    {
        // Set up the Mocked EmailOtpService.
        this.emailOtpService = new Mock<IEmailOtpService>();

        // Create an instance of the controller class and use across the Test class.
        this.emailOTPController = new EmailOtpController(this.emailOtpService.Object);
    }

    /// <summary>
    /// Generating the OTP and Sending the Email returning an OK response.
    /// </summary>
    [Fact]
    public async Task GenerateOtpEmailTest_ValidEmail_ReturnsOKResult()
    {
        // Arrange
        var model = new SendEmailOtpRequestModel
        {
            Email = "pabodha.capgemini@gmail.com"
        };

        var response = new EmailOtpResponse
        {
            StateId = "12345678"
        };
        this.emailOtpService.Setup(x => x.SendEmailOTP(It.IsAny<SendEmailOtpRequestModel>())).ReturnsAsync(new Response<EmailOtpResponse>(new MojoAuth.NET.Http.HttpResponse()) {
            StatusCode = HttpStatusCode.OK,
            Result = response
        });

        // Act
        var result = await this.emailOTPController.GenerateOTPEmail(model);

        // Assert
        Assert.NotNull(result);
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
    }

    /// <summary>
    /// Generating the OTP and Sending the Email returning a Bad Request response.
    /// </summary>
    [Fact]
    public async Task GenerateOtpEmailTest_ValidEmail_ReturnsBadRequestResult()
    {
        // Arrange
        var model = new SendEmailOtpRequestModel
        {
            Email = "pabodha.capgemini@gmail.com"
        };

        this.emailOtpService.Setup(x => x.SendEmailOTP(It.IsAny<SendEmailOtpRequestModel>())).ReturnsAsync(new Response<EmailOtpResponse>(new MojoAuth.NET.Http.HttpResponse())
        {
            StatusCode = HttpStatusCode.BadRequest,
            Error = new MojoAuthError
            {
                Code = 400,
                Message = "Invalid Request",
                Description = "Invalid Request"
            }
        });

        // Act
        var result = await this.emailOTPController.GenerateOTPEmail(model);

        // Assert
        Assert.NotNull(result);
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal(400, badRequestResult.StatusCode);
    }

    /// <summary>
    /// Error returned for an invalid email.
    /// </summary>
    [Fact]
    public async Task GenerateOtpEmailTest_InvalidEmail()
    {
        // Arrange
        var model = new SendEmailOtpRequestModel
        {
            Email = "pabodha.capgemini"
        };

        this.emailOTPController.ModelState.AddModelError("Error", "Invalid data");

        // Act
        var result = await this.emailOTPController.GenerateOTPEmail(model);

        // Assert
        Assert.NotNull(result);
        Assert.IsType<BadRequestResult>(result);
    }

    /// <summary>
    /// Validating the OTP using the StateId and the OTP returns an OK response. 
    /// </summary>
    [Fact]
    public async Task CheckOTPTest_ValidStateIdOTP_ReturnsOKResult()
    {
        // Arrange
        var model = new VerifyEmailOtpRequestModel
        {
            StateId = "642648c639e8ef5cd12a496d",
            OTP = "913866"
        };

        this.emailOtpService.Setup(x => x.CheckOTP(It.IsAny<VerifyEmailOtpRequestModel>())).ReturnsAsync(new VerifyEmailOtpResponseModel
        {
            StatusCode = HttpStatusCode.OK,
            User = new User
            {
                UserId = "1",
                CreatedAt = "",
                Identifier = "",
                Issuer = "",
                UpdatedAt = ""
            }
        });

        // Act
        var result = await this.emailOTPController.CheckOTP(model);

        // Assert
        Assert.NotNull(result);
        Assert.IsType<OkObjectResult>(result);
    }

    /// <summary>
    /// Validating the OTP using the StateId and the OTP returns a BadRequest response.
    /// </summary>
    [Fact]
    public async Task CheckOTPTest_ValidStateIdOTP_ReturnsBadRequestResult()
    {
        // Arrange
        var model = new VerifyEmailOtpRequestModel
        {
            StateId = "642648c639e8ef5cd12a496d",
            OTP = "913866"
        };

        this.emailOtpService.Setup(x => x.CheckOTP(It.IsAny<VerifyEmailOtpRequestModel>())).ReturnsAsync(new VerifyEmailOtpResponseModel
        {
            StatusCode = HttpStatusCode.BadRequest,
            Response = new Response
            {
                ErrorCode = 400,
                ErrorMessage = "OTP has expired.",
                ErrorDescription = "OTP has expired."
            }
        });

        // Act
        var result = await this.emailOTPController.CheckOTP(model);

        // Assert
        Assert.NotNull(result);
        Assert.IsType<BadRequestObjectResult>(result);
    }

    /// <summary>
    /// Error returned for an invalid StateId or OTP.
    /// </summary>
    /// <param name="stateId"></param>
    /// <param name="otp"></param>
    [Theory]
    [InlineData("12345678", "")]
    [InlineData("", "123456")]
    [InlineData("", "")]
    public async Task CheckOTPTest_InvalidStateIdAndOTP(string stateId, string otp)
    {
        // Arrange
        var model = new VerifyEmailOtpRequestModel
        {
            StateId = stateId,
            OTP = otp
        };

        this.emailOTPController.ModelState.AddModelError("Error", "Invalid data");

        // Act
        var result = await this.emailOTPController.CheckOTP(model);

        // Assert
        Assert.NotNull(result);
        Assert.IsType<BadRequestResult>(result);
    }
}