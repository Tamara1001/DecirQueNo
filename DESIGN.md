# DESIGN — Decir Que No

Aventura narrativa de terror psicológico en 2D. Sos **Sam**, una influencer que acaba de viralizarse. Una influencer famosa, su manager y un patrocinador te llevan, de a poco, a ceder cada vez más. La moraleja: **aprender a decir que no**.

El guion completo (textos, personajes, árboles) está en `GUION.md`, transcripto del tablero de Miro de los guionistas. **El tablero manda.** Los números están en `BALANCE.md`.

## Idea central
- Cada mensaje trae **más de una frase posible**. Se arman con **fragmentos** (1 a 3 palabras, con emojis) que se tocan en orden. Elegir cuál armar es la decisión.
- **Sí (✅):** la frase que complace. Da seguidores y cuesta cordura.
- **Evasiva (🚨):** la "casi negativa". Excusa o rodeo; el antagonista insiste. Hace perder seguidores.
- **No real:** solo existe en el último mensaje del juego. Es larga y es el cierre de la historia.
- Cada nivel es un antagonista nuevo: la presión sube y Sam se degrada.

## Estructura
- **3 días, 3 mensajes por día**, siempre los mismos y en el mismo orden (9 árboles del tablero).

| Día | Nivel | Antagonista | Tono |
|---|---|---|---|
| 1 | 1 | Valerie | Condescendiente, emojis, ultimátums. No hay No: solo Sí o evasiva |
| 2 | 2 | Christian | Paternal, culpa, contratos. La evasiva es el "No tibio" |
| 3 | 3 | Contacto VIP | Frío, sin emojis. La evasiva falla; el último mensaje trae el No absoluto |

- Meta: **1.000.000 de seguidores** (se empieza con 320). Si se llega, la partida termina en ese momento.
- Los mensajes llegan **de a uno**: el siguiente aparece unos segundos después de responder o dejar vencer el anterior. El día termina al terminar sus 3 mensajes; el reloj del celu es solo ambiente.

## Core loop
1. Arranca el día. Llega una notificación (Valerie / Christian / VIP).
2. La abrís: ves el mensaje y arranca el timer.
3. Armás una de las frases con los fragmentos y enviás.
4. Se aplican seguidores y cordura. Se ve lo que mandó Sam y **la respuesta del antagonista**; se cierra sola o con *Continuar*.
5. Siguiente mensaje. Tras el tercero: pantalla de dormir (resumen + espejo) y siguiente día.
6. Último mensaje (día 3): el **final**.

## Cordura (oculta)
- Empieza en 70, de 0 a 100. No se muestra con números: se nota en el **tamaño de los botones**, el **tiempo para responder**, la viñeta y el espejo.
- Más cordura, más tiempo (de 20 a 60 s) y botones más grandes (de 60 % a 100 %).
- Aviso "Decir que no te cuesta menos / más" cuando cambia el escalón de dificultad.
- La evasiva de los niveles 2 y 3 (el "No tibio") **suma cordura**; el Sí la baja.

## Mensajes y respuestas
Cada mensaje define: texto entrante, frase Sí, frase evasiva (salvo el final), frase No (solo el final) y la respuesta del antagonista a cada una.

- **Banco:** es la unión de las frases, mezclada por partida. Sin distractores.
- **Frase válida:** coincide exactamente con una de las frases del mensaje. Si no, Enviar queda bloqueado.
- Si se acaba el tiempo se envía el **Sí** por default.
- La etiqueta "Hace N min" sigue el reloj del celu.
- Los **emojis** no se muestran hasta tener una fuente TMP que los soporte (`GameConfig.showEmojis`).

## Notificaciones que vencen
- Cada notificación vive 30 s sin abrirse (barrita que se acorta).
- Si vence: cuenta como una evasiva con **50 % más de costo en seguidores**, sin cambio de cordura y sin respuesta del antagonista. No cuenta como No.

## Estructura del contador
Cada cambio de seguidores muestra debajo del número cuánto cambió y por qué ("-1.500 (Valerie sin abrir)").

## Finales
Dos, como en el tablero ("Final de la ruta"):
- **Decir que no:** se dice el No absoluto del último mensaje.
- **No dije que no:** se cede en el último mensaje, o éste vence.

Los seguidores se muestran pero no deciden el final.

## Tutorial
- Pantalla de instrucciones con el *storytelling inicial* y el *cómo jugar* del tablero.
- En la primera conversación de la partida aparece un recordatorio de cómo armar y enviar la frase.

## Pantallas
1. **Menú principal:** *Jugar*.
2. **Instrucciones:** storytelling inicial y cómo jugar.
3. **Juego:** celular al centro (notificaciones o conversación), aro de luz, monitor con el contador de seguidores, día y hora a la vista.
4. **Dormir:** resumen del día y espejo (cuadrado que cambia de color con la cordura).
5. **Final:** uno de los dos finales, estadísticas, *Reiniciar* y *Volver al menú*.

Formato: **16:9 landscape**.

## Todavía sin hacer
- **Degradación visual y de tipeo de Sam** por nivel (aspecto físico, escritura sin emojis, en minúsculas, con errores). Hoy solo se escribe en los textos del guion.
- Arte, animación y audio.
- **Emojis en pantalla:** falta una fuente TMP que los incluya y funcione en WebGL.
- Corregir con los guionistas los bancos del tablero que no cuadran (ver `GUION.md`).
