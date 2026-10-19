using System.Net;
using System.Net.Http.Headers;
using ApprovalService.Exceptions;

namespace ApprovalService.Services;

public class UserRepositoryClient(HttpClient httpClient, ILogger<UserRepositoryClient> logger)
    : IUserRepositoryClient
{
    private readonly HttpClient _httpClient = httpClient;
    private readonly ILogger<UserRepositoryClient> _logger = logger;

    public async Task<User?> GetUserAsync(
        int userId,
        string accessToken,
        CancellationToken cancellationToken = default)
    {
        using var httpRequest = new HttpRequestMessage(HttpMethod.Get, $"/api/users/{userId}");
        httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        HttpResponseMessage response;
        try
        {
            response = await _httpClient.SendAsync(httpRequest, cancellationToken);
        }
        catch (Exception exception) when (exception is HttpRequestException or TaskCanceledException)
        {
            _logger.LogError(exception, "Users service is unreachable while resolving user {UserId}", userId);
            throw new MicroServiceUnavailableException("Users service is unavailable.", exception);
        }

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            _logger.LogWarning("User {UserId} was not found in the users service", userId);
            return null;
        }

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError(
                "Users service returned status {StatusCode} while resolving user {UserId}",
                (int)response.StatusCode,
                userId);
            throw new MicroServiceUnavailableException(
                $"Users service returned status code {(int)response.StatusCode}.");
        }

        return await response.Content.ReadFromJsonAsync<User>(cancellationToken);
    }
}
