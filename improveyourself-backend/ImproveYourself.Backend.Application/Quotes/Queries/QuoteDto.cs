namespace ImproveYourself.Backend.Application.Quotes.Queries;

public record QuoteDto(Guid Id, string Text, string Author, string? Category);
