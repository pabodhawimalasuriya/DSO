// Auth - Capgemini
// pabodha.wimalasuriya@capgemini.com

using System.Net;
using MojoAuth.NET.Core;

namespace Shared.Models;

public class VerifyEmailOtpResponseModel
{
    public HttpStatusCode? StatusCode { get; set; }

    public User? User { get; set; }

    public Response? Response { get; set; }
}

public class Response
{
    public bool? Authenticated { get; set; }

    public int? ErrorCode { get; set; }

    public string? ErrorMessage { get; set; }

    public string? ErrorDescription { get; set; }
}