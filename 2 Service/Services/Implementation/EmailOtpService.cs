// Auth - Capgemini
// pabodha.wimalasuriya@capgemini.com

using System.Net;
using DSO.Services.Contract;
using DSO.Shared.Constant;
using DSO.Shared.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using MojoAuth.NET;
using MojoAuth.NET.Core;
using Shared.Models;

namespace DSO.Services.Implementation;

/// <summary>
/// EmailOtpService - Is used to make the 3rd party calls and handle the Business logic.
/// </summary>
public class EmailOtpService : IEmailOtpService
{
    // MojoAuth.NET Library is used to Generate the OTP, Send the Email and Verify the OTP.
    private readonly MojoAuthHttpClient mojoAuthHttpClient;

    // MemoryCache is used to store the Re-Try attempts.
    private readonly IMemoryCache memoryCache;

    /// <summary>
    /// Class Constructor - Dependency Injecting
    /// </summary>
    /// <param name="configurations">Configuration to access the appsettings</param>
    /// <param name="memoryCache">Memory Cache to cache the Re-Try attempts</param>
    public EmailOtpService(IConfiguration configurations, IMemoryCache memoryCache)
    {
        // Getting the MojoAuth config values from the AppSettings file.
        this.mojoAuthHttpClient = new MojoAuthHttpClient(configurations["MojoAuth:APIKey"], configurations["MojoAuth:SecretKey"]);

        // Assign the resolved instance of MemoryCache to the local variable.
        this.memoryCache = memoryCache;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    public async Task<Response<EmailOtpResponse>> SendEmailOTP(SendEmailOtpRequestModel model)
    {
        // Calling the MojoAuth library method to generate the OTP and send to the given email.
        var response = await this.mojoAuthHttpClient.SendEmailOTP(model.Email);
        return response;
    }

    public async Task<VerifyEmailOtpResponseModel> CheckOTP(VerifyEmailOtpRequestModel model)
    {
        // Defining the variables for the Re-Try scenarios.
        var otpTries = 0;
        var checkOTPResponse = new VerifyEmailOtpResponseModel();
        var otpKey = string.Format(Constants.OTPKey, model.StateId);
        var userKey = string.Format(Constants.UserKey, model.StateId);

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
                this.memoryCache.Set<User>(userKey, response?.Result?.User);
                checkOTPResponse.StatusCode = HttpStatusCode.OK;
                checkOTPResponse.User = response?.Result?.User;
                return checkOTPResponse;
            }

            // If fails, returning the response's status (Http code), Error Code, Error Message and the Error Description.
            checkOTPResponse.StatusCode = HttpStatusCode.BadRequest; 
            checkOTPResponse.Response = new Response
            {
                Authenticated = response?.Result?.Authenticated,
                ErrorCode = response?.Error?.Code,
                ErrorMessage = response?.Error?.Message,
                ErrorDescription = response?.Error?.Description,
            };
            return checkOTPResponse;
        }

        // Until the Re-Try attempts are less than 10, we return the saved User data from the MemoryCache.
        else if (otpTries < 10)
        {
            var user = this.memoryCache.Get<User>(userKey);
            this.memoryCache.Set(otpKey, ++otpTries);
            checkOTPResponse.StatusCode = HttpStatusCode.OK;
            checkOTPResponse.User = user;
            return checkOTPResponse;
        }

        // Once the Re-Try count is equals to 10, then we return an error message.
        // At the same time, we clear the saved cache for the StateId.
        this.memoryCache.Remove(otpKey);
        this.memoryCache.Remove(userKey);
        checkOTPResponse.StatusCode = HttpStatusCode.Unauthorized;
        checkOTPResponse.Response = new Response
        {
            ErrorCode = (int)HttpStatusCode.Unauthorized,
            ErrorMessage = Constants.RetriesErrorMessage
        };
        return checkOTPResponse;
    }
}