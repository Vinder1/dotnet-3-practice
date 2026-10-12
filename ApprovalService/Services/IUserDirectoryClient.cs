namespace ApprovalService.Services;

public sealed record UserDirectoryUser(int Id, string Username, string Email);

public interface IUserDirectoryClient
{
    Task<UserDirectoryUser?> GetUserAsync(
        int userId,
        string accessToken,
        CancellationToken cancellationToken = default);
}
