# Still Typing... (antes "Decir Que No")

Aventura narrativa 2D de terror psicológico hecha en Unity 6 para la **Women Game Jam 2026**.

Sos **Sam**, una influencer que acaba de viralizarse. Una influencer famosa, su manager y un patrocinador te llevan, de a poco, a ceder cada vez más. Cada mensaje trae más de una respuesta posible y se arma con fragmentos de frase, bajo presión de tiempo. La moraleja: **aprender a decir que no**.

## Cómo se juega
- **3 días, 3 mensajes por día**, con un antagonista nuevo en cada nivel (Valerie, Christian y un contacto VIP).
- Cada respuesta se arma tocando fragmentos de 1 a 3 palabras en orden. Elegir qué frase armar es la decisión:
  **Sí** (complace, da seguidores y cuesta cordura), **evasiva** (la "casi negativa", el antagonista insiste) o **No** (solo existe en el último mensaje).
- La **cordura** es oculta: no se muestra con números, se nota en el tamaño de los botones, el tiempo para responder y la viñeta de la pantalla.
- Hay dos finales: **decir que no** o **no dije que no**.

Las reglas completas están en [`DESIGN.md`](DESIGN.md) y los números tuneables en [`BALANCE.md`](BALANCE.md).

## Tecnología
- Unity 6 (6000.3), URP 2D.
- uGUI + TextMeshPro + Input System nuevo, solo puntero (mouse y touch).
- Build WebGL para itch.io: sin hilos, sin guardado a disco.
- Tests de PlayMode con tres bots (Influencer, MentalHealth y Balanced) para validar el balance. Ver [`BOTS.md`](BOTS.md).

## Documentación
| Archivo | Contenido |
|---|---|
| [`DESIGN.md`](DESIGN.md) | Mecánicas y finales |
| [`GUION.md`](GUION.md) | Guion transcripto del tablero de Miro del equipo |
| [`BALANCE.md`](BALANCE.md) | Valores de balance |
| [`STRUCTURE.md`](STRUCTURE.md) | Carpetas, escenas y sistemas |
| [`CODESTYLE.md`](CODESTYLE.md) | Convenciones de código |
| [`MVP.md`](MVP.md), [`ROADMAP.md`](ROADMAP.md) | Alcance y estado |
| [`BUILD.md`](BUILD.md) | Cómo armar la build WebGL |
| [`TRAILER.md`](TRAILER.md) | Cómo se graba el gameplay para el trailer |

## Probarlo en el editor
1. Abrir el proyecto con Unity 6000.3 (ver `ProjectSettings/ProjectVersion.txt`).
2. Abrir la escena `Assets/_Project/Scenes/MainMenu.unity` y darle Play.
3. Para tests: menú **DecirQueNo > Tests > Run PlayMode bots**.
4. Para la build web: menú **DecirQueNo > Build > WebGL for itch.io (zip)**.

## Equipo
| Rol | Persona |
|---|---|
| Programación | Tamara D'Angelo ([@Tamara1001](https://github.com/Tamara1001)) |
| Game Design | Eugenia Vullioud |
| Arte 2D, UI/UX | Massiel Colín, Jessie, Aldana |
| Narrativa | Manuel Martinez Miraglia, Jessica Briseño |
| Audio para videojuegos | Oriana Oseguera |

## Licencias
- El **código** (scripts C#, tests y `Tools/`) está bajo licencia MIT. Ver [`LICENSE`](LICENSE).
- El **arte, el audio y el guion** pertenecen a sus autoras y autores y se incluyen con su permiso. No están cubiertos por la licencia MIT ni se pueden reutilizar sin autorización.
- Iconos: [Material Icons](https://github.com/google/material-design-icons) (Apache 2.0). Cursores: [Cursor Pack de Kenney](https://kenney.nl/assets/cursor-pack) (CC0).
