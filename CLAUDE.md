# Decir Que No — contexto para Claude

Aventura narrativa 2D en Unity 6 (URP 2D). Prototipo del core loop con el guion real de los guionistas; build final WebGL para itch.io.

## Leer antes de tocar código
- `DESIGN.md` — mecánicas y finales.
- `GUION.md` — guion transcripto del tablero de Miro (el tablero manda).
- `BALANCE.md` — números tuneables.
- `STRUCTURE.md` — carpetas, escenas, sistemas.
- `CODESTYLE.md` — convenciones.
- `ROADMAP.md` — hitos y estado.
- `MVP.md` — definición del MVP para itch.io y plan por fases.
- `BUILD.md` — cómo armar la build WebGL zippeada (menú **DecirQueNo > Build > WebGL for itch.io (zip)**, o `Tools/build-webgl.ps1`).
- `BOTS.md` — bots de test (Influencer, MentalHealth, Balanced), resultados y hallazgos de balance.

Después de tocar reglas, balance, guion o UI: correr los tests de PlayMode (menú **DecirQueNo > Tests > Run PlayMode bots**, resultado en `BotReports/summary.txt`) y actualizar `BOTS.md`.

## Reglas del proyecto
- Los **textos son los del guion** (`GUION.md`); se generan con `ScriptAssetsBuilder` (menú **DecirQueNo > Build > Generate script assets**). No editar a mano los assets de `Data/Messages`.
- El arte sigue siendo placeholder: cuadrados default, sin audio.
- Emojis: el guion los usa, pero se ocultan en pantalla (`GameConfig.showEmojis`) hasta tener una fuente TMP que los soporte en WebGL.
- Código en inglés, docs en español.
- uGUI + TextMeshPro + Input System nuevo; solo puntero (mouse/touch).
- Código propio en `Assets/_Project/`.
- Compatible con WebGL: sin hilos, sin guardado a disco, sin "Salir".
- Valores de balance en `GameConfig`, no hardcodeados.
- No commitear sin que se pida.
