# Improve Yourself website

Static marketing site in Russian, English, and German, based on the approved design.
No runtime packages, API keys, accounts, analytics, or backend are required for visitors.

## Local development

Requires Node.js 20+ and Python 3 for the optional local server.

```sh
cd website
npm run build
npm run preview
```

Open http://127.0.0.1:4173. The root chooses a language using the saved preference or browser language. `/ru/`, `/en/`, and `/de/` are complete pages that also work without JavaScript. The phone is an interactive demonstration, not a signed-in account.

Edit `src/template.html`, `src/assets/site.css`, `src/assets/site.js`, and `src/translations.json`, then rebuild. Generated files are in `dist/`.

## Cloudflare Pages

- For Git integration: use this repository, root directory `website`, build command `npm run build`, output directory `dist`.
- For Direct Upload: upload the contents of `dist/` or a ZIP with `index.html` at its root. A Direct Upload project cannot later be changed to Git integration; automated uploads can instead use Wrangler or CI.
- Connect `improveyourself-app.com` and `www.improveyourself-app.com` after the preview is verified. Set a canonical redirect from `www` to the apex domain.
- Before switching nameservers, export and verify the existing DNS records, especially email, verification, and API records. Keep the previous configuration available for rollback.

The app privacy policy and terms link to the existing public documents; their legal text is not modified by this website.

Icons are derived from Lucide 1.17.0 under the ISC license; see `src/assets/lucide-license.txt`.
