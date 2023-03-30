// Auth - Capgemini
// pabodha.wimalasuriya@capgemini.com

using System.ComponentModel.DataAnnotations;

// VerifyEmailOTPModel - The model used to get the Body from the Request.
public class VerifyEmailOTPModel
{
    // StateId - This is the value we got as the return response from the `GenerateOTPEmail` method.
    // This is a required field.
    [Required(ErrorMessage = "Invalid Request - Please provide valid StateId.")]
    public string? StateId { get; set; }

    // OTP - This is the value we got as the OTP to the Email.
    // This is a required field. And should have only 6 digits.
    [Required(ErrorMessage = "Invalid Request - Please provide valid OTP.")]
    [MaxLength(6)]
    public string? OTP { get; set; }
}