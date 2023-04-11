// Auth - Capgemini
// pabodha.wimalasuriya@capgemini.com

using System.ComponentModel.DataAnnotations;

namespace DSO.Shared.Models;

/// <summary>
/// VerifyEmailOtpRequestModel - The model used to get the Body from the Request.
/// </summary>
public class VerifyEmailOtpRequestModel
{
    /// <summary>
    /// StateId - This is the value we got as the return response from the `GenerateOTPEmail` method.
    /// This is a required field.
    /// </summary>
    [Required(ErrorMessage = "Invalid Request - Please provide valid StateId.")]
    public string? StateId { get; set; }

    /// <summary>
    /// OTP - This is the value we got as the OTP to the Email.
    /// This is a required field. And should have only 6 digits.
    /// </summary>
    [Required(ErrorMessage = "Invalid Request - Please provide valid OTP.")]
    [MaxLength(6)]
    public string? OTP { get; set; }
}