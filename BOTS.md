# BOTS — jugadores automáticos

Bots que juegan la partida completa con el **Unity Test Framework** (PlayMode). Sirven para detectar bugs de flujo y comprobar los finales sin jugar a mano.

## Cómo funcionan
- Cargan `MainMenu`, hacen click en **Jugar** y juegan los días hasta la escena `Ending`, usando la UI real (notificaciones, bloques del banco, **Enviar**).
- Un bot decide **qué** responder (`IBotStrategy.Choose`: Sí, Tal vez, Evasiva o No). `BotPlayer` toma la frase de ese tipo; si no existe usa la más cercana (No → Evasiva → Tal vez → Sí).
- Tiempo humano: `ReactionSeconds` (piensa) y `ClickSeconds` (por bloque). El tiempo corre a `timeScale 30`.
- Cada test guarda `BotReports/<Bot>.txt`.

## Los bots
| Bot | Respuestas | Final esperado |
|---|---|---|
| **Influencer** | Siempre Sí | Malo (llega al millón) |
| **GoalChaser** | Sí, Sí, Tal vez ×3 (~70 % Sí) | Malo (llega al millón) |
| **Hesitant** | Siempre Tal vez | Intermedio (no llega al millón) |
| **Balanced** | Sí, Sí, Tal vez, 4 evasivas intercaladas y el No final | Bueno |
| **MentalHealth** | Siempre intenta el No (8 evasivas + No final), rápido | Bueno |
| **Average** | Igual, pero lento (piensa 4 s, 1,2 s por bloque) | Bueno |
| **Ignorer** | Nunca abre nada | Intermedio |

`ScriptBankTests` valida los datos del guion (frases armables, ninguna prefijo de otra, orden de días) y que el banco cambie con la cordura (juntas y en orden / desordenadas / mezcladas).

## Cómo correrlos
- Unity: menú **DecirQueNo > Tests > Run PlayMode bots** (resultado en `BotReports/summary.txt`) o Test Runner > PlayMode.
- Código en `Assets/_Project/Tests/PlayMode/`.

## Estado
2026-10-18: guion del tablero con "Tal vez", tres finales, dificultad solo en tamaño/orden de bloques. Una sola corrida de verificación, **sin balanceo fino** (ver `BALANCE.md`).

## Cómo mantenerlos
Si cambian los números de `GameConfig`, el guion o los finales: correr los tests y ajustar las aserciones de `BotPlaythroughTests`.
