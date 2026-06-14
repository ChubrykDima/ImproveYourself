using ImproveYourself.Backend.Application.Analytics;
using ImproveYourself.Backend.Application.Analytics.Commands;
using Microsoft.AspNetCore.Mvc;

namespace ImproveYourself.Backend.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AnalyticsController : ControllerBase
{
    private readonly IAnalyticsService _analyticsService;

    public AnalyticsController(IAnalyticsService analyticsService)
    {
        _analyticsService = analyticsService;
    }

    [HttpPost]
    public async Task<IActionResult> LogEvent([FromBody] LogAnalyticsEventCommand command)
    {
        await _analyticsService.LogEventAsync(command);
        return NoContent();
    }
}
