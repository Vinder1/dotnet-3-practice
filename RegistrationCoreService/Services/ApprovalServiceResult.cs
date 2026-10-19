using System.Net;

namespace RegistrationCoreService.Services;

public class ApprovalServiceResult
{
    public HttpStatusCode StatusCode { get; init; }
    public string? ErrorBody { get; init; }
    public bool IsSuccess => (int)StatusCode is >= 200 and < 300;
}

public sealed class ApprovalServiceResult<T> : ApprovalServiceResult
{
    public T? Value { get; init; }
}
