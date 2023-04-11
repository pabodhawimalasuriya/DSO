using DSO.Shared.Models;
using MojoAuth.NET.Core;
using MojoAuth.NET;
using Shared.Models;

namespace DSO.Services.Contract;

public interface IEmailOtpService
{
    Task<Response<EmailOtpResponse>> SendEmailOTP(SendEmailOtpRequestModel model);
    Task<VerifyEmailOtpResponseModel> CheckOTP(VerifyEmailOtpRequestModel model);
}