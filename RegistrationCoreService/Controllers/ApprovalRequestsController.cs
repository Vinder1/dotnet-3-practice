using RegistrationCoreService.DTOs;
using RegistrationCoreService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace RegistrationCoreService.Controllers;

[ApiController]
[Route("api/approval-requests")]
[Authorize]
public class ApprovalRequestsController(
    IApprovalServiceClient approvalServiceClient,
    ILogger<ApprovalRequestsController> logger) : ControllerBase
{
    private readonly IApprovalServiceClient _approvalServiceClient = approvalServiceClient;
    private readonly ILogger<ApprovalRequestsController> _logger = logger;

    [HttpPost]
    [ProducesResponseType(typeof(ApprovalRequestResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> Create([FromBody] CreateApprovalRequest request)
    {
        var result = await _approvalServiceClient.CreateAsync(GetAccessToken(), request);
        if (!result.IsSuccess)
        {
            return ToErrorResult(result);
        }

        return Created($"/api/approval-requests/{result.Value!.Id}", result.Value);
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyCollection<ApprovalRequestResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] FilterApprovalRequestsRequest filter)
    {
        var result = await _approvalServiceClient.GetAsync(GetAccessToken(), filter);
        return result.IsSuccess ? Ok(result.Value) : ToErrorResult(result);
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ApprovalRequestResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _approvalServiceClient.GetByIdAsync(GetAccessToken(), id);
        return result.IsSuccess ? Ok(result.Value) : ToErrorResult(result);
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(ApprovalRequestResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateApprovalRequest request)
    {
        var result = await _approvalServiceClient.UpdateAsync(GetAccessToken(), id, request);
        return result.IsSuccess ? Ok(result.Value) : ToErrorResult(result);
    }

    [HttpPost("{id:int}/approve")]
    [ProducesResponseType(typeof(ApprovalRequestResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Approve(int id, [FromBody] DecisionRequest request)
    {
        var result = await _approvalServiceClient.ApproveAsync(GetAccessToken(), id, request);
        return result.IsSuccess ? Ok(result.Value) : ToErrorResult(result);
    }

    [HttpPost("{id:int}/reject")]
    [ProducesResponseType(typeof(ApprovalRequestResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Reject(int id, [FromBody] DecisionRequest request)
    {
        var result = await _approvalServiceClient.RejectAsync(GetAccessToken(), id, request);
        return result.IsSuccess ? Ok(result.Value) : ToErrorResult(result);
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _approvalServiceClient.DeleteAsync(GetAccessToken(), id);
        return result.IsSuccess ? NoContent() : ToErrorResult(result);
    }

    private IActionResult ToErrorResult(ApprovalServiceResult result)
    {
        _logger.LogWarning(
            "Approval service call failed with status {StatusCode} for {Method} {Path}",
            (int)result.StatusCode,
            Request.Method,
            Request.Path.Value);

        if (string.IsNullOrWhiteSpace(result.ErrorBody))
        {
            return StatusCode((int)result.StatusCode, new ProblemDetails
            {
                Status = (int)result.StatusCode,
                Title = "Approval service error"
            });
        }

        return new ContentResult
        {
            StatusCode = (int)result.StatusCode,
            Content = result.ErrorBody,
            ContentType = "application/json"
        };
    }

    private string GetAccessToken()
    {
        var header = Request.Headers.Authorization.ToString();
        return header.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase)
            ? header["Bearer ".Length..]
            : header;
    }
}
