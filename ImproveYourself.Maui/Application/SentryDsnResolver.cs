using System.Reflection;

namespace ImproveYourself.Maui.Application;

internal static class SentryDsnResolver
{
    private const string DsnMetadataKey = "SentryDsn";
    private const string ReleaseMetadataKey = "SentryRelease";
    private const string EnvironmentVariableName = "SENTRY_DSN";

    private static readonly string? CachedDsn = ReadAssemblyMetadata(DsnMetadataKey);
    private static readonly string? CachedRelease = ReadAssemblyMetadata(ReleaseMetadataKey);

    public static string? ResolveDsn()
    {
        var fromEnvironment = Environment.GetEnvironmentVariable(EnvironmentVariableName);
        if (!string.IsNullOrWhiteSpace(fromEnvironment))
        {
            return fromEnvironment.Trim();
        }

        return CachedDsn;
    }

    public static string? ResolveRelease() => CachedRelease;

    private static string? ReadAssemblyMetadata(string key)
    {
        var value = typeof(SentryDsnResolver).Assembly
            .GetCustomAttributes<AssemblyMetadataAttribute>()
            .FirstOrDefault(attribute => attribute.Key == key)
            ?.Value;

        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}
