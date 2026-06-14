using FluentValidation;
using ImproveYourself.Backend.Application.Analytics;
using ImproveYourself.Backend.Application.Challenges;
using ImproveYourself.Backend.Application.Quotes;
using ImproveYourself.Backend.Application.Stats;
using Microsoft.Extensions.DependencyInjection;

namespace ImproveYourself.Backend.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);

        services.AddScoped<IChallengeService, ChallengeService>();
        services.AddScoped<IQuoteService, QuoteService>();
        services.AddScoped<IAnalyticsService, AnalyticsService>();
        services.AddScoped<IStatsService, StatsService>();

        return services;
    }
}
