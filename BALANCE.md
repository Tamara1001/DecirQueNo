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
| Tal vez | +3.750 | -6 |
| No | -500 | +8 |
| Notificación que expira | -1.500 | +2 |

Los mensajes de insistencia multiplican los efectos por ×1.5.

## Niveles del "No"
| Nivel | Salud | Palabras del No | Distractores | Escala de botones |
|---|---|---|---|---|
| 1 | ≥ 80 | 3 | 4 | 1,00 |
| 2 | 65–79 | 5 | 6 | 0,85 |
| 3 | 50–64 | 8 | 8 | 0,70 |
| 4 | < 50 | 12 | 10 | 0,60 |

- Palabras del Sí: 1. Palabras del Tal vez: 2 a 4 según el mensaje.
- Solo es válida la frase del No del nivel actual.

## Timer de respuesta
`segundos = 9 + 6 × (salud / 100)`, entre 9 y 15 s. Con salud 45, unos 11,7 s.

## Finales
| Final | Condición |
|---|---|
| Feliz | salud ≥ 65 y al menos 5 "No" dichos |
| Triste | salud < 30 |
| Neutro | el resto |

Una notificación expirada **no** cuenta como "No" dicho.

## Chequeo de sanidad (a verificar con bots)
- Todo "Sí": 14 × 7.500 + 10.000 = 115.000 seguidores y salud al piso (0). Final triste con la meta lograda.
- Todo "No" posible: cada "No" real sube 8 de salud, así que de 45 a ≥ 65 hacen falta 3 aciertos; los 5 "No" para el final feliz entran bien en 14 mensajes.
- El primer "No" es de nivel 4 (12 palabras en ~11,7 s): tiene que ser lograble por una jugadora rápida y casi imposible para una promedio.

## Qué ajustar si...
- Nadie logra el primer "No": bajar palabras del nivel 4, subir salud inicial, o subir el timer base.
- Todos logran los "No" desde el principio: subir palabras, achicar botones, subir distractores.
- Ignorar todo da buen resultado: subir el costo de seguidores de la expiración o bajar su salud.
- El juego se inflama por la insistencia: limitar a una por remitente.
