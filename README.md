# DimDam - Reino de Chicle

Juego arcade cooperativo local 2D para dos personas. Mora y Lima son gotas morada y verde conectadas por un rayo: posicionar ese segmento sobre enemigos es la unica forma de hacerles dano.

## Ejecutar

1. Abrir el proyecto desde Unity Hub con Unity **6000.6.5f1**.
2. Esperar la restauracion de paquetes e importacion de assets.
3. Abrir `Assets/ReinoDeChicle/0_0_Scenes/Candy_Level_01.unity`.
4. Pulsar Play y luego Enter.

| Personaje | Movimiento | Salto | Bajar | Dash |
|---|---|---|---|---|
| Mora | A/D | W | S | Shift izquierdo |
| Lima | Flechas izquierda/derecha | Flecha arriba | Flecha abajo | Ctrl derecho |

R reinicia la partida; M abre el mapa desde el menu. Cinco niveles, tres oleadas por nivel y jefe final. Las victorias celebran y trasladan a los protagonistas al siguiente escenario automaticamente.

## Estructura

`Assets/ReinoDeChicle/` contiene carpetas numeradas para escenas, scripts, prefabs, arte, herramientas de Editor, documentacion, QA y datos de niveles. `Assets/Prototype_CoopBeam/` conserva sistemas compartidos y el prototipo anterior.

URP e Input System se restauran desde `Packages/manifest.json` y `Packages/packages-lock.json`. Los archivos `.meta` forman parte del proyecto y deben conservarse.

## Pruebas y alcance

Consultar `Assets/ReinoDeChicle/0_5_Documentation/QA_REPORT.md` y `Assets/ReinoDeChicle/0_6_QA/Results/`. Las pruebas se ejecutaron mediante Unity Editor; no se ha probado una build standalone ni el ghosting de un teclado fisico. Progreso durante la sesion, sin multijugador online.

El arte combina imagenes originales generadas con image_gen y elementos pixel art creados por codigo; su procedencia esta registrada en `Assets/ReinoDeChicle/0_3_Art/PixelArt/AssetManifest.json`.

## Expansiones locales: Hielo y Fuego
La campaña contiene ahora tres reinos de cinco arenas cada uno. Para probar Fuego directamente, abrir `Assets/ReinoDeFuego/0_0_Scenes/Fire_Level_01.unity`, Play y Enter. En campaña se desbloquea al completar Hielo; TAB cambia de reino en el mapa.
Cada enemigo derrotado deja botín visible que se recoge tocándolo o cruzándolo con el rayo. Hay Prisma, Burbuja, Corazón, Chispa Encadenada y Anillo Solar; el HUD muestra sus temporizadores. La ulti compartida Resonancia se confirma con Q y Shift derecho.
Guías: [Hielo y Resonancia](Assets/CoopExpansion/Documentation/EXPANSION.md), [Fuego y botín visible](Assets/ReinoDeFuego/0_5_Documentation/REINO_DE_FUEGO.md). Arte y prompts de Fuego: `Assets/ReinoDeFuego/0_3_Art/`. Resultados de pruebas: `Assets/ReinoDeFuego/0_6_QA/`.
