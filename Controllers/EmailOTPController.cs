using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using MojoAuth.NET;

// EmailOTPController - Is used to generate and validate the OTP.
[ApiController]
[Route("api/[controller]")]
public class EmailOTPController : ControllerBase
{
    // MojoAuth.NET Library is used to Generate the OTP, Send the Email and Verify the OTP.
    private readonly MojoAuthHttpClient mojoAuthHttpClient;

    // MemoryCache is used to store the Re-Try attempts.
    private readonly IMemoryCache memoryCache;

    // Class Constructor - Dependency Injecting
    public EmailOTPController(IConfiguration configurations, IMemoryCache memoryCache)
    {
        // Getting the MojoAuth config values from the AppSettings file.
        this.mojoAuthHttpClient = new MojoAuthHttpClient(configurations["MojoAuth:APIKey"], configurations["MojoAuth:SecretKey"]);

        // Assign the resolved instance of MemoryCache to the local variable.
        this.memoryCache = memoryCache;
    }

    // GenerateOTPEmail - This is a POST method to generate the OTP and send to the given Email.
    // Method Expects : Email
    // Method Returns : StateId which will be used later to validate the OTP.
    // Method also returns the respective error messages in case there's a failure.
    [HttpPost]
    [Route("GenerateOTPEmail")]
    public async Task<IActionResult> GenerateOTPEmail([FromBody] SendEmailOTPModel model)
    {
        // Calling the MojoAuth library method to generate the OTP and send to the given email.
        var response = await this.mojoAuthHttpClient.SendEmailOTP(model.Email);

        // Checking the response status from the MojoAuth library method. 
        // If successful, then returning the StateId.
        if (response?.StatusCode == HttpStatusCode.OK)
        {
            return Ok(response.Result.StateId);
        }

        // If fails, returning the response's status (Http code), Error Code, Error Message and the Error Description.
        return BadRequest($@"Invalid Request - 
                Status : {response?.StatusCode} , 
                Error Code : {response?.Error?.Code} ,
                Error Message : {response?.Error?.Message},
                Error Description : {response?.Error?.Description}");
    }

    // CheckOTP - This is a POST method to validate the OTP with the returned StateId from the `GenerateOTPEmail` method.
    // Method Expects : StateId and OTP
    // Method Returns : Valid User details extracted from the OTP. With MojoAuth, we can retirve the User data when the OTP is valid.
    // Method also returns the respective error messages in case there's a failure. 
    [HttpPost]
    [Route("CheckOTP")]
    public async Task<IActionResult> CheckOTP([FromBody] VerifyEmailOTPModel model)
    {
        // Defining the variables for the Re-Try scenarios.
        var otpTries = 0;
        var otpKey = $"{model.StateId}-ValidOTPTries";
        var userKey = $"{model.StateId}-ValidOTPUser";

        // Getting the Re-Try counts from the MemoryCache.
        this.memoryCache.TryGetValue(otpKey, out otpTries);

        // At the initial moment, the OTP Tries are 0. Then, we validate the OTP with the StateId from the MojoAuth.
        // If the request is valid, then we store the Re-Try counts (as 1) in the Memory Cache (Saved respective to the StateId).
        // At the same time, We also save the User details we get from the MojoAuth OTP validation method.
        if (otpTries == 0)
        {
            var response = await this.mojoAuthHttpClient.VerifyOTP(model.StateId, model.OTP);
            if (response?.StatusCode == HttpStatusCode.OK)
            {
                this.memoryCache.Set(otpKey, ++otpTries);
                this.memoryCache.Set(userKey, response?.Result?.User);
                return Ok(response?.Result?.User);
            }

            // If fails, returning the response's status (Http code), Error Code, Error Message and the Error Description.
            return BadRequest($@"Invalid Request - 
                    Status : {response?.StatusCode} , 
                    Auth Status : {response?.Result?.Authenticated} ,
                    Error Code : {response?.Error?.Code} ,
                    Error Message : {response?.Error?.Message},
                    Error Description : {response?.Error?.Description}");
        }

        // Until the Re-Try attempts are less than 10, we return the saved User data from the MemoryCache.
        else if (otpTries < 10)
        {
            var user = this.memoryCache.Get(userKey);
            this.memoryCache.Set(otpKey, ++otpTries);
            return Ok(user);
        }

        // Once the Re-Try count is equals to 10, then we return an error message.
        // At the same time, we clear the saved cache for the StateId.
        this.memoryCache.Remove(otpKey);
        this.memoryCache.Remove(userKey);
        return BadRequest("OTP is wrong after 10 tries");
    }
}