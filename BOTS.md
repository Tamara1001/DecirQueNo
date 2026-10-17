# BOTS — jugadores automáticos

Bots que juegan la partida completa con el **Unity Test Framework** (PlayMode). Sirven para detectar bugs de flujo y medir el balance sin jugar a mano.

## Cómo funcionan
- Cargan la escena `MainMenu`, hacen click en **Jugar** y juegan los días hasta la escena `Ending`.
- Usan la UI real: notificaciones, palabras del banco y **Enviar** (`WordButtonUI.Click`, `NotificationItemUI.Click`, `ConversationUI.ClickSend`). Si el cableado de la UI se rompe, los tests fallan.
- Un bot decide **qué** responder (`IBotStrategy.Choose`: Sí, Tal vez o No). `BotPlayer` toma de `ConversationController.Bank.Options` la frase de ese tipo para el nivel de "No" actual.
- **Tiempo humano:** cada bot tiene `ReactionSeconds` (piensa) y `ClickSeconds` (por palabra). Antes de clickear espera `reacción + click × palabras`. Si el timer de respuesta vence antes, el juego envía "Sí" y el bot cuenta un *timeout*.
- El tiempo corre a `timeScale 30` y `maximumDeltaTime 1`. Los 3 tests tardan unos 40 s.
- Cada test guarda su reporte en `BotReports/<Bot>.txt` y lo imprime con el prefijo `[BotReport]`.

## Los bots (reglas del "No" difícil)
| Bot | Estrategia | Reacción / click |
|---|---|---|
| **Influencer** | Siempre "Sí". | 1 s / 0,5 s |
| **MentalHealth** | Siempre intenta el "No", rápido y sin errores. | 1,5 s / 0,4 s |
| **Balanced** | Con salud < 55 intenta el "No" si la frase entra en el tiempo; si no, "Sí" cuando va atrasado en seguidores y "Tal vez" cuando va adelantado. | 2 s / 0,6 s |
| **Average** | Siempre intenta el "No", pero como una persona: piensa 4 s y tarda 1,2 s por palabra. Si no puede lograr un "No", el juego es demasiado difícil. | 4 s / 1,2 s |
| **Ignorer** | Nunca abre una notificación: todo expira. Sirve para que ignorar no sea un atajo. | — |

## Qué verifica cada test
Todos: el juego termina en un final, se juegan 1 a 3 días, salud y seguidores no son negativos y no hay timeouts.

| Test | Verifica además |
|---|---|
| `InfluencerBot_BurnsOut` | Final **triste**. |
| `MentalHealthBot_GetsHappyEnding` | Al menos 5 "No" y final **feliz**. |
| `AverageBot_CanSayNo` | Al menos 5 "No" a ritmo humano, sin timeouts, y final **feliz**. |
| `IgnorerBot_DoesNotWin` | Todo expira, no responde nada y el final **no** es feliz. |
| `BalancedBot_FinishesTheGame` | Solo lo común (el resultado se documenta). |

## Cómo correrlos
- Unity: menú **DecirQueNo > Tests > Run PlayMode bots** (escribe el resultado en `BotReports/summary.txt`), o `Window > General > Test Runner > PlayMode > Run All`.
- Desde Claude: `BotTestRunner.Run()` (por RunCommand) y leer `BotReports/summary.txt`.
- Código en `Assets/_Project/Tests/PlayMode/` (assembly `DecirQueNo.Tests.PlayMode`).

## Últimos resultados
Fecha: 2026-10-17. "No" difícil + notificaciones que expiran + pool mezclado por semilla (Fase 2), valores de `BALANCE.md`. 4 de 4 tests en verde.

| Bot | Final | Seguidores | Salud | Sí / Tal vez / No / Expiradas |
|---|---|---|---|---|
| Influencer | Triste | 115.000 / 100.000 | 0 | 14 / 0 / 0 / 0 |
| MentalHealth | Feliz | 0 / 100.000 | 100 | 0 / 0 / 28 / 0 |
| Balanced | Neutro | 84.250 / 100.000 | 45 | 9 / 0 / 14 / 0 |
| Ignorer | Neutro | 0 / 100.000 | 100 | 0 / 0 / 0 / 28 |
| Average | Feliz | 0 / 100.000 | 100 | 0 / 0 / 28 / 0 |

Actualizado tras el primer playtest ("No" de 2 a 8 palabras, timer de 45 a 60 s, práctica, racha de "No"). 5 de 5 tests en verde. Resultados del Balanced: Neutro, 81.000 seguidores, salud 53.

**Hallazgo nuevo:** con el "No" más fácil, decir siempre "No" lleva a salud 100 en el día 1 (los 8 primeros "No" dan +55) y deja los seguidores en 0. Es el camino más seguro y barato; si se quiere más tensión hay que subir el costo en seguidores del "No" o hacer que el "No" siga siendo parte de la partida con más esfuerzo.

## Hallazgos de balance
1. **El Influencer cumple el diseño:** llega a 115.000 con la salud al piso y final triste. La meta ahora es alcanzable.
2. **El "No" es trivial para un bot perfecto.** El MentalHealth arma frases de 12 palabras en ~6 s sin fallar nunca y termina con salud 100. Esto no dice nada sobre una persona: falta el bot **Average** (clicks más lentos, errores) para ver si el nivel 4 es "casi imposible" para una jugadora promedio.
3. **Ganar sin seguidores:** el MentalHealth termina feliz con 0 seguidores (el "No" resta -500 y la insistencia -750). Por diseño los seguidores no deciden el final; hay que decidir si eso es lo que queremos.
4. **La insistencia duplica la partida:** cada "No" suma un mensaje extra, así que el MentalHealth respondió 28 mensajes y el Balanced 23, contra los 14 del guion.
5. **El Balanced logra un camino intermedio** (final neutro, 84.250 seguidores, salud 45), lo que sugiere que mezclar "Sí" y "No" da un resultado distinto de los extremos.
6. **Ignorer: no gana, pero cobra salud gratis.** Con todo expirado termina con salud 100 y 0 seguidores, final neutro (no llega a 5 "No"). Los -1.500 seguidores por expirar lo dejan sin seguidores, pero la salud sube igual. Hay que decidir si alcanza con eso o si la expiración debe dar menos salud (hoy +2 por mensaje, +3 en insistencias).
7. **El orden del guion no cambia los totales:** el pool se mezcla por partida, pero los bots dan los mismos resultados porque cada mensaje tiene los mismos efectos. La variación se nota en el orden y en los distractores, no en los números.
8. Pendientes: bot **Slow** (timeout) y bot **Average** (clicks lentos y errores).

## Cómo mantenerlos
- Cambiaste un número de `GameConfig` / `BALANCE.md`, el guion o las reglas: corré los tests y actualizá resultados y hallazgos.
- Cambiaste la UI (botones por escena): ajustá `BotPlayer` (`ClickOnlyButton` espera exactamente 1 botón en Menú, Dormir y Final).
- Agregaste una mecánica nueva: sumá un caso a `BotContext` / estrategias.
- Cambiaste los finales: actualizá las aserciones de `BotPlaythroughTests`.
