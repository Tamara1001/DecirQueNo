# ROADMAP

Objetivo: tener el core loop jugable lo antes posible para ver si es divertido. El detalle del MVP para itch.io está en `MVP.md`.

## Hecho
- **Hito 0 — Setup:** carpetas, escenas en el Build, config WebGL (Gzip + fallback, 1280×720).
- **Hito 1 — Datos y estado:** `GameConfig`, `DayData`, `MessageData`, `PhraseData`, `PlayerStats`, `GameManager`.
- **Hito 2 — Puzzle de frase:** armado por click, validador Sí / Tal vez / No, timer con timeout a "Sí".
- **Hito 3 — Día completo:** notificaciones, `DayController`, reloj 09:00–23:00, efectos, monitor de seguidores, "No" permanente, insistencia, 14 mensajes placeholder.
- **Stubs:** menú, dormir y final con texto y botón.
- **Bots:** Influencer, MentalHealth y Balanced juegan la partida completa por la UI (`BOTS.md`).

## En curso: MVP para itch.io
Diseño actualizado el 2026-10-17: el "No" siempre existe pero es una frase larga cuya dificultad depende de la salud. Detalle en `DESIGN.md` y `MVP.md`.

| Fase | Contenido | Estado |
|---|---|---|
| 0 | `.gitignore` y dos commits (URP viejo / juego) | Pendiente |
| 1 | Nuevo núcleo del "No": datos por mensaje, niveles según salud, botones y banco por nivel, timer por salud, salud inicial 45 | **Hecho** (2026-10-17) |
| 2 | Expiración de notificaciones, pool con semilla y mensajes mezclados, estadísticas, órdenes y tres finales | **Hecho** (2026-10-17) |
| 3 | Bots: Influencer, MentalHealth, Balanced e Ignorer hechos; faltan Average y Slow | Parcial |
| 4 | Instrucciones, espejo en Sleep, finales con estadísticas y Reiniciar | **Hecho** (2026-10-17), falta ojo humano |
| 5 | Game feel simple | **Hecho** (2026-10-17), falta ojo humano |
| 6 | Build WebGL probada en navegador (la herramienta de build ya existe, ver `BUILD.md`) | Parcial |
| 7 | Borrador privado en itch.io, revisión final | Pendiente |

## Validación antes de publicar
- Los bots en verde en cada fase.
- Una persona (vos) juega un día completo en el editor y la build alojada, de menú a final.
- Sin playtest con terceros antes de publicar; el feedback se junta después.

## Después del MVP (no ahora)
Arte, audio, narrativa real, ramas de personajes, ajustes, créditos, pausa, save/load, localización, playtest con terceros, versión mobile.
