# Deployment

Published on Cloudflare Pages on 2026-11-21 using Direct Upload.

- Project: `improveyourself-app`
- Live site: https://improveyourself-app.com/
- Technical address: https://improveyourself-app.pages.dev/
- Language pages: `/ru/`, `/en/`, `/de/`
- Active custom domains: `improveyourself-app.com`, `www.improveyourself-app.com`
- Registrar: GoDaddy
- DNS plan: Cloudflare Free
- Assigned nameservers: `brad.ns.cloudflare.com`, `maeve.ns.cloudflare.com`
- Previous nameservers: `ns47.domaincontrol.com`, `ns48.domaincontrol.com`

The approved nameserver change has been saved in GoDaddy and confirmed by public DNS. Cloudflare DNS is active. Both custom domains are Active in Pages with SSL enabled. The `/ru/`, `/en/`, and `/de/` pages on the main domain and `/en/` on the `www` domain returned HTTP 200 with validated TLS and the expected content and assets. Language switching and the mobile layout were also checked in the browser. Registration and renewal remain with GoDaddy.

The existing GoDaddy DNS zone was exported before migration. Cloudflare's quick scan omitted `dc-fd741b8612._spfm.send`; its TXT value `v=spf1 include:amazonses.com ~all` was restored in the new Cloudflare zone. The existing Resend DKIM, `send` MX and SPF, and DMARC records were preserved. All five email-related records were verified through public DNS after the nameserver switch. GoDaddy DNSSEC was already disabled; it was not changed.

## Updating the site

```sh
cd website
npm run build
```

In Cloudflare Pages, open this project, select **Create deployment**, and upload `dist/` or a ZIP containing its files. This initial deployment uses Direct Upload rather than Git integration; commits do not currently publish automatically.
