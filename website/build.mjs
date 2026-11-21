import { readFile, writeFile, mkdir, cp, rm } from 'node:fs/promises';
import path from 'node:path';
import { fileURLToPath } from 'node:url';

const base = path.dirname(fileURLToPath(import.meta.url));
const dist = path.join(base, 'dist');
const template = await readFile(path.join(base, 'src/template.html'), 'utf8');
const copy = JSON.parse(await readFile(path.join(base, 'src/translations.json'), 'utf8'));
const escape = value => String(value).replaceAll('&', '&amp;').replaceAll('"', '&quot;').replaceAll('<', '&lt;').replaceAll('>', '&gt;');
const origin = 'https://improveyourself-app.com';
const keys = [...template.matchAll(/data-copy="([^"]+)"/g)].map(match => match[1]);
await rm(dist, { recursive: true, force: true });
await mkdir(dist, { recursive: true });
await cp(path.join(base, 'src/assets'), path.join(dist, 'assets'), { recursive: true });

for (const lang of ['ru', 'en', 'de']) {
  const t = copy[lang];
  for (const key of [...keys, 'headline', 'title', 'description']) {
    if (!t[key]) throw new Error(`Missing ${lang}.${key}`);
  }
  let body = template.replace(/(<([a-z0-9-]+)\b[^>]*data-copy="([^"]+)"[^>]*>)([\s\S]*?)(<\/\2>)/gi,
    (_match, open, _tag, key, _content, close) => open + t[key] + close);
  body = body.replace('<h1>', '<h1 data-headline>').replace(/(<h1 data-headline>)[\s\S]*?(<\/h1>)/, '$1' + t.headline + '$2');
  body = body.replace('lang="ru"', `lang="${lang}"`);
  body = body.replace('aria-label="Legal and support"', `aria-label="${escape(t.legalLabel)}"`);
  body = body.replace(/(data-lang="(ru|en|de)"[^>]*)(?:aria-current="[^"]*"|aria-pressed="[^"]*")/g,
    (_match, prefix, value) => prefix + (value === lang ? 'aria-current="page"' : ''));
  body = body.replace(/data-iy-count>[^<]+/, `data-iy-count>7 / 30 ${t.days}`)
    .replace(/data-iy-task-text>[^<]+/, `data-iy-task-text>${t.question}`)
    .replace(/(class="iy-phone-action"[^>]*>)[^<]+/, '$1' + t.mark);
  let ariaIndex = 0;
  body = body.replace(/data-aria="(\d+)" aria-label="[^"]*"/g,
    (_match, index) => { ariaIndex++; return `aria-label="${escape(t.aria[Number(index)])}"`; });
  if (ariaIndex !== t.aria.length) throw new Error('Incomplete accessibility labels');

  const document = `<!doctype html>
<html lang="${lang}">
<head>
  <meta charset="utf-8">
  <meta name="viewport" content="width=device-width, initial-scale=1">
  <meta name="theme-color" content="#090c12">
  <title>${escape(t.title)}</title>
  <meta name="description" content="${escape(t.description)}">
  <link rel="canonical" href="${origin}/${lang}/">
  <link rel="alternate" hreflang="ru" href="${origin}/ru/">
  <link rel="alternate" hreflang="en" href="${origin}/en/">
  <link rel="alternate" hreflang="de" href="${origin}/de/">
  <link rel="alternate" hreflang="x-default" href="${origin}/">
  <meta property="og:type" content="website">
  <meta property="og:title" content="${escape(t.title)}">
  <meta property="og:description" content="${escape(t.description)}">
  <meta property="og:url" content="${origin}/${lang}/">
  <link rel="icon" type="image/svg+xml" href="/assets/favicon.svg">
  <link rel="stylesheet" href="/assets/site.css">
  <script defer src="/assets/site.js"></script>
</head>
<body>${body}</body>
</html>
`;
  await mkdir(path.join(dist, lang), { recursive: true });
  await writeFile(path.join(dist, lang, 'index.html'), document);
  if (lang === 'en') await writeFile(path.join(dist, 'index.html'), document.replace(`href="${origin}/en/"`, `href="${origin}/"`).replace('<html lang="en">', '<html lang="en" data-language-entry>'));
}
await writeFile(path.join(dist, 'robots.txt'), `User-agent: *\nAllow: /\nSitemap: ${origin}/sitemap.xml\n`);
await writeFile(path.join(dist, 'sitemap.xml'), `<?xml version="1.0" encoding="UTF-8"?>\n<urlset xmlns="http://www.sitemaps.org/schemas/sitemap/0.9">${['ru','en','de'].map(lang => `<url><loc>${origin}/${lang}/</loc></url>`).join('')}</urlset>\n`);
await writeFile(path.join(dist, '404.html'), await readFile(path.join(dist, 'en/index.html'), 'utf8'));
await writeFile(path.join(dist, '_headers'), `/*
  X-Content-Type-Options: nosniff
  Referrer-Policy: strict-origin-when-cross-origin
  Content-Security-Policy: default-src 'self'; script-src 'self'; style-src 'self' 'unsafe-inline'; img-src 'self' data:; connect-src 'self'; frame-ancestors 'none'; base-uri 'self'; form-action 'none'
/assets/*
  Cache-Control: public, max-age=3600
`);
console.log('Built RU, EN, DE and language entry page in ' + dist);
