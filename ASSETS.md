# ASSETS — arte y audio

Fuente: carpeta de Drive "Carpeta WGJ 2026". Los originales se descargan a `DriveAssets/` (no se versiona) y se importan reducidos.

## Mapa
| Drive | Proyecto | Uso |
|---|---|---|
| `Escenarios/Gameplay/` fondo_monitor, celular_luz, microfono | `Art/Scenes/Game_*.png` (1920x1080, capas del mismo lienzo) | Escena Game: fondo + micrófono + aro con celular (el panel del celular es la UI real) |
| `Escenarios/CamaEspejo/` Fondo-cuarto, Espejo | `Art/Scenes/Room_*.png` | Escena Sleep: cuarto + marco del espejo encima de Sam |
| `Expresiones/Sam_{Feliz,Blush,Triste}_{Dia,Sombra}` | `Resources/Characters/` (724x1024) | Espejo de Sleep. Dia = mañana, Sombra = noche. Feliz/Blush/Triste según los tramos de cordura (los mismos de los niveles del No) |
| `Concept/` bocetos y Sam SAD (solo línea) | no se importan (solo `Sam_Triste_Lineart.png` en `Resources/Characters/`, sin uso) | Referencia |
| `UI/cuadros-texto-mensajes/` vip, cristian, valerie, protagonista | `Resources/UI/Bubble_*.png` (9-slice) | Burbujas del chat: remitente recibido a la izquierda, Sam (protagonista) a la derecha |
| `Audio/BGM/*.wav` | `Resources/Audio/BGM_*.ogg` | Menu = menú/instrucciones; Buena_Salud / Poca_Salud = Game y Sleep según salud; Ganar / Perder = Ending |
| `Audio/SFX/*.wav` | `Resources/Audio/SFX_*.ogg` (recortados al sonido útil) | Notificación = llega mensaje; Escribir = cada palabra; Enviar_Mensaje = enviar; Selección_Menú = botones de menú |
| `celular/` celular, fondo-celular, bottom-menu-celular | `Art/Phone/` (sin reducir) | Panel del celular en Game: pantalla blanca + marco encima + barra inferior |
| `banner-notificaciones/` cristian, valerie, vip | `Resources/UI/Banner_*.png` (256 px, 9-slice) | Fondo de cada notificación según remitente |

## Decisiones
- Sleep tiene dos fases: **noche** (resumen + espejo "Sombra", fondo oscurecido) y **mañana** (espejo "Dia"). Tras el último día va directo al final.
- Música de salud: Buena_Salud y Poca_Salud suenan a la vez y sincronizadas en Game y Sleep. Buena = salud% del volumen de música, Poca = lo que le falta (100% salud = solo Buena, 0% = solo Poca). El volumen sigue a la salud con un suavizado de 1 s.
- Las imágenes de 4961x7016 y 3840x2160 se bajaron a 724x1024 y 1920x1080 para WebGL; BGM de 164 s pasó de 43 MB (wav) a ~2,4 MB (ogg).

## Pendiente
- Mañana del día 1 (antes del primer día) sin espejo todavía.
- Fondo de noche propio (hoy es el mismo cuarto con tinte).
- Skin del panel del celular, botones de palabras y menús con el arte final.

## Iconos de la interfaz
- `Assets/_Project/Resources/UI/Icons/`: Material Icons de Google (licencia Apache 2.0, https://github.com/google/material-design-icons), PNG de 96 px convertidos a blanco para poder teñirlos desde código. Se acreditan en la ventana de Créditos.
- `Resources/UI/Chat_HeaderBar.png`: barra superior de la app de mensajes, de las artistas (Drive, carpeta de UI del celular).
- Quedan en el Drive sin usar: `cuadro-texto-respuesta.png`, `boton-enviar.png` y `boton-enviar-interfaz-celular.png` (el botón de enviar usa el ícono del pack).
