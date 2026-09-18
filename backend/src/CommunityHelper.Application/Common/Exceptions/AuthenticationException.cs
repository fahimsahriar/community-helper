namespace CommunityHelper.Application.Common.Exceptions;

/// <summary>
/// Thrown when credentials are invalid, the session is missing, or a token
/// is expired/revoked. Mapped to 401 Unauthorized by the API middleware.
/// </summary>
public class AuthenticationException : Exception
{
    public AuthenticationException(string message)
        : base(message)
    {
    }
}
