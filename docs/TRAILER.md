# TRAILER — gameplay grabado y montaje

El trailer se arma en dos pasos: un "bot humano" graba el juego dentro del editor y un script de Python/ffmpeg monta el video. Los archivos generados van a `Trailer/` (ignorada por git).

## 1. Grabar el gameplay
Código: `Assets/_Project/Editor/TrailerRecorder.cs`.

1. Abrir la escena **MainMenu**.
2. Menú **DecirQueNo > Trailer > Record gameplay**. Fija la Game view en 1920×1080, entra en Play y juega solo (~15 min; el juego corre a 24 fps fijos).
3. Salida: `Trailer/frames/fNNNNNN.jpg` y `Trailer/markers.txt` (frame donde empieza cada escena: `menu`, `instructions`, `day1`, `sleep1`, `day2_low`, `sleep2_low`, `sleep_*`/`ending_*` para `sad`, `neutral` y `happy`).

Cómo juega:
- Mueve el mouse real por el Input System (hover y clics reales) con curvas, temblor y pausas. Si un clic no cae, lo hace directo para no trabar la toma.
- Arma frases con dudas: una palabra equivocada que borra y, en el primer mensaje, un orden mal que corrige **arrastrando** el bloque con el mouse (`DragToStart`).
- Día 2 con la salud bajada a mano para mostrar el efecto de poca salud mental.
- Los finales se fuerzan con estadísticas armadas (`PlayerStats.Apply`) y `GameManager.EndDay(true)`; por eso los números de los finales no son de una partida real.
- Las capturas no incluyen el cursor del sistema: se dibuja un cursor encima con las mismas imágenes de Kenney.

Notas: la captura es a resolución real de la Game view (no usar `superSize`, escala una imagen chica y se ve pixelada). Cerrar otras ventanas del editor no hace falta, pero el editor tiene que estar en foco razonable.

## 2. Montar el video
Script: `Tools/make_trailer.py` (trabaja dentro de `Trailer/`) (necesita `ffmpeg` y `Pillow`; la tipografía sale de `Resources/Fonts/Fredoka.ttf`).

```bash
python Tools/make_trailer.py
```

Resultado: `Trailer/trailer.mp4` (1920×1080, 24 fps, ~71 s). Estructura:
título → menú → tutorial (acelerado) → día 1 → tarjetas de Valerie, Christian y VIP → día 2 con poca salud → noche → final malo → neutral → bueno → cierre.

- Subtítulos con la estética del juego (tarjeta lila redondeada, borde azul marino, Fredoka), generados con PIL.
- Música del juego: `BGM_Menu`, `BGM_Poca_Salud` y `BGM_Ganar` (esta última se repite en bucle para cubrir los finales y el cierre). Sonido de notificación en las tarjetas.
- La foto de Valerie de las tarjetas es `Trailer/valerie_new.png` (no es el avatar del juego).
- Los cortes se calculan a partir de `markers.txt`; si cambia el ritmo de la grabación, ajustar los offsets en el script.
