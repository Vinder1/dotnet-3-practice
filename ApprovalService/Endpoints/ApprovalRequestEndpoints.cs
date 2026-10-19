using ApprovalService.DTOs;
using ApprovalService.Extensions;
using ApprovalService.Services;

namespace ApprovalService.Endpoints;

public static class ApprovalRequestEndpoints
{
    public static IEndpointRouteBuilder MapApprovalRequestEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/approval-requests")
            .WithTags("ApprovalRequests")
            .RequireAuthorization();

        group.MapPost("/", async (
            CreateApprovalRequest request,
            HttpContext httpContext,
            IApprovalRequestService approvalRequestService) =>
        {
            var created = await approvalRequestService.CreateAsync(
                httpContext.User.GetUserId(),
                GetBearerToken(httpContext),
                request);

            return Results.Created($"/api/approval-requests/{created.Id}", created);
        })
        .WithName("CreateApprovalRequest")
        .Produces<ApprovalRequestResponse>(StatusCodes.Status201Created)
        .ProducesValidationProblem()
        .Produces(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status503ServiceUnavailable);

        group.MapGet("/", async (
            [AsParameters] FilterApprovalRequestsRequest filter,
            IApprovalRequestService approvalRequestService) =>
        {
            return Results.Ok(await approvalRequestService.GetAsync(filter));
        })
        .WithName("GetApprovalRequests")
        .Produces<IReadOnlyCollection<ApprovalRequestResponse>>()
        .ProducesValidationProblem();

        group.MapGet("/{id}", async(int id,
            IApprovalRequestService approvalRequestService) =>
        {
            return Results.Ok(await approvalRequestService.GetByIdAsync(id));
        })
        .WithName("GetApprovalRequestById")
        .Produces<ApprovalRequestResponse>()
        .Produces(StatusCodes.Status404NotFound);

        group.MapPut("/{id}", async (int id,
            UpdateApprovalRequest request,
            HttpContext httpContext,
            IApprovalRequestService approvalRequestService) =>
        {
            var updated = await approvalRequestService.UpdateAsync(id, httpContext.User.GetUserId(), request);
            return Results.Ok(updated);
        })
        .WithName("UpdateApprovalRequest")
        .Produces<ApprovalRequestResponse>()
        .ProducesValidationProblem()
        .Produces(StatusCodes.Status403Forbidden)
        .Produces(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status409Conflict);

        group.MapPost("/{id}/approve", async (int id,
            DecisionRequest request,
            HttpContext httpContext,
            IApprovalRequestService approvalRequestService) =>
        {
            var approved = await approvalRequestService.ApproveAsync(id, httpContext.User.GetUserId(), request);
            return Results.Ok(approved);
        })
        .WithName("ApproveApprovalRequest")
        .Produces<ApprovalRequestResponse>()
        .Produces(StatusCodes.Status403Forbidden)
        .Produces(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status409Conflict);

        group.MapPost("/{id}/reject", async (int id,
            DecisionRequest request,
            HttpContext httpContext,
            IApprovalRequestService approvalRequestService) =>
        {
            var rejected = await approvalRequestService.RejectAsync(id, httpContext.User.GetUserId(), request);
            return TypedResults.Ok(rejected);
        })
        .WithName("RejectApprovalRequest")
        .Produces<ApprovalRequestResponse>()
        .Produces(StatusCodes.Status403Forbidden)
        .Produces(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status409Conflict);

        group.MapDelete("/{id}", async (int id,
            HttpContext httpContext,
            IApprovalRequestService approvalRequestService) =>
        {
            var deleted = await approvalRequestService.DeleteAsync(id, httpContext.User.GetUserId());
            return deleted ? Results.NoContent() : Results.NotFound();
        })
        .WithName("DeleteApprovalRequest")
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status403Forbidden)
        .Produces(StatusCodes.Status404NotFound);

        return app;
    }

    private static string GetBearerToken(HttpContext httpContext)
    {
        var header = httpContext.Request.Headers.Authorization.ToString();
        return header.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase)
            ? header["Bearer ".Length..]
            : header;
    }
}
