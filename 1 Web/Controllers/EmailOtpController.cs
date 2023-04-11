// Auth - Capgemini
// pabodha.wimalasuriya@capgemini.com

using System.Net;
using DSO.Services.Contract;
using DSO.Shared.Models;
using Microsoft.AspNetCore.Mvc;

namespace DSO.Controllers;

/// <summary>
/// EmailOtpController - Is used to generate and validate the OTP.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class EmailOtpController : ControllerBase
{
    // EmailOtpService is used to make the 3rd party calls and handle the Business logic.
    private readonly IEmailOtpService emailOtpService;

    /// <summary>
    ///  Class Constructor - Dependency Injecting
    /// </summary>
    /// <param name="emailOtpService">Service to generate, email and verify OTP</param>
    public EmailOtpController(IEmailOtpService emailOtpService)
    {
        // Assign the resolved instance of EmailOtpService to the local variable.
        this.emailOtpService = emailOtpService;
    }

    /// <summary>
    /// GenerateOTPEmail - This is a POST method to generate the OTP and send to the given Email.
    /// </summary>
    /// <param name="model">Method Expects : Email</param>
    /// <returns>
    /// StateId which will be used later to validate the OTP.
    /// Method also returns the respective error messages in case there's a failure.
    /// </returns>
    [HttpPost]
    [Route("GenerateOTPEmail")]
    public async Task<IActionResult> GenerateOTPEmail([FromBody] SendEmailOtpRequestModel model)
    {
        if (!ModelState.IsValid) return BadRequest();

        // Call the Service to generate and send the OTP.
        var response = await this.emailOtpService.SendEmailOTP(model);

        // Checking the response status from the MojoAuth library method. 
        switch (response?.StatusCode)
        {
            case HttpStatusCode.OK:
                // If successful, then returning the StateId.
                return Ok(response.Result.StateId);
            default:
                // If fails, returning the response's status (Http code), Error Code, Error Message and the Error Description.
                return BadRequest($@"Invalid Request - 
                Status : {response?.StatusCode} , 
                Error Code : {response?.Error?.Code} ,
                Error Message : {response?.Error?.Message},
                Error Description : {response?.Error?.Description}");
        }
    }

    /// <summary>
    /// CheckOTP - This is a POST method to validate the OTP with the returned StateId from the `GenerateOTPEmail` method.
    /// </summary>
    /// <param name="model">Method Expects : StateId and OTP</param>
    /// <returns>
    /// Method Returns : Valid User details extracted from the OTP. With MojoAuth, we can retirve the User data when the OTP is valid.
    /// Method also returns the respective error messages in case there's a failure. 
    /// </returns>
    [HttpPost]
    [Route("CheckOTP")]
    public async Task<IActionResult> CheckOTP([FromBody] VerifyEmailOtpRequestModel model)
    {
        if (!ModelState.IsValid) return BadRequest();

        // Call the Service to validate the OTP.
        var response = await this.emailOtpService.CheckOTP(model);
        switch (response.StatusCode)
        {
            case HttpStatusCode.OK:
                // If successful, then returning the User.
                return Ok(response.User);
            case HttpStatusCode.Unauthorized:
            case HttpStatusCode.BadRequest:
                // If fails, returning the response's status (Http code), Error Code, Error Message and the Error Description.
                return BadRequest($@"Invalid Request - 
                    Status : {response?.StatusCode} , 
                    Auth Status : {response?.Response?.Authenticated} ,
                    Error Code : {response?.Response?.ErrorCode} ,
                    Error Message : {response?.Response?.ErrorMessage},
                    Error Description : {response?.Response?.ErrorDescription}");
            default:
                return Unauthorized();
        }
    }
}