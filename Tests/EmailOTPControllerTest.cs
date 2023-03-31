using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Moq;
using Xunit;

// EmailOTPControllerTest - Is used to cover the high level scenarios on the EmailOTPController class. 
public class EmailOTPControllerTest
{
    // Initialize the EmailOTPController class.
    private EmailOTPController emailOTPController;

    // Initialize the Appsettings Configuration.
    private readonly Mock<IConfiguration> configurations;

    // Initialize the MemoryCache.
    private readonly Mock<IMemoryCache> memoryCache;

    // Class Constructor - Declare the relevant common values which are getting used across the whole Test Class.
    public EmailOTPControllerTest()
    {
        // Set up the Appsettings Configurations.
        this.configurations = new Mock<IConfiguration>();
        this.configurations.SetupGet(x => x[It.Is<string>(s => s == "MojoAuth:APIKey")]).Returns("test-89e4637c-ffc0-45fe-9923-6a8951d37f95");
        this.configurations.SetupGet(x => x[It.Is<string>(s => s == "MojoAuth:SecretKey")]).Returns("cghq7e9vu52c70c7jl00.ETRAG47D7yZH3UbRgutzdC");

        // Set up the MemoryCache.
        this.memoryCache = new Mock<IMemoryCache>();

        // Create an instance of the controller class and use across the Test class.
        this.emailOTPController = new EmailOTPController(configurations.Object, memoryCache.Object);
    }

    // Generating the OTP and Sending the Email.
    // Please note to comment the `.dso.org.sg` validation before executing this.
    [Fact]
    public async void GenerateOTPEmailTest_ValidEmail()
    {
        // Arrange
        var model = new SendEmailOTPModel
        {
            Email = "pabodha.capgemini@gmail.com"
        };

        // Act
        var result = await this.emailOTPController.GenerateOTPEmail(model);

        // Assert
        Assert.NotNull(result);
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
    }

    // Error returned for an invalid email.
    [Fact]
    public async void GenerateOTPEmailTest_InvalidEmail()
    {
        // Arrange
        var model = new SendEmailOTPModel
        {
            Email = "pabodha.capgemini"
        };

        // Act
        var result = await this.emailOTPController.GenerateOTPEmail(model);

        // Assert
        Assert.NotNull(result);
        Assert.IsType<BadRequestObjectResult>(result);
    }

    // Error returned for an invalid configurations.
    [Fact]
    public async void GenerateOTPEmailTest_InvalidConfigurations()
    {
        // Arrange
        this.configurations.SetupGet(x => x[It.Is<string>(s => s == "MojoAuth:APIKey")]).Returns(string.Empty);
        this.configurations.SetupGet(x => x[It.Is<string>(s => s == "MojoAuth:SecretKey")]).Returns(string.Empty);
        this.emailOTPController = new EmailOTPController(configurations.Object, memoryCache.Object);

        var model = new SendEmailOTPModel
        {
            Email = "pabodha.capgemini@gmail.com"
        };

        // Act
        var result = await this.emailOTPController.GenerateOTPEmail(model);

        // Assert
        Assert.NotNull(result);
        Assert.IsType<BadRequestObjectResult>(result);
    }

    // Validating the OTP using the StateId and the OTP.
    // Please note - To make this passed, we have to manually generate the OTP and get the StateId from the GenerateOTPEmailTest_ValidEmail.
    //               And then those values should be entered here. This will survive you only once. All the other times, if you didn't change the values, then this Test will fail.
    [Fact]
    public async void CheckOTPTest_ValidStateIdOTP()
    {
        // Arrange
        var model = new VerifyEmailOTPModel
        {
            StateId = "642648c639e8ef5cd12a496d",
            OTP = "913866"
        };

        // Act
        var result = await this.emailOTPController.CheckOTP(model);

        // Assert
        Assert.NotNull(result);
        Assert.IsType<OkObjectResult>(result);
    }

    // Error returned for an invalid configurations.
    [Fact]
    public async void CheckOTPTest_InvalidConfigurations()
    {
        // Arrange
        this.configurations.SetupGet(x => x[It.Is<string>(s => s == "MojoAuth:APIKey")]).Returns(string.Empty);
        this.configurations.SetupGet(x => x[It.Is<string>(s => s == "MojoAuth:SecretKey")]).Returns(string.Empty);
        this.emailOTPController = new EmailOTPController(configurations.Object, memoryCache.Object);

        var model = new VerifyEmailOTPModel
        {
            StateId = "12345678",
            OTP = "123456"
        };

        // Act
        var result = await this.emailOTPController.CheckOTP(model);

        // Assert
        Assert.NotNull(result);
        Assert.IsType<BadRequestObjectResult>(result);
    }

    // Error returned for an invalid StateId or OTP.
    [Theory]
    [InlineData("12345678", "")]
    [InlineData("", "123456")]
    [InlineData("", "")]
    public async void CheckOTPTest_InvalidStateIdAndOTP(string stateId, string otp)
    {
        // Arrange
        this.configurations.SetupGet(x => x[It.Is<string>(s => s == "MojoAuth:APIKey")]).Returns(string.Empty);
        this.configurations.SetupGet(x => x[It.Is<string>(s => s == "MojoAuth:SecretKey")]).Returns(string.Empty);
        this.emailOTPController = new EmailOTPController(configurations.Object, memoryCache.Object);

        var model = new VerifyEmailOTPModel
        {
            StateId = "12345678",
            OTP = "123456"
        };

        // Act
        var result = await this.emailOTPController.CheckOTP(model);

        // Assert
        Assert.NotNull(result);
        Assert.IsType<BadRequestObjectResult>(result);
    }
}