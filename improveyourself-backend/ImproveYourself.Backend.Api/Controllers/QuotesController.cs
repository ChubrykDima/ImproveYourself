using ImproveYourself.Backend.Application.Quotes;
using ImproveYourself.Backend.Application.Quotes.Queries;
using Microsoft.AspNetCore.Mvc;

namespace ImproveYourself.Backend.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class QuotesController : ControllerBase
{
    private readonly IQuoteService _quoteService;

    public QuotesController(IQuoteService quoteService)
    {
        _quoteService = quoteService;
    }

    [HttpGet("daily")]
    public async Task<ActionResult<QuoteDto>> GetDailyQuote()
    {
        var quote = await _quoteService.GetDailyQuoteAsync();
        if (quote == null) return NotFound();
        return Ok(quote);
    }
}
