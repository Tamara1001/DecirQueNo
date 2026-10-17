# CODESTYLE

Objetivo: código simple, legible y fácil de tirar o cambiar. Prototipo, no arquitectura final.

## Idioma
- Código, nombres de clases/variables y comentarios en **inglés**.
- Documentación (`.md`) en español.
- Textos visibles al jugador: placeholders genéricos ("Si", "No", "Tal vez", "Nombre_1", "Hace 1").

## Nombres
- Clases, métodos, propiedades, enums: `PascalCase`.
- Campos privados: `_camelCase`. Parámetros y locales: `camelCase`.
- Campos serializados: `[SerializeField] private` con `_camelCase`.
- Constantes: `PascalCase`.
- Eventos: `OnSomethingHappened`.
- Un archivo por clase, el nombre del archivo igual al de la clase.

## Estructura de una clase
1. Constantes
2. Campos serializados
3. Campos privados
4. Eventos y propiedades públicas
5. Métodos de Unity (`Awake`, `Start`, `Update`…)
6. Métodos públicos
7. Métodos privados

## Reglas
- Namespace único: `DecirQueNo` (con subnamespaces solo si hace falta).
- Lógica separada de UI: la UI escucha eventos y no decide reglas.
- Valores de balance en `GameConfig`, nunca números mágicos en código.
- Sin singletons salvo `GameManager`.
- Sin `FindObjectOfType` en runtime; referencias por `[SerializeField]`.
- Sin `Update` si alcanza con eventos o corrutinas.
- Preferir ScriptableObjects para datos.
- Nada de `async`/hilos (WebGL).
- Comentarios solo cuando el porqué no es obvio.
- No dejar `Debug.Log` sueltos; usar el prefijo `[Tag]` si hay que dejarlos.

## Unity
- Guardar escenas y prefabs; no commitear `Library/`, `Temp/`, `Logs/`.
- Un `Canvas` por escena, con `CanvasScaler` 1920×1080.
- Placeholders visuales: `Image` o `SpriteRenderer` con el sprite Square default y color plano.

## Git
- Commits chicos y descriptivos, en español o inglés pero consistentes.
- Un commit por sistema o hito del ROADMAP.
