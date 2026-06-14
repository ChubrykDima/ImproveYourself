using ImproveYourself.Backend.Application.Challenges;
using ImproveYourself.Backend.Application.Challenges.Commands;
using ImproveYourself.Backend.Application.Challenges.Queries;
using Microsoft.AspNetCore.Mvc;

namespace ImproveYourself.Backend.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ChallengesController : ControllerBase
{
    private readonly IChallengeService _challengeService;

    public ChallengesController(IChallengeService challengeService)
    {
        _challengeService = challengeService;
    }

    [HttpGet]
    public async Task<ActionResult<List<DailyChallengeDto>>> GetChallenges([FromQuery] string? startDate, [FromQuery] string? endDate)
    {
        var challenges = await _challengeService.GetChallengesAsync(startDate, endDate);
        return Ok(challenges);
    }

    [HttpPost("sync")]
    public async Task<IActionResult> SyncChallenges([FromBody] SyncChallengesCommand command)
    {
        await _challengeService.SyncChallengesAsync(command.Challenges);
        return NoContent();
    }
}
