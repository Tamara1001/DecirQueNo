# BUILD — WebGL para itch.io

Una herramienta arma la build WebGL y la deja **zippeada** lista para subir a itch.io. Código: `Assets/_Project/Editor/WebGLBuildTool.cs`.

**Resultado:** `Builds/DecirQueNo-WebGL.zip` (con `index.html` en la raíz del zip).

## Formas de usarla
| Cómo | Cuándo |
|---|---|
| Menú de Unity: **DecirQueNo > Build > WebGL for itch.io (zip)** | La más rápida con el editor abierto. Al terminar abre la carpeta del zip. |
| `powershell -ExecutionPolicy Bypass -File Tools\build-webgl.ps1` | Sin abrir Unity (hay que **cerrar el editor** antes). Log en `Builds/build-webgl.log`. |
| Desde Claude: aplicar ajustes, `build` (target WebGL, `Builds/WebGL`) y después `WebGLBuildTool.Zip()` | Cuando lo pedís en la sesión. |
| Solo zippear la última build: **DecirQueNo > Build > Zip last WebGL build** | Si ya hay una build en `Builds/WebGL`. |

La primera vez cambia la plataforma activa a WebGL y reimporta el proyecto (tarda más). Las siguientes son más rápidas.

## Qué configura
- Compresión **Gzip** con *Decompression Fallback* activado (funciona en itch.io sin configurar headers).
- Resolución 1280×720.
- Soporte de excepciones apagado (build más chica) y cache de datos activado.
- Escenas: las habilitadas en *Build Settings* (MainMenu, Game, Sleep, Ending).

## Subir a itch.io
1. En itch.io: *Create new project* → *Kind of project:* **HTML**.
2. Subir `Builds/DecirQueNo-WebGL.zip` y marcar **This file will be played in the browser**.
3. *Embed options:* viewport **1280 × 720**, activar **Fullscreen button**.
4. Visibilidad: **Draft** o **Restricted** hasta probarla. Verla con *View page* antes de publicar.

## Si algo falla
- El zip dice "No WebGL build found": corré primero la build (el zip se arma desde `Builds/WebGL`).
- Pantalla en negro en el navegador: abrir la consola del navegador (F12) y revisar errores; probar con otro navegador.
- El `.ps1` no encuentra Unity: definir la variable de entorno `UNITY_EXE` con la ruta completa a `Unity.exe`.
- `Builds/` está en `.gitignore`; no se commitea.

## Qué incluye hoy
La build refleja el estado actual del juego. Mientras se implementa el diseño nuevo (ver `MVP.md`), la salud sigue apareciendo como texto DEBUG en el monitor.
