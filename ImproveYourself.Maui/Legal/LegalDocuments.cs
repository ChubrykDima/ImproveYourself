namespace ImproveYourself.Maui.Legal;

/// <summary>
/// In-app Privacy Policy and Terms of Service copy for store listing readiness.
/// Public console URLs live in <see cref="LegalUrls"/> (mirrored under /docs in the repo).
/// </summary>
public static class LegalDocuments
{
    public const string PrivacyPolicyBody = """
Last updated: July 24, 2026

Improve Yourself (“we”, “us”, “the App”) helps you build daily social-confidence habits. This Privacy Policy explains what data the App processes and why.

1. Data we process
• Account data: email address and authentication tokens when you create or sign in to a cloud account.
• App content: display name, challenge progress, step completion, self-assessment answers, and settings you enter in the App.
• Device/technical data: app version, platform, and similar diagnostics needed to operate sync and analytics when you are signed in.
• Local storage: progress and settings are stored on your device (including Secure Storage for access/refresh tokens).

2. How we use data
• Provide offline-first daily challenges and local progress tracking.
• Sync progress to our companion backend when you are signed in.
• Authenticate your session with JWT access and refresh tokens.
• Improve reliability with limited authenticated analytics events.

3. Legal bases / purposes
We process data to provide the App’s core functionality, secure your account, and (when enabled) sync and analytics that support the product.

4. Sharing
We do not sell your personal data. Data may be processed by infrastructure providers that host the companion backend (currently Railway) solely to operate the service.

5. Retention
• Local data remains on your device until you clear app data or delete the App.
• Cloud account data is retained while your account exists.
• When you delete your account in Settings and the server completes the request, cloud account data associated with that account is removed and you are signed out.
• Authentication tokens are stored securely on device and cleared on sign-out or when the session can no longer be refreshed.

6. Your choices
• Use the App offline without creating an account.
• Sign out at any time in Settings.
• Delete your account in Settings. If the server cannot complete deletion, the App tells you and you can still sign out locally; contact support for follow-up.
• Export your cloud account data from Settings (JSON download/share) while signed in.
• Contact us about privacy requests at the support email listed in the store listing / app metadata.

7. Children
The App is not directed to children under 13 (or the minimum age required in your region). Do not use the App if you are under that age.

8. International transfers
If you use cloud sync, your account data may be processed in the region where the backend is hosted.

9. Changes
We may update this Policy. The “Last updated” date will change when we do. Continued use after an update means you accept the revised Policy.

10. Contact
For privacy questions: use the developer contact email published with the App on the App Store / Google Play.
""";

    public const string TermsOfServiceBody = """
Last updated: July 24, 2026

These Terms of Service (“Terms”) govern your use of Improve Yourself (the “App”). By using the App you agree to these Terms.

1. The service
Improve Yourself provides daily challenges and progress tracking intended for personal self-improvement. The App works offline. Optional cloud sync requires an account and an internet connection.

2. Accounts
• You must provide a valid email and keep your credentials confidential.
• You are responsible for activity under your account.
• We may suspend accounts that abuse the service or attempt unauthorized access.

3. Acceptable use
You agree not to reverse engineer, disrupt, or misuse the App or companion backend, and not to attempt to access other users’ data.

4. Health / advice disclaimer
The App provides general self-improvement prompts only. It is not medical, psychological, or therapeutic advice. If you need professional help, consult a qualified provider.

5. Intellectual property
The App, branding, and content structure are owned by the developer. You retain rights to the personal content you enter.

6. Subscriptions / pricing
Current versions may be free. If paid features are introduced later, pricing and renewal terms will be shown in the store listing and in-app purchase sheets before you buy.

7. Privacy
Your use of the App is also governed by the Privacy Policy available in the App and at the public Privacy Policy URL published with the App.

8. Termination
You may stop using the App at any time. You may delete your account in Settings. Successful server-side deletion removes the cloud account and signs you out. If deletion cannot be completed on the server, the App informs you and local sign-out remains available.

9. Disclaimer of warranties
The App is provided “as is” without warranties of any kind to the maximum extent permitted by law.

10. Limitation of liability
To the maximum extent permitted by law, the developer is not liable for indirect, incidental, or consequential damages arising from your use of the App.

11. Changes
We may update these Terms. Continued use after changes means you accept the updated Terms.

12. Contact
Questions about these Terms: use the developer contact email published with the App on the App Store / Google Play.
""";
}

/// <summary>
/// Public URLs for App Store Connect / Google Play Console.
/// Mirrored markdown lives in the repo under /docs (same branch as this client).
/// </summary>
public static class LegalUrls
{
    /// <summary>Public Privacy Policy URL for App Store / Google Play consoles.</summary>
    public const string PrivacyPolicy =
        "https://github.com/ChubrykDima/ImproveYourself/blob/master/docs/privacy-policy.md";

    /// <summary>Public Terms of Service URL for App Store / Google Play consoles.</summary>
    public const string TermsOfService =
        "https://github.com/ChubrykDima/ImproveYourself/blob/master/docs/terms-of-service.md";

    public static bool HasPrivacyPolicyUrl =>
        Uri.TryCreate(PrivacyPolicy, UriKind.Absolute, out var uri)
        && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);

    public static bool HasTermsOfServiceUrl =>
        Uri.TryCreate(TermsOfService, UriKind.Absolute, out var uri)
        && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
}
