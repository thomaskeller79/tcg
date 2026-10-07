// Drives the real UI against the live server (load the combat-lab scenario first). Run with
// headless Chrome --dump-dom and read <pre id="result">. Dev-only.
'use strict';
(async function () {
  const out = [];
  const wait = (ms) => new Promise(r => setTimeout(r, ms));
  const until = async (fn, what) => { for (let i = 0; i < 60; i++) { if (fn()) return; await wait(100); } throw new Error('timeout: ' + what); };
  const buttons = () => [...document.querySelectorAll('#prompt button')];
  const clickButton = (text) => { const b = buttons().find(x => x.textContent.includes(text)); if (!b) throw new Error('no button ' + text + ' in ' + buttons().map(x => x.textContent).join(' | ')); b.click(); };
  try {
    await until(() => ui.view, 'view');
    const warrior = ui.view.permanents.find(p => p.card === 'creature.fire-warrior' && p.controller === 'A');
    const token = [...document.querySelectorAll('.token')].find(g => g.querySelector('title').textContent.includes(`#${warrior.id} `));
    token.dispatchEvent(new MouseEvent('click', { bubbles: true }));
    out.push('selected ' + warrior.name);
    clickButton('Attack');
    const hex = [...document.querySelectorAll('polygon.hex.candidate')];
    out.push('candidate hexes ' + hex.length);
    hex[0].dispatchEvent(new MouseEvent('click', { bubbles: true }));
    const confirm = buttons().find(b => b.textContent.startsWith('Confirm'));
    out.push(confirm ? confirm.textContent : 'buttons: ' + buttons().map(x => x.textContent).join(' | '));
    confirm.click();
    await until(() => ui.view.pending.length === 1, 'attack in Pending');
    out.push('pending: ' + ui.view.pending[0].text + ' / ' + JSON.stringify(ui.view.pending[0].attack));
    out.push('OK');
  } catch (e) {
    out.push('FAIL ' + e.message);
  }
  document.getElementById('result').textContent = out.join('\n');
})();
