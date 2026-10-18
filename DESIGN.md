# DESIGN — Decir Que No (MVP)

Prototipo 2D para validar el **core loop**. Todo con cuadrados default de Unity, sin arte, sin narrativa, sin audio.

## Premisa
Sos una influencer estancada. Una influencer más importante te propone cosas dudosas. Querés llegar a **100.000 seguidores** sin destruir tu **salud mental**. La moraleja: **aprender a decir que no**.

## Idea central
- Decir **"Sí" es fácil**: una palabra ("Dale"). Insinuar que sí ("Tal vez") es apenas más largo.
- Decir **"No" siempre se puede, pero es difícil**: hay que armar una frase larga ("respuesta larga rechazando"). Con poca salud mental es casi imposible dentro del tiempo.
- Cuando la jugadora logra un "No", sube la salud y el próximo "No" **se acorta**. Si se queda en el "Sí", la salud baja y el "No" se vuelve **más largo, con botones más chicos y más distractores**.
- Por eso es rejugable: cada partida es un intento distinto de aprender a decir que no.

## Pilares
1. **Velocidad + skill:** armar la frase bajo presión de tiempo.
2. **Decisión:** el camino fácil (Sí) o el camino con esfuerzo (No).
3. **Espiral:** el esfuerzo del "No" depende de cómo te cuidaste antes.

## Core loop
1. Arranca el día.
2. Llegan notificaciones según el guion de la partida.
3. Elegís una, se abre la conversación y arranca el timer.
4. Armás la frase (click en palabras) y enviás.
5. Se aplican seguidores y salud. La salud cambia la dificultad del próximo "No".
6. Termina el día.
7. Pantalla de dormir: resumen del día + espejo que refleja la salud.
8. Siguiente día, hasta el día 3. Al final: ending y estadísticas.

**Meta alcanzada:** al llegar a 100.000 seguidores la partida termina en ese momento (si hay una conversación abierta, se termina de responder primero) y va directo a dormir/final.

## Estructura
- **3 días**, ~2,5 minutos cada uno, 4–5 mensajes por día.
- Los mensajes de la partida salen de un pool y se **mezclan en cada partida** (ver "Rejugabilidad").
- Meta de seguidores: 100.000.
- Salud inicial: **45** (la influencer ya viene desgastada). Mínimo 0, máximo 100. No baja de 0 y se juegan siempre los 3 días.

## Mensajes y respuestas
Cada mensaje es una **orden** del NPC con número de misión: "Hacé misión_3" (placeholder). La etiqueta "Hace N min" se calcula con el reloj del celu (de a 10 minutos) y avanza mientras la notificación espera; "Ahora" si recién llegó.

Cada mensaje define tres tipos de respuesta:
| Tipo | Largo | Ejemplo placeholder |
|---|---|---|
| **Sí** | 1 palabra | `Si` |
| **Tal vez** | variable según el mensaje (2 a 4 palabras) | `Tal vez` |
| **No** | 4 niveles: 2, 4, 6 o 8 palabras | `No_1 No_2 No_3 …` |

Solo la frase del nivel de "No" actual es válida; las demás longitudes no se pueden armar porque sus palabras no están en el banco.

## Nivel del "No" (dificultad)
La **salud** determina el nivel:
| Nivel | Salud | Palabras del No | Distractores | Tamaño de botones |
|---|---|---|---|---|
| 1 (fácil) | ≥ 80 | 2 | 2 | 100 % |
| 2 | 65–79 | 4 | 3 | 85 % |
| 3 | 50–64 | 6 | 4 | 70 % |
| 4 (el más difícil) | < 50 | 8 | 5 | 60 % |

Como la salud arranca en 45, **el primer "No" es del nivel 4**: se puede lograr, pero hay que ser rápida y precisa. Cada "No" real sube la salud (y puede bajar el nivel); cada "Sí" la baja (y puede subirlo).
Los números exactos están en BALANCE.md.

## Timer
- Depende de la salud: más salud, más tiempo. Rango de **45 a 60 s** (fórmula en BALANCE.md). La presión no viene de armar rápido sino del reloj del día: mientras pensás, las otras notificaciones siguen venciendo.
- Arranca al abrir la conversación. La barra se vacía de derecha a izquierda además de cambiar de color. Si llega a 0 se envía "Sí" por default.

## Que sea mental, no de velocidad
Después del primer playtest (el "No" de 12 palabras en ~12 s era inalcanzable) se ajustó para que decidir pese más que hacer clicks rápido:
- **Leyenda de esfuerzo:** al abrir un mensaje se ve cuántas palabras pide cada respuesta ("Palabras que hay que armar para responder: Si: 1 palabra / Tal vez: 3 palabras / No: 6 palabras"). La decisión es cuánto esfuerzo vale cada una.
- **Práctica:** el primer mensaje que abrís en la partida siempre tiene el "No" más corto (2 palabras) y lo avisa, para descubrirlo y lograrlo una vez.
- **Aviso de nivel:** cuando la salud sube o baja y cambia el nivel del "No", aparece "Decir que no te cuesta menos / más" al costado de la pantalla.
- **Bonus por racha:** cada "No" seguido después del primero da +2 de salud extra (hasta +8). Un "Sí" o "Tal vez" corta la racha; una expiración no.
- **Reloj en decenas:** el reloj del día muestra solo decenas de minutos (09:10, 09:20…), sin minutos exactos.

## Mecánica de respuesta
- Pantalla de conversación: historial arriba, área de armado de frase abajo, banco de palabras, barra de timer, botón Enviar.
- **Click** en una palabra del banco la agrega a la frase; click en una palabra de la frase la saca.
- **Frase válida:** coincide exactamente con la frase de Sí, de Tal vez o del No del nivel actual. Si no, Enviar queda bloqueado.
- **Banco:** palabras de las tres respuestas válidas + distractores. El tamaño de los botones y la cantidad de distractores dependen del nivel.
- **Varias notificaciones:** podés abrir cualquiera; las demás siguen corriendo su vida útil (ver abajo).

## Efectos
| Respuesta | Seguidores | Salud |
|---|---|---|
| Sí | ++ | -- |
| Tal vez | 0 (evadir no hace crecer) | mitad del Sí |
| No | - (leve) | ++ |
| Notificación que expira | - (más que un No) | + (poco) |

## Notificaciones que expiran
- Cada notificación vive **30 s** (tuneable) mientras no se abre. Se muestra una barrita que se acorta.
- Cada cambio del contador de seguidores muestra debajo del número cuánto cambió y por qué ("-1.500 (Nombre_1 sin abrir)"), porque puede cambiar con otra conversación abierta.
- Si expira, desaparece y cuenta como una **mezcla de "No"**: resta **más seguidores** que un "No", suma **poca salud** y **dispara la insistencia**. No cuenta como un "No" dicho para el final.
- Es la forma de evitar una situación sin esfuerzo, pero sale cara: no es un atajo al final feliz.
- Los mensajes de insistencia que expiran no generan una insistencia nueva.

## Insistencia
Un "No" (o una expiración) agenda un mensaje extra del mismo remitente, más insistente (efectos ×1.5), en el mismo día si queda tiempo, o al inicio del día siguiente.

## Fin del día
- Termina cuando ya llegaron todos los mensajes y no queda ninguno pendiente, o cuando el reloj llega a las 23:00 (150 s). Una conversación abierta no se corta por el reloj.
- Reloj visible de 09:00 a 23:00 y día actual siempre a la vista.

## Finales (al terminar el día 3)
Tres finales según salud y cantidad de "No" dichos:
- **Feliz:** salud ≥ 65 **y** al menos 5 "No" dichos.
- **Triste:** salud < 30.
- **Neutro:** el resto.
Los seguidores aparecen en el resumen pero no deciden el final.

## Estadísticas finales
Pantalla con: cantidad de Sí / Tal vez / No / expiradas, mejor racha de "No", tiempo promedio de respuesta, nivel de "No" final, salud, seguidores y la semilla de la partida.

## Rejugabilidad
- Pool de mensajes (~20 placeholders). Cada partida elige 14 y los reparte en los 3 días con una **semilla al azar**.
- Las palabras de los distractores y su orden en el banco cambian por partida.
- Estadísticas al final para comparar partidas.

## Pantallas
1. **Menú principal:** solo *Jugar*.
2. **Instrucciones:** 3 o 4 líneas placeholder y *Empezar*.
3. **Juego:**
   - Celular al centro con lista de notificaciones o conversación.
   - De fondo: aro de luz (cuadrado) y monitor de PC con el contador de seguidores.
   - Día y hora siempre visibles.
   - La salud **no** se muestra con números; se nota en el tamaño de los botones, la viñeta oscura y el espejo.
4. **Dormir:** resumen del día, espejo (cuadrado que cambia de color con la salud), velador y cama (cuadrados).
5. **Final:** final feliz / neutro / triste, estadísticas, *Reiniciar* y *Volver al menú*.

Formato: **16:9 landscape**.

## Placeholders
- Textos: `Si`, `Tal vez`, `No_1…No_12`, distractores genéricos (`Palabra_1`).
- Remitentes: `Nombre_1`, `Nombre_2`. Órdenes: `Hacé misión_N`. Hora: "Hace N".

## Fuera de scope (MVP)
Arte, animación, audio, narrativa real, ramas de personajes, ajustes, créditos, pausa, save/load, localización.

## Preguntas abiertas
- ¿Hay que volver a pensar el nivel 4 si ningún jugador logra el primer "No"? Se mide con bots y a mano.
- ¿Se recuerda algo entre partidas? Por ahora no.
- ¿Entra un momento especial (destello) al bajar de nivel el "No"?
