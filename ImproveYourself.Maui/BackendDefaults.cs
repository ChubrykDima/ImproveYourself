namespace ImproveYourself.Maui;

public static class BackendDefaults
{
    public const string ProductionBaseUrl = "https://improveyourself-backend-production.up.railway.app";

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
