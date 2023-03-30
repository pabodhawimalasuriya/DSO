// Auth - Capgemini
// pabodha.wimalasuriya@capgemini.com

using System.ComponentModel.DataAnnotations;

// SendEmailOTPModel - The model used to get the Body from the Request.
public class SendEmailOTPModel
{
    // Email - This is the OTP we use to generate the OTP. 
    // This is a required field. And should have the Email validations.
    // And the Emails that have to be accepted from the system should be `.dso.org.sg` domain ones. 
    [Required(ErrorMessage = "Invalid Request - Please provide valid Email address.")]
    [EmailAddress(ErrorMessage = "Invalid Request - Please provide valid Email address.")]
    [RegularExpression(".dso.org.sg$", ErrorMessage = "Invalid Request - Please provide valid Email address which is on the DSO domain.")]
    public string? Email { get; set; }
}