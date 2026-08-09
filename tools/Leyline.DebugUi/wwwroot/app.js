const HEX_SIZE = 26;
const HEX_DIRS = [[1, 0], [1, -1], [0, -1], [-1, 0], [-1, 1], [0, 1]];
const PHASE_SEQUENCE = ['Beginning', 'Action', 'End'];
const PHASE_LABELS = { Beginning: 'Start', Action: 'Main', End: 'End' };

// Forced/pending decisions and turn control — never tied to a selected thing, so they stay
// visible no matter what (if anything) is selected.
const GLOBAL_KINDS = new Set([
  'EndPhaseCommand', 'PassPriorityCommand',
  'DeclareDefendersCommand', 'AssignDamageCommand', 'ChooseUndefendedTargetCommand',
]);

const cache = { view: { 1: null, 2: null }, trueState: null, legal: { 1: null, 2: null }, cards: [] };

function cardById(id) {
  return cache.cards.find(c => c.id.value === id);
}

/// Plain-text printed-card summary: creature stats, or the Rite's hardcoded damage/heal effect
/// (RiteEffectIds — the M1 placeholder for the real card-effect system, see CardDefinition's
/// doc comment). Champion is never drawn/cast (design-champions.md), so no case needed here.
function describeCardEffect(def) {
  if (!def) return '';
  if (def.type === 'Creature') return `Creature ${def.attack}/${def.life}/${def.maxAp}`;
  if (def.type === 'Rite') {
    if (def.effectId === 'rite.damage') return `Deal ${def.effectAmount} damage`;
    if (def.effectId === 'rite.heal') return `Heal ${def.effectAmount}`;
    return def.effectId ? `${def.effectId} (${def.effectAmount})` : 'No effect';
  }
  return def.type;
}

// The selected "thing" per panel: { type: 'actor', id: ActorId } | { type: 'card', id: CardDefinitionId } | null.
const selection = { 1: null, 2: null, true: null };

async function api(path, options) {
  const res = await fetch(path, options);
  const text = await res.text();
  if (!res.ok) throw new Error(`${path} -> ${res.status}: ${text || res.statusText}`);
  return text ? JSON.parse(text) : null;
}

function hexCenter(q, r) {
  const x = HEX_SIZE * (Math.sqrt(3) * q + (Math.sqrt(3) / 2) * r);
  const y = HEX_SIZE * (1.5 * r);
  return { x, y };
}

function hexPoints(cx, cy, size) {
  const pts = [];
  for (let i = 0; i < 6; i++) {
    const angle = (Math.PI / 180) * (60 * i - 30); // pointy-top
    pts.push(`${cx + size * Math.cos(angle)},${cy + size * Math.sin(angle)}`);
  }
  return pts.join(' ');
}

const SVG_NS = 'http://www.w3.org/2000/svg';

function svgEl(tag, attrs) {
  const el = document.createElementNS(SVG_NS, tag);
  for (const [k, v] of Object.entries(attrs)) el.setAttribute(k, v);
  return el;
}

/// Shared renderer: View and DebugStateDto both expose {cells:[{coord,terrain,ground,below,above}], actors:[{id,owner,name,kind,attack,life,maxLife,currentAp,maxAp,abilityIds,position,layer}]}.
/// draggableSeat (1, 2, or null for the read-only true-state panel) enables drag-to-move/
/// attack on that seat's own actors; panelKey (1, 2, or 'true') drives click-to-select
/// regardless of ownership; sel is that panel's current selection (see `selection`'s doc comment).
function renderBoard(svg, cells, actors, draggableSeat, panelKey, sel) {
  svg.innerHTML = '';
  svg._cells = cells; // stashed for handleDrop's client-side pathfinding
  if (!cells || cells.length === 0) return;

  const selectedActorId = sel?.type === 'actor' ? sel.id : null;

  const centers = cells.map(c => hexCenter(c.coord.q, c.coord.r));
  const xs = centers.map(p => p.x);
  const ys = centers.map(p => p.y);
  const pad = HEX_SIZE * 1.3;
  const minX = Math.min(...xs) - pad, maxX = Math.max(...xs) + pad;
  const minY = Math.min(...ys) - pad, maxY = Math.max(...ys) + pad;
  svg.setAttribute('viewBox', `${minX} ${minY} ${maxX - minX} ${maxY - minY}`);

  const actorsById = new Map(actors.map(a => [a.id.value, a]));

  cells.forEach((cell, i) => {
    const { x, y } = centers[i];
    const networkClass = cell.networkOwner != null
      ? ` network-${cell.networkOwner.value}${cell.networkProducing ? '' : ' network-paused'}`
      : '';
    const hex = svgEl('polygon', {
      points: hexPoints(x, y, HEX_SIZE - 1),
      class: 'hex' + networkClass,
      'data-q': cell.coord.q,
      'data-r': cell.coord.r,
    });
    const networkNote = cell.networkOwner != null
      ? ` — P${cell.networkOwner.value} network (${cell.networkProducing ? 'producing' : 'paused'})`
      : '';
    hex.appendChild(svgEl('title', {})).textContent = `(${cell.coord.q},${cell.coord.r})${networkNote}`;
    hex.addEventListener('pointerdown', () => { dragState = { kind: 'hex', panelKey }; });
    svg.appendChild(hex);

    const occupants = [...cell.ground, ...cell.below, ...cell.above];
    occupants.forEach((idRef, oi) => {
      const actor = actorsById.get(idRef.value);
      if (!actor) return;
      const offset = (oi - (occupants.length - 1) / 2) * 18;
      drawActor(svg, actor, x + offset, y, draggableSeat, panelKey, actor.id.value === selectedActorId);
    });
  });
}

function drawActor(svg, actor, x, y, draggableSeat, panelKey, isSelected) {
  const ownerClass = actor.owner.value === 1 ? 'owner-1' : actor.owner.value === 2 ? 'owner-2' : 'owner-unknown';
  const layerClass = actor.layer === 'Below' ? ' layer-below' : '';
  const draggable = draggableSeat != null && actor.owner.value === draggableSeat;
  const circle = svgEl('circle', {
    cx: x, cy: y, r: 12,
    class: `actor ${ownerClass}${layerClass}${draggable ? ' draggable' : ''}${isSelected ? ' selected' : ''}`,
    'data-q': actor.position.q,
    'data-r': actor.position.r,
    'data-actor-id': actor.id.value,
  });
  circle.appendChild(svgEl('title', {})).textContent =
    `${actor.name} (${actor.kind}) P${actor.owner.value} — attack=${actor.attack}, life=${actor.life}/${actor.maxLife}, ap=${actor.currentAp}/${actor.maxAp}, layer=${actor.layer}`;
  circle.addEventListener('pointerdown', evt => startDrag(evt, draggableSeat, actor.id.value, actor.position.q, actor.position.r, circle, panelKey, draggable));
  svg.appendChild(circle);

  // Always the three D10 stats, in the game's own Attack/Life/AP order; current AP (not max)
  // since that's the actionable number during play — max values are in the tooltip above.
  const label = svgEl('text', { x, y: y + 3, class: 'actor-label' });
  label.textContent = `${actor.attack}/${actor.life}/${actor.currentAp}`;
  svg.appendChild(label);
}

// --- Selection + drag-and-drop. A pointerdown on a hex starts a "click to deselect"; a
// pointerdown on any token (owned or not) starts a "click to select" that upgrades to a real
// move/attack drag if it ends on a different, owned-and-draggable hex. One state machine
// resolves all three interactions (click-select, click-deselect, drag-move/attack) from a
// single global pointerup handler. ---

let dragState = null;

function startDrag(evt, seat, actorId, q, r, el, panelKey, draggable) {
  evt.preventDefault();
  dragState = { kind: 'actor', seat, actorId, q, r, panelKey, draggable };
  if (draggable) el.classList.add('dragging');
}

document.addEventListener('pointerup', evt => {
  if (!dragState) return;
  const ds = dragState;
  dragState = null;
  document.querySelectorAll('.actor.dragging').forEach(el => el.classList.remove('dragging'));

  if (ds.kind === 'hex') {
    selection[ds.panelKey] = null;
    renderAllPanels();
    return;
  }

  const dropTarget = document.elementFromPoint(evt.clientX, evt.clientY)?.closest('[data-q]');
  const toQ = dropTarget ? Number(dropTarget.getAttribute('data-q')) : null;
  const toR = dropTarget ? Number(dropTarget.getAttribute('data-r')) : null;
  const droppedOnStart = dropTarget && toQ === ds.q && toR === ds.r;

  if (!dropTarget || droppedOnStart || !ds.draggable) {
    // A plain click (no target, or released where it started), or a non-owned token dragged
    // elsewhere — either way nothing can move/attack, so just select it.
    selection[ds.panelKey] = { type: 'actor', id: ds.actorId };
    renderAllPanels();
    return;
  }

  selection[ds.panelKey] = { type: 'actor', id: ds.actorId }; // moved via drag -> also select it
  handleDrop(ds.seat, ds.actorId, ds.q, ds.r, toQ, toR);
});

/// BFS over the cells that exist on the board (occupancy/cost aren't modeled here — each hop
/// is re-validated against the real legal-move list as the walk executes).
function findPath(cells, from, to) {
  const key = (q, r) => `${q},${r}`;
  const present = new Set(cells.map(c => key(c.coord.q, c.coord.r)));
  if (!present.has(key(to.q, to.r))) return null;

  const start = key(from.q, from.r);
  const cameFrom = new Map([[start, null]]);
  const queue = [from];
  while (queue.length) {
    const cur = queue.shift();
    if (cur.q === to.q && cur.r === to.r) {
      const path = [];
      let k = key(cur.q, cur.r);
      while (k !== start) {
        const [q, r] = k.split(',').map(Number);
        path.push({ q, r });
        k = cameFrom.get(k);
      }
      return path.reverse();
    }
    for (const [dq, dr] of HEX_DIRS) {
      const nq = cur.q + dq, nr = cur.r + dr, nk = key(nq, nr);
      if (present.has(nk) && !cameFrom.has(nk)) {
        cameFrom.set(nk, key(cur.q, cur.r));
        queue.push({ q: nq, r: nr });
      }
    }
  }
  return null;
}

/// Walks the planned path one hop at a time, submitting the real MoveCommand for each hop
/// (re-checked against fresh legal commands every step — the path is only a route, not a
/// legality guarantee). If a hop matches a legal Attack instead of a Move — i.e. we've arrived
/// adjacent to a drop target occupied by an enemy — attack and stop, satisfying both "drag to
/// walk" and "drag onto an enemy to attack" with one loop. Stops cleanly (partial move) if a
/// hop is blocked, out of AP, or otherwise illegal.
async function handleDrop(seat, actorId, fromQ, fromR, toQ, toR) {
  const status = document.getElementById('status-line');
  const cells = document.querySelector(`#panel-p${seat} svg.board`)?._cells;
  if (!cells) return;

  const path = findPath(cells, { q: fromQ, r: fromR }, { q: toQ, r: toR });
  if (!path) {
    status.textContent = `No path from (${fromQ},${fromR}) to (${toQ},${toR}).`;
    return;
  }

  let steps = 0;
  for (const hop of path) {
    const legal = await api(`/api/legal/${seat}`);
    const move = legal.find(c => c.kind === 'Move' && c.actorId === actorId && c.targetHex?.q === hop.q && c.targetHex?.r === hop.r);
    const attack = legal.find(c => c.kind === 'Attack' && c.actorId === actorId && c.targetHex?.q === hop.q && c.targetHex?.r === hop.r);

    // Attack takes priority over Move: ground layers allow shared occupancy among allies
    // (capacity 3, D12), so a hex could in principle be both a legal move and attack target —
    // dropping onto an enemy must attack, never walk in.
    if (attack) {
      await submitAction(seat, attack.index, { skipRefresh: true });
      steps++;
      break; // combat declared — the rest of the sequence (defend/assign/etc.) stays button-driven
    } else if (move) {
      await submitAction(seat, move.index, { skipRefresh: true });
      steps++;
    } else {
      break; // blocked, out of AP, or otherwise illegal — stop where we are
    }
  }

  if (steps < path.length)
    status.textContent = `Moved ${steps}/${path.length} hex(es) toward (${toQ},${toR}) — stopped (no further legal move).`;
  await refreshAll();
}

/// Once a unit is selected: Move/Attack filtered to that exact unit (their ActorId already
/// only ever matches this seat's own units, since `legal` is itself seat-scoped). Bond/Draw/
/// Collapse are PlayerId-scoped commands with no ActorId to match on, so they're attributed to
/// "the selected Champion" instead — but only when it's this seat's *own* Champion selected, not
/// just any Champion (selecting the enemy's to inspect it must not surface your own actions).
/// Once a card is selected: only CastCreature/CastRite for that exact card. Nothing selected:
/// only the global, not-tied-to-one-thing commands (end phase, forced combat decisions).
function filterForSelection(legal, sel, actors, seat) {
  if (!legal) return [];
  if (!sel)
    return legal.filter(c => GLOBAL_KINDS.has(c.kind));

  if (sel.type === 'card') {
    return legal.filter(c => GLOBAL_KINDS.has(c.kind) || ((c.kind === 'CastCreature' || c.kind === 'CastRite') && c.card === sel.id));
  }

  const selectedActor = actors?.find(a => a.id.value === sel.id);
  const isOwnChampion = selectedActor?.kind === 'Champion' && selectedActor.owner.value === seat;
  return legal.filter(c => {
    if (GLOBAL_KINDS.has(c.kind)) return true;
    if (c.kind === 'Move' || c.kind === 'Attack') return c.actorId === sel.id;
    if (c.kind === 'Bond' || c.kind === 'Draw' || c.kind === 'Collapse') return isOwnChampion;
    return false;
  });
}

function renderSelectionInfo(container, actors, sel, mana, panelKey) {
  if (!container) return;
  if (!sel) {
    container.innerHTML = '<span class="empty">Nothing selected — click a token or a card.</span>';
    return;
  }
  if (sel.type === 'card') {
    const def = cardById(sel.id);
    if (!def) {
      container.innerHTML = `<strong>${sel.id}</strong> <span class="abilities">(card in hand)</span>`;
      return;
    }
    // Step 2 of the cast process (select -> pay -> target -> cast): show the cost and what it
    // does before any target is chosen. Casting itself still pays the cost atomically — mana
    // isn't spent until a target below is actually picked.
    const ownMana = panelKey !== 'true' ? mana?.find(m => m.player.value === panelKey)?.mana : null;
    const costNote = ownMana == null
      ? `cost: ${def.manaCost} mana`
      : ownMana >= def.manaCost
        ? `cost: ${def.manaCost} mana (you have ${ownMana})`
        : `cost: ${def.manaCost} mana (you have ${ownMana} — not enough)`;
    container.innerHTML =
      `<strong>${def.name}</strong> (${def.type}) — ${costNote}<br>` +
      `<span class="abilities">${describeCardEffect(def)} — pick a target below to cast</span>`;
    return;
  }
  const actor = actors?.find(a => a.id.value === sel.id);
  if (!actor) {
    container.innerHTML = '<span class="empty">Nothing selected — click a token or a card.</span>';
    return;
  }
  container.innerHTML =
    `<strong>${actor.name}</strong> (${actor.kind}, P${actor.owner.value}) — ` +
    `attack=${actor.attack} life=${actor.life}/${actor.maxLife} ap=${actor.currentAp}/${actor.maxAp}<br>` +
    `<span class="abilities">abilities: ${actor.abilityIds.join(', ') || 'none'}</span>`;
}

/// Step 3 of the cast process (select -> pay -> target -> cast): once a card is selected,
/// filterForSelection has already narrowed `commands` to just that card's legal CastCreature/
/// CastRite (card, target) pairs — describeCastTarget relabels each with what it actually hits
/// instead of the raw command dump, so the button list reads as "pick a target."
function describeCastTarget(cmd, actors) {
  if (cmd.kind === 'CastCreature') return `Summon at (${cmd.targetHex.q},${cmd.targetHex.r})`;
  if (cmd.kind === 'CastRite') {
    const target = actors?.find(a => a.id.value === cmd.targetActorId);
    return target ? `Target: ${target.name} (P${target.owner.value})` : cmd.label;
  }
  return cmd.label;
}

/// What hovering a given command's button should highlight on the board: the hex it targets
/// (Move/Attack/Bond/CastCreature all carry targetHex), or the actor it targets (CastRite's
/// targetActorId — no hex of its own). Everything else (Draw, Collapse, EndPhase, Pass, combat
/// decisions) has no single resolved target, so no highlight.
function resolveHoverTarget(cmd) {
  if (cmd.targetHex) return { type: 'hex', q: cmd.targetHex.q, r: cmd.targetHex.r };
  if (cmd.kind === 'CastRite' && cmd.targetActorId != null) return { type: 'actor', id: cmd.targetActorId };
  return null;
}

function setBoardHighlight(svg, target, on) {
  if (!svg || !target) return;
  const el = target.type === 'hex'
    ? svg.querySelector(`polygon.hex[data-q="${target.q}"][data-r="${target.r}"]`)
    : svg.querySelector(`circle.actor[data-actor-id="${target.id}"]`);
  el?.classList.toggle('hover-target', on);
}

function renderActions(container, seat, commands, onPick, actors, svg) {
  container.innerHTML = '';
  if (!commands || commands.length === 0) {
    const empty = document.createElement('div');
    empty.className = 'empty';
    empty.textContent = 'No legal actions right now.';
    container.appendChild(empty);
    return;
  }
  for (const cmd of commands) {
    const btn = document.createElement('button');
    btn.textContent = (cmd.kind === 'CastCreature' || cmd.kind === 'CastRite') ? describeCastTarget(cmd, actors) : cmd.label;
    btn.onclick = () => onPick(seat, cmd.index);
    const target = resolveHoverTarget(cmd);
    if (target) {
      btn.addEventListener('mouseenter', () => setBoardHighlight(svg, target, true));
      btn.addEventListener('mouseleave', () => setBoardHighlight(svg, target, false));
    }
    container.appendChild(btn);
  }
}

function cardChipText(cardId) {
  const def = cardById(cardId);
  if (!def) return cardId;
  return `${def.name} (${def.manaCost} mana) — ${describeCardEffect(def)}`;
}

function renderHand(container, hands, seat, sel) {
  container.innerHTML = '';
  const own = hands.find(h => h.player.value === seat);
  if (!own) return;

  const label = document.createElement('div');
  label.textContent = `Hand (${own.count}):`;
  container.appendChild(label);

  for (const card of own.cards ?? []) {
    const chip = document.createElement('span');
    const isSelected = sel?.type === 'card' && sel.id === card.value;
    chip.className = 'card' + (isSelected ? ' selected' : '');
    chip.textContent = cardChipText(card.value);
    chip.onclick = () => {
      selection[seat] = { type: 'card', id: card.value };
      renderAllPanels();
    };
    container.appendChild(chip);
  }
}

/// The true-state panel reveals everything (the deliberate exception, see DebugStateDto's own
/// doc comment) — both players' hands as chips, plus a compact library line each.
function renderTrueHands(container, zones) {
  container.innerHTML = '';
  for (const z of zones) {
    const row = document.createElement('div');
    row.className = 'hand-row';

    const label = document.createElement('div');
    label.className = 'library-line';
    label.textContent = `P${z.player.value} hand (${z.hand.length}) — library (${z.library.length}): ${z.library.map(c => c.value).join(', ') || '—'}`;
    row.appendChild(label);

    for (const card of z.hand) {
      const chip = document.createElement('span');
      chip.className = `card owner-${z.player.value}`;
      chip.textContent = cardChipText(card.value);
      row.appendChild(chip);
    }
    container.appendChild(row);
  }
}

/// Mana used to live only in the tiny header status line, easy to miss. Each panel now gets its
/// own prominent readout: the P1/P2 panels show that seat's own mana (D21 — a snapshot taken
/// once per Beginning phase, not live; see RefreshManaEffect), the true-state panel shows both.
function renderMana(container, manaList, panelKey) {
  if (!container) return;
  if (panelKey === 'true') {
    container.textContent = manaList.map(m => `P${m.player.value} mana: ${m.mana}`).join('    ');
    return;
  }
  const own = manaList.find(m => m.player.value === panelKey)?.mana ?? 0;
  container.textContent = `Mana: ${own}`;
}

function renderPhaseStepper(currentPhase) {
  const container = document.getElementById('phase-stepper');
  container.innerHTML = '';
  for (const phase of PHASE_SEQUENCE) {
    const step = document.createElement('span');
    step.className = 'phase-step' + (phase === currentPhase ? ' current' : '');
    step.textContent = PHASE_LABELS[phase] ?? phase;
    container.appendChild(step);
  }
}

function statusLine(view) {
  const parts = [`Turn ${view.turnNumber}`, `Active P${view.activePlayer.value}`];
  if (view.winner) parts.push(`WINNER: P${view.winner.value}`);
  else if (view.awaitingYourPriority) parts.push('awaiting your priority');
  // D18: live mana balance is hidden from every observer but its own — a redacted View reports
  // null for every other player, so only the observer's own entry ever has a number to show.
  const mana = view.mana.filter(m => m.mana != null).map(m => `P${m.player.value} mana=${m.mana}`).join(', ');
  return parts.join(' | ') + (mana ? ' | ' + mana : '');
}

function renderPanel(panelKey) {
  const isTrue = panelKey === 'true';
  const data = isTrue ? cache.trueState : cache.view[panelKey];
  if (!data) return;

  const idPrefix = isTrue ? 'panel-true' : `panel-p${panelKey}`;
  const draggableSeat = isTrue ? null : panelKey;
  const sel = selection[panelKey];
  const svg = document.querySelector(`#${idPrefix} svg.board`);

  renderMana(document.querySelector(`#${idPrefix} .mana`), data.mana, panelKey);
  renderBoard(svg, data.cells, data.actors, draggableSeat, panelKey, sel);
  renderSelectionInfo(document.querySelector(`#${idPrefix} .selection`), data.actors, sel, data.mana, panelKey);

  if (isTrue) {
    renderTrueHands(document.querySelector(`#${idPrefix} .hand`), cache.trueState.zones);
    renderCombatInfo(document.querySelector(`#${idPrefix} .combat-info`), cache.trueState);
  } else {
    renderHand(document.querySelector(`#${idPrefix} .hand`), data.hands, panelKey, sel);
    const filtered = filterForSelection(cache.legal[panelKey], sel, data.actors, panelKey);
    renderActions(document.querySelector(`#${idPrefix} .actions`), panelKey, filtered, submitAction, data.actors, svg);

    // Whoever currently has legal commands needs input right now — the active player on
    // their own turn, or the other seat mid-combat-decision (declare defenders, etc.).
    document.getElementById(idPrefix).classList.toggle('needs-input', (cache.legal[panelKey]?.length ?? 0) > 0);
  }
}

function renderAllPanels() {
  renderPanel(1);
  renderPanel(2);
  renderPanel('true');
}

async function refreshAll() {
  const status = document.getElementById('status-line');
  try {
    const [view1, view2, trueState] = await Promise.all([
      api('/api/view/1'),
      api('/api/view/2'),
      api('/api/truestate'),
    ]);
    const [legal1, legal2] = await Promise.all([api('/api/legal/1'), api('/api/legal/2')]);

    cache.view[1] = view1;
    cache.view[2] = view2;
    cache.trueState = trueState;
    cache.legal[1] = legal1;
    cache.legal[2] = legal2;
    cache.cards = trueState.cards ?? [];

    // Drop a selection that's gone stale: its actor died (or a new scenario was loaded), or
    // its card left hand (cast, or a new scenario was loaded).
    for (const key of [1, 2, 'true']) {
      const sel = selection[key];
      if (!sel) continue;
      const actors = key === 'true' ? trueState.actors : cache.view[key].actors;
      if (sel.type === 'actor' && !actors.some(a => a.id.value === sel.id)) selection[key] = null;
      if (sel.type === 'card') {
        const hand = key === 'true' ? null : cache.view[key].hands.find(h => h.player.value === key)?.cards;
        if (!hand?.includes(sel.id)) selection[key] = null;
      }
    }

    // Default to this seat's own Champion whenever nothing else is selected — covers initial
    // load, a scenario reload, and the turn passing to the other seat, without ever
    // overriding a real (still-valid) selection or a deliberate deselect-to-null mid-turn
    // (this only fires from a fresh server round-trip, not from the plain-click deselect path).
    for (const seat of [1, 2]) {
      if (selection[seat] != null) continue;
      const champion = cache.view[seat].actors.find(a => a.kind === 'Champion' && a.owner.value === seat);
      if (champion) selection[seat] = { type: 'actor', id: champion.id.value };
    }

    renderAllPanels();
    renderPhaseStepper(trueState.currentPhase);

    status.textContent = statusLine(view1);
    status.className = view1.winner ? 'winner-banner' : '';
  } catch (err) {
    status.textContent = String(err);
  }
}

function renderCombatInfo(container, trueState) {
  if (trueState.activeCombats.length === 0 && !trueState.activeWindow) {
    container.textContent = '';
    return;
  }
  const lines = trueState.activeCombats.map(c =>
    `Combat ${c.id.value}: A${c.attacker.value} -> (${c.targetHex.q},${c.targetHex.r}) [${c.phase}]` +
    (c.defenders.length ? ` defenders=${c.defenders.map(d => 'A' + d.value).join(',')}` : ''));
  if (trueState.activeWindow) {
    const w = trueState.activeWindow;
    lines.push(`Priority window (${w.kind}): current=P${w.currentPriority.value}, order=${w.order.map(p => 'P' + p.value).join('>')}`);
  }
  container.textContent = lines.join('\n');
}

async function submitAction(seat, index, options = {}) {
  const status = document.getElementById('status-line');
  try {
    const result = await api('/api/submit', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ seat, index }),
    });
    if (!result.accepted) status.textContent = `Rejected: ${result.rejectionReason}`;
  } catch (err) {
    status.textContent = String(err);
  }
  if (!options.skipRefresh) await refreshAll();
}

async function loadScenarioList() {
  const select = document.getElementById('scenario-select');
  const names = await api('/api/scenarios');
  select.innerHTML = '';
  for (const name of names) {
    const opt = document.createElement('option');
    opt.value = name;
    opt.textContent = name;
    select.appendChild(opt);
  }
}

async function loadSelectedScenario() {
  const select = document.getElementById('scenario-select');
  if (!select.value) return;
  await api(`/api/scenarios/${encodeURIComponent(select.value)}/load`, { method: 'POST' });
  selection[1] = null;
  selection[2] = null;
  selection.true = null;
  await refreshAll();
}

document.getElementById('load-button').addEventListener('click', loadSelectedScenario);

(async function init() {
  await loadScenarioList();
  await loadSelectedScenario();
})();
