# CHICLE RUMBLE (Gum Remix)

Reimaginación independiente del proyecto: run & gun cooperativo al estilo Cuphead con la mecánica de
"atrapar y patear" y de subir de piso de Snow Bros. Vive aislada en `Assets/GumRemix/` (asmdef propio, escena propia);
no toca ningún otro script ni escena del proyecto. Dos mundos: **Reino de Chicle** y **Reino de Hielo** (la versión remix
del Reino de Hielo original: ver `REINO_DE_HIELO_REMIX.md`).

## Jugar
Abre `Assets/GumRemix/0_0_Scenes/GumRemix.unity` y pulsa Play. Si falta la escena: menú **Gum Remix > Create Scene**.

El juego trae su propia ayuda: la primera vez que eliges jugar aparecen tres pantallas (controles, cómo se juega, Reino de Hielo).
Se puede volver a verlas en el menú principal (**COMO JUGAR**), cada nivel muestra un consejo breve arriba, y **Esc** pausa
(con el resumen de controles). En el menú: W/S elegir, A/D cambia de mundo, Enter acepta. Backspace en la pausa vuelve al menú.

| | Jugador 1 (Celeste) | Jugador 2 (Coral) |
|---|---|---|
| Mover | A / D | Flechas izq / der |
| Apuntar arriba (con mover = diagonal) | W | Flecha arriba |
| Saltar (en el aire sobre algo rosa = PARRY) | Espacio | Shift derecho |
| Bajar de una plataforma | S + salto | Flecha abajo + salto |
| Disparar chicle | F | Ctrl derecho |
| Dash (invulnerable) | G | . |
| Super (5 cartas) | H | / |

Mando: stick/cruceta, A salta, X dispara, B/RB dash, Y/LB super. P2 puede entrar en cualquier momento con una de sus teclas.

## Mecánicas comunes
- **3 corazones** por jugador. Al caer te conviertes en fantasma en burbuja: tu compañero te revive tocándote. Si caen los dos, se reintenta el nivel.
- **Atrapar y patear (Snow Bros):** los disparos pegan chicle; al agotar la vida del enemigo queda atrapado en una bola (de chicle en Chicle, de nieve en Hielo). Tócala para patearla: rueda, rebota, revienta a los enemigos que encuentra, rompe cristales y golpea al jefe. Si no la pateas, el enemigo se libera a los 7 s.
- **Parry:** lo **rosa** hace daño, pero saltando sobre ello en el aire lo "parreas": rebotas y ganas una carta. 5 cartas = **Super**, un rayo de chicle que atrapa a todo lo que tiene delante.
- **Subir de nivel:** al eliminar a todos los enemigos una burbuja lleva a los jugadores al piso de arriba; los niveles están apilados y la cámara asciende (los caídos reviven y se cura un corazón).
- **10 niveles y 2 jefes:** cada mundo son 4 niveles que se escalan y un jefe en el quinto. Al vencer a Gran Chiclón se pasa al Reino de Hielo.

## Estructura de carpetas (misma convención que ReinoDeChicle / ReinoDeHielo)
| Carpeta | Contenido |
|---|---|
| `0_0_Scenes` | `GumRemix.unity` (un solo objeto: el arranque). |
| `0_1_Scripts` | Todo el código del juego (`GumGame` flujo/menús/HUD, `GumPlayer`, `GumEnemies`, `GumHazards`, `GumBoss*`, `GumEnv`, `GumArt`, `GumCore` input/audio/texto). |
| `0_2_Prefabs` | `GumRemixBootstrap.prefab`: arrástralo a una escena vacía para arrancar el juego. |
| `0_3_Art` | `Resources/GumRemix/<categoría>/` (PNG, `.meta` y `manifest.json`) y `ArtSource~/` (el generador en Python). |
| `0_4_Editor` | Menú **Gum Remix** (crear escena / prefab). |
| `0_5_Documentation` | Esta guía y `REINO_DE_HIELO_REMIX.md`. |
| `0_6_QA` | Pruebas PlayMode con bot y `Results/` (informe y capturas). |
| `0_7_LevelData` | Los 10 niveles (plataformas, enemigos, cristales, viento, carámbanos) en `GumWorld.cs`. |

## Arte (100% creado por código, nada de IA generativa)
`0_3_Art/ArtSource~/` es el generador (Python + Pillow). Cada sprite se dibuja a resolución nativa con paleta limitada
(héroes 19-30 colores, cielos 5-9), luz fija arriba-izquierda, contorno selectivo y *dithering* ordenado. Las animaciones
son ciclos parametrizados (senos con retraso para el movimiento secundario: cuernos, cola, bufanda, tentáculos) y se mide
que ningún bucle tenga saltos (diferencia de píxeles entre fotogramas, incluido el cierre del bucle) ni se corte por el borde de la celda.
- Héroes: 41 fotogramas (idle, run, salto, caída, aterrizaje, dash, daño, celebración) + brazos de disparo en 3 ángulos con retroceso; versión de invierno con bufanda, orejeras y manoplas.
- Chicle: oso de gominola (3 colores), jelly bean, piruleta voladora con espiral que gira, cañón de azúcar, bolas de chicle y el Gran Chiclón (90 fotogramas, 3 formas).
- Hielo: Pingüino Escudero, Muñeco Remendado, Lechuza de Escarcha, Medusa Boreal, Cangrejo Iglú, Semilla de Nieve, sus bolas de nieve y el Monarca del Deshielo (100 fotogramas, 3 formas, con el sol como punto débil calculado por fotograma).
- Entorno animado en los 10 niveles: molino, árboles que se mecen, cascadas, auroras, humo de iglú, farolillos, candelabros, estandartes, cristales que cantan, partículas y capas con deriva/parallax.
- Texto con fuente de píxeles propia (5x7), audio y música sintetizados en código (un tema por nivel; campanas en Hielo).
- Se renderiza a 640x360 con filtro Point y se escala por enteros.

Regenerar todo: `python 0_3_Art/ArtSource~/build_all.py`.

## QA
`0_6_QA/GumRemixPlayTests.cs` (PlayMode): un bot recorre los dos mundos completos (títulos, 10 niveles, ascensos, ambos jefes, K.O.,
resultados y game over); pruebas dirigidas demuestran con cifras el deslizamiento en hielo, la congelación y el rescate, las plataformas
quebradizas/móviles/elásticas, los cristales y el puente, la ventisca con refugios, el escudo del pingüino y el punto débil del Monarca.
Con la variable de entorno `GUMREMIX_SHOTS` guarda capturas de cada paso. No certifica el balance con jugadores humanos.
