namespace ImproveYourself.Maui.Resources.Strings;

/// <summary>
/// Provides statically typed access to localized strings.
/// Must be initialized via <see cref="Load"/> before any UI is created.
/// </summary>
public static class AppStrings
{
    private static IReadOnlyDictionary<string, string> _strings = new Dictionary<string, string>(StringComparer.Ordinal);

    public static void Load(IReadOnlyDictionary<string, string> strings) => _strings = strings;

    private static string Get(string key) =>
        _strings.TryGetValue(key, out var value) ? value : key;

    // Common
    public static string OK => Get("OK");
    public static string Save => Get("Save");
    public static string Back => Get("Back");
    public static string Next => Get("Next");
    public static string Later => Get("Later");
    public static string Enable => Get("Enable");
    public static string Done => Get("Done");

    // App bootstrap
    public static string AppPreparing => Get("AppPreparing");
    public static string AppInitError => Get("AppInitError");

    // HomePage
    public static string HomePage_Title => Get("HomePage_Title");
    public static string Welcome => Get("Welcome");
    public static string DefaultDisplayName => Get("DefaultDisplayName");
    public static string CurrentStreak => Get("CurrentStreak");
    public static string DaysInRow => Get("DaysInRow");
    public static string MonthProgress => Get("MonthProgress");
    public static string MonthProgressFormat => Get("MonthProgressFormat");
    public static string MainFocusOfDay => Get("MainFocusOfDay");
    public static string OpenDailyChallenge => Get("OpenDailyChallenge");
    public static string GoToNextDay => Get("GoToNextDay");
    public static string PleaseWait => Get("PleaseWait");
    public static string PreparingChallenge => Get("PreparingChallenge");
    public static string TryOpenInSecond => Get("TryOpenInSecond");
    public static string FocusOnDate => Get("FocusOnDate");
    public static string DateStepsFormat => Get("DateStepsFormat");
    public static string StatisticsButtonTitle => Get("StatisticsButtonTitle");
    public static string StatisticsButtonSubtitle => Get("StatisticsButtonSubtitle");
    public static string CalendarButtonTitle => Get("CalendarButtonTitle");
    public static string CalendarButtonSubtitle => Get("CalendarButtonSubtitle");
    public static string OpenSettings => Get("OpenSettings");
    public static string RemainingDaysGoal => Get("RemainingDaysGoal");

    // SettingsPage
    public static string Settings_Title => Get("Settings_Title");
    public static string Profile => Get("Profile");
    public static string GreetingName => Get("GreetingName");
    public static string EnterNamePlaceholder => Get("EnterNamePlaceholder");
    public static string SaveName => Get("SaveName");
    public static string Reminders => Get("Reminders");
    public static string ReminderDescription => Get("ReminderDescription");
    public static string EnableNotifications => Get("EnableNotifications");
    public static string OfflineFirstTitle => Get("OfflineFirstTitle");
    public static string OfflineFirstDescription => Get("OfflineFirstDescription");
    public static string Backend => Get("Backend");
    public static string BackendDescription => Get("BackendDescription");
    public static string SaveButton => Get("SaveButton");
    public static string CheckButton => Get("CheckButton");
    public static string BackendNotConfigured => Get("BackendNotConfigured");
    public static string BackendSaved => Get("BackendSaved");
    public static string Saved_Title => Get("Saved_Title");
    public static string NameUpdated => Get("NameUpdated");
    public static string NoNotificationAccess => Get("NoNotificationAccess");
    public static string NotificationPermissionInstructions => Get("NotificationPermissionInstructions");
    public static string BackendSettingsUpdated => Get("BackendSettingsUpdated");
    public static string CheckingBackend => Get("CheckingBackend");
    public static string BackendVerifiedSyncing => Get("BackendVerifiedSyncing");
    public static string LanguageSection => Get("LanguageSection");

    // Onboarding
    public static string Onboarding1_Title => Get("Onboarding1_Title");
    public static string Onboarding1_Body => Get("Onboarding1_Body");
    public static string Onboarding2_Title => Get("Onboarding2_Title");
    public static string Onboarding2_Body => Get("Onboarding2_Body");
    public static string Onboarding3_Title => Get("Onboarding3_Title");
    public static string Onboarding3_Body => Get("Onboarding3_Body");
    public static string HowToAddress => Get("HowToAddress");
    public static string GoToSelfAssessment => Get("GoToSelfAssessment");

    // SelfAssessmentPage
    public static string SelfAssessment_Title => Get("SelfAssessment_Title");
    public static string BeforeStart => Get("BeforeStart");
    public static string FinalAssessment => Get("FinalAssessment");
    public static string QuickStateCheck => Get("QuickStateCheck");
    public static string WhatChangedIn30Days => Get("WhatChangedIn30Days");
    public static string SelfAssessmentStartDesc => Get("SelfAssessmentStartDesc");
    public static string SelfAssessmentFinalDesc => Get("SelfAssessmentFinalDesc");
    public static string SaveAndStart => Get("SaveAndStart");
    public static string ShowResult => Get("ShowResult");
    public static string YourResult => Get("YourResult");
    public static string StartScore => Get("StartScore");
    public static string FinishScore => Get("FinishScore");
    public static string Shift => Get("Shift");
    public static string Notifications_Title => Get("Notifications_Title");
    public static string PermissionNotGranted => Get("PermissionNotGranted");
    public static string EnableNotificationsLater => Get("EnableNotificationsLater");
    public static string FinalComparisonFormat => Get("FinalComparisonFormat");

    // ChallengeDetailPage
    public static string ChallengeDetail_Title => Get("ChallengeDetail_Title");
    public static string DatePlaceholder => Get("DatePlaceholder");
    public static string ChallengeNotFound => Get("ChallengeNotFound");
    public static string ProgressFormat => Get("ProgressFormat");
    public static string ChallengeCompleted => Get("ChallengeCompleted");
    public static string QuoteOfDay => Get("QuoteOfDay");
    public static string TapToOpenNote => Get("TapToOpenNote");
    public static string TapToCloseNote => Get("TapToCloseNote");
    public static string NoteLabel => Get("NoteLabel");
    public static string TipFormat => Get("TipFormat");
    public static string DateFormat => Get("DateFormat");

    // StatisticsPage
    public static string Statistics_Title => Get("Statistics_Title");
    public static string BestStreak => Get("BestStreak");
    public static string Days => Get("Days");
    public static string WeekCompletion => Get("WeekCompletion");
    public static string DaysClosedFormat => Get("DaysClosedFormat");
    public static string WeeklyActivity => Get("WeeklyActivity");
    public static string CompletionRateFormat => Get("CompletionRateFormat");
    public static string TaskCategories => Get("TaskCategories");
    public static string MorningPractice => Get("MorningPractice");
    public static string DailyChallengeName => Get("DailyChallengeName");
    public static string DayMon => Get("DayMon");
    public static string DayTue => Get("DayTue");
    public static string DayWed => Get("DayWed");
    public static string DayThu => Get("DayThu");
    public static string DayFri => Get("DayFri");
    public static string DaySat => Get("DaySat");
    public static string DaySun => Get("DaySun");

    // ProfilePage
    public static string ProfileCalendar_Title => Get("ProfileCalendar_Title");
    public static string SelfAssessmentLabel => Get("SelfAssessmentLabel");
    public static string StartCutFormat => Get("StartCutFormat");
    public static string StartFinishFormat => Get("StartFinishFormat");
    public static string ShiftFormat => Get("ShiftFormat");
    public static string MonthActivity => Get("MonthActivity");
    public static string AllChallenges => Get("AllChallenges");
    public static string CompletionRateLabel => Get("CompletionRateLabel");

    // TypeMappings — step statuses and actions
    public static string StatusNotStarted => Get("StatusNotStarted");
    public static string StatusInProgress => Get("StatusInProgress");
    public static string StatusCompleted => Get("StatusCompleted");
    public static string ActionStart => Get("ActionStart");
    public static string ActionMarkCompleted => Get("ActionMarkCompleted");
    public static string ActionCompleted => Get("ActionCompleted");

    // SelfAssessment questions
    public static string Question_Conversation => Get("Question_Conversation");
    public static string Question_Opinion => Get("Question_Opinion");
    public static string Question_Body => Get("Question_Body");
    public static string Question_EyeContact => Get("Question_EyeContact");
    public static string Question_Action => Get("Question_Action");

    // BackendConnectionService
    public static string BackendProvideUrl => Get("BackendProvideUrl");
    public static string BackendInvalidUrl => Get("BackendInvalidUrl");
    public static string BackendAllOk => Get("BackendAllOk");
    public static string BackendNoDb => Get("BackendNoDb");
    public static string BackendBadApiKey => Get("BackendBadApiKey");
    public static string BackendBadStatusFormat => Get("BackendBadStatusFormat");
    public static string BackendUnavailable => Get("BackendUnavailable");
    public static string BackendTimeout => Get("BackendTimeout");
    public static string BackendConnectionFailedFormat => Get("BackendConnectionFailedFormat");
    public static string BackendSyncNoChallenges => Get("BackendSyncNoChallenges");
    public static string BackendSyncSucceeded => Get("BackendSyncSucceeded");
    public static string BackendSyncSucceededWithStatsFormat => Get("BackendSyncSucceededWithStatsFormat");
    public static string BackendSyncFailedStatusFormat => Get("BackendSyncFailedStatusFormat");
    public static string BackendSyncFailedFormat => Get("BackendSyncFailedFormat");
    public static string BackendSyncTimeout => Get("BackendSyncTimeout");
    public static string BackendSyncRecently => Get("BackendSyncRecently");
    public static string BackendSyncInProgress => Get("BackendSyncInProgress");

    public static string AuthAccountSection => Get("AuthAccountSection");
    public static string AuthAccountDescription => Get("AuthAccountDescription");
    public static string AuthLoginTitle => Get("AuthLoginTitle");
    public static string AuthLoginDescription => Get("AuthLoginDescription");
    public static string AuthRegisterTitle => Get("AuthRegisterTitle");
    public static string AuthRegisterDescription => Get("AuthRegisterDescription");
    public static string AuthEmailPlaceholder => Get("AuthEmailPlaceholder");
    public static string AuthPasswordPlaceholder => Get("AuthPasswordPlaceholder");
    public static string AuthLoginButton => Get("AuthLoginButton");
    public static string AuthRegisterButton => Get("AuthRegisterButton");
    public static string AuthGoToRegister => Get("AuthGoToRegister");
    public static string AuthLogoutButton => Get("AuthLogoutButton");
    public static string AuthSyncButton => Get("AuthSyncButton");
    public static string AuthLoggedInAsFormat => Get("AuthLoggedInAsFormat");
    public static string AuthNotLoggedIn => Get("AuthNotLoggedIn");
    public static string AuthLoginRequired => Get("AuthLoginRequired");
    public static string AuthSessionExpired => Get("AuthSessionExpired");
    public static string AuthSucceeded => Get("AuthSucceeded");
    public static string AuthFailed => Get("AuthFailed");
    public static string AuthInvalidCredentials => Get("AuthInvalidCredentials");
    public static string AuthEmailAlreadyRegistered => Get("AuthEmailAlreadyRegistered");
    public static string AuthWorking => Get("AuthWorking");

    // Domain — challenge structure
    public static string DailyChallenge_DefaultTitle => Get("DailyChallenge_DefaultTitle");
    public static string PracticeStep_Title => Get("PracticeStep_Title");
    public static string SocialStep_Title => Get("SocialStep_Title");
    public static string QuoteStep_Title => Get("QuoteStep_Title");
    public static string QuoteStep_Description => Get("QuoteStep_Description");
    public static string StepOfDay => Get("StepOfDay");

    // ChallengePersonalizer — focus titles
    public static string Focus_ConversationStart => Get("Focus_ConversationStart");
    public static string Focus_OpinionExpression => Get("Focus_OpinionExpression");
    public static string Focus_BodyCalm => Get("Focus_BodyCalm");
    public static string Focus_EyeContact => Get("Focus_EyeContact");
    public static string Focus_SocialAction => Get("Focus_SocialAction");
    public static string FocusDayReasonFormat => Get("FocusDayReasonFormat");
}
