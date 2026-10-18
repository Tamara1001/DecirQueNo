# BALANCE — valores tuneables

Viven en `GameConfig` y se generan desde `ScriptAssetsBuilder` (menú **DecirQueNo > Build > Generate script assets**). Punto de partida, sin balancear a fondo. Última revisión: 2026-10-18.

## Globales
Seguidores iniciales 320 · meta 1.000.000 · cordura inicial 70 (0–100, oculta) · 3 días de 3 mensajes · notificación vive 30 s · tiempo para responder 40 s (igual para todos).

## Efectos por nivel
| Nivel | Sí: seguidores / cordura | Tal vez: seguidores / cordura | Evasiva: seguidores / cordura |
|---|---|---|---|
| 1 (Valerie) | +120.000 / -15 | +60.000 / -7 | -3.000 / 0 |
| 2 (Christian) | +140.000 / -20 | +70.000 / -10 | -5.000 / +5 |
| 3 (VIP) | +155.000 / -25 | +77.500 / -12 | -10.000 / +5 |

- **Tal vez** = la mitad del Sí (como en la mecánica original).
- **No final:** -100.000 seguidores, +30 cordura, cierra la partida.
- **Notificación que vence:** seguidores de la evasiva × 1,5. **Se acaba el tiempo:** se envía el Sí.

## Meta del millón
Con ~70 % de Sí (6–7 de 9) y el resto Tal vez (como mucho una evasiva) se llega al millón; con 5 Sí no, y con todo Tal vez se queda en ~620.000.

## Cuánto cuesta decir que no
La cordura solo cambia el **tamaño y el orden de los bloques** (el tiempo no cambia):
| Cordura | Bloques | Orden |
|---|---|---|
| ≥ 70 | 1,00 | Frases del No (evasiva y No) juntas y en orden |
| 40–69 | 0,80 | Juntas pero desordenadas |
| < 40 | 0,60 | Mezcladas con las demás |

El Sí y el Tal vez siempre salen juntos y en orden.

## Finales
Ver `GUION.md`: bueno (final No + máx. 3 Sí + al menos 5 de 9 con evasiva/No), malo (llegó al millón), intermedio (el resto).
