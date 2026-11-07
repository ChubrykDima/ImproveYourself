using System.Reflection;

namespace ImproveYourself.Maui.Application;

internal static class SentryDsnResolver
{
    private const string AssemblyMetadataKey = "SentryDsn";
    private const string EnvironmentVariableName = "SENTRY_DSN";

    public static string? Resolve()
    {
        var fromEnvironment = Environment.GetEnvironmentVariable(EnvironmentVariableName);
        if (!string.IsNullOrWhiteSpace(fromEnvironment))
        {
            return fromEnvironment.Trim();
        }

        var fromAssembly = typeof(SentryDsnResolver).Assembly
            .GetCustomAttributes<AssemblyMetadataAttribute>()
            .FirstOrDefault(attribute => attribute.Key == AssemblyMetadataKey)
            ?.Value;

        return string.IsNullOrWhiteSpace(fromAssembly) ? null : fromAssembly.Trim();
    }
}
