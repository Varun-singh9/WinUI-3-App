# Design References

Illustrative mockups of how the launcher should look. They are **references, not specifications**: they sit below every
document in the source-of-truth hierarchy in `../README.md`, and where an image and a document differ, the document
wins and a decision is recorded in `03_Live_Repository/DECISIONS.md`.

## Owner direction (2026-10-28)

- Use the colors shown in the images.
- From `prefix-popup-and-results.jpg`, take the **context** (what appears and where), not the **design style**: the popups
  in it use generic styling, and a more refined design is wanted.
- The images were made by the owner with a tool the owner no longer remembers, so their rights are not confirmed. They are to
  be removed at the end of the project, before any public release (tracked as a Release row in `03_Live_Repository/TESTS.md`).

## Files

| File | Original name | What it shows |
|---|---|---|
| `command-bar-idle.jpg` | `Application_Model.jpg` | The bar alone on white, in its idle state |
| `desktop-placement.jpg` | `Desktop_Model.jpg` | The bar placed on a Windows 11 desktop, above the taskbar |
| `prefix-popup-and-results.jpg` | `Reference_Model.jpg` | Three bar states: idle, the prefix list open, and a typed prefix with a results list |

## What the images show

The numbers below were measured from the image pixels on 2026-10-28. The true screen resolution of the mockups is unknown,
so treat sizes as proportions, not exact pixel values.

**The bar** (all three images): a black pill (corner radius about half the height) with a soft neutral shadow below it that
fades out over roughly 20 px. Left to right: a white search glyph; an amber chevron; the placeholder text "Search" in white;
then, on the right, a cluster of six white icons: an audio waveform, overlapping windows, a command-key square, a light bulb,
a monitor and a gear. The images do not say what the icons do, and nothing here assumes it: their functions are defined in the
task plans that build them.

**Placement** (`desktop-placement.jpg`, the desktop capture is 1886 × 1073 px inside a white margin):
- horizontally centered (offset 0 px);
- width about 75.7 % of the screen width and height about 3.4 % of the screen height, which is about 36 px high and 1437 px wide on a 1080-px-high screen (the other two images show the bar 36 to 40 px high);
- its bottom edge is 72 px above the bottom of the display (about 6.7 % of the screen height) and 23 px above the top of the taskbar, which is about 49 px high.

**Prefix popup** (`prefix-popup-and-results.jpg`, second bar): with the pointer over the chevron, a vertical list opens above the
chevron, attached to the top of the bar and left-aligned with the chevron. It has four entries: `RUN:`, `APP:`, `FILE:`, `WEB:`.

**Typed prefix and results** (same image, third bar): the prefix becomes a colored chip inside the bar (`APP:`, white text on
red), followed by the query text (`chr`). A results list opens above the bar, attached to its top and aligned near where the
query text starts, with three rows: `Google Chrome`, `Razer Chroma`, `Chronophoto`.

## Palette

| Use | Value | Note |
|---|---|---|
| Bar | `#000000` | Measured |
| Text and icons | `#F5F5F5` to `#FFFFFF` | Measured |
| Chevron | about `#E5BD45` (amber/gold) | Estimated from a stroke about 2 px wide; anti-aliasing makes the exact value uncertain |
| Prefix chip (`APP:`) | `#D63E44` (red) | Median of the solid area |
| Popup tiles | about `#171A1F` and `#101010` | Part of the generic popup styling; **not** to be copied |
| Page behind the bar in the mockups | white | A mockup background only; the real bar sits over the desktop |

## Take and leave

Take: the bar anatomy and left-to-right order; the amber chevron that opens the prefix list; the prefix shown as a colored
chip in front of the query; the results list opening above the bar and attached to it; the right-hand icon cluster; the
placement proportions; the palette.

Leave: the look of the popups and tiles (dark rectangles, small bold italic labels). They are to be redesigned in a more
refined way, using the same palette.

## Consistency with the canonical documents

- The Overview describes a black or dark rounded bar with text prefixes and symbols, distinct prefix colors, keyboard-first
  input and a dynamic right-side action rail. The images agree: the red chip is a distinct prefix color, and the right-hand
  cluster fits the action rail.
- The Overview says color is never the only meaning. The chip also carries the text `APP:`, so it agrees.
- The Motion and Interaction Specification says prefix popup buttons show text, symbol, color and a short description. The
  reference tiles show text only, so the documents ask for more than the image and the documents win.
- The test matrix requires keyboard navigation of the popup and customization without hover. A chevron that opens only on
  hover is therefore not enough: it must also be reachable by keyboard.
- The Constitution and roadmap ask for a bottom display gap fixed relative to the physical display boundary, independent of
  the taskbar. The mockup is consistent with a gap of 72 px from the display edge and equally with about 24 px above a 48 px
  taskbar; the numeric rule is a Task 3 decision, not settled by this image.

## Not decided here (for the task plans)

- The numeric bottom gap and how it is defined (Task 3).
- The width rule: a fixed proportion of the screen width, or limits (Task 3).
- The exact height, font, light or dark variants and the icon artwork and its license (Tasks 3 and 5).
- What each right-hand icon does, and how the cursor-reactive field behaves around them (Task 5 and Phase 5).
- How the chevron behaves with the keyboard, and the refined popup design (Task 5 and Phase 2).
