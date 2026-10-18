# BALANCE — valores tuneables

Todos estos valores viven en `GameConfig` (ScriptableObject) y no se hardcodean. Son un **punto de partida** que se valida con los bots (`BOTS.md`) y a mano. Última revisión del diseño: 2026-10-17.

## Globales
| Variable | Valor inicial |
|---|---|
| Seguidores iniciales | 10.000 |
| Meta de seguidores | 100.000 |
| Salud inicial | 45 |
| Salud mínima / máxima | 0 / 100 |
| Días | 3 |
| Mensajes por día | 4, 5 y 5 (14 en total, elegidos de un pool de ~20) |
| Duración del día | 150 s (reloj 09:00–23:00) |
| Vida de una notificación | 30 s |

## Efectos por respuesta
| Respuesta | Seguidores | Salud |
|---|---|---|
| Sí | +7.500 | -12 |
| Tal vez | 0 | -6 |
| No | -500 | +8 |
| Notificación que expira | -1.500 | +2 |

Los mensajes de insistencia multiplican los efectos por ×1.5.

**Racha de "No":** cada "No" seguido después del primero suma `+2` de salud extra, hasta `+8` (`noStreakBonusHealth`, `noStreakBonusCap`).

## Niveles del "No"
| Nivel | Salud | Palabras del No | Distractores | Escala de botones |
|---|---|---|---|---|
| 1 | ≥ 80 | 2 | 2 | 1,00 |
| 2 | 65–79 | 4 | 3 | 0,85 |
| 3 | 50–64 | 6 | 4 | 0,70 |
| 4 | < 50 | 8 | 5 | 0,60 |

- Palabras del Sí: 1. Palabras del Tal vez: 2 a 4 según el mensaje.
- Solo es válida la frase del No del nivel actual.

## Timer de respuesta
`segundos = 45 + 15 × (salud / 100)`, entre 45 y 60 s. Con salud 45, unos 51,8 s.

## Finales
| Final | Condición |
|---|---|
| Feliz | salud ≥ 65 y al menos 5 "No" dichos |
| Triste | salud < 30 |
| Neutro | el resto |

Una notificación expirada **no** cuenta como "No" dicho.

## Chequeo de sanidad (a verificar con bots)
- Todo "Sí": la meta (100.000) se alcanza con el "Sí" número 12 y la partida **termina en ese momento**. Salud al piso (0), final triste.
- "Tal vez" no da seguidores: evadir cuesta salud y no hace crecer.
- Todo "No" posible: cada "No" real sube 8 de salud, así que de 45 a ≥ 65 hacen falta 3 aciertos; los 5 "No" para el final feliz entran bien en 14 mensajes.
- El primer "No" de la partida es una práctica con el largo del nivel 1 (2 palabras). Después el nivel depende de la salud (nivel 4 al arrancar: 8 palabras en ~52 s).

## Qué ajustar si...
- Nadie logra el primer "No": bajar palabras del nivel 4, subir salud inicial, o subir el timer base.
- Todos logran los "No" desde el principio: subir palabras, achicar botones, subir distractores.
- Ignorar todo da buen resultado: subir el costo de seguidores de la expiración o bajar su salud.
- El juego se inflama por la insistencia: limitar a una por remitente.
