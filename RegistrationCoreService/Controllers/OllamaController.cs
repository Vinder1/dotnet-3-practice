using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.AI;
using RegistrationCoreService.DTOs;

namespace RegistrationCoreService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OllamaController : ControllerBase
{
    [HttpPost("chat")]
    public async Task ChatAsync(
        [FromBody] ChatRequest request,
        IChatClient chatClient, CancellationToken ct)
    {
        var httpResponse = HttpContext.Response;
        if (string.IsNullOrWhiteSpace(request.Prompt))
        {
            httpResponse.StatusCode = StatusCodes.Status400BadRequest;
            return;
        }

        httpResponse.ContentType = "text/plain; charset=utf-8";

        await foreach (var update in chatClient.GetStreamingResponseAsync(request.Prompt, cancellationToken: ct))
        {
            await httpResponse.WriteAsync(update.Text, ct);
            await httpResponse.Body.FlushAsync(ct);
        }
    }
}
