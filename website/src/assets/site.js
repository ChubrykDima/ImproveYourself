(() => {
  'use strict';
  const supported = ['ru', 'en', 'de'];
  const key = 'improveyourself.language';
  const root = document.getElementById('iy-landing-preview');
  if (!root) return;
  if (document.documentElement.hasAttribute('data-language-entry')) {
    let stored;
    try { stored = localStorage.getItem(key); } catch {}
    const preferred = (navigator.languages || [navigator.language || 'en']).map(value => value.split('-')[0]);
    const lang = supported.includes(stored) ? stored : preferred.find(value => supported.includes(value)) || 'en';
    location.replace('/' + lang + '/');
    return;
  }
  const language = document.documentElement.lang;
  try { localStorage.setItem(key, language); } catch {}
  const button = root.querySelector('.iy-phone-action');
  const taskText = root.querySelector('[data-iy-task-text]');
  const count = root.querySelector('[data-iy-count]');
  const bar = root.querySelector('[role="progressbar"]');
  const fill = root.querySelector('.iy-fill');
  const result = root.querySelector('[data-iy-result]');
  const initial = { task: taskText.textContent, button: button.textContent, count: count.textContent };
  const completed = {
    ru: ['Вызов выполнен ✓', 'Открытый вопрос задан ✓', '8 / 30 дней', 'Пример: выполнено 8 из 30 дней.'],
    en: ['Challenge complete ✓', 'Open-ended question asked ✓', '8 / 30 days', 'Example: 8 of 30 days completed.'],
    de: ['Tagesaufgabe erledigt ✓', 'Offene Frage gestellt ✓', '8 / 30 Tage', 'Beispiel: 8 von 30 Tagen abgeschlossen.']
  };
  button.addEventListener('click', () => {
    const done = button.getAttribute('aria-pressed') !== 'true';
    const t = completed[language];
    button.setAttribute('aria-pressed', String(done));
    button.textContent = done ? t[0] : initial.button;
    taskText.textContent = done ? t[1] : initial.task;
    count.textContent = done ? t[2] : initial.count;
    fill.style.width = `${(done ? 8 : 7) / 30 * 100}%`;
    bar.setAttribute('aria-valuenow', done ? '8' : '7');
    result.textContent = done ? t[3] : initial.count;
  });
})();
