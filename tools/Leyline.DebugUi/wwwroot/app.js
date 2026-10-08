// Leyline debug UI. One script for every page: index.html (hub, data-seat="hub"), p1.html /
// p2.html (a Champion's own view, data-seat="1"/"2") and true.html (the omniscient view,
// data-seat="0"). Bump ?v=N in every *.html when this file or style.css changes.
'use strict';

const SEAT_ATTR = document.body.dataset.seat;
const SEAT = SEAT_ATTR === 'hub' ? null : Number(SEAT_ATTR); // 0 = true state
const ME = SEAT === 1 ? 'A' : SEAT === 2 ? 'B' : null;
const HEX = 46;
const SQ3 = Math.sqrt(3);
const ELEMENT_COLORS = {
  Light: '#e9dc95', Fire: '#d9643f', Metal: '#9aa4ae', Earth: '#9a7444',
  Darkness: '#5b4a73', Ice: '#b4dcea', Water: '#4385c8', Air: '#b8e2c8',
};
const ELEMENT_LETTER = { Light: 'L', Fire: 'F', Metal: 'M', Earth: 'E', Darkness: 'D', Ice: 'I', Water: 'W', Air: 'A' };
const PHASES = ['Setup', 'Beginning', 'Action', 'End'];

const ui = {
  view: null,
  legal: [],
  layer: 'surface',
  selection: null, // {type:'perm'|'card'|'hex', id, q, r}
  wizard: null,    // {source, ability, chosen:[keys], focusHex}
  error: null,
  autoPass: [true, true],
  scenario: null,
  busy: false,
};

// ---------------------------------------------------------------------------- util

async function api(path, options) {
  const res = await fetch(path, options);
  const text = await res.text();
  if (!res.ok) throw new Error(`${path} → ${res.status}: ${text || res.statusText}`);
  return text ? JSON.parse(text) : null;
}

function el(tag, attrs = {}, ...children) {
  const e = document.createElement(tag);
  for (const [k, v] of Object.entries(attrs)) {
    if (k === 'class') e.className = v;
    else if (k.startsWith('on')) e.addEventListener(k.slice(2), v);
    else if (v !== undefined && v !== null && v !== false) e.setAttribute(k, v === true ? '' : v);
  }
  for (const c of children.flat()) {
    if (c === null || c === undefined || c === false) continue;
    e.append(c instanceof Node ? c : document.createTextNode(String(c)));
  }
  return e;
}

const SVG_NS = 'http://www.w3.org/2000/svg';
function svg(tag, attrs = {}, text) {
  const e = document.createElementNS(SVG_NS, tag);
  for (const [k, v] of Object.entries(attrs)) if (v !== undefined && v !== null) e.setAttribute(k, v);
  if (text !== undefined) e.textContent = text;
  return e;
}

const hexKey = (h) => `${h.q},${h.r}`;
const targetKey = (list) => JSON.stringify(list.map(t => [t.object ?? null, t.hex ? hexKey(t.hex) : null, t.slice ?? null, t.entity ?? null]));
const cardInfo = (id) => ui.view?.cards.find(c => c.id === id);
const permById = (id) => ui.view?.permanents.find(p => p.id === id);
const center = (q, r) => ({ x: HEX * (SQ3 * q + SQ3 / 2 * r), y: HEX * 1.5 * r });
const initials = (name) => name.replace(/^Remnant of /, '').split(/[\s,]+/).filter(Boolean).slice(0, 2).map(w => w[0]).join('').toUpperCase();

function hexCorners(cx, cy, size) {
  const pts = [];
  for (let i = 0; i < 6; i++) {
    const a = Math.PI / 180 * (60 * i - 30);
    pts.push(`${cx + size * Math.cos(a)},${cy + size * Math.sin(a)}`);
  }
  return pts.join(' ');
}

// Corner positions (pointy-top, corner i at 60i−30°). Track B item 5's layout: three
// alternating corners for Ground creatures, the other three for Sky creatures.
const G_CORNERS = [4, 0, 2];
const S_CORNERS = [5, 1, 3];
function cornerPos(cx, cy, i, k = 0.6) {
  const a = Math.PI / 180 * (60 * i - 30);
  return { x: cx + HEX * k * Math.cos(a), y: cy + HEX * k * Math.sin(a) };
}

// ---------------------------------------------------------------------------- data

async function refresh() {
  if (SEAT === null || ui.busy) return;
  ui.busy = true;
  try {
    const data = await api(`/api/view/${SEAT}`);
    ui.view = data.view;
    ui.scenario = data.scenario;
    ui.autoPass = data.autoPass;
    ui.legal = SEAT === 0 ? [] : await api(`/api/legal/${SEAT}`);
    validateUiState();
    render();
  } catch (err) {
    document.getElementById('banner').textContent = String(err.message || err);
    document.getElementById('banner').className = 'banner waiting';
  } finally {
    ui.busy = false;
  }
}

function validateUiState() {
  if (ui.selection?.type === 'perm' && !permById(ui.selection.id)) ui.selection = null;
  if (ui.selection?.type === 'card' && !myHand().some(c => c.id === ui.selection.id)) ui.selection = null;
  if (ui.wizard && matching().length === 0) ui.wizard = null;
}

async function submit(index) {
  ui.error = null;
  try {
    const res = await api('/api/submit', { method: 'POST', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify({ seat: SEAT, index }) });
    if (!res.accepted) ui.error = res.error;
  } catch (err) {
    ui.error = String(err.message || err);
  }
  ui.wizard = null;
  await refresh();
}

// ---------------------------------------------------------------------------- wizard

function activations() { return ui.legal.filter(c => c.kind === 'Activate'); }

function matching() {
  const w = ui.wizard;
  if (!w) return [];
  return activations().filter(c => c.source === w.source && c.ability === w.ability
    && w.chosen.every((k, i) => targetKey(c.targets[i]) === k)
    && (w.payment == null || c.payment === w.payment));
}

function wizardStep() {
  const cmds = matching();
  if (cmds.length === 0) return null;
  const step = ui.wizard.chosen.length;
  const total = cmds[0].targets.length;
  if (step >= total) {
    // D125: the payment, once the targets are chosen — asked only when it needs a choice.
    const payments = [...new Set(cmds.map(c => c.payment).filter(p => p != null))];
    if (payments.length > 1) return { done: false, payment: true, options: payments };
    return { done: true, command: cmds[0] };
  }
  const options = new Map();
  for (const c of cmds) {
    const k = targetKey(c.targets[step]);
    if (!options.has(k)) options.set(k, c.targets[step]);
  }
  return { done: false, step, total, options: [...options.entries()].map(([key, list]) => ({ key, list })) };
}

function startWizard(source, ability) {
  ui.wizard = { source, ability, chosen: [], focusHex: null, payment: null };
  advanceTrivialSteps();
  render();
}

function choose(key) {
  ui.wizard.chosen.push(key);
  ui.wizard.focusHex = null;
  advanceTrivialSteps();
  render();
}

/// A target selection with exactly one option still needs the player to see it — but a
/// selection whose single option is "nothing" (an up-to-zero target with no candidates) is skipped.
function advanceTrivialSteps() {
  for (let guard = 0; guard < 10; guard++) {
    const s = wizardStep();
    if (!s || s.done || s.payment) return;
    if (s.options.length === 1 && s.options[0].list.length === 0) ui.wizard.chosen.push(s.options[0].key);
    else return;
  }
}

/// Candidates of the current step that are a single object or a single location, for board
/// highlighting and click-to-pick.
function stepCandidates() {
  const s = ui.wizard ? wizardStep() : null;
  const result = { objects: new Map(), hexes: new Map() };
  if (!s || s.done || s.payment) return result;
  for (const o of s.options) {
    if (o.list.length !== 1) continue;
    const t = o.list[0];
    if (t.object != null) result.objects.set(t.object, o.key);
    else if (t.hex) {
      const k = hexKey(t.hex);
      if (!result.hexes.has(k)) result.hexes.set(k, []);
      result.hexes.get(k).push(o);
    }
  }
  return result;
}

// ---------------------------------------------------------------------------- render

function render() {
  if (!ui.view) return;
  renderHeader();
  renderBanner();
  renderBoard();
  renderPrompt();
  renderDetails();
  renderHand();
  renderAether();
  renderMind();
  renderLog();
}

function renderHeader() {
  const v = ui.view;
  const info = document.getElementById('turn-info');
  info.replaceChildren(
    el('span', {}, `${ui.scenario ?? ''} · Round ${v.round} · `),
    el('span', { class: 'seat' }, `${v.activeSeat}'s turn`),
    ...PHASES.slice(1).map(p => el('span', { class: 'phase' + (p === v.phase ? ' current' : '') }, p)),
  );
  const ap = document.getElementById('autopass');
  if (ap) {
    if (SEAT === 0) ap.parentElement.style.display = 'none';
    else ap.checked = ui.autoPass[SEAT - 1];
  }
}

function renderBanner() {
  const v = ui.view;
  const b = document.getElementById('banner');
  if (v.winner || v.isDraw) {
    b.className = 'banner over';
    b.textContent = v.isDraw ? 'The match is a draw — both Champions fell from the same instruction.' : `Champion ${v.winner} wins!`;
    return;
  }
  if (v.decision) {
    b.className = 'banner decision';
    b.textContent = v.yourDecision ? `Your decision: ${v.decision.text}` : `Waiting for Champion ${v.decision.decider}'s decision: ${v.decision.text}`;
    return;
  }
  if (SEAT === 0) {
    b.className = 'banner waiting';
    b.textContent = `True state — priority: ${v.priorityHolder ? 'Champion ' + v.priorityHolder : 'nobody'}${v.pending.length ? ` · ${v.pending.length} in Pending` : ''}`;
    return;
  }
  if (v.yourPriority) {
    b.className = 'banner yours';
    const own = v.activeSeat === `Champion ${ME}`;
    b.textContent = v.pending.length
      ? `Your priority — respond (Quick/Reactive/Instant), or pass to let the top of Pending resolve.`
      : own ? 'Your priority — your Action phase: act, or end it.' : 'Your priority — Pending is empty: play something Quick/Reactive/Instant, or pass.';
  } else {
    b.className = 'banner waiting';
    b.textContent = v.priorityHolder ? `Waiting for Champion ${v.priorityHolder}…` : `${v.phase}…`;
  }
}

function renderBoard() {
  const board = document.getElementById('board');
  const v = ui.view;
  board.replaceChildren();
  const centers = v.hexes.map(h => center(h.coord.q, h.coord.r));
  const xs = centers.map(c => c.x), ys = centers.map(c => c.y);
  const pad = HEX * 1.1;
  board.setAttribute('viewBox', `${Math.min(...xs) - pad} ${Math.min(...ys) - pad} ${Math.max(...xs) - Math.min(...xs) + 2 * pad} ${Math.max(...ys) - Math.min(...ys) + 2 * pad}`);

  const cand = stepCandidates();
  const attacked = new Set(v.pending.filter(t => t.attack).map(t => hexKey(t.attack.hex)));
  const byHex = new Map(v.hexes.map(h => [hexKey(h.coord), h]));
  const root = ui.layer === 'root';

  const gHex = svg('g'), gLines = svg('g'), gMarks = svg('g'), gTokens = svg('g');
  board.append(gHex, gLines, gMarks, gTokens);

  for (const h of v.hexes) {
    const { x, y } = center(h.coord.q, h.coord.r);
    const k = hexKey(h.coord);
    const bonder = h.bondedBy != null ? permById(h.bondedBy) : null;
    const classes = ['hex'];
    if (h.isVoid) classes.push('void');
    if (h.bondedByChampion) classes.push('bond-' + h.bondedByChampion);
    if (bonder && bonder.kind === 'Companion') classes.push('companion-bond');
    if (ui.selection?.type === 'hex' && ui.selection.q === h.coord.q && ui.selection.r === h.coord.r) classes.push('selected');
    if (cand.hexes.has(k)) classes.push('candidate');
    if (attacked.has(k)) classes.push('attacked');
    const poly = svg('polygon', { points: hexCorners(x, y, HEX - 1.5), class: classes.join(' '), fill: terrainFill(h) });
    poly.append(svg('title', {}, hexTooltip(h)));
    poly.addEventListener('click', () => onHexClick(h));
    gHex.append(poly);
    if (root && !h.isVoid) gHex.append(svg('polygon', { points: hexCorners(x, y, HEX - 1.5), class: 'root-shade' }));

    if (!h.isVoid) {
      const letters = h.produces.map(e => ELEMENT_LETTER[e]).join('') || '·';
      gMarks.append(svg('text', { x, y: y - HEX * 0.08, class: 'terrain-label', 'text-anchor': 'middle' }, letters + (h.moveCost > 1 ? ` ⛰${h.moveCost}` : '')));
      if (h.hasAbilities) gMarks.append(svg('text', { x: x + 12, y: y - HEX * 0.3, class: 'badge' }, '✦'));
      // Leyline node: green = drawn this cycle, red = paused and undrawn (D77).
      if (h.bondedBy != null) {
        const cls = h.drawn ? 'drawn' : h.paused || !h.flowing ? 'undrawn' : 'open';
        gMarks.append(svg('circle', { cx: x, cy: y + HEX * 0.08, r: 4, class: 'node ' + cls }));
      }
      if (h.homeOf) gMarks.append(svg('text', { x, y: y + HEX * 0.82, class: 'slot-mark', 'text-anchor': 'middle' }, `home ${h.homeOf}`));
      if (!root) {
        for (const [i, label] of [[G_CORNERS[0], 'G'], [S_CORNERS[0], 'S']]) {
          const p = cornerPos(x, y, i, 0.86);
          gMarks.append(svg('text', { x: p.x, y: p.y + 3, class: 'slot-mark', 'text-anchor': 'middle' }, label));
        }
      }
    }
    // Leylines between neighbouring terrain bonded by the same root.
    if (h.bondedBy != null) {
      for (const [dq, dr] of [[1, 0], [0, 1], [-1, 1]]) {
        const n = byHex.get(`${h.coord.q + dq},${h.coord.r + dr}`);
        if (!n || n.bondedBy !== h.bondedBy) continue;
        const c2 = center(n.coord.q, n.coord.r);
        const ok = h.flowing && n.flowing;
        gLines.append(svg('line', { x1: x, y1: y + HEX * 0.08, x2: c2.x, y2: c2.y + HEX * 0.08, class: 'leyline ' + (ok ? 'flow' : 'paused') }));
      }
    }
  }

  // Tokens.
  const loose = (p) => p.carrier == null;
  for (const h of v.hexes) {
    const { x, y } = center(h.coord.q, h.coord.r);
    const here = v.permanents.filter(p => loose(p) && p.hex.q === h.coord.q && p.hex.r === h.coord.r);
    const creaturesIn = (slice) => here.filter(p => ['Creature', 'Companion', 'Champion'].includes(p.kind) && p.slice === slice);
    const layerSlice = root ? 'Root' : 'Ground';
    creaturesIn(layerSlice).forEach((p, i) => drawToken(gTokens, p, cornerPos(x, y, G_CORNERS[i % 3]), cand));
    if (!root) creaturesIn('Sky').forEach((p, i) => drawToken(gTokens, p, cornerPos(x, y, S_CORNERS[i % 3]), cand));
    const structure = here.find(p => p.kind === 'Structure' && p.slice === layerSlice);
    if (structure) drawToken(gTokens, structure, { x, y: y + HEX * 0.08 }, cand);
    here.filter(p => p.kind === 'Remnant' && p.slice === layerSlice)
      .forEach((p, i) => drawToken(gTokens, p, { x: x - HEX * 0.5, y: y - 6 + i * 11 }, cand));
    here.filter(p => p.kind === 'Item' && p.slice === layerSlice)
      .forEach((p, i) => drawToken(gTokens, p, { x: x + HEX * 0.5, y: y - 6 + i * 11 }, cand));
  }
}

function terrainFill(h) {
  if (h.isVoid) return '#0c0c10';
  const colors = h.produces.map(e => ELEMENT_COLORS[e]);
  return colors[0] ?? '#666';
}

function hexTooltip(h) {
  const lines = [`(${h.coord.q},${h.coord.r}) ${h.terrainName}${h.terrainType ? ' — ' + h.terrainType : ''}`];
  if (h.isVoid) return lines[0] + '\nVoid: can\'t be entered or bonded.';
  lines.push(`Produces: ${h.produces.join(', ') || 'nothing'} · move cost ${h.moveCost}`);
  if (h.bondedBy != null) lines.push(`Bonded by #${h.bondedBy} (Champion ${h.bondedByChampion}) — ${h.flowing ? 'flowing' : h.paused ? 'PAUSED by a knot' : 'not connected'}${h.drawn ? ', drawn this cycle' : ''}`);
  else lines.push('Unbonded');
  if (h.homeOf) lines.push(`Home ground of Champion ${h.homeOf}`);
  return lines.join('\n');
}

function drawToken(layer, p, pos, cand) {
  const kind = p.kind.toLowerCase();
  const classes = ['token', 'ctrl-' + p.controller, kind];
  if (p.locked) classes.push('locked');
  if (p.inRoot && SEAT === 0) classes.push('hidden-to-others');
  if (ui.selection?.type === 'perm' && ui.selection.id === p.id) classes.push('selected');
  if (cand.objects.has(p.id)) classes.push('candidate');
  const g = svg('g', { class: classes.join(' '), transform: `translate(${pos.x},${pos.y})` });
  let size;
  if (p.kind === 'Structure') {
    size = 13;
    g.append(svg('rect', { x: -size, y: -size, width: 2 * size, height: 2 * size, rx: 4, class: 'body' }));
  } else if (p.kind === 'Item') {
    size = 7;
    g.append(svg('polygon', { points: `0,${-size} ${size},0 0,${size} ${-size},0`, class: 'body' }));
  } else if (p.kind === 'Remnant') {
    size = 6;
    g.append(svg('rect', { x: -size, y: -size, width: 2 * size, height: 2 * size, class: 'body', fill: '#777', style: 'fill:#777' }));
  } else {
    size = p.kind === 'Champion' ? 15 : 12.5;
    g.append(svg('circle', { r: size, class: 'body' }));
  }
  if (p.kind !== 'Item' && p.kind !== 'Remnant') {
    g.append(svg('text', { y: 3.5, class: 'initials' }, initials(p.name)));
    const stats = p.kind === 'Structure' ? `♥${p.life} ·${p.ap}` : `${p.attack}/${p.life} ·${p.ap}`;
    g.append(svg('text', { y: size + 9, class: 'stats' }, stats));
  }
  const carried = ui.view.permanents.filter(q => q.carrier === p.id);
  if (carried.length) g.append(svg('text', { x: size - 1, y: -size + 4, class: 'badge' }, '⚔'));
  g.append(svg('title', {}, tokenTooltip(p, carried)));
  g.addEventListener('click', (e) => { e.stopPropagation(); onTokenClick(p); });
  layer.append(g);
}

function tokenTooltip(p, carried) {
  const lines = [`${p.name} #${p.id} — ${p.kind}, ${p.controller === 'Neutral' ? 'Neutral' : 'Champion ' + p.controller}`];
  if (['Creature', 'Companion', 'Champion'].includes(p.kind)) lines.push(`Attack ${p.attack} · Life ${p.life}/${p.maxLife} · AP ${p.ap}/${p.maxAp}`);
  else if (p.kind === 'Structure') lines.push(`Life ${p.life}/${p.maxLife} · AP ${p.ap}/${p.maxAp}`);
  if (p.keywords.length) lines.push(p.keywords.join(', '));
  if (p.locked) lines.push('Done for this turn (~)');
  if (p.behavior) lines.push(`Behavior: ${p.behavior}`);
  if (carried.length) lines.push('Carries: ' + carried.map(c => c.name).join(', '));
  return lines.join('\n');
}

// ---------------------------------------------------------------------------- board clicks

function onTokenClick(p) {
  const cand = stepCandidates();
  if (cand.objects.has(p.id)) { choose(cand.objects.get(p.id)); return; }
  ui.selection = { type: 'perm', id: p.id };
  if (ui.wizard && ui.wizard.source !== p.id) ui.wizard = null;
  render();
}

function onHexClick(h) {
  const cand = stepCandidates();
  const options = cand.hexes.get(hexKey(h.coord));
  if (options) {
    if (options.length === 1) { choose(options[0].key); return; }
    ui.wizard.focusHex = hexKey(h.coord);
    render();
    return;
  }
  ui.selection = { type: 'hex', q: h.coord.q, r: h.coord.r };
  ui.wizard = null;
  render();
}

// ---------------------------------------------------------------------------- side panels

function abilityGroups(sourceId) {
  const groups = new Map();
  for (const c of activations().filter(c => c.source === sourceId)) {
    if (!groups.has(c.ability)) groups.set(c.ability, []);
    groups.get(c.ability).push(c);
  }
  return groups;
}

function renderPrompt() {
  const box = document.getElementById('prompt');
  box.replaceChildren();
  if (SEAT === 0) {
    box.append(el('div', { class: 'hint' }, 'The true-state view is read-only. It shows hidden permanents (dashed outline) and both hands.'));
    return;
  }
  if (ui.error) box.append(el('div', { style: 'color: var(--bad)' }, `Rejected: ${ui.error}`));

  // Decisions first.
  const decisions = ui.legal.filter(c => ['SplitDamage', 'Redirect', 'CancelRedirect'].includes(c.kind));
  if (decisions.length) {
    box.append(el('h2', {}, 'Decision'));
    box.append(el('div', { class: 'row' }, decisions.map(c => el('button', { class: 'choice', onclick: () => submit(c.index) }, c.label))));
    return;
  }

  if (ui.wizard) {
    renderWizard(box);
    return;
  }

  const pass = ui.legal.find(c => c.kind === 'Pass');
  const sel = ui.selection;
  if (sel && (sel.type === 'perm' || sel.type === 'card')) {
    const groups = abilityGroups(sel.id);
    const perm = sel.type === 'perm' ? permById(sel.id) : null;
    box.append(el('h2', {}, perm ? `Abilities of ${perm.name}` : 'Cast'));
    if (groups.size === 0) box.append(el('div', { class: 'hint' }, ui.view.yourPriority ? 'Nothing it can do right now.' : 'You don\'t have priority.'));
    const shown = new Set();
    for (const [ability, cmds] of groups) {
      shown.add(ability);
      const first = cmds[0];
      const name = first.label.split(' → ')[0];
      box.append(el('button', { class: 'ability', onclick: () => startWizard(sel.id, ability) },
        el('span', {}, name), el('span', { class: 'meta' }, `${first.cost ?? ''} · ${first.speed ?? ''}${cmds.length > 1 ? ` · ${cmds.length} options` : ''}`)));
    }
    if (perm && perm.controller === ME) {
      for (const a of perm.abilities.filter(a => !a.trigger && !shown.has(a.id))) {
        box.append(el('button', { class: 'ability', disabled: true },
          el('span', {}, a.name), el('span', { class: 'meta' }, `${a.cost} · ${a.speed} · not now`)));
      }
    }
  } else if (ui.view.yourPriority) {
    box.append(el('div', { class: 'hint' }, 'Select one of your permanents or a card in your hand to see what it can do.'));
  } else {
    box.append(el('div', { class: 'hint' }, ui.view.priorityHolder ? `Waiting for Champion ${ui.view.priorityHolder}. You can still inspect anything.` : 'Waiting…'));
  }

  // Quick Defend shortcuts for attacks in Pending.
  const defends = activations().filter(c => c.ability === 'defend');
  if (defends.length) {
    box.append(el('h2', { style: 'margin-top:10px' }, 'Defend'));
    box.append(el('div', { class: 'row' }, defends.map(c => el('button', { class: 'choice', onclick: () => submit(c.index) }, `${permById(c.source)?.name ?? '#' + c.source} defends (${c.cost})`))));
  }

  if (pass) box.append(el('div', { class: 'row', style: 'margin-top:10px' }, el('button', { class: 'primary', onclick: () => submit(pass.index) }, pass.label)));
}

function renderWizard(box) {
  const w = ui.wizard;
  const s = wizardStep();
  const any = matching()[0];
  if (!s || !any) { ui.wizard = null; return; }
  const title = any.label.split(' → ')[0];
  box.append(el('h2', {}, `${title} — ${any.cost ?? ''} · ${any.speed ?? ''}`));
  box.append(el('div', { class: 'hint' }, 'Pay → target → Pending. Nothing is paid until you confirm; abort any time.'));
  w.chosen.forEach((k, i) => {
    const label = matching()[0].targets[i].map(t => t.label).join(' + ') || 'nothing';
    box.append(el('div', {}, `Target ${i + 1}: ${label}`));
  });
  if (s.done) {
    box.append(el('div', { class: 'row' },
      el('button', { class: 'primary', onclick: () => submit(s.command.index) }, `Confirm: ${s.command.label}`),
      el('button', { class: 'danger', onclick: () => { ui.wizard = null; render(); } }, 'Abort')));
    return;
  }
  if (s.payment) {
    box.append(el('div', { class: 'step' }, 'Choose how to pay:'));
    box.append(el('div', { class: 'row' }, s.options.map(p =>
      el('button', { class: 'choice', onclick: () => { w.payment = p; render(); } }, p))));
    box.append(el('div', { class: 'row' }, el('button', { class: 'danger', onclick: () => { ui.wizard = null; render(); } }, 'Abort')));
    return;
  }
  box.append(el('div', { class: 'step' }, `Choose target ${s.step + 1} of ${s.total} (highlighted on the board, or below):`));
  let options = s.options;
  if (w.focusHex) options = options.filter(o => o.list.length === 1 && o.list[0].hex && hexKey(o.list[0].hex) === w.focusHex);
  box.append(el('div', { class: 'row' }, options.map(o =>
    el('button', { class: 'choice', onclick: () => choose(o.key) }, o.list.map(t => t.label).join(' + ') || 'nothing'))));
  box.append(el('div', { class: 'row' }, el('button', { class: 'danger', onclick: () => { ui.wizard = null; render(); } }, 'Abort')));
}

function renderDetails() {
  const box = document.getElementById('details');
  box.replaceChildren();
  const sel = ui.selection;
  if (!sel) { box.append(el('div', { class: 'hint' }, 'Nothing selected.')); return; }
  if (sel.type === 'hex') {
    const h = ui.view.hexes.find(x => x.coord.q === sel.q && x.coord.r === sel.r);
    if (!h) return;
    box.append(el('h2', {}, `Terrain (${h.coord.q},${h.coord.r})`));
    box.append(el('pre', { style: 'white-space:pre-wrap;margin:0' }, hexTooltip(h)));
    const here = ui.view.permanents.filter(p => p.carrier == null && p.hex.q === h.coord.q && p.hex.r === h.coord.r);
    for (const slice of ['Sky', 'Ground', 'Root']) {
      const list = here.filter(p => p.slice === slice);
      box.append(el('div', {}, `${slice}: ${list.map(p => `${p.name} (${p.kind}, ${p.controller})`).join(', ') || '—'}`));
    }
    const info = cardInfo(h.terrainCard);
    if (info?.text) box.append(el('div', { class: 'hint' }, info.text));
    return;
  }
  if (sel.type === 'card') {
    const card = myHand().find(c => c.id === sel.id);
    const info = card && cardInfo(card.card);
    if (info) box.append(cardDetails(info));
    return;
  }
  const p = permById(sel.id);
  if (!p) return;
  box.append(el('h2', {}, `${p.name} #${p.id}`));
  const dl = el('dl', {},
    el('dt', {}, 'Kind'), el('dd', {}, `${p.kind}${p.inRoot ? ' (hidden Root)' : ''}`),
    el('dt', {}, 'Controller'), el('dd', {}, p.controller === 'Neutral' ? 'Neutral' : `Champion ${p.controller}`),
    el('dt', {}, 'Location'), el('dd', {}, p.carrier != null ? `carried by #${p.carrier}` : `(${p.hex.q},${p.hex.r}) ${p.slice}`));
  box.append(dl);
  if (['Creature', 'Companion', 'Champion'].includes(p.kind)) box.append(el('div', { class: 'stats' }, `⚔ ${p.attack}   ♥ ${p.life}/${p.maxLife}   AP ${p.ap}/${p.maxAp}`));
  else if (p.kind === 'Structure') box.append(el('div', { class: 'stats' }, `♥ ${p.life}/${p.maxLife}   AP ${p.ap}/${p.maxAp}`));
  const extra = [];
  if (p.keywords.length) extra.push(p.keywords.join(', '));
  if (p.locked) extra.push('done for this turn (~)');
  if (p.usedThisCycle.length) extra.push(`used this cycle: ${p.usedThisCycle.join(', ')}`);
  if (p.behavior) extra.push(`Behavior: ${p.behavior}`);
  if (p.kind === 'Champion' || p.kind === 'Companion') extra.push(p.rootConnected ? 'network: connected (realm lock / doubled costs)' : 'network: disconnected');
  if (p.pool) extra.push(`mana pool: ${fmtPool(p.pool)}`);
  const carried = ui.view.permanents.filter(q => q.carrier === p.id);
  if (carried.length) extra.push(`carries: ${carried.map(c => c.name).join(', ')}`);
  for (const e of extra) box.append(el('div', {}, e));
  if (p.abilities.length) {
    box.append(el('div', { class: 'abilities' }, p.abilities.map(a =>
      el('div', {}, el('b', {}, a.name), ' ', el('span', { class: 'meta' }, `${a.trigger ? 'when ' + a.trigger : a.cost} · ${a.speed}${a.physical ? ' · physical' : ''}`), a.text ? ` — ${a.text}` : ''))));
  }
  const info = cardInfo(p.card);
  if (info?.text) box.append(el('div', { class: 'hint' }, info.text));
}

function cardDetails(info) {
  const box = el('div', {});
  box.append(el('h2', {}, info.name));
  box.append(el('div', {}, `${info.type}${info.subtypes.length ? ' — ' + info.subtypes.join(' ') : ''} · cost ${info.cost} · ${info.speed}`));
  if (['Creature', 'Companion'].includes(info.type)) box.append(el('div', { class: 'stats' }, `⚔ ${info.attack}   ♥ ${info.life}   AP ${info.ap}`));
  if (['Structure'].includes(info.type)) box.append(el('div', { class: 'stats' }, `♥ ${info.life}   AP ${info.ap}`));
  if (info.keywords.length) box.append(el('div', {}, info.keywords.join(', ')));
  if (info.instructions.length) box.append(el('div', {}, info.instructions.join(' ')));
  for (const a of info.abilities.filter(a => !['move', 'attack', 'defend', 'equip', 'unequip', 'bond', 'draw', 'collapse', 'ascend', 'descend'].includes(a.id)))
    box.append(el('div', {}, el('b', {}, a.name), ` (${a.trigger ? 'when ' + a.trigger : a.cost}) ${a.text}`));
  if (info.text) box.append(el('div', { class: 'hint' }, info.text));
  return box;
}

function fmtPool(pool) {
  const entries = Object.entries(pool);
  return entries.length ? entries.map(([e, n]) => `${n} ${e}`).join(', ') : 'empty';
}

function myHand() {
  if (!ui.view || !ME) return [];
  return ui.view.players.find(p => p.player === ME)?.hand ?? [];
}

function cardEl(cardId, objId, opts = {}) {
  const info = cardInfo(cardId);
  const playable = objId != null && activations().some(c => c.source === objId);
  const selected = ui.selection?.type === 'card' && ui.selection.id === objId;
  const e = el('div', { class: `card${playable ? ' playable' : ''}${selected ? ' selected' : ''}`, title: info?.text ?? '' },
    el('div', { class: 'name' }, info?.name ?? cardId),
    el('div', { class: 'cost' }, `${info?.cost ?? ''} · ${info?.speed ?? ''}`),
    el('div', { class: 'type' }, info?.type ?? ''),
    info && ['Creature', 'Companion'].includes(info.type) ? el('div', {}, `${info.attack}/${info.life} · AP ${info.ap}`) : null,
    info?.instructions?.length ? el('div', { class: 'text' }, info.instructions.join(' ')) : null,
    info?.keywords?.length ? el('div', { class: 'text' }, info.keywords.join(', ')) : null);
  if (opts.clickable) e.addEventListener('click', () => { ui.selection = { type: 'card', id: objId }; ui.wizard = null; render(); });
  return e;
}

function renderHand() {
  const box = document.getElementById('hand');
  box.replaceChildren();
  if (SEAT === 0) {
    for (const pl of ui.view.players) {
      box.append(el('h2', {}, `Champion ${pl.player}'s hand (${pl.handCount})`));
      box.append(el('div', { class: 'cards' }, (pl.hand ?? []).map(c => cardEl(c.card, null))));
    }
    return;
  }
  const hand = myHand();
  box.append(el('h2', {}, `Your hand (${hand.length})`));
  box.append(el('div', { class: 'cards' }, hand.map(c => cardEl(c.card, c.id, { clickable: true }))));
}

function traceEl(t, opts = {}) {
  const cls = ['trace', 'ctrl-' + t.controller];
  if (opts.top) cls.push('top');
  if (opts.resolving) cls.push('resolving');
  if (t.hidden) cls.push('hidden');
  const e = el('div', { class: cls.join(' ') },
    el('div', { class: 'title' }, t.text),
    el('div', { class: 'tags' },
      el('span', { class: 'tag' }, t.kind),
      el('span', { class: 'tag' + (t.speed === 'Instant' ? ' instant' : '') }, t.speed),
      t.physical ? el('span', { class: 'tag physical' }, 'physical') : null,
      t.controller ? el('span', { class: 'tag' }, t.controller === 'Neutral' ? 'Neutral' : 'Champion ' + t.controller) : null,
      t.paidCost ? el('span', { class: 'tag' }, 'paid ' + t.paidCost) : null,
      t.fadesAtRound ? el('span', { class: 'tag' }, 'fades r' + t.fadesAtRound) : null));
  if (t.attack) {
    const defenders = t.attack.defenders.map(d => permById(d)?.name ?? '#' + d).join(', ') || 'none yet';
    e.append(el('div', { class: 'attack' }, `⚔ (${t.attack.hex.q},${t.attack.hex.r}) ${t.attack.slice} vs ${t.attack.entity === 'Neutral' ? 'Neutral' : 'Champion ' + t.attack.entity} — defenders: ${defenders}`));
  }
  if (t.targets.length) e.append(el('ul', {}, t.targets.map(x => el('li', {}, x))));
  if (t.instructions.length) e.append(el('ul', {}, t.instructions.map(x => el('li', {}, x))));
  if (t.notes.length) e.append(el('div', { class: 'notes' }, t.notes.join(' · ')));
  if (SEAT !== 0) {
    for (const c of activations().filter(c => c.ability === 'defend' && c.targets[0]?.[0]?.object === t.id))
      e.append(el('button', { onclick: () => submit(c.index) }, `Defend with ${permById(c.source)?.name ?? '#' + c.source} (${c.cost})`));
  }
  if (t.actingPermanent != null) e.addEventListener('mouseenter', () => highlightPerm(t.actingPermanent, true));
  if (t.actingPermanent != null) e.addEventListener('mouseleave', () => highlightPerm(t.actingPermanent, false));
  return e;
}

function highlightPerm(id, on) {
  for (const g of document.querySelectorAll('.token')) {
    if (g.querySelector('title')?.textContent.includes(`#${id} `)) g.classList.toggle('candidate', on);
  }
}

function renderAether() {
  const box = document.getElementById('aether');
  box.replaceChildren();
  const v = ui.view;
  const past = el('div', { class: 'aether-zone' }, el('div', { class: 'zone-label' }, 'Past'),
    v.past.length ? v.past.slice(-8).map(t => traceEl(t)) : el('div', { class: 'empty' }, '—'));
  const pendingTop = [...v.pending].reverse();
  const pending = el('div', { class: 'aether-zone' }, el('div', { class: 'zone-label' }, 'Pending (top first)'),
    v.resolving ? traceEl(v.resolving, { resolving: true }) : null,
    pendingTop.length ? pendingTop.map((t, i) => traceEl(t, { top: i === 0 })) : (v.resolving ? null : el('div', { class: 'empty' }, 'empty')));
  box.append(past, el('div', { class: 'now-line', title: 'Now' }), pending);
}

function renderMind() {
  const box = document.getElementById('mind');
  box.replaceChildren();
  for (const pl of ui.view.players) {
    const names = (list) => list ? list.map(c => cardInfo(c.card)?.name ?? c.card).join(', ') || '—' : 'hidden';
    box.append(el('div', { class: 'player' },
      el('b', {}, `Champion ${pl.player}`), pl.pool ? el('span', { class: 'pool' }, `  ·  mana: ${fmtPool(pl.pool)}`) : null,
      el('div', { class: 'zones' },
        el('div', { class: 'zone' }, el('div', { class: 'label' }, `Hand (${pl.handCount})`), names(pl.hand)),
        el('div', { class: 'zone', title: 'You know which cards remain, never their order (D71).' }, el('div', { class: 'label' }, `Library (${pl.libraryCount})`), names(pl.library)),
        el('div', { class: 'zone' }, el('div', { class: 'label' }, `Discard (${pl.discardCount})`), names(pl.discard)))));
  }
}

function renderLog() {
  const box = document.getElementById('log');
  const atBottom = box.scrollTop + box.clientHeight >= box.scrollHeight - 20;
  box.replaceChildren(...ui.view.log.map(l => el('div', { class: l.text.startsWith('—') ? 'round' : '' }, l.text)));
  if (atBottom) box.scrollTop = box.scrollHeight;
}

// ---------------------------------------------------------------------------- wiring

function wireSeatPage() {
  // ?layer=root and ?select=<permanent id> open the page in that state (handy for links and headless screenshots).
  const params = new URLSearchParams(location.search);
  if (params.get('layer') === 'root') {
    ui.layer = 'root';
    for (const x of document.querySelectorAll('.view-toggle button')) x.classList.toggle('active', x.dataset.layer === 'root');
  }
  if (params.get('select')) ui.selection = { type: 'perm', id: Number(params.get('select')) };
  for (const b of document.querySelectorAll('.view-toggle button')) {
    b.addEventListener('click', () => {
      ui.layer = b.dataset.layer;
      for (const x of document.querySelectorAll('.view-toggle button')) x.classList.toggle('active', x === b);
      render();
    });
  }
  document.getElementById('autopass')?.addEventListener('change', async (e) => {
    await api('/api/autopass', { method: 'POST', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify({ seat: SEAT, enabled: e.target.checked }) });
    await refresh();
  });
  document.addEventListener('keydown', (e) => {
    if (e.key === 'Escape' && ui.wizard) { ui.wizard = null; render(); }
  });
  refresh();
  setInterval(refresh, 1500);
}

async function wireHub() {
  const select = document.getElementById('scenario-select');
  const status = document.getElementById('hub-status');
  const names = await api('/api/scenarios');
  select.replaceChildren(...names.map(n => el('option', { value: n }, n)));
  document.getElementById('load-button').addEventListener('click', async () => {
    try {
      await api(`/api/scenarios/${encodeURIComponent(select.value)}/load`, { method: 'POST' });
      status.textContent = `Loaded "${select.value}". Open the seat tabs.`;
    } catch (err) {
      status.textContent = String(err.message || err);
    }
  });
}

if (SEAT === null) wireHub();
else wireSeatPage();
