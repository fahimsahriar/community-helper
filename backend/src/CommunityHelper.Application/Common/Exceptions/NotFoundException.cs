namespace CommunityHelper.Application.Common.Exceptions;

/// <summary>
/// Thrown by handlers when a requested resource does not exist.
/// Mapped to 404 by the API's exception handling middleware.
/// </summary>
public class NotFoundException : Exception
{
    public NotFoundException(string message) : base(message) { }

    public NotFoundException(string name, object key)
        : base($"Entity \"{name}\" ({key}) was not found.") { }
}
