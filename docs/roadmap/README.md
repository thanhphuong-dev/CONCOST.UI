# Planning documents

The active plan is [roadmap revision 3](roadmap-v3.md). Everything else is kept as project history and may describe a multi-project layout that is no longer used.

The current repository structure and supported setup are documented in the root [README](../../README.md) and in the [documentation wiki](../index.html). The active library project is `src/CONCOST.UI/CONCOST.UI.csproj`, which produces the single `CONCOST.UI.dll` assembly.

## Active

- [Roadmap revision 3](roadmap-v3.md) — current direction, control coverage and release themes. Sections 96, 97, 100, 101, 107.4, 107.9, 107.12, 107.13 and 107.16 are annotated with what actually shipped.
- [Effects roadmap](effects-roadmap.md) — planned visual effects (web 2026 trends). Phases 0–2 shipped in 1.0.0, **phase 3 shipped in 1.1.0** (CommandPalette, ParticleField, RadialGauge, PageTransitionHost).
- [Ribbon plan](ribbon-plan.md) — 22 controls in P0/P1/P2, layout engine, keytips, persistence. **All 22/22 shipped in 1.1.0**, earlier than the planned 1.1.0 = P0 / 1.2.0 = P1 / 1.3.0 = P2 split.
- [Revit-inspired CAD UI spec — gap analysis](revit-spec-gap.md) — `Revit_Inspired_CAD_UI_Control_Specification_v1.0.docx` (TT-UI-SPEC-001) mapped item by item: **132 of 135 covered (98%), 3 partial, 0 missing**, plus **11 of the 12** services and **20 of 20** acceptance criteria.
- [Revit-inspired CAD UI spec — plan](revit-spec-plan.md) — the phased plan that closed the gap (phases A–F), its verification rule, and the four items deliberately left partial with the boundary decision behind each.

## Release 1.1.0 at a glance

| Item | State |
| --- | --- |
| Ribbon | 22/22 controls |
| Phase 3 effects | 4/4 controls |
| Public control classes | 167 |
| `CONCOST.*` style keys | 352 |
| Theme presets | 8 |
| Tests | 291 green |
| Target frameworks | `net48`, `net8.0-windows`, `net10.0-windows` — 0 warnings |
| Assembly signing | strong-name, token `5aec1392da75741c` |
| Authenticode | not done (needs an external code-signing certificate) |

## Archived

- [Roadmap revision 1](archive/roadmap-v1.md)
- [Roadmap revision 2](archive/roadmap-v2.md)
- [Implementation plan revision 1](archive/implementation-plan-v1.md)
- [Implementation plan revision 2](archive/implementation-plan-v2.md)

Archived documents are read-only references: do not update them to reflect current work.
