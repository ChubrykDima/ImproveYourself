namespace ImproveYourself.Maui;

public static class BackendDefaults
{
    public const string ProductionBaseUrl = "https://improveyourself-backend-production.up.railway.app";

    // Never embed shared secrets in a mobile app binary.
    public const string ProductionApiKey = "";

    public static bool AllowManualBackendSettings
    {
        get
        {
#if DEBUG
            return true;
#else
            return false;
#endif
        }
    }
}
