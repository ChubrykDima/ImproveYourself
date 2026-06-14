using ImproveYourself.Backend.Application.Stats;
using Microsoft.AspNetCore.Mvc;

namespace ImproveYourself.Backend.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class StatsController : ControllerBase
{
    private readonly IStatsService _statsService;

    public StatsController(IStatsService statsService)
    {
        _statsService = statsService;
    }

    [HttpGet]
    public async Task<ActionResult<UserStatsDto>> GetStats([FromQuery] string clientId)
    {
        var stats = await _statsService.GetUserStatsAsync(clientId);
        return Ok(stats);
    }
}
