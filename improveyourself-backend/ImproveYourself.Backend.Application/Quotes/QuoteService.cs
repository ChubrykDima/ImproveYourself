using ImproveYourself.Backend.Application.Common.Interfaces;
using ImproveYourself.Backend.Application.Quotes.Queries;
using Microsoft.EntityFrameworkCore;

namespace ImproveYourself.Backend.Application.Quotes;

public interface IQuoteService
{
    Task<QuoteDto?> GetDailyQuoteAsync();
}

public class QuoteService : IQuoteService
{
    private readonly IApplicationDbContext _context;

    public QuoteService(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<QuoteDto?> GetDailyQuoteAsync()
    {
        // Simple logic for MVP: get a random quote
        var count = await _context.Quotes.CountAsync();
        if (count == 0) return null;

        var random = new Random();
        var index = random.Next(0, count);

        var quote = await _context.Quotes.Skip(index).FirstOrDefaultAsync();

        return quote == null ? null : new QuoteDto(quote.Id, quote.Text, quote.Author, quote.Category);
    }
}
