# The Bubblegum Kingdom — La roca fugitiva

Escena: `Assets/BubblegumKingdom/0_0_Scenes/Level_BubblegumKingdom.unity`.
Unity 6000.6.5f1, sin cambios de versión ni paquetes. Acceso: mapa de campaña, botón inferior o B; también abrir la escena y pulsar Play. Se inicia sin esperar Enter.

## Personajes y controles
Se instancian Coral y Celeste, con sus sprites, animaciones, Rigidbody2D, PlayerHealth y PlayerController2D originales. El proyecto no tenía personajes llamados Mimo/Pipo ni diferencias de fuerza/tamaño. Solo en esta escena, Coral representa a Mimo (escala 1.35) y Celeste a Pipo (0.8). Sus colliders conservan la forma original y se escalan con la instancia. Los prefabs originales no se modifican. El sistema existente ignora colisiones entre jugadores: no se usa al compañero como escalón.

Mimo: A/D, W salto, Shift dash, S mecanismos. Pipo: flechas, arriba salto, Ctrl derecho dash, abajo mecanismos. S/abajo conservan además el descenso por plataformas del controlador original. Caminar contra un objeto pesado lo empuja si es Mimo. R recupera juntos a ambos y restaura objetos; M vuelve al mapa.

## Recorrido
A: Pipo pasa por el túnel y conecta el auxiliar; se acciona la palanca; Mimo empuja la roca hasta el otro lado de la compuerta.
B: preparar el bloque en la marca y el desvío verde; Mimo empuja la roca hasta el borde. Al llegar a la bifurcación se evalúan ambas condiciones. Si falla, vuelve al origen. Si acierta, alcanza la placa, abre la puerta y rebota con sonido y partículas.
C: Mimo empuja el escalón hasta la marca; Pipo activa el interruptor del túnel. Las tres plataformas aparecen secuencialmente y permanecen, con oscilación pequeña. Subir por escalón, burbujas y refugio superior.
D: ambos se acercan al fragmento para recogerlo, y después a la salida. Si existe una escena habilitada llamada `Level_TheNeonCore`, se carga. Mientras no exista, muestra el cierre de la aventura sin intentar cargar una escena ausente.

Los puntos de control se confirman cuando los dos pasan la puerta de B y cuando ambos llegan al refugio superior. R conserva los puzles anteriores al control y restablece los pendientes. Una caída por debajo de -4 recupera automáticamente al equipo. Los objetos pesados están limitados a raíles y nunca pueden perderse por física.

## Reutilización e integración
Reutilizados: Coral.prefab, Celeste.prefab, PlayerController2D, PlayerHealth.ReviveAt/ResetMotion, ActorPhysics2D, PixelSpriteAnimator, PixelActorVisual, CandyEnvironment para plataformas móviles y transporte de pasajeros, BeamFX, GameManager.Tone, CandyCampaign.Map y arte de ReinoDeChicle/PixelArt/Tiles.

Nuevos: BubblegumLevel (estados y recuperación), BubblegumHeavy (empuje restringido y raíles), BubblegumCamera (seguimiento compartido), BubblegumBuilder (construcción reproducible, solo editor), BubblegumPlayQA (pruebas explícitas, no está adjunto a la escena guardada).

Modificados: GameManager agrega `explorationOnly` y BeginExploration para habilitar el controlador original sin oleadas ni componentes de combate. La opción vale false en todas las escenas anteriores. CandyWorldMap añade acceso B/botón. EditorBuildSettings agrega la escena al final sin reordenar las existentes. No se sobrescriben escenas anteriores.

El progreso del fragmento se registra con una única clave PlayerPrefs `BubblegumKingdom.FirstFriendshipFragment`; la campaña existente mantiene sus desbloqueos en memoria y no tenía guardado persistente reutilizable para este objeto. La escena puede volver a jugarse aunque la clave ya esté registrada.

## Verificación
Ver `QA/PlayMode-results.txt` para los resultados de la última ejecución automatizada. Las pruebas usan un teclado virtual y reposicionamientos explícitos para aislar mecanismos; no equivalen a una sesión manual completa de dos personas. La configuración temporal de foco del teclado se restaura al finalizar. No se instala el ejecutor QA en la escena.

Resultado final: 29 comprobaciones en Play Mode, 0 fallos, incluidos saltos encadenados de ambos personajes desde el suelo hasta el refugio. Validación de la escena nueva: 0 scripts ausentes y 0 prefabs rotos. Las 16 escenas anteriores registradas en Build Settings se abrieron sin scripts ausentes; Candy_Level_01 arrancó en modo práctica con dos jugadores y sus sistemas de combate originales, sin errores en consola. No se recorrió manualmente toda la campaña. The Neon Core no existe aún: la salida muestra la finalización y conserva preparada la transición futura.
