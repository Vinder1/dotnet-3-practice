using ApprovalService.DTOs;
using ApprovalService.Extensions;
using ApprovalService.Services;
using Microsoft.AspNetCore.Http.HttpResults;

namespace ApprovalService.Endpoints;

public static class ApprovalRequestEndpoints
{
    public static IEndpointRouteBuilder MapApprovalRequestEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/approval-requests")
            .WithTags("ApprovalRequests")
            .RequireAuthorization();

        group.MapPost("/", Create)
            .WithName("CreateApprovalRequest")
            .Produces<ApprovalRequestResponse>(StatusCodes.Status201Created)
            .ProducesValidationProblem()
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status503ServiceUnavailable);

        group.MapGet("/", GetAll)
            .WithName("GetApprovalRequests")
            .Produces<IReadOnlyCollection<ApprovalRequestResponse>>()
            .ProducesValidationProblem();

        group.MapGet("/{id:int}", GetById)
            .WithName("GetApprovalRequestById")
            .Produces<ApprovalRequestResponse>()
            .Produces(StatusCodes.Status404NotFound);

        group.MapPut("/{id:int}", Update)
            .WithName("UpdateApprovalRequest")
            .Produces<ApprovalRequestResponse>()
            .ProducesValidationProblem()
            .Produces(StatusCodes.Status403Forbidden)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status409Conflict);

        group.MapPost("/{id:int}/approve", Approve)
            .WithName("ApproveApprovalRequest")
            .Produces<ApprovalRequestResponse>()
            .Produces(StatusCodes.Status403Forbidden)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status409Conflict);

        group.MapPost("/{id:int}/reject", Reject)
            .WithName("RejectApprovalRequest")
            .Produces<ApprovalRequestResponse>()
            .Produces(StatusCodes.Status403Forbidden)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status409Conflict);

        group.MapDelete("/{id:int}", Delete)
            .WithName("DeleteApprovalRequest")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces(StatusCodes.Status404NotFound);

        return app;
    }

    private static async Task<Created<ApprovalRequestResponse>> Create(
        CreateApprovalRequest request,
        HttpContext httpContext,
        IApprovalRequestService approvalRequestService)
    {
        var created = await approvalRequestService.CreateAsync(
            httpContext.User.GetUserId(),
            GetBearerToken(httpContext),
            request);

        return TypedResults.Created($"/api/approval-requests/{created.Id}", created);
    }

    private static async Task<Ok<IReadOnlyCollection<ApprovalRequestResponse>>> GetAll(
        [AsParameters] FilterApprovalRequestsRequest filter,
        IApprovalRequestService approvalRequestService)
    {
        return TypedResults.Ok(await approvalRequestService.GetAsync(filter));
    }

    private static async Task<Ok<ApprovalRequestResponse>> GetById(
        int id,
        IApprovalRequestService approvalRequestService)
    {
        return TypedResults.Ok(await approvalRequestService.GetByIdAsync(id));
    }

    private static async Task<Ok<ApprovalRequestResponse>> Update(
        int id,
        UpdateApprovalRequest request,
        HttpContext httpContext,
        IApprovalRequestService approvalRequestService)
    {
        var updated = await approvalRequestService.UpdateAsync(id, httpContext.User.GetUserId(), request);
        return TypedResults.Ok(updated);
    }

    private static async Task<Ok<ApprovalRequestResponse>> Approve(
        int id,
        DecisionRequest request,
        HttpContext httpContext,
        IApprovalRequestService approvalRequestService)
    {
        var approved = await approvalRequestService.ApproveAsync(id, httpContext.User.GetUserId(), request);
        return TypedResults.Ok(approved);
    }

    private static async Task<Ok<ApprovalRequestResponse>> Reject(
        int id,
        DecisionRequest request,
        HttpContext httpContext,
        IApprovalRequestService approvalRequestService)
    {
        var rejected = await approvalRequestService.RejectAsync(id, httpContext.User.GetUserId(), request);
        return TypedResults.Ok(rejected);
    }

    private static async Task<Results<NoContent, NotFound>> Delete(
        int id,
        HttpContext httpContext,
        IApprovalRequestService approvalRequestService)
    {
        var deleted = await approvalRequestService.DeleteAsync(id, httpContext.User.GetUserId());
        return deleted ? TypedResults.NoContent() : TypedResults.NotFound();
    }

    private static string GetBearerToken(HttpContext httpContext)
    {
        var header = httpContext.Request.Headers.Authorization.ToString();
        return header.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase)
            ? header["Bearer ".Length..]
            : header;
    }
}
