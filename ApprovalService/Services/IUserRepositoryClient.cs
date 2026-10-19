namespace ApprovalService.Services;

public sealed record User(int Id, string Username, string Email);

public interface IUserRepositoryClient
{
    Task<User?> GetUserAsync(
        int userId,
        string accessToken,
        CancellationToken cancellationToken = default);
}
