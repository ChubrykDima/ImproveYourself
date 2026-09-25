namespace ImproveYourself.Maui.Legal;

/// <summary>
/// In-app Privacy Policy and Terms of Service copy for store listing readiness.
/// Public console URLs live in <see cref="LegalUrls"/> (mirrored under /docs in the repo).
/// </summary>
public static class LegalDocuments
{
    public const string PrivacyPolicyBody = """
Last updated: September 25, 2026

Improve Yourself (“we”, “us”, “the App”) helps you build daily social-confidence habits. This Privacy Policy explains what data the App processes, why, and who else processes it.

1. Who is responsible
Improve Yourself is operated by Dzmitry Chubryk. Privacy questions, data requests, and support requests: supportimproveyourself@gmail.com.

2. Data we process
• Account data: email address, password hash (the password itself is never stored), account identifier, and authentication tokens when you create or sign in to a cloud account.
• App content: display name, challenge progress, step completion, self-assessment answers, and settings you enter in the App.
• Usage events: a limited, allow-listed set of in-app events (for example “challenge completed”) with app version, platform, and device type. They are sent only while you are signed in and are linked to your account.
• Operational data: IP address, request time, route, response status, and error diagnostics in server logs.
• Local storage: progress and settings are stored on your device; access and refresh tokens are kept in the operating system’s secure storage.

Daily reminders are scheduled locally on your device. No reminder data is sent to our servers.

3. How we use data
• Provide offline-first daily challenges and local progress tracking.
• Sync progress to our backend when you are signed in.
• Authenticate your session and send password-reset codes you request.
• Understand how the App is used and keep it reliable.

We do not sell personal data, do not use it for advertising, and do not track you across other companies’ apps or websites.

4. Service providers
We share data only with processors that operate features you use:
• Railway hosts the backend application and database.
• Resend delivers password-reset emails (your email address and the one-time code).
These providers act on our instructions and process data under their own privacy and security terms, which provide protection comparable to this Policy.

5. Retention and deletion
• Local data remains on your device until you clear app data or delete the App.
• Cloud account data is retained while your account exists.
• When you delete your account in Settings → Delete account, your account, synced challenges and steps, usage events, and authentication and password-reset tokens are permanently deleted from our database, and you are signed out.
• Server logs are kept only for the limited period set by our hosting provider and are not used to restore deleted accounts.

6. Your choices and rights
• Use the App offline without creating an account.
• Turn daily reminders on or off in Settings or in your device’s notification settings.
• Sign out at any time in Settings.
• Delete your account in Settings. If the server cannot complete deletion, the App tells you; you can still sign out locally and contact us to finish the request.
• Export your cloud account data from Settings (JSON download/share) while signed in.
• Request access, correction, or deletion of your data by email. Depending on where you live, you may also have the right to restrict or object to processing and to lodge a complaint with a data-protection authority.

7. Children
The App is not directed to children under 13 (or the minimum age required in your region), and we do not knowingly collect their personal data. Contact us if you believe a child has provided personal data.

8. International transfers
Our service providers may process data in countries other than your own, under their terms and applicable safeguards.

9. Changes
We may update this Policy. The “Last updated” date will change when we do, and material changes will be announced in the App or the store listing.

10. Contact
Dzmitry Chubryk — supportimproveyourself@gmail.com
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
Questions about these Terms: Dzmitry Chubryk — supportimproveyourself@gmail.com.
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
