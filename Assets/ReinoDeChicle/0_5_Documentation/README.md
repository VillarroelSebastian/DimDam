# Mundo 1 — Reino de Chicle

## Jugar

Con Unity **6000.6.5f1**, pulsar **Play**: el Editor inicia directamente `Candy_Level_01` (Praderas de Caramelo). El primer destino de Build Settings también es ese nivel. Pulsar **Enter** para comenzar. **M** abre el mapa desde el menú y permite elegir niveles desbloqueados.

El arranque del Editor está fijado mediante `EditorSceneManager.playModeStartScene`, para evitar iniciar accidentalmente el prototipo anterior. Para depurar otra escena directamente se puede quitar temporalmente esa propiedad desde una herramienta de Editor. El mapa respeta los desbloqueos. El progreso se conserva al cambiar de escena durante la sesión de Play y se reinicia al salir de ella con la configuración normal de recarga de dominio de Unity.

| Jugador | Movimiento | Salto variable | Bajar de semisólida | Dash |
|---|---|---|---|---|
| Mora (gota morada) | A / D | W | S | Shift izquierdo |
| Lima (gota verde) | ← / → | ↑ | ↓ | Ctrl derecho |

Después de victoria: **N** o botón Siguiente. En las pantallas de inicio y resultado: **M** o botón Mapa. Durante una partida o después de victoria/derrota: **R** para reiniciar; también hay botón al terminar. El enlace entre ambos es la única arma: no hay ataque individual, cuerda física, rebotes, modos de disparo ni daño del rayo a jugadores.

Cada jugador tiene tres corazones; uno puede reaparecer tras tres segundos si el otro sobrevive. Si ambos están muertos a la vez, termina la partida. El rayo se apaga mientras falta un jugador. La puntuación es compartida por ambos en el nivel y el combo recompensa eliminaciones rápidas.

## Los cinco niveles

| Escena | Enseñanza y composición |
|---|---|
| `Candy_Level_01` | Praderas de Caramelo. Suelo amplio, tres islas fáciles, caminantes de caramelo y gomitas saltarinas. Sin peligros ambientales. |
| `Candy_Level_02` | Bosque de Gomitas. Dos plataformas verdes impulsan automáticamente al aterrizar; rutas altas e inferiores y gomitas voladoras con oscilación predecible. |
| `Candy_Level_03` | Fábrica de Dulces. Cintas en sentidos opuestos, isla móvil que transporta jugadores, prensas con aviso amarillo y ventana de peligro roja; cañones de azúcar anunciados. |
| `Candy_Level_04` | Fortaleza de Nube y Cacao. Malvaviscos que tiemblan, desaparecen a los 0,8 segundos de pisarlos y vuelven a los 3,8 segundos; refugio central estable, plataformas móviles, guardias de cacao y presión ambiental lateral. |
| `Candy_Level_05_Boss` | Trono del Rey Burbuja. Dos encuentros preparatorios y combate final; galerías a varias alturas, lluvia de perlas y núcleo de menta que cambia de altura en tres fases. |

Cada escena tiene tres oleadas. La progresión cambia trayectorias, superficies y tiempos de posicionamiento, en lugar de limitarse a multiplicar enemigos o vida. Los proyectiles y prensas dañan por contacto; los personajes siguen sin ataques propios.

El texto recibido para esta ampliación terminaba al comenzar la descripción de la fábrica. Las cintas, prensas, fortaleza de malvavisco/cacao y diseño detallado del jefe completan esa parte mediante decisiones de diseño de este prototipo.

## Jefe: Rey Burbuja

El cuerpo es sólido, causa contacto y está protegido. **Solo el collider circular del núcleo de menta acepta el rayo**, durante las ventanas en que brilla. Colocar un jugador abajo y otro en una isla permite cruzar ese punto. La protección no puede saltarse por atravesar otra parte del cuerpo.

- Fase 1: desplazamiento lento y núcleo bajo; lluvia de tres perlas cada cuatro segundos.
- Fase 2, bajo 66% de vida: núcleo más alto y oscilante; movimiento más amplio, perlas cada tres segundos.
- Fase 3, bajo 33%: núcleo recorre alturas bajas y medias; desplazamiento más rápido y abanico más abierto.

El HUD muestra vida, fase e indicación de apertura. Las dos galerías y los escalones laterales siguen accesibles y el jefe no ocupa permanentemente un límite de la arena.

## Carpetas numeradas

- `0_0_Scenes`: cinco niveles y mapa.
- `0_1_Scripts`: definición de nivel, progreso de sesión, superficies, prensas, enemigos especiales, proyectiles y jefe.
- `0_2_Prefabs`: siete enemigos del mundo.
- `0_3_Art`: cinco fondos, siete criaturas y cinco texturas de plataformas.
- `0_4_Editor`: constructor del mundo.
- `0_5_Documentation`: guía e informe.
- `0_6_QA`: suite optativa y evidencias reales.
- `0_7_LevelData`: cinco ScriptableObjects editables con oleadas y puntos de aparición.

El prototipo anterior permanece archivado en `Assets/Prototype_CoopBeam/0_0_Scenes/Prototype_CoopBeam.unity`, fuera de Build Settings. Se reutilizan sus controladores, salud, rayo y prefabs de jugadores, ahora con los sprites nuevos. No se cambió URP/Input System. Build Settings contiene únicamente los seis destinos del Reino de Chicle, con el nivel 1 primero.

## Arte y límites

La presentación activa es **pixel art para todo el juego**: nueve personajes con hojas de animación y **96 fotogramas**, cinco fondos y un logotipo originales generados mediante la herramienta de imágenes. Cada protagonista tiene 20 poses y cada enemigo/jefe, 8. El estado real selecciona respiración/parpadeo, carrera, salto, caída, aterrizaje, dash, daño y muerte; los enemigos anticipan y atacan. Las plataformas, corazones, partículas, nubes, engranajes y tipografía bitmap son recursos propios dibujados por código. Los recursos están en `0_3_Art/PixelArt/`. Hay filtro Point, superficies repetidas y animadas, viewport de 800×450, interfaz bitmap, destellos y ecos de dash. El escenario tiene motas, burbujas y decoración en movimiento. El núcleo del jefe se dibuja aparte para reflejar su vulnerabilidad real. Véase `PIXEL_ART_DIRECTION.md`.

No requiere instalar paquetes nuevos. Usa el Input System y URP ya presentes. La cámara es ortográfica y muestra la arena completa, ajustando el margen según la relación de aspecto. El progreso es de sesión; no se implementó guardado permanente ni multijugador online.

## Construcción y pruebas

El mundo ya está generado. `Coop Beam > Build Candy World` regenera las escenas, datos y prefabs del mundo: guardar una copia antes de usarlo si se han hecho ediciones manuales. Conserva el prototipo previo.

`CandyWorldQA` es una prueba optativa, sin instancias en las escenas finales. Añadirla a un objeto vacío durante Play desde el nivel 1 permite recorrer y resolver automáticamente el mundo. Comprueba consultas reales del rayo, las quince oleadas, desbloqueos, superficies, prensas y fases del jefe. Para acelerar el recorrido aplica intervalos grandes de DPS con la misma consulta del rayo y aparta a los jugadores durante la resolución de oleadas. Esto valida integración y estados, **no demuestra que un equipo humano haya completado la campaña**.

Las suites originales verifican controles simultáneos mediante un teclado virtual del Input System, salto/dash, rutas de plataformas, daño multiblanco/vertical/diagonal, muerte, reaparición y reinicio. El ghosting de un teclado físico y el balance cooperativo necesitan probarse con dos personas.

No se hicieron commits ni pushes.


## Gotas y escenario vivo

Mora y Lima usan hojas originales generadas con image_gen: 20 fotogramas por personaje. Incluyen idle, carrera, salto, caida, aterrizaje, dash, impacto, muerte y cuatro poses de victoria. Los prefabs internos Coral/Celeste conservan sus nombres para mantener referencias.

Tras ganar, celebran durante 1,6 segundos y ascienden durante 2,2 segundos. El siguiente mapa se carga y tiene una entrada de 0,9 segundos; despues se restauran controles y empieza la partida. Tras el jefe se vuelve al mapa del mundo.

Las flores abren sus petalos, las maquinas animan pistones y vapor, y las fuentes cambian chorros y salpicaduras. Son bucles de cuatro imagenes generadas, adicionales a cintas, gelatinas, engranajes, plataformas moviles y prensas. La decoracion no tiene colliders que obstaculicen el combate.
