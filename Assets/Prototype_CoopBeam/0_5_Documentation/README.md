# Lazo de Luz — prototipo cooperativo local

## Ejecutar

Unity **6000.6.5f1**. Abrir `Assets/Prototype_CoopBeam/0_0_Scenes/Prototype_CoopBeam.unity`, pulsar **Play**, enfocar la pestaña Game y pulsar **Enter** o el botón Comenzar. Dos personas comparten teclado; no necesita mandos, paquetes nuevos ni servicios externos.

| Acción | Coral | Celeste |
|---|---|---|
| Movimiento | A / D | ← / → |
| Salto variable | W (mantener para mayor altura) | ↑ (mantener) |
| Bajar de plataforma | S | ↓ |
| Dash horizontal | Shift izquierdo | Ctrl derecho |
| Reiniciar al terminar | R | R |

El segmento entre los personajes es la **única fuente de daño**. Rodear enemigos o situarse a distintas alturas para atravesarlos. El rayo atraviesa plataformas y varios enemigos. Coincidir en un mismo punto no produce daño.

Cada criatura tiene tres corazones, breve invulnerabilidad y pequeño retroceso. Una criatura derrotada reaparece tras tres segundos si la otra sigue viva; el enlace se apaga durante su muerte. Si ambas mueren a la vez, Game Over. Completar las tres oleadas da la victoria. Las eliminaciones dentro de tres segundos aumentan el combo y la puntuación compartida.

## Organización

- `0_0_Scenes`: escena independiente; no se modificó SampleScene.
- `0_1_Scripts`: movimiento, vida, rayo, enemigos, partida/oleadas, HUD, cámara y efectos.
- `0_2_Prefabs`: Coral, Celeste, Walker, Jumper, Chaser y Armored.
- `0_3_Art`: seis PNG originales transparentes, texturas auxiliares, material sin iluminación y fondo original generado con IA.
- `0_4_Editor`: constructor reproducible de escena y sprites.
- `0_5_Documentation`: esta guía y resultados resumidos.
- `0_6_QA`: pruebas optativas y evidencias; ningún componente QA está en la escena entregada.

## Enemigos

- **Sprout / Walker**: verde, patrulla y gira ante bordes y obstáculos; 27 PV.
- **Jelly / Jumper**: violeta, salta periódicamente y busca cambiar de altura; 48 PV.
- **Comet / Chaser**: dorado, alterna jugador objetivo cada tres segundos, corre y salta hacia objetivos más altos; 42 PV.
- **Shell / Armored**: azul grisáceo, grande, lento y con 150 PV; se tiñe rojo antes de una embestida horizontal. Todo su collider corporal es vulnerable al enlace, incluso durante la embestida.

Oleadas: 3 caminantes; 2 caminantes + 2 saltarines; 2 perseguidores + 1 blindado + 2 caminantes.

## Parámetros y arquitectura

En el Inspector: `PlayerController2D` configura velocidad, aceleración, salto y dash; `PlayerHealth` configura corazones y reaparición; `CoopBeam` configura DPS (38); los prefabs enemigos configuran velocidad y vida; `GameManager` configura ventana y límite del combo.

La detección del rayo utiliza `Physics2D.Linecast` con filtro de capa 10 y un buffer de 64 impactos reutilizado, deduplicando enemigos antes de aplicar DPS por intervalo fijo. Las capas numéricas 8/9/10/11 corresponden a suelo, semisólidas, enemigos y jugadores dentro de esta escena; no se alteró TagManager ni la matriz de colisión global del proyecto. El buffer es suficiente para las oleadas de hasta cinco enemigos; debe ampliarse para niveles mucho mayores.

No se cambió URP ni el sistema de entrada. El proyecto conserva URP 17.6 con renderizador general y usa sprites sin iluminación. La cámara adapta el encuadre a la relación de aspecto. El HUD usa IMGUI y admite teclado y botones; es una implementación de prototipo. Las animaciones idle, run, jump, fall, dash, hurt y death se producen por deformación, inclinación, transparencia y partículas; no son clips de Animator. El audio consiste en tonos sintetizados originales.

## Reconstrucción y QA

La escena ya está construida; **no es necesario reconstruirla para jugar**. `Coop Beam > Build Prototype` regenera los assets del prototipo y reemplaza su escena/prefabs. Guardar previamente la escena abierta. No usar esta opción sobre una versión del prototipo que se haya editado manualmente sin hacer antes una copia.

Para repetir las pruebas optativas, entrar en Play con una partida nueva y añadir `PrototypeRuntimeQA` a un objeto vacío. Ejecuta entrada simulada del Input System, consultas reales de física para el rayo y resolución de oleadas. Reiniciar Play y añadir `PrototypeHealthQA` para contacto, invulnerabilidad, muerte, reaparición, derrota y recarga de escena. Los resultados se escriben en `0_6_QA/Results`. No añadir ambas suites a la vez. Mantener Game activo o habilitar `Application.runInBackground` durante QA remota.

## Alcance

Prototipo local en teclado, sin red. El arte es original, con fondo generado y personajes dibujados por código exportados a PNG. Las animaciones y la interfaz son básicas. El ghosting de ciertos teclados puede limitar combinaciones de teclas físicas; las pruebas de entradas virtuales no certifican el hardware del usuario. La dificultad y el disfrute cooperativo necesitan una sesión humana de dos personas.

No se hicieron commits ni pushes; el directorio inicial no tenía Git.
