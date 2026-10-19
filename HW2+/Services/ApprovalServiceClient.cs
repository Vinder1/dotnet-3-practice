using System.Globalization;
using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;
using HW2.DTOs;
using HW2.Exceptions;
using Microsoft.AspNetCore.WebUtilities;

namespace HW2.Services;

public sealed class ApprovalServiceClient(HttpClient httpClient, ILogger<ApprovalServiceClient> logger)
    : IApprovalServiceClient
{
    private const string BasePath = "/api/approval-requests";

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };

    private readonly HttpClient _httpClient = httpClient;
    private readonly ILogger<ApprovalServiceClient> _logger = logger;

    public Task<ApprovalServiceResult<ApprovalRequestResponse>> CreateAsync(
        string accessToken,
        CreateApprovalRequest request,
        CancellationToken cancellationToken = default)
    {
        return SendAsync<ApprovalRequestResponse>(
            HttpMethod.Post, BasePath, accessToken, request, cancellationToken);
    }

    public async Task<ApprovalServiceResult<IReadOnlyCollection<ApprovalRequestResponse>>> GetAsync(
        string accessToken,
        FilterApprovalRequestsRequest filter,
        CancellationToken cancellationToken = default)
    {
        var query = new Dictionary<string, string?>();

        if (filter.Status is not null)
        {
            query["status"] = filter.Status.Value.ToString();
        }

        if (filter.RequesterUserId is not null)
        {
            query["requesterUserId"] = filter.RequesterUserId.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (filter.CreatedFrom is not null)
        {
            query["createdFrom"] = filter.CreatedFrom.Value.ToString("O", CultureInfo.InvariantCulture);
        }

        if (filter.CreatedTo is not null)
        {
            query["createdTo"] = filter.CreatedTo.Value.ToString("O", CultureInfo.InvariantCulture);
        }

        var url = query.Count == 0 ? BasePath : QueryHelpers.AddQueryString(BasePath, query);

        var res = await SendAsync<ApprovalRequestResponse[]>(
            HttpMethod.Get, url, accessToken, body: null, cancellationToken: cancellationToken);

        return new ApprovalServiceResult<IReadOnlyCollection<ApprovalRequestResponse>>
        {
            ErrorBody = res.ErrorBody,
            StatusCode = res.StatusCode,
            Value = res.Value
        };
    }

    public Task<ApprovalServiceResult<ApprovalRequestResponse>> GetByIdAsync(
        string accessToken,
        int id,
        CancellationToken cancellationToken = default)
    {
        return SendAsync<ApprovalRequestResponse>(
            HttpMethod.Get, $"{BasePath}/{id}", accessToken, null, cancellationToken: cancellationToken);
    }

    public Task<ApprovalServiceResult<ApprovalRequestResponse>> UpdateAsync(
        string accessToken,
        int id,
        UpdateApprovalRequest request,
        CancellationToken cancellationToken = default)
    {
        return SendAsync<ApprovalRequestResponse>(
            HttpMethod.Put, $"{BasePath}/{id}", accessToken, request, cancellationToken);
    }

    public Task<ApprovalServiceResult<ApprovalRequestResponse>> ApproveAsync(
        string accessToken,
        int id,
        DecisionRequest request,
        CancellationToken cancellationToken = default)
    {
        return SendAsync<ApprovalRequestResponse>(
            HttpMethod.Post, $"{BasePath}/{id}/approve", accessToken, request, cancellationToken);
    }

    public Task<ApprovalServiceResult<ApprovalRequestResponse>> RejectAsync(
        string accessToken,
        int id,
        DecisionRequest request,
        CancellationToken cancellationToken = default)
    {
        return SendAsync<ApprovalRequestResponse>(
            HttpMethod.Post, $"{BasePath}/{id}/reject", accessToken, request, cancellationToken);
    }

    public async Task<ApprovalServiceResult> DeleteAsync(
        string accessToken,
        int id,
        CancellationToken cancellationToken = default)
    {
        return await SendAsync<object>(
            HttpMethod.Delete, $"{BasePath}/{id}", accessToken, null, cancellationToken);
    }

    private async Task<ApprovalServiceResult<T>> SendAsync<T>(
        HttpMethod method,
        string url,
        string accessToken,
        object? body,
        CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(method, url);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        if (body is not null)
        {
            request.Content = JsonContent.Create(body, body.GetType(), options: JsonOptions);
        }

        HttpResponseMessage response;
        try
        {
            response = await _httpClient.SendAsync(
                request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        }
        catch (Exception exception)
        {
            _logger.LogError(
                exception, "Approval service is unreachable while calling {Method} {Url}", method, url);
            throw new MicroServiceUnavailableException("Approval service is unavailable.", exception);
        }

        using (response)
        {
            if (response.IsSuccessStatusCode)
            {
                if (response.StatusCode == HttpStatusCode.NoContent)
                {
                    return new ApprovalServiceResult<T>
                    {
                        StatusCode = response.StatusCode,
                        Value = default,
                        ErrorBody = null
                    };
                }

                var payload = await response.Content.ReadFromJsonAsync<T>(JsonOptions, cancellationToken);
                return new ApprovalServiceResult<T>
                {
                    StatusCode = response.StatusCode,
                    Value = payload,
                    ErrorBody = null
                };
            }

            var errorBody = await response.Content.ReadAsStringAsync(cancellationToken);

            _logger.LogWarning(
                "Approval service returned status {StatusCode} for {Method} {Url}",
                (int)response.StatusCode,
                method,
                url);

            return new ApprovalServiceResult<T>
            {
                StatusCode = response.StatusCode,
                Value = default,
                ErrorBody = errorBody
            };
        }
    }
}
