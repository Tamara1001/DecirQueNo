# STRUCTURE — arquitectura del proyecto

Unity 6, 2D URP, uGUI + TextMeshPro, Input System nuevo (`com.unity.inputsystem` 1.19). Build final: **WebGL** para itch.io.

## Carpetas
```
Assets/
  _Project/
    Scenes/        MainMenu, Game, Sleep, Ending
    Scripts/
      Core/        GameManager, GameState, GameConfig
      Data/        DayData, MessageData, PhraseData (ScriptableObjects)
      Gameplay/    DayController, NotificationSystem, ConversationController,
                   PhraseBuilder, PhraseValidator, ResponseTimer
      Stats/       PlayerStats (seguidores, salud)
      UI/          PhoneUI, NotificationItemUI, ConversationUI, WordButtonUI,
                   MonitorUI, SleepUI, EndingUI, MainMenuUI
    Data/          Configs y ScriptableObjects (GameConfig, Day_1..3, mensajes)
    Prefabs/
    Art/Scenes/    Capas de Game (monitor, aro con celular, micrófono) y cuarto/espejo de Sleep
    Resources/     GameConfig, Audio/ (BGM y SFX .ogg), Characters/ (Sam día/noche), UI/ (burbujas de chat)
```
Tests: `Assets/_Project/Tests/PlayMode/` (assembly `DecirQueNo.Tests.PlayMode`, con los bots en `Bots/`). El código del juego es el assembly `DecirQueNo` (`Assets/_Project/Scripts/DecirQueNo.asmdef`). Ver `BOTS.md`.

Todo el contenido propio va dentro de `Assets/_Project/`. No tocar `Assets/Settings` ni `TutorialInfo`.

## Cursor
`GameCursor` (UI) se instancia con `RuntimeInitializeOnLoadMethod`, vive entre escenas y usa el pack de Kenney (ver `ASSETS.md`). Requiere la referencia `Unity.InputSystem` en `DecirQueNo.asmdef`.

## Escenas
| Escena | Contenido |
|---|---|
| MainMenu | Botón Jugar |
| Instructions | Texto de cómo se juega y botón Empezar (inicia la partida) |
| Game | Celular, aro de luz, monitor de seguidores. Corre el día |
| Sleep | Resumen del día + espejo según salud |
| Ending | Final feliz / triste |

Flujo: `MainMenu → Instructions → Game (día N) → Sleep → Game (día N+1) … → Ending → MainMenu` (o Reiniciar, que vuelve directo a `Game` con una partida nueva).

## Estado persistente entre escenas
`GameManager` (singleton, `DontDestroyOnLoad`) guarda: día actual, `PlayerStats`, mensajes pendientes (insistencias) y el resumen del día. No hay save/load a disco.

## Datos (ScriptableObjects)
- **GameConfig:** valores de BALANCE.md y lista de días. Vive en `Assets/_Project/Resources/GameConfig.asset` y `GameManager` lo carga con `Resources.Load`.
- **DayData:** número de día, tiempo límite de respuesta, lista de `MessageData` en orden de llegada.
- **MessageData:** `sender` ("Nombre_1"), `timeAgo` ("Hace 1"), `body`, `wordBank` (lista de palabras), `phrases` (lista de `PhraseData`), efectos por tipo si difieren de los globales, `pressure` (nivel de insistencia).
- **PhraseData:** `words` (lista ordenada de palabras) + `type` (`Yes` / `Maybe` / `No`).

## Sistemas
- **DayController:** programa la llegada de mensajes según el guion, detecta fin del día, resuelve pendientes como "Sí".
- **NotificationSystem:** lista de mensajes sin responder; evento `OnNotificationAdded`.
- **ConversationController:** abre un mensaje, arranca `ResponseTimer`, recibe la frase del `PhraseBuilder`.
- **PhraseBuilder:** mantiene las palabras elegidas; agregar/sacar por click.
- **PhraseValidator:** compara la frase con `phrases` del mensaje; devuelve tipo o inválida. Bloquea "No" si la salud ≥ umbral.
- **ResponseTimer:** cuenta atrás, evento `OnTimeout` ⇒ respuesta "Sí" por default.
- **AudioManager:** música por escena (menú, buena/poca salud mezcladas según la salud, ganar/perder) y SFX (`AudioManager.Play(Sfx.X)`). Clips en `Resources/Audio`; volúmenes en `GameConfig`.
- **ChatBubbleView:** historial de la conversación con burbujas por remitente; reduce la letra hasta que entra en el panel.
- **PlayerStats:** seguidores y salud; eventos `OnFollowersChanged`, `OnHealthChanged`.

## Comunicación
Sistemas expuestos por eventos C# (`event Action<…>`); la UI se suscribe y no contiene lógica de juego. Sin singletons salvo `GameManager`.

## Input
Solo puntero (mouse/touch) mediante `UnityEngine.UI` Button + `InputSystemUIInputModule`. Sin acciones personalizadas en el MVP.

## WebGL
- Sin `System.Threading`, sin guardado a disco, sin botón "Salir".
- Compresión Brotli/Gzip según hosting de itch.io; Decompression Fallback activado.
- Resolución de referencia 1920×1080 (16:9) con `CanvasScaler` Scale With Screen Size.
