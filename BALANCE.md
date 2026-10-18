# BALANCE — valores tuneables

Todos estos valores viven en `GameConfig` (ScriptableObject) y se generan desde `ScriptAssetsBuilder` (menú **DecirQueNo > Build > Generate script assets**). Son un **punto de partida** acordado con el equipo; todavía no se balancearon con bots ni a mano. Última revisión: 2026-10-18.

## Globales
| Variable | Valor inicial |
|---|---|
| Seguidores iniciales | 320 |
| Meta de seguidores | 1.000.000 |
| Cordura inicial | 70 (rango 0 a 100) |
| Días | 3, de 3 mensajes (9 en total, en orden fijo) |
| Vida de una notificación | 30 s |
| Pausa entre mensajes | 3 s al empezar el día, 5 s después |

## Efectos por nivel del antagonista
| Nivel | ✅ Sí: seguidores | ✅ Sí: cordura | 🚨 Evasiva: seguidores | 🚨 Evasiva: cordura |
|---|---|---|---|---|
| 1 (Valerie) | +30.000 | -15 | -3.000 | 0 |
| 2 (Christian) | +100.000 | -20 | -10.000 | +5 ("No tibio") |
| 3 (VIP) | +230.000 | -25 | -23.000 | +5 ("No tibio") |

- **No absoluto** (solo mensaje final): -100.000 seguidores, +30 cordura. Cierra la partida.
- **Notificación que vence:** seguidores de la evasiva × 1,5, cordura sin cambio.
- **Se acaba el tiempo:** se envía el Sí.

## Dificultad por cordura
| Cordura | Escala de botones |
|---|---|
| ≥ 70 | 1,00 |
| 40–69 | 0,80 |
| < 40 | 0,60 |

Timer de respuesta: `segundos = 20 + 40 × (cordura / 100)`, entre 20 y 60 s.

## Chequeo de sanidad
- **Todo "Sí":** tras 8 mensajes hay ~850.000 seguidores; con el noveno se pasa 1.000.000 y la partida termina (final "No dije que no"). La cordura queda en 0.
- **Todo evasiva y No final:** la cordura sube a ~95 (nivel 1 no suma, niveles 2 y 3 suman 5 cada uno) y se llega al No con el timer casi completo. Termina con pocos seguidores.
- **Ignorar todo:** los 9 mensajes vencen; pierde seguidores y no llega al final "Decir que no".

## Qué ajustar si...
- Nadie logra armar el No final: subir `timerMinSeconds`, bajar la pérdida de cordura del Sí o subir la cordura inicial.
- Decir "No" no cuesta nada (el final es gratis): subir lo que se pierde con la evasiva o bajar la cordura que suma.
- Se llega a la meta antes del último mensaje: bajar las ganancias del Sí del nivel 3. La meta no debería alcanzarse antes del mensaje 3.3.
