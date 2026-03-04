# 🔥 Forest Fire — Cellular Automaton

> Симуляция лесного пожара на основе двумерного стохастического клеточного автомата с GUI.  

---

## Скриншот

<img width="1492" height="905" alt="image" src="https://github.com/user-attachments/assets/4d8dd97d-a4ef-4b48-b59c-5b208de797e0" />


---

## О проекте

Модель симулирует возникновение и распространение лесных пожаров с помощью двумерного клеточного автомата на сетке **78 × 110** клеток. Каждая клетка смотрит на своих **8 соседей** (окрестность Мура) и меняет состояние по вероятностным правилам. Из простых локальных правил возникает сложное глобальное поведение — распространение огня, образование фронтов, восстановление леса.

---

## Состояния клеток

| Состояние | Описание |
|-----------|----------|
| `EMPTY` | Пустая клетка |
| `TREE` | Взрослое дерево |
| `BURNING` | Горит |
| `BURNED` | Пепел |
| `WET` | Мокрая клетка — доп. правило 1 |
| `YOUNG` | Молодое дерево — доп. правило 2 |
| `EMBER` | Летящая искра — доп. правило 3 |

---

## Правила перехода

### Базовые
- **TREE** → если горящий сосед: загорается с вероятностью `p_spread`; случайная молния с вероятностью `p_ignite`
- **BURNING** → через 1 шаг всегда становится `BURNED`
- **BURNED** → с вероятностью `p_regrow` становится `YOUNG`

### Дополнительные правила

**Правило 1 — Мокрые клетки (WET)**  
Моделируют реки и болота. Мокрая клетка никогда не становится `BURNING` — огонь через неё не проходит. Медленно высыхает и даёт росток.

**Правило 2 — Сукцессия растительности (YOUNG)**  
Восстановление леса: `BURNED → YOUNG → TREE`. Молодые деревья уязвимее взрослых — загораются при наличии горящего соседа без проверки вероятности.

**Правило 3 — Летящие искры (EMBER)**  
Горящая клетка с шансом `p_ember` выбрасывает искру, которая летит на 3–10 клеток по направлению ветра. Так пожар перепрыгивает через барьеры.

---

## Параметры модели

| Параметр | Значение | Описание |
|----------|----------|----------|
| `p_tree` | 0.62 | Плотность леса при старте |
| `p_ignite` | 0.0003 | Вероятность молнии |
| `p_spread` | 0.82 | Вероятность распространения огня |
| `p_wet` | 0.04 | Доля мокрых клеток |
| `p_regrow` | 0.007 | Вероятность роста ростка из пепла |
| `p_mature` | 0.04 | Вероятность взросления дерева |
| `p_ember` | 0.013 | Вероятность выброса искры |
| `wind_boost` | 0.28 | Усиление огня по ветру |

---

## Код программы
```python
import tkinter as tk
from tkinter import font as tkfont
import random
import math
import time

# ══════════════════════════════════════════════
#  STATES
# ══════════════════════════════════════════════
EMPTY   = 0
TREE    = 1
BURNING = 2
BURNED  = 3
WET     = 4
YOUNG   = 5
EMBER   = 6

# ══════════════════════════════════════════════
#  NATURE + NEON PALETTE
# ══════════════════════════════════════════════
CELL_COLORS = {
    EMPTY:   "#050a05",
    TREE:    "#1a7a1a",
    BURNING: "#ff3d00",
    BURNED:  "#1a0800",
    WET:     "#0077b6",
    YOUNG:   "#57cc04",
    EMBER:   "#ffca3a",
}

# Neon glow colours for burning states
GLOW = {
    BURNING: "#8b0000",
    EMBER:   "#4a3800",
}

# UI palette – deep forest + neon accents
BG          = "#030b03"
PANEL_BG    = "#060f06"
BORDER      = "#0d2b0d"
NEON_GREEN  = "#39ff14"
NEON_ORANGE = "#ff6600"
NEON_CYAN   = "#00ffe1"
NEON_YELLOW = "#ffe600"
DIM_GREEN   = "#1a4d1a"
TEXT        = "#b8f0b8"
MUTED       = "#2a5c2a"
DARK_TEXT   = "#0a1f0a"

CELL  = 8
COLS  = 110
ROWS  = 78

HISTORY_LEN = 120   # frames to keep in chart


# ══════════════════════════════════════════════
#  CELLULAR AUTOMATON
# ══════════════════════════════════════════════
class ForestFireCA:
    def __init__(self):
        self.p_tree    = 0.62
        self.p_ignite  = 0.0003
        self.p_spread  = 0.82
        self.p_wet     = 0.04
        self.p_regrow  = 0.007
        self.p_mature  = 0.04
        self.p_ember   = 0.013
        self.wind_dx   = 0
        self.wind_dy   = 0
        self.wind_boost= 0.28
        self.generation= 0
        self.stats     = {s: 0 for s in CELL_COLORS}
        self.grid      = self._init()

    def _init(self):
        g = []
        for r in range(ROWS):
            row = []
            for c in range(COLS):
                if random.random() < self.p_wet:
                    row.append(WET)
                elif random.random() < self.p_tree:
                    row.append(TREE)
                else:
                    row.append(EMPTY)
            g.append(row)
        return g

    def reset(self):
        self.generation = 0
        self.grid = self._init()

    def _neighbours(self, r, c):
        for dr in [-1, 0, 1]:
            for dc in [-1, 0, 1]:
                if dr == 0 and dc == 0:
                    continue
                nr, nc = r + dr, c + dc
                if 0 <= nr < ROWS and 0 <= nc < COLS:
                    yield nr, nc, dr, dc

    def step(self):
        new = [[EMPTY] * COLS for _ in range(ROWS)]
        embers = []

        for r in range(ROWS):
            for c in range(COLS):
                s = self.grid[r][c]

                if s == TREE:
                    fired = False
                    for nr, nc, dr, dc in self._neighbours(r, c):
                        ns = self.grid[nr][nc]
                        if ns in (BURNING, EMBER):
                            prob = self.p_spread
                            if dr == self.wind_dy and dc == self.wind_dx:
                                prob = min(1.0, prob + self.wind_boost)
                            if random.random() < prob:
                                fired = True; break
                    if fired:
                        new[r][c] = BURNING
                    elif random.random() < self.p_ignite:
                        new[r][c] = BURNING
                    else:
                        new[r][c] = TREE

                elif s == BURNING:
                    new[r][c] = BURNED
                    if random.random() < self.p_ember:
                        dist = random.randint(3, 10)
                        er = r + self.wind_dy * dist + random.randint(-2, 2)
                        ec = c + self.wind_dx * dist + random.randint(-2, 2)
                        embers.append((er, ec))

                elif s == BURNED:
                    new[r][c] = YOUNG if random.random() < self.p_regrow else BURNED

                elif s == YOUNG:
                    for nr, nc, dr, dc in self._neighbours(r, c):
                        if self.grid[nr][nc] in (BURNING, EMBER):
                            new[r][c] = BURNING; break
                    else:
                        new[r][c] = TREE if random.random() < self.p_mature else YOUNG

                elif s == WET:
                    new[r][c] = YOUNG if random.random() < 0.002 else WET

                elif s == EMBER:
                    new[r][c] = BURNED

                else:
                    new[r][c] = EMPTY

        for er, ec in embers:
            if 0 <= er < ROWS and 0 <= ec < COLS:
                if new[er][ec] == TREE:
                    new[er][ec] = BURNING
                elif new[er][ec] in (EMPTY, BURNED):
                    new[er][ec] = EMBER

        self.grid = new
        self.generation += 1
        self._calc_stats()

    def _calc_stats(self):
        self.stats = {s: 0 for s in CELL_COLORS}
        for row in self.grid:
            for c in row:
                self.stats[c] = self.stats.get(c, 0) + 1

    def ignite_center(self):
        cr, cc = ROWS // 2, COLS // 2
        for dr in range(-3, 4):
            for dc in range(-3, 4):
                r, c = cr+dr, cc+dc
                if 0 <= r < ROWS and 0 <= c < COLS and self.grid[r][c] == TREE:
                    self.grid[r][c] = BURNING

    def add_wet_strip(self):
        c = random.randint(15, COLS - 15)
        for r in range(ROWS):
            for dc in range(2):
                if 0 <= c+dc < COLS:
                    self.grid[r][c+dc] = WET


# ══════════════════════════════════════════════
#  ANIMATED BUTTON
# ══════════════════════════════════════════════
class NeonButton(tk.Canvas):
    def __init__(self, parent, text, command, color=NEON_GREEN, width=120, height=32, **kw):
        super().__init__(parent, width=width, height=height,
                         bg=PANEL_BG, highlightthickness=0, **kw)
        self.command = command
        self.color   = color
        self.text    = text
        self.w       = width
        self.h       = height
        self._hover  = False
        self._draw()
        self.bind("<Enter>",    self._on_enter)
        self.bind("<Leave>",    self._on_leave)
        self.bind("<Button-1>", self._on_click)

    def _draw(self):
        self.delete("all")
        alpha = 1.0 if self._hover else 0.6
        bg    = self._dim(self.color, 0.15 if self._hover else 0.07)
        # Outer glow
        if self._hover:
            self.create_rectangle(0, 0, self.w, self.h,
                                  outline=self.color, width=2, fill=bg)
            self.create_rectangle(2, 2, self.w-2, self.h-2,
                                  outline=self._dim(self.color, 0.4), width=1, fill="")
        else:
            self.create_rectangle(0, 0, self.w, self.h,
                                  outline=self._dim(self.color, 0.4), width=1, fill=bg)
        self.create_text(self.w//2, self.h//2, text=self.text,
                         font=("Consolas", 9, "bold"),
                         fill=self.color if self._hover else self._dim(self.color, 0.75))

    def _dim(self, hex_color, factor):
        h = hex_color.lstrip("#")
        r, g, b = int(h[0:2],16), int(h[2:4],16), int(h[4:6],16)
        r = min(255, int(r * factor + (1-factor)*255)) if factor > 1 else int(r*factor)
        g = min(255, int(g * factor + (1-factor)*255)) if factor > 1 else int(g*factor)
        b = min(255, int(b * factor + (1-factor)*255)) if factor > 1 else int(b*factor)
        # simpler: just scale
        r2 = min(255, int(int(h[0:2],16) * factor))
        g2 = min(255, int(int(h[2:4],16) * factor))
        b2 = min(255, int(int(h[4:6],16) * factor))
        return f"#{r2:02x}{g2:02x}{b2:02x}"

    def _on_enter(self, e):
        self._hover = True;  self._draw()
    def _on_leave(self, e):
        self._hover = False; self._draw()
    def _on_click(self, e):
        self.command()

    def set_text(self, text):
        self.text = text; self._draw()


# ══════════════════════════════════════════════
#  MINI LINE CHART
# ══════════════════════════════════════════════
class MiniChart(tk.Canvas):
    def __init__(self, parent, width, height, color, label, **kw):
        super().__init__(parent, width=width, height=height,
                         bg=PANEL_BG, highlightthickness=0, **kw)
        self.w      = width
        self.h      = height
        self.color  = color
        self.label  = label
        self.data   = []

    def push(self, value):
        self.data.append(value)
        if len(self.data) > HISTORY_LEN:
            self.data.pop(0)
        self._draw()

    def _draw(self):
        self.delete("all")
        # border
        self.create_rectangle(0, 0, self.w-1, self.h-1,
                               outline=self._dim(self.color, 0.3), fill=PANEL_BG)
        if len(self.data) < 2:
            return
        mx = max(self.data) or 1
        pad = 4
        pts = []
        for i, v in enumerate(self.data):
            x = pad + i * (self.w - 2*pad) / (HISTORY_LEN - 1)
            y = self.h - pad - (v / mx) * (self.h - 2*pad)
            pts.append((x, y))

        # fill area
        poly = [pad, self.h-pad]
        for x, y in pts:
            poly += [x, y]
        poly += [pts[-1][0], self.h-pad]
        self.create_polygon(poly, fill=self._dim(self.color, 0.15), outline="")

        # line
        flat = [coord for p in pts for coord in p]
        self.create_line(flat, fill=self.color, width=1, smooth=True)

        # label + value
        self.create_text(4, 3, anchor="nw", text=self.label,
                         font=("Consolas", 7), fill=self._dim(self.color, 0.6))
        self.create_text(self.w-4, 3, anchor="ne",
                         text=f"{self.data[-1]:,}",
                         font=("Consolas", 7, "bold"), fill=self.color)

    def _dim(self, hex_color, factor):
        h = hex_color.lstrip("#")
        r = min(255, int(int(h[0:2],16)*factor))
        g = min(255, int(int(h[2:4],16)*factor))
        b = min(255, int(int(h[4:6],16)*factor))
        return f"#{r:02x}{g:02x}{b:02x}"


# ══════════════════════════════════════════════
#  NEON SLIDER
# ══════════════════════════════════════════════
def neon_slider(parent, label, from_, to, init, cmd, color=NEON_GREEN, res=0.001):
    f = tk.Frame(parent, bg=PANEL_BG)
    f.pack(fill="x", pady=2)
    top = tk.Frame(f, bg=PANEL_BG)
    top.pack(fill="x")
    tk.Label(top, text=label, font=("Consolas", 8),
             bg=PANEL_BG, fg=MUTED).pack(side="left")
    val_lbl = tk.Label(top, text=f"{init:.4f}", font=("Consolas", 8, "bold"),
                       bg=PANEL_BG, fg=color)
    val_lbl.pack(side="right")

    var = tk.DoubleVar(value=init)
    def _cmd(v):
        val_lbl.config(text=f"{float(v):.4f}")
        cmd(v)

    s = tk.Scale(f, from_=from_, to=to, resolution=res,
                 orient="horizontal", variable=var,
                 bg=PANEL_BG, fg=color,
                 troughcolor=DIM_GREEN,
                 activebackground=color,
                 highlightthickness=0, bd=0,
                 sliderlength=12, sliderrelief="flat",
                 showvalue=False,
                 command=_cmd, length=220)
    s.pack(fill="x")
    return var


# ══════════════════════════════════════════════
#  MAIN APP
# ══════════════════════════════════════════════
class App:
    def __init__(self, root):
        self.root     = root
        self.root.title("FOREST FIRE — Cellular Automaton v2")
        self.root.configure(bg=BG)
        self.root.resizable(False, False)

        self.ca       = ForestFireCA()
        self.running  = False
        self.speed    = 60
        self._after   = None

        # Scanline tick for animation
        self._scan_y  = 0
        self._pulse   = 0.0

        # History for charts
        self._h_tree  = []
        self._h_fire  = []
        self._h_burn  = []

        self._build()
        self._render()
        self._animate()

    # ─── BUILD ──────────────────────────────
    def _build(self):
        # ── HEADER ──
        hdr = tk.Frame(self.root, bg=BG)
        hdr.pack(fill="x", padx=0, pady=0)

        # Top neon bar
        tk.Frame(self.root, bg=NEON_GREEN, height=2).pack(fill="x")

        inner_hdr = tk.Frame(hdr, bg=BG)
        inner_hdr.pack(fill="x", padx=24, pady=10)

        title_f = tk.Frame(inner_hdr, bg=BG)
        title_f.pack(side="left")
        tk.Label(title_f, text="🌲", font=("Segoe UI Emoji", 24),
                 bg=BG, fg=NEON_GREEN).pack(side="left")
        tk.Label(title_f, text=" FOREST", font=("Consolas", 22, "bold"),
                 bg=BG, fg=NEON_GREEN).pack(side="left")
        tk.Label(title_f, text="FIRE", font=("Consolas", 22, "bold"),
                 bg=BG, fg=NEON_ORANGE).pack(side="left", padx=(4,0))
        tk.Label(title_f, text="  ·  Клеточный автомат",
                 font=("Consolas", 9), bg=BG, fg=MUTED).pack(side="left", pady=6)

        # Generation counter in header
        self.gen_lbl = tk.Label(inner_hdr, text="GEN  00000",
                                font=("Consolas", 12, "bold"),
                                bg=BG, fg=NEON_CYAN)
        self.gen_lbl.pack(side="right")

        tk.Frame(self.root, bg=DIM_GREEN, height=1).pack(fill="x")

        # ── BODY ──
        body = tk.Frame(self.root, bg=BG)
        body.pack(padx=16, pady=12)

        # Canvas wrapper with double border glow
        outer = tk.Frame(body, bg=NEON_GREEN, padx=1, pady=1)
        outer.pack(side="left")
        inner = tk.Frame(outer, bg=NEON_ORANGE, padx=1, pady=1)
        inner.pack()

        self.canvas = tk.Canvas(inner,
                                width=COLS*CELL, height=ROWS*CELL,
                                bg=CELL_COLORS[EMPTY],
                                highlightthickness=0, cursor="crosshair")
        self.canvas.pack()
        self.canvas.bind("<Button-1>",  self._click)
        self.canvas.bind("<B1-Motion>", self._drag)

        # ── RIGHT PANEL ──
        panel = tk.Frame(body, bg=PANEL_BG, padx=16, pady=12, width=270)
        panel.pack(side="left", fill="y", padx=(10, 0))
        panel.pack_propagate(False)

        self._build_panel(panel)

    def _section(self, parent, text, color=NEON_GREEN):
        f = tk.Frame(parent, bg=PANEL_BG)
        f.pack(fill="x", pady=(12, 4))
        tk.Label(f, text=text, font=("Consolas", 9, "bold"),
                 bg=PANEL_BG, fg=color).pack(side="left")
        tk.Frame(parent, bg=color, height=1).pack(fill="x")

    def _build_panel(self, p):
        ca = self.ca

        # ── CONTROLS ──
        self._section(p, "[ CONTROLS ]", NEON_GREEN)

        r1 = tk.Frame(p, bg=PANEL_BG); r1.pack(fill="x", pady=4)
        self.btn_run = NeonButton(r1, "▶  START", self._toggle, NEON_GREEN, 118, 30)
        self.btn_run.pack(side="left", padx=(0,4))
        NeonButton(r1, "↺  RESET", self._reset, NEON_CYAN, 118, 30).pack(side="left")

        r2 = tk.Frame(p, bg=PANEL_BG); r2.pack(fill="x", pady=(2,0))
        NeonButton(r2, "🔥 Ignite", self._ignite, NEON_ORANGE, 118, 30).pack(side="left", padx=(0,4))
        NeonButton(r2, "💧 Wet Strip", self._wet, NEON_CYAN, 118, 30).pack(side="left")

        # ── WIND ──
        self._section(p, "[ WIND ]", NEON_CYAN)
        wf = tk.Frame(p, bg=PANEL_BG); wf.pack(fill="x", pady=4)
        self.wind_lbl = tk.Label(wf, text="· OFF", font=("Consolas", 9, "bold"),
                                 bg=PANEL_BG, fg=NEON_CYAN, width=6)
        self.wind_lbl.pack(side="left", padx=(0,6))
        for lbl, dx, dy in [("W←",-1,0),("N↑",0,-1),("S↓",0,1),("E→",1,0),("·",0,0)]:
            c = NEON_CYAN if lbl != "·" else MUTED
            NeonButton(wf, lbl, lambda dx=dx,dy=dy,l=lbl: self._wind(dx,dy,l),
                       c, 38, 26).pack(side="left", padx=1)

        # ── PARAMETERS ──
        self._section(p, "[ PARAMETERS ]", NEON_YELLOW)
        neon_slider(p, "Tree density",    0.1, 0.95, ca.p_tree,
                    lambda v: setattr(ca,'p_tree',float(v)), NEON_GREEN)
        neon_slider(p, "Fire spread",     0.1, 1.0,  ca.p_spread,
                    lambda v: setattr(ca,'p_spread',float(v)), NEON_ORANGE)
        neon_slider(p, "Lightning",       0.0001, 0.005, ca.p_ignite,
                    lambda v: setattr(ca,'p_ignite',float(v)), NEON_YELLOW, 0.0001)
        neon_slider(p, "Ember prob",      0.0, 0.06, ca.p_ember,
                    lambda v: setattr(ca,'p_ember',float(v)), NEON_YELLOW)
        neon_slider(p, "Regrowth",        0.001, 0.05, ca.p_regrow,
                    lambda v: setattr(ca,'p_regrow',float(v)), NEON_GREEN)

        # Speed
        self._section(p, "[ SPEED ]", NEON_CYAN)
        self.spd_var = tk.IntVar(value=self.speed)
        tk.Scale(p, from_=5, to=300, resolution=5,
                 orient="horizontal", variable=self.spd_var,
                 bg=PANEL_BG, fg=NEON_CYAN, troughcolor=DIM_GREEN,
                 activebackground=NEON_CYAN, highlightthickness=0,
                 bd=0, sliderlength=12, sliderrelief="flat", showvalue=True,
                 command=lambda v: setattr(self,'speed',int(v)),
                 font=("Consolas",8), length=238).pack(fill="x")

        # ── CHARTS ──
        self._section(p, "[ LIVE CHARTS ]", NEON_GREEN)
        self.chart_tree = MiniChart(p, 238, 36, NEON_GREEN,  "TREES")
        self.chart_tree.pack(pady=(2,2))
        self.chart_fire = MiniChart(p, 238, 36, NEON_ORANGE, "BURNING")
        self.chart_fire.pack(pady=(0,2))
        self.chart_burn = MiniChart(p, 238, 36, "#555555",   "BURNED")
        self.chart_burn.pack()

        # ── LEGEND ──
        self._section(p, "[ LEGEND ]", NEON_GREEN)
        items = [
            (EMPTY,   "Empty ground",     MUTED),
            (TREE,    "Mature tree",       NEON_GREEN),
            (YOUNG,   "Young tree  ✦ R1", NEON_GREEN),
            (WET,     "Wet barrier  ✦ R2", NEON_CYAN),
            (BURNING, "Burning",           NEON_ORANGE),
            (EMBER,   "Flying ember ✦ R3", NEON_YELLOW),
            (BURNED,  "Burned ash",        "#444"),
        ]
        for state, label, col in items:
            lf = tk.Frame(p, bg=PANEL_BG); lf.pack(fill="x", pady=1)
            tk.Canvas(lf, width=12, height=12, bg=CELL_COLORS[state],
                      highlightthickness=1,
                      highlightbackground=col).pack(side="left", padx=(0,6))
            tk.Label(lf, text=label, font=("Consolas", 7),
                     bg=PANEL_BG, fg=col).pack(side="left")

        # ── STATS ──
        self._section(p, "[ STATS ]", NEON_CYAN)
        self.stat_var = tk.StringVar()
        tk.Label(p, textvariable=self.stat_var,
                 font=("Consolas", 8), bg=PANEL_BG,
                 fg=TEXT, justify="left", anchor="w").pack(anchor="w")

        # Bottom bar
        tk.Frame(p, bg=NEON_GREEN, height=1).pack(fill="x", side="bottom", pady=(8,0))
        tk.Label(p, text="🖱 Click/drag to paint fire",
                 font=("Consolas", 7), bg=PANEL_BG, fg=MUTED).pack(side="bottom")

    # ─── INTERACTIONS ───────────────────────
    def _click(self, e): self._paint(e.x, e.y)
    def _drag(self, e):  self._paint(e.x, e.y)
    def _paint(self, x, y):
        c, r = x // CELL, y // CELL
        if 0 <= r < ROWS and 0 <= c < COLS:
            self.ca.grid[r][c] = BURNING
        self._render()

    def _toggle(self):
        self.running = not self.running
        self.btn_run.set_text("⏸  PAUSE" if self.running else "▶  START")
        if self.running: self._loop()

    def _reset(self):
        self.running = False
        self.btn_run.set_text("▶  START")
        if self._after: self.root.after_cancel(self._after)
        self.ca.reset()
        self.chart_tree.data.clear()
        self.chart_fire.data.clear()
        self.chart_burn.data.clear()
        self._render()

    def _ignite(self):  self.ca.ignite_center(); self._render()
    def _wet(self):     self.ca.add_wet_strip(); self._render()

    def _wind(self, dx, dy, lbl):
        self.ca.wind_dx = dx; self.ca.wind_dy = dy
        arrows = {"W←":"← WEST","N↑":"↑ NORTH","S↓":"↓ SOUTH","E→":"→ EAST","·":"· OFF"}
        self.wind_lbl.config(text=arrows.get(lbl, lbl))

    # ─── LOOP ───────────────────────────────
    def _loop(self):
        if not self.running: return
        self.ca.step()
        self._render()
        self._update_charts()
        self._after = self.root.after(self.speed, self._loop)

    def _animate(self):
        """Scanline pulse animation on idle"""
        self._pulse = (self._pulse + 0.05) % (2 * math.pi)
        self.root.after(50, self._animate)

    # ─── RENDER ─────────────────────────────
    def _render(self):
        cv = self.canvas
        cv.delete("all")
        grid = self.ca.grid

        for r in range(ROWS):
            for c in range(COLS):
                s = grid[r][c]
                x0, y0 = c*CELL, r*CELL
                x1, y1 = x0+CELL-1, y0+CELL-1

                if s in GLOW:
                    cv.create_rectangle(x0-1, y0-1, x1+1, y1+1,
                                        fill=GLOW[s], outline="")

                cv.create_rectangle(x0, y0, x1, y1,
                                    fill=CELL_COLORS[s], outline="")

        # Subtle scanline overlay every 4 rows
        for r in range(0, ROWS*CELL, 4):
            cv.create_line(0, r, COLS*CELL, r,
                           fill="#ffffff", stipple="gray12")

        # Update stats
        s  = self.ca.stats
        total = ROWS * COLS
        self.gen_lbl.config(text=f"GEN  {self.ca.generation:05d}")
        self.stat_var.set(
            f"Trees    {s.get(TREE,0)+s.get(YOUNG,0):>6,}   "
            f"{100*(s.get(TREE,0)+s.get(YOUNG,0))//total:>2}%\n"
            f"Burning  {s.get(BURNING,0):>6,}   "
            f"{100*s.get(BURNING,0)//total:>2}%\n"
            f"Burned   {s.get(BURNED,0):>6,}   "
            f"{100*s.get(BURNED,0)//total:>2}%\n"
            f"Wet      {s.get(WET,0):>6,}   "
            f"{100*s.get(WET,0)//total:>2}%\n"
            f"Embers   {s.get(EMBER,0):>6,}"
        )

    def _update_charts(self):
        s = self.ca.stats
        self.chart_tree.push(s.get(TREE,0) + s.get(YOUNG,0))
        self.chart_fire.push(s.get(BURNING,0))
        self.chart_burn.push(s.get(BURNED,0))


# ══════════════════════════════════════════════
if __name__ == "__main__":
    root = tk.Tk()
    App(root)
    root.mainloop()
```

---

## Выводы

В ходе работы реализована симуляция лесного пожара на основе стохастического клеточного автомата. Реализованы три дополнительных правила поведения системы: мокрые клетки как противопожарный барьер, сукцессия растительности и перенос искр ветром.

Установлено наличие критического порога плотности леса (~0.5–0.6): при плотности ниже порога огонь быстро затухает, выше — распространяется лавинообразно. Это явление перколяционного перехода характерно для стохастических КА.

Метод клеточных автоматов показал эффективность для имитационного моделирования природных явлений: при минимальных вычислительных затратах модель воспроизводит реалистичное поведение лесного пожара, включая нелинейную динамику и самовосстановление системы.
