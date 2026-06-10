"""High-quality pixel-art sprite drawers for CyberPet (64×64, centered, neon palette)."""
from __future__ import annotations

from PIL import Image, ImageDraw

CELL = 64
BG = (184, 184, 184, 255)

# ── cyber-neon palette + warm cat tones ─────────────────────────────────────
OL = (18, 22, 34)          # dark outline
CY = (0, 229, 200)         # #00e5c8
CY_D = (0, 180, 158)
CY_L = (120, 255, 235)
PU = (167, 139, 250)       # #a78bfa
PU_D = (120, 90, 200)
PU_L = (200, 180, 255)
PK = (244, 114, 182)       # #f472b6
PK_D = (200, 70, 140)
PK_L = (255, 180, 210)
GD = (255, 200, 80)
GD_D = (220, 150, 40)
GD_L = (255, 230, 150)
WH = (255, 252, 245)
GR = (72, 200, 120)
GR_D = (40, 140, 80)
GR_L = (140, 240, 170)
RD = (240, 90, 90)
RD_D = (180, 50, 50)
BL = (60, 100, 180)
BL_D = (35, 65, 130)
BL_L = (120, 170, 240)
SL = (140, 155, 175)
SL_D = (90, 105, 125)
SL_L = (200, 210, 225)
OR = (255, 160, 70)
OR_D = (200, 110, 40)
OR_L = (255, 200, 120)
BR = (130, 90, 55)
BR_D = (90, 60, 35)
BR_L = (180, 130, 80)
BG_D = (12, 16, 28)
ICE = (180, 230, 255)
ICE_D = (120, 190, 240)
# cat fur
F1, F2, F3 = (255, 175, 95), (245, 155, 70), (210, 120, 45)
FN = (255, 220, 180)       # nose / inner ear


def blank() -> Image.Image:
    return Image.new("RGBA", (CELL, CELL), BG)


def _d(img: Image.Image) -> ImageDraw.ImageDraw:
    return ImageDraw.Draw(img)


def dot(img, x, y, c):
    if 0 <= x < CELL and 0 <= y < CELL:
        _d(img).point((x, y), fill=c)


def r(img, x, y, w, h, c, outline=True):
    """Filled rect with optional 1px dark outline."""
    d = _d(img)
    if outline and w > 0 and h > 0:
        d.rectangle([x - 1, y - 1, x + w, y + h], fill=OL)
    d.rectangle([x, y, x + w - 1, y + h - 1], fill=c)


def rr(img, rects: list[tuple], outline=True):
    for item in rects:
        if len(item) == 5:
            x, y, w, h, c = item
            r(img, x, y, w, h, c, outline)


def glow_dots(img, pts: list[tuple[int, int]], c):
    for x, y in pts:
        dot(img, x, y, c)
        dot(img, x + 1, y, c)
        dot(img, x, y + 1, c)


# ── fish helpers (side view → right, centered ~y=30) ────────────────────────
def _fish_standard(img, body, belly, tail, fin, accent=None, spots=None, glow=False):
    """Classic side-view fish, anchored for visual center in 64×64."""
    ox, oy = 10, 22
    rr(img, [
        (ox, oy + 2, 6, 5, tail), (ox + 5, oy, 4, 9, tail),
        (ox + 8, oy + 1, 22, 7, body),
        (ox + 28, oy + 2, 6, 5, body), (ox + 33, oy + 3, 4, 3, body),
        (ox + 14, oy - 2, 5, 3, fin), (ox + 20, oy + 7, 6, 3, fin),
        (ox + 30, oy + 3, 2, 2, OL), (ox + 30, oy + 4, 1, 1, WH),
    ])
    if belly:
        rr(img, [(ox + 14, oy + 5, 14, 3, belly)], outline=False)
    if accent:
        rr(img, accent, outline=False)
    if spots:
        for sx, sy, sc in spots:
            r(img, ox + sx, oy + sy, 2, 2, sc, outline=False)
    if glow:
        glow_dots(img, [(ox + 12, oy - 4), (ox + 24, oy - 3), (ox + 18, oy + 9)], CY_L)


def fish_grey_carp():
    img = blank()
    _fish_standard(img, SL, SL_L, SL_D, SL_D)
    rr(img, [(22, 20, 8, 2, SL_L)], outline=False)
    return img


def fish_green_bass():
    img = blank()
    _fish_standard(img, GR_D, GR_L, GR, GR, spots=[(16, 2, GR_L), (22, 3, GR)])
    rr(img, [(20, 19, 10, 3, GR)], outline=False)
    return img


def fish_shrimp():
    img = blank()
    rr(img, [
        (16, 28, 20, 5, PK), (14, 26, 6, 3, PK_D), (32, 26, 6, 3, PK_D),
        (18, 24, 14, 3, PK_L), (24, 22, 4, 2, PK_D),
        (20, 20, 2, 3, PK_D), (26, 20, 2, 3, PK_D), (30, 20, 2, 3, PK_D),
        (28, 23, 2, 2, OL),
    ])
    return img


def fish_loach():
    img = blank()
    rr(img, [
        (12, 30, 32, 4, OR_D), (14, 28, 28, 2, OR),
        (38, 29, 6, 2, OR_L), (40, 28, 2, 4, OR_L),
        (34, 30, 2, 2, OL),
    ])
    return img


def fish_trout():
    img = blank()
    _fish_standard(img, BL_D, BL_L, BL, BL, spots=[(12, 1, BL_L), (18, 2, WH), (24, 1, BL_L)])
    return img


def fish_silver():
    img = blank()
    _fish_standard(img, SL_L, WH, SL, SL_L, accent=[(24, 19, 6, 2, WH)])
    return img


def fish_eel():
    img = blank()
    rr(img, [
        (10, 30, 30, 3, OR_D), (12, 28, 26, 2, OR),
        (36, 29, 8, 2, OR_L), (42, 27, 3, 6, OR_L), (44, 28, 2, 4, OR),
        (32, 30, 2, 2, OL),
    ])
    return img


def fish_catfish():
    img = blank()
    _fish_standard(img, BR, OR_L, BR_D, BR_D)
    rr(img, [(18, 33, 8, 4, BR_L), (16, 35, 3, 3, BR_D), (27, 35, 3, 3, BR_D)], outline=False)
    return img


def fish_squid():
    img = blank()
    rr(img, [
        (22, 18, 14, 12, PU_D), (24, 16, 10, 3, PU),
        (20, 30, 3, 8, PU_D), (24, 30, 3, 10, PU), (28, 30, 3, 9, PU_D),
        (32, 30, 3, 7, PU_D), (36, 30, 2, 6, PU),
        (28, 22, 2, 2, OL), (28, 23, 1, 1, WH),
    ])
    return img


def fish_ray():
    img = blank()
    rr(img, [
        (12, 28, 28, 6, PU_D), (16, 26, 20, 3, PU),
        (34, 30, 10, 3, PU_D), (14, 32, 6, 2, PU),
        (26, 26, 2, 2, OL), (26, 27, 1, 1, WH),
    ])
    return img


def fish_gold_koi():
    img = blank()
    _fish_standard(img, GD, GD_L, GD_D, GD_D, spots=[(14, 1, WH), (20, 2, RD)], accent=[(24, 19, 8, 2, GD_L)])
    return img


def fish_shark():
    img = blank()
    rr(img, [
        (8, 24, 8, 10, SL_D), (14, 22, 26, 9, SL_D),
        (38, 24, 8, 6, SL_D), (44, 26, 4, 3, SL),
        (20, 20, 6, 4, SL), (28, 30, 5, 3, SL),
        (40, 27, 2, 2, OL), (40, 28, 1, 1, WH),
        (18, 21, 8, 2, SL_L),
    ])
    return img


def fish_glow():
    img = blank()
    _fish_standard(img, CY_D, CY_L, CY, CY, glow=True)
    glow_dots(img, [(24, 17), (30, 18), (36, 17)], CY_L)
    return img


def fish_ice():
    img = blank()
    _fish_standard(img, ICE_D, WH, ICE, ICE_D, accent=[(20, 18, 4, 2, CY_L)])
    rr(img, [(16, 19, 2, 2, WH), (28, 20, 2, 2, CY_L)], outline=False)
    return img


def fish_coral():
    img = blank()
    _fish_standard(img, PK, PK_L, PK_D, PK_D, accent=[(18, 18, 6, 3, OR), (26, 19, 4, 2, GD)])
    return img


def fish_abyss():
    img = blank()
    _fish_standard(img, BG_D, PU_D, OL, PU_D, accent=[(22, 17, 6, 4, CY), (32, 18, 3, 2, CY_L)])
    glow_dots(img, [(20, 16), (34, 17)], CY_L)
    return img


def fish_angler():
    img = blank()
    _fish_standard(img, SL_D, SL, OL, SL_D)
    rr(img, [(38, 12, 2, 12, GD), (36, 10, 6, 3, GD_L), (38, 9, 2, 2, WH)])
    return img


def fish_myth_cyan():
    img = blank()
    _fish_standard(img, CY, CY_L, CY_D, CY, glow=True)
    rr(img, [(16, 17, 4, 2, WH), (26, 17, 4, 2, WH), (20, 15, 8, 2, CY_L)], outline=False)
    glow_dots(img, [(14, 15), (30, 16), (22, 10), (28, 11)], CY_L)
    return img


def fish_myth_purple():
    img = blank()
    _fish_standard(img, PU, PU_L, PU_D, PU, glow=True)
    rr(img, [(18, 16, 10, 3, GD), (14, 18, 3, 2, PK_L)], outline=False)
    glow_dots(img, [(16, 14), (28, 15), (24, 9)], PU_L)
    return img


def fish_myth_green():
    img = blank()
    _fish_standard(img, GR, GR_L, GR_D, GR, glow=True)
    rr(img, [(22, 14, 10, 4, CY), (18, 17, 4, 2, GD_L)], outline=False)
    glow_dots(img, [(20, 12), (30, 13)], GR_L)
    return img


def fish_myth_gold():
    img = blank()
    rr(img, [
        (6, 24, 10, 10, GD_D), (14, 22, 28, 10, GD),
        (40, 24, 10, 7, GD), (48, 26, 4, 4, GD_D),
        (18, 20, 8, 4, GD_L), (28, 30, 6, 3, GD_D),
        (44, 27, 2, 2, OL), (44, 28, 1, 1, WH),
    ])
    glow_dots(img, [(16, 18), (24, 17), (32, 18), (12, 20), (38, 20)], GD_L)
    rr(img, [(14, 17, 4, 2, RD), (30, 17, 4, 2, RD)], outline=False)
    return img


def fish_myth_ice():
    img = blank()
    rr(img, [
        (8, 24, 10, 9, ICE_D), (16, 22, 28, 9, ICE),
        (42, 24, 8, 6, ICE_D), (48, 26, 3, 3, ICE),
        (18, 19, 10, 4, WH), (30, 30, 6, 3, ICE_D),
        (46, 27, 2, 2, CY), (46, 28, 1, 1, WH),
    ])
    glow_dots(img, [(14, 16), (26, 15), (36, 16), (20, 13), (32, 12)], WH)
    return img


def fish_myth_sea():
    img = blank()
    rr(img, [
        (6, 24, 12, 10, BL_D), (16, 22, 26, 10, BL),
        (40, 24, 10, 7, BL_D), (48, 26, 4, 4, BL),
        (14, 18, 12, 5, CY), (30, 19, 8, 4, OR),
        (44, 27, 2, 2, GD), (44, 28, 1, 1, WH),
    ])
    glow_dots(img, [(18, 15), (28, 16), (38, 15)], CY_L)
    return img


def fish_myth_leviathan():
    img = blank()
    rr(img, [
        (4, 24, 14, 12, OL), (16, 22, 24, 11, BG_D),
        (38, 24, 12, 8, PU_D), (48, 26, 5, 5, PU),
        (12, 20, 12, 5, PU), (28, 30, 8, 4, OL),
        (46, 27, 2, 2, RD), (46, 28, 1, 1, GD_L),
        (20, 18, 8, 4, RD), (34, 19, 6, 3, CY),
    ])
    glow_dots(img, [(10, 17), (22, 16), (36, 17), (42, 18), (14, 21)], PU_L)
    return img


# ── cat (centered ~40×44, y≈12) ───────────────────────────────────────────
def _cat_body(img, extra=None):
    rr(img, [
        (22, 14, 4, 4, F3), (34, 14, 4, 4, F3),
        (24, 12, 3, 3, F2), (35, 12, 3, 3, F2),
        (22, 16, 4, 3, FN), (34, 16, 4, 3, FN),
        (20, 18, 20, 14, F2), (22, 20, 16, 10, F1),
        (22, 32, 16, 12, F2), (24, 34, 12, 8, WH),
        (20, 42, 5, 5, F3), (35, 42, 5, 5, F3),
        (22, 44, 4, 3, F2), (36, 44, 4, 3, F2),
    ])
    if extra:
        rr(img, extra)


def draw_cat_happy():
    img = blank()
    _cat_body(img, [(18, 22, 2, 2, PK_L), (38, 22, 2, 2, PK_L)])
    rr(img, [
        (24, 26, 4, 1, OL), (32, 26, 4, 1, OL),
        (26, 28, 8, 2, PK),
        (14, 18, 2, 2, CY_L), (42, 16, 2, 2, PK_L), (46, 12, 2, 2, GD_L),
    ], outline=False)
    return img


def draw_cat_sleep():
    img = blank()
    _cat_body(img)
    rr(img, [
        (24, 27, 5, 1, OL), (31, 27, 5, 1, OL),
        (27, 29, 6, 1, F3),
        (42, 10, 2, 2, CY_L), (46, 8, 2, 2, CY_L), (44, 14, 2, 2, CY),
    ], outline=False)
    return img


def draw_cat_hungry():
    img = blank()
    _cat_body(img)
    rr(img, [
        (25, 25, 3, 4, OL), (32, 25, 3, 4, OL),
        (25, 26, 2, 2, WH), (33, 26, 2, 2, WH),
        (28, 30, 4, 3, OL), (29, 31, 2, 2, RD),
        (10, 38, 10, 4, OL), (11, 39, 8, 2, SL),
        (12, 41, 6, 2, OL),
    ], outline=False)
    return img


def draw_cat_fishing():
    img = blank()
    _cat_body(img, [(8, 22, 14, 2, BR_D), (6, 18, 2, 6, BR), (4, 12, 2, 8, BR_L)])
    rr(img, [
        (25, 25, 3, 3, OL), (32, 25, 3, 3, OL),
        (26, 26, 2, 2, WH), (33, 26, 2, 2, WH),
        (27, 29, 6, 1, F3),
        (2, 10, 2, 2, CY), (2, 12, 1, 6, CY_D),
    ], outline=False)
    return img


def draw_cat_content():
    img = blank()
    _cat_body(img, [(19, 23, 2, 2, PK_L), (39, 23, 2, 2, PK_L)])
    rr(img, [
        (25, 26, 3, 3, OL), (32, 26, 3, 3, OL),
        (26, 27, 2, 2, WH), (33, 27, 2, 2, WH),
        (27, 29, 6, 1, F3),
    ], outline=False)
    return img


def draw_cat_wink():
    img = blank()
    _cat_body(img, [(18, 22, 2, 2, PK_L), (38, 22, 2, 2, PK_L)])
    rr(img, [
        (24, 27, 5, 1, OL), (32, 26, 3, 3, OL),
        (33, 27, 2, 2, WH),
        (27, 29, 6, 2, PK),
    ], outline=False)
    return img


# ── furniture (distinct silhouettes, neon accents) ──────────────────────────
def draw_sofa():
    img = blank()
    rr(img, [
        (10, 28, 36, 10, PU_D), (12, 26, 6, 12, PU), (38, 26, 6, 12, PU),
        (14, 30, 28, 4, PU_L), (16, 32, 24, 4, PK_L),
        (12, 36, 32, 3, OL), (14, 37, 4, 1, CY), (26, 37, 4, 1, CY), (38, 37, 4, 1, CY),
    ])
    return img


def draw_tv():
    img = blank()
    rr(img, [
        (14, 14, 28, 20, OL), (16, 16, 24, 16, BG_D),
        (18, 18, 20, 12, BL_D),
        (20, 20, 6, 4, CY), (28, 20, 8, 6, PU_D),
        (20, 28, 14, 2, CY_D),
        (24, 34, 8, 3, SL_D), (26, 37, 4, 2, SL),
    ])
    return img


def draw_cattoy():
    img = blank()
    rr(img, [
        (30, 8, 2, 22, BR), (28, 6, 6, 3, BR_L),
        (24, 4, 3, 3, PK), (28, 2, 3, 4, CY), (32, 3, 3, 3, GD),
        (34, 5, 3, 3, PU_L), (22, 6, 2, 2, PK_L),
        (16, 36, 20, 4, BR_D), (18, 38, 16, 2, BR),
    ])
    return img


def draw_joypad():
    img = blank()
    rr(img, [
        (14, 30, 28, 6, OL), (16, 24, 24, 8, SL_D),
        (18, 20, 20, 6, PU_D), (20, 16, 16, 5, PU),
        (22, 12, 12, 5, PU_L),
        (20, 26, 4, 3, OL), (34, 26, 4, 3, OL),
        (24, 18, 3, 2, CY), (30, 17, 4, 3, PK),
        (26, 14, 2, 2, GD_L),
    ])
    return img


def draw_water_dispenser():
    img = blank()
    rr(img, [
        (20, 10, 16, 6, OL), (22, 12, 12, 2, CY),
        (22, 16, 12, 18, WH), (24, 18, 8, 14, CY_L),
        (18, 34, 20, 6, OL), (20, 36, 16, 2, SL),
        (14, 22, 3, 2, CY), (12, 20, 2, 2, CY_L),
        (14, 24, 2, 4, CY_D), (16, 26, 2, 3, CY),
        (26, 14, 4, 6, CY), (28, 20, 2, 4, CY_L),
    ])
    return img


def draw_fishtank():
    img = blank()
    rr(img, [
        (12, 16, 32, 18, OL), (14, 18, 28, 14, CY_L),
        (14, 16, 28, 2, SL_D), (14, 32, 28, 2, SL_D),
        (12, 16, 2, 18, SL_L), (42, 16, 2, 18, SL_L),
        (20, 24, 10, 4, OR), (34, 26, 6, 3, PK),
        (36, 22, 2, 2, WH), (30, 20, 2, 2, WH),
        (18, 20, 2, 2, CY_L), (24, 18, 2, 2, CY_L),
        (16, 36, 24, 3, BR_D),
    ])
    return img


def draw_sunlamp():
    img = blank()
    rr(img, [
        (22, 22, 12, 12, GD), (24, 20, 8, 3, GD_D),
        (18, 12, 3, 3, GD_L), (26, 8, 3, 3, GD_L), (34, 12, 3, 3, GD_L),
        (16, 18, 2, 2, GD_L), (38, 16, 2, 2, GD_L),
        (20, 34, 16, 3, BR_D), (22, 37, 12, 2, BR),
    ])
    return img


def draw_aroma():
    img = blank()
    rr(img, [
        (24, 28, 8, 10, WH), (26, 26, 4, 3, PK_D),
        (22, 18, 3, 3, PU_L), (26, 14, 3, 4, PU), (30, 16, 3, 3, PK_L),
        (20, 12, 2, 2, PU_L), (34, 12, 2, 2, PK),
        (20, 38, 16, 3, BR_D),
    ])
    return img


def draw_luxury_tower():
    img = blank()
    rr(img, [
        (14, 38, 28, 4, BR_D), (18, 32, 20, 6, BR),
        (20, 24, 16, 8, BR_L), (22, 16, 12, 8, PU_D),
        (24, 8, 8, 8, PU), (26, 4, 4, 5, PK_L),
        (16, 26, 6, 3, PU_L), (34, 22, 6, 3, CY_L),
        (20, 34, 5, 3, PK), (30, 30, 5, 3, CY),
        (24, 10, 4, 2, GD_L),
    ])
    return img


def draw_fridge():
    img = blank()
    rr(img, [
        (20, 8, 18, 36, WH), (20, 8, 18, 2, OL),
        (22, 12, 14, 14, CY_L), (22, 28, 14, 14, SL_L),
        (34, 14, 2, 4, OL), (34, 30, 2, 4, OL),
        (24, 14, 8, 4, CY), (24, 30, 6, 3, BL_L),
    ])
    return img


def draw_stove():
    img = blank()
    rr(img, [
        (14, 14, 28, 24, OL), (16, 16, 24, 16, SL_D),
        (18, 32, 20, 4, BR_D), (20, 34, 16, 2, OL),
        (18, 10, 5, 3, CY), (26, 10, 5, 3, CY), (34, 10, 5, 3, CY),
        (22, 20, 4, 3, CY_L), (30, 22, 4, 3, PK_L),
    ])
    return img


def draw_auto_feeder():
    img = blank()
    rr(img, [
        (16, 10, 20, 14, OL), (18, 12, 16, 10, SL_L),
        (14, 24, 24, 10, BR_D), (16, 26, 20, 6, BR),
        (20, 14, 12, 4, CY), (22, 28, 8, 3, OR),
        (24, 8, 4, 3, CY_L),
    ])
    return img


def draw_bed():
    img = blank()
    rr(img, [
        (10, 30, 36, 8, PU_D), (12, 26, 10, 6, PU),
        (14, 32, 32, 4, PU_L), (16, 34, 28, 2, PK_L),
        (18, 28, 8, 4, WH),
    ])
    return img


def draw_cozy_bed():
    img = blank()
    rr(img, [
        (14, 30, 28, 10, PK_D), (16, 28, 24, 4, PK),
        (18, 32, 20, 6, PK_L), (22, 34, 12, 3, WH),
        (28, 26, 4, 4, RD),
    ])
    return img


def draw_toilet():
    img = blank()
    rr(img, [
        (20, 12, 16, 8, WH), (18, 20, 20, 12, SL_L),
        (20, 22, 16, 8, CY_L), (22, 32, 12, 4, OL),
        (24, 14, 8, 4, SL),
    ])
    return img


def draw_sink():
    img = blank()
    rr(img, [
        (14, 28, 24, 10, WH), (16, 30, 20, 6, CY_L),
        (24, 14, 4, 12, SL_D), (22, 10, 8, 4, SL),
        (20, 16, 2, 6, CY), (22, 18, 2, 4, CY_L),
        (18, 12, 2, 2, CY_L),
    ])
    return img


def draw_garden():
    img = blank()
    rr(img, [
        (18, 32, 20, 8, BR_D), (20, 30, 16, 4, BR),
        (22, 22, 3, 10, GR_D), (28, 18, 3, 14, GR),
        (20, 16, 3, 8, GR_D), (32, 20, 3, 10, GR),
        (24, 12, 6, 6, PK), (26, 10, 3, 3, PK_L),
        (30, 14, 3, 3, GD), (18, 14, 2, 2, PK_L),
    ])
    return img


# ── items ───────────────────────────────────────────────────────────────────
def draw_food():
    img = blank()
    rr(img, [
        (22, 14, 12, 4, OR_D), (18, 18, 20, 14, OR),
        (20, 20, 16, 10, OR_L), (22, 22, 4, 3, RD),
        (30, 24, 4, 3, GR), (26, 28, 3, 2, GR_D),
    ])
    return img


def draw_can():
    img = blank()
    rr(img, [
        (20, 12, 16, 22, OL), (22, 14, 12, 18, SL_L),
        (22, 12, 12, 2, SL_D), (22, 32, 12, 2, SL_D),
        (22, 16, 12, 4, CY), (24, 22, 8, 6, PK_L),
        (26, 24, 4, 2, WH),
    ])
    return img


def draw_bowl():
    img = blank()
    rr(img, [
        (14, 26, 28, 8, OL), (16, 28, 24, 4, WH),
        (18, 30, 20, 2, CY_L), (20, 28, 16, 3, OR),
        (22, 29, 12, 2, OR_L),
    ])
    return img


def draw_rod():
    img = blank()
    rr(img, [
        (8, 34, 24, 2, BR_D), (30, 12, 2, 22, BR),
        (32, 10, 6, 6, OL), (34, 12, 2, 10, CY),
        (28, 8, 4, 3, SL), (36, 14, 2, 2, CY_L),
    ])
    return img


def draw_pouch():
    img = blank()
    rr(img, [
        (18, 14, 20, 22, BR_D), (20, 12, 16, 6, BR),
        (22, 20, 12, 10, BR_L), (26, 24, 6, 4, GD),
        (24, 26, 3, 2, GD_L), (28, 14, 4, 2, CY),
    ])
    return img


# ── registries (imported by build_assets.py) ────────────────────────────────
FURNITURE: list[tuple[str, callable]] = [
    ("Sofa", draw_sofa), ("TV", draw_tv), ("CatToy", draw_cattoy),
    ("JoyPad", draw_joypad), ("WaterDispenser", draw_water_dispenser),
    ("FishTank", draw_fishtank), ("SunLamp", draw_sunlamp),
    ("AromaDiffuser", draw_aroma), ("LuxuryTower", draw_luxury_tower),
    ("Fridge", draw_fridge), ("Stove", draw_stove),
    ("AutoFeederUnit", draw_auto_feeder), ("Bed", draw_bed),
    ("CozyBed", draw_cozy_bed), ("Toilet", draw_toilet),
    ("Sink", draw_sink), ("Garden", draw_garden),
]

ITEMS: list[tuple[str, callable]] = [
    ("food", draw_food), ("can", draw_can), ("bowl", draw_bowl),
    ("rod", draw_rod), ("pouch", draw_pouch),
]

FISH: list[tuple[str, callable]] = [
    ("01-grey-carp", fish_grey_carp), ("02-green-bass", fish_green_bass),
    ("03-shrimp", fish_shrimp), ("04-loach", fish_loach),
    ("05-trout", fish_trout), ("06-silver", fish_silver),
    ("07-eel", fish_eel), ("08-catfish", fish_catfish),
    ("09-squid", fish_squid), ("10-ray", fish_ray),
    ("11-gold-koi", fish_gold_koi), ("12-shark", fish_shark),
    ("13-glow", fish_glow), ("14-ice", fish_ice),
    ("15-coral", fish_coral), ("16-abyss", fish_abyss),
    ("17-angler", fish_angler), ("18-myth-cyan", fish_myth_cyan),
    ("19-myth-purple", fish_myth_purple), ("20-myth-green", fish_myth_green),
    ("21-myth-gold", fish_myth_gold), ("22-myth-ice", fish_myth_ice),
    ("23-myth-sea", fish_myth_sea), ("24-myth-leviathan", fish_myth_leviathan),
]

CATS: list[tuple[str, callable]] = [
    ("happy", draw_cat_happy), ("sleep", draw_cat_sleep),
    ("hungry", draw_cat_hungry), ("fishing", draw_cat_fishing),
    ("content", draw_cat_content), ("wink", draw_cat_wink),
]
