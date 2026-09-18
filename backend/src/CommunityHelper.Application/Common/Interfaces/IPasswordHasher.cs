namespace CommunityHelper.Application.Common.Interfaces;

/// <summary>
/// Password hashing (bcrypt). Never log or return hashes.
/// </summary>
public interface IPasswordHasher
{
    string Hash(string password);
    bool Verify(string password, string passwordHash);
}
