# MVP — versión para itch.io

Objetivo: una partida de ~7 minutos (3 días), jugable en el navegador, que permita responder **¿es divertido armar frases bajo presión y descubrir que decir que no cuesta pero se puede aprender?** No busca arte ni narrativa final.

## Estado de partida (2026-10-17)
- Hechos: datos, puzzle de frase, día completo con reloj, notificaciones, insistencia, stubs de menú / dormir / final, 3 bots de PlayMode.
- **Cambio de diseño grande:** el "No" ya no se bloquea; es una frase larga cuya dificultad depende de la salud (ver `DESIGN.md`). Hay que rehacer el modelo de datos de los mensajes, el validador, el banco de palabras y los bots.
- Sin commits desde el registro inicial.

## Reglas definitivas del MVP
| Tema | Decisión |
|---|---|
| Meta | 100.000 seguidores; el "Sí" da +7.500 |
| Salud | Empieza en 45, mínimo 0, máximo 100; no termina la partida |
| "No" | Siempre existe. Largo según la salud: 3 / 5 / 8 / 12 palabras (niveles 1 a 4) |
| Botones y banco | A peor nivel, botones más chicos y más distractores |
| Timer | `9 + 6 × salud/100` s (9 a 15 s) |
| "Sí" / "Tal vez" | 1 palabra / 2 a 4 palabras según el mensaje |
| Notificación sin abrir | Vive 30 s; al expirar resta -1.500 seguidores, suma +2 salud y agenda insistencia. No cuenta como "No" dicho |
| Insistencia | Un "No" o una expiración agenda un mensaje extra ×1.5 |
| Finales | Feliz: salud ≥ 65 y ≥ 5 "No". Triste: salud < 30. Neutro: el resto |
| Rejugabilidad | Pool de ~20 mensajes, 14 por partida, semilla al azar; estadísticas al final |
| Contenido | Órdenes "Hacé misión_N" + etiqueta "Hace N". Todo placeholder |
| Salud en pantalla | Sin números: se nota en botones, viñeta y espejo |
| Audio | Sin audio |
| Extras | Instrucciones antes del día 1; Reiniciar en el final. Sin pausa, ajustes ni créditos |
| Formato | 16:9, WebGL |

## Dentro del MVP
1. Menú → instrucciones → día 1.
2. 3 días con reloj 09:00–23:00, 14 mensajes por partida y sus insistencias.
3. Armado de frase por click con tres tipos de respuesta y nivel de "No" según la salud.
4. Pantalla de dormir con resumen y espejo según la salud.
5. Tres finales con estadísticas y botón Reiniciar.
6. Game feel simple.
7. Build WebGL probada y subida como borrador privado a itch.io.

## Fuera del MVP
Arte, audio, narrativa real, ramas de personajes, ajustes, créditos, pausa, save/load, localización, memoria entre partidas, versión mobile pulida.

## Plan por fases
Cada fase termina con los tests de bots en verde y los docs al día.

### Fase 0 — Orden (corta)
- [ ] `.gitignore` (Library, Temp, Logs, BotReports, builds).
- [ ] Commit 1: cambios viejos de URP / configuración.
- [ ] Commit 2: el juego (`_Project`, docs, tests).
- **Listo cuando:** `git status` limpio y el proyecto abre sin errores.

### Fase 1 — Nuevo núcleo del "No"
- [x] `MessageData`: frase de Sí (1 palabra), de Tal vez (2 a 4) y 12 palabras de No; el nivel actual usa las primeras N. Los distractores salen del pool de `GameConfig`.
- [x] `GameConfig`: niveles del "No", fórmula del timer, salud inicial 45, nuevos efectos y condiciones de final.
- [x] `PlayerStats`: sin desbloqueo del "No"; `NoLevelIndex`; contadores de Sí / Tal vez / No; salud con piso 0 sin fin de partida.
- [x] `BankBuilder`, `ConversationController` y `PhraseValidator`: banco por nivel y validación de la frase del nivel actual.
- [x] `ConversationUI`: botones escalados por nivel (grillas de banco y frase), sin palabras grisadas. Verificado en pantalla: el nivel 4 (26 botones) entra.
- [x] Timer por salud (9 a 15 s).
- [x] Adelantado de la Fase 2: tres finales (feliz / neutro / triste) con `RunSeed` por partida.
- **Listo:** se puede decir Sí, Tal vez y No en los 4 niveles. Los 3 bots pasan (ver `BOTS.md`).

### Fase 2 — Reglas del día y rejugabilidad
- [x] Expiración de notificaciones (30 s, barrita que se achica, -1.500 seguidores / +2 salud / insistencia). Lo que sigue en la lista al cerrar el día también expira.
- [x] Mensajes como órdenes: "Hacé misión_N" + "Hace N".
- [x] Pool de 20 mensajes (`MessagePool`) y partida con semilla: 14 repartidos en 4 / 5 / 5, con distractores y orden del banco mezclados. Los horarios de llegada salen de cada día (`DayData.arrivalTimes`).
- [x] Tres finales (feliz, neutro, triste).
- [x] Estadísticas en la pantalla final: Sí / Tal vez / No / expiradas, mejor racha de "No", timeouts, tiempo promedio de respuesta, nivel de "No" final y semilla.
- [x] Bot Ignorer y runner de tests (`DecirQueNo > Tests > Run PlayMode bots`). 4 de 4 en verde.
- **Falta verificar a mano:** que dos partidas seguidas tengan un guion distinto y cómo se ve la barrita de vida de la notificación.

### Fase 3 — Bots
- [ ] Actualizar a los 3 bots existentes a las reglas nuevas y sumarles un tiempo por click (armar 12 palabras toma tiempo real).
- [ ] **Influencer:** siempre Sí.
- [ ] **Persistent:** siempre intenta el No, rápido; debería aprender y llegar al final feliz.
- [ ] **Average:** intenta el No solo cuando es corto (nivel 1 o 2) y, si no, dice Sí o Tal vez; debería terminar neutro o triste.
- [ ] **Balanced:** mezcla Sí / Tal vez y No según la salud.
- [ ] **Ignorer:** deja expirar todo; no debe ganar.
- [ ] **Slow:** responde tarde para cubrir el timeout.
- **Listo cuando:** los finales esperados por bot se cumplen y el balance está documentado en `BOTS.md`.

### Fase 4 — Pantallas que faltan
- [x] Instrucciones (escena `Instructions` entre el menú y el día 1; texto placeholder y botón Empezar).
- [x] Sleep: espejo (cuadrado) que pasa de rojo oscuro a celeste según la salud, velador y cama (cuadrados) y resumen del día sin mostrar la salud en números.
- [x] Ending: tres finales, estadísticas, Reiniciar y Volver al menú.
- [x] El texto DEBUG de salud solo aparece en el editor y en builds de desarrollo.
- Los bots recorren el flujo completo (menú, instrucciones, juego, dormir y final) y pasan.
- **Falta verificar a mano:** cómo se ven las pantallas Instructions, Sleep y Ending (distribución de cuadrados y textos).

### Fase 5 — Game feel simple
Todo con código (tweens simples con corrutinas), sin assets.
- [x] Botones con hover y click (escala y color).
- [x] Notificación con pop al aparecer, barrita que se achica y fade out al expirar; badge rojo en las insistencias.
- [x] Timer verde → rojo, con latido en los últimos 3 s.
- [x] Contador de seguidores que cuenta hacia arriba con golpe de escala.
- [x] Palabras con pop al pasar a la frase; sacudida de la frase si es inválida.
- [x] Pantalla que se oscurece según la salud (viñeta).
- [x] Fundido a negro entre días y mensaje que "escribe" (...) antes de aparecer.
- Propuesto y no elegido: destello al bajar de nivel el "No".
- Notas: los botones solo cambian de escala (el color lo da el propio botón). La viñeta es un degradé radial negro que se oscurece al bajar la salud. El "..." del mensaje dura 0,4 s y el timer ya corre durante ese tiempo. El fundido entre escenas dura 0,35 s de ida y 0,35 s de vuelta.
- **Falta verificar a mano:** que las animaciones se sientan bien (pop, sacudida, pulso del timer, contador, viñeta y fundidos). Los bots pasan (4 de 4), pero no ven nada.

### Fase 6 — Build WebGL
- [x] Herramienta de build + zip (`BUILD.md`). Primera build con el juego previo al nuevo diseño: `Builds/DecirQueNo-WebGL.zip`, 18,4 MB, sin errores (2026-10-17). Falta probarla en un navegador.
- [x] Cambiar plataforma a WebGL; compresión Gzip + fallback; 1280×720.
- [ ] Revisar tamaño, tiempo de carga y Input System en navegador.
- [ ] Probar en servidor local en Chrome y Firefox.
- [ ] Sin botón "Salir" ni logs de debug.
- **Listo cuando:** una partida completa corre en el navegador sin errores de consola.

### Fase 7 — Itch.io (privado)
- [ ] Zip de la build (`index.html` en la raíz) subido como "HTML, playable in browser".
- [ ] Viewport 1280×720 y pantalla completa activada.
- [ ] Página en borrador / privada: título, descripción corta con cómo se juega, tags, aviso de terror psicológico.
- [ ] Sin playtest con terceros antes de publicar: validan los bots y una pasada tuya con la build alojada.
- **Listo cuando:** vos jugás la versión alojada de punta a punta.

## Riesgos
| Riesgo | Mitigación |
|---|---|
| Nadie logra el primer "No" (12 palabras en ~11,7 s) | Medirlo con el bot Persistent y a mano; ajustar nivel 4, salud inicial o timer |
| El "No" es demasiado fácil y no hay espiral | Bots Average e Influencer deben terminar mal |
| Ignorar todo es un atajo | Bot Ignorer; costo de expiración en `BALANCE.md` |
| Botones chicos + banco largo no entran en el celular | Probar el nivel 4 en pantalla temprano (Fase 1) |
| Build WebGL pesada o con errores | Hacer una build de prueba temprano |
| Cambios de balance rompen aserciones de bots | Correr tests en cada fase y actualizar `BOTS.md` |

## Preguntas abiertas
- ¿Se ajusta la vida de 30 s de las notificaciones? Depende de las pruebas.
- ¿Entra un momento especial al bajar de nivel el "No"?
