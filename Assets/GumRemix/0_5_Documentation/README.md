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
| Saltar | Espacio | Shift derecho |
| Bajar de una plataforma | S + salto | Flecha abajo + salto |
| Disparar chicle | F | Ctrl derecho |
| Dash (invulnerable) | G | . |

Mando: stick/cruceta, A salta, X dispara, B/RB dash. P2 puede entrar en cualquier momento con una de sus teclas.

## Mecánicas comunes
- **3 corazones** por jugador. Al caer te conviertes en fantasma en burbuja: tu compañero te revive tocándote. Si caen los dos, se reintenta el nivel.
- **Atrapar y patear (Snow Bros):** los disparos pegan chicle; al agotar la vida del enemigo queda atrapado en una bola (de chicle en Chicle, de nieve en Hielo). Tócala para patearla: rueda, rebota, revienta a los enemigos que encuentra, rompe cristales y golpea al jefe. Si no la pateas, el enemigo se libera a los 7 s.
- **Sin súper ni parry:** el juego se apoya en el lazo y en jugar juntos (ver abajo). Los proyectiles se esquivan o se revientan con el lazo.
- **Subir de nivel:** al eliminar a todos los enemigos una burbuja lleva a los jugadores al piso de arriba; los niveles están apilados y la cámara asciende (los caídos reviven y se cura un corazón).
- **10 niveles y 2 jefes:** cada mundo son 4 niveles que se escalan y un jefe en el quinto. Al vencer a la Princesa Chicle se pasa al Reino de Hielo.

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
- Chicle: oso de gominola (3 colores), jelly bean, piruleta voladora con espiral que gira, cañón de azúcar, bolas de chicle y el la Princesa Chicle (90 fotogramas, 3 formas).
- Hielo: Pingüino Escudero, Muñeco Remendado, Lechuza de Escarcha, Medusa Boreal, Cangrejo Iglú, Semilla de Nieve, sus bolas de nieve y el Monarca del Deshielo (100 fotogramas, 3 formas, con el sol como punto débil calculado por fotograma).
- Entorno animado en los 10 niveles: molino, árboles que se mecen, cascadas, auroras, humo de iglú, farolillos, candelabros, estandartes, cristales que cantan, partículas y capas con deriva/parallax.
- Texto con fuente de píxeles propia (5x7), audio y música sintetizados en código (un tema por nivel; campanas en Hielo).
- Se renderiza a 640x360 con filtro Point y se escala por enteros.

Regenerar todo: `python 0_3_Art/ArtSource~/build_all.py`.

## QA
`0_6_QA/GumRemixPlayTests.cs` (PlayMode): un bot recorre los dos mundos completos (títulos, 10 niveles, ascensos, ambos jefes, K.O.,
resultados y game over); pruebas dirigidas demuestran con cifras el deslizamiento en hielo, la congelación y el rescate, las plataformas
quebradizas/móviles/elásticas, los cristales y el puente, la ventisca con refugios, el escudo del pingüino y el punto débil del Rey de Hielo.
Con la variable de entorno `GUMREMIX_SHOTS` guarda capturas de cada paso. No certifica el balance con jugadores humanos.

## Juntos es mejor (Better Together)

Pensado para la temática de la jam: el juego se disfruta más con otra persona, pero también se puede jugar **solo con ambas manos**.

- **Solo (1 JUGADOR)**: una persona controla a los dos héroes (P1 con WASD, P2 con flechas). El disparo es **automático** (solo aprieta el gatillo por ti: tú apuntas con W / W+lado); se puede cambiar a MANUAL en el menú ("DISPARO"). Si un héroe cae, su fantasma vuela solo hacia el compañero para revivir.
- **Vínculo de chicle**: mientras los héroes están cerca, una cuerda de chicle los une. Lo que la cruce recibe daño (enemigos, cristales, punto débil del jefe) y revienta los disparos enemigos. Cada uno se coloca a un lado de la amenaza.
- **Rebote**: caer sobre la cabeza del compañero te impulsa alto (mantén salto para más).
- **Abrazo**: quedarse juntos y cerca un momento cura 1 corazón (enfriamiento 14 s).
- **Sin súper:** ya no hay barra de cartas; la cooperación (lazo, rebote, abrazo) es la mecánica central.
- **Revivir** tocando al compañero caído (ahora con 2 corazones).

### Ajustes de jugabilidad
- Los héroes se mueven durante el banner de inicio de nivel; los enemigos esperan a que termine.
- Máximo de enemigos a la vez por nivel (`maxAlive`); el resto llega en oleadas.
- 4 corazones; jefes con menos vida, ataques más lentos y espaciados, corazón extra entre formas y corazones completos al empezar el jefe.

## Música

Toda la música se compone y sintetiza dentro del juego (sin archivos de audio), en `GumMusic.cs`: `SongBook` (partituras como texto), `MusicRenderer` (32 kHz estéreo, bucles sin clic) y `MusicHost` (render en segundo plano, fundido cruzado).

Pistas (`Track`): Prado, Cueva, Nubes, Fabrica, CircoBoss1-3 (la Princesa Chicle, 3 formas), Lago, Aldea, Cristales, Jardines, PalacioBoss1-3 (Monarca, 3 formas), Title, Victory, GameOver, WorldDone.

## Lazo, acorazados y guardia de jefes
- **El lazo siempre une a los heroes** (horizontal, diagonal o vertical, largo maximo 9.5 u; mas alla los jala). Daña lo que cruza.
- **Acorazados** (oso de acero en Chicle, muneco de hierro en Hielo; arte generado con `0_3_Art/ArtSource~/armor.py`): el disparo rebota. Hay que dejar el lazo cruzandolos ~0.65 s para encapsularlos en bolita; hay que rodearlos y usar el plataformeo para quedar a cada lado.
- **Guardia de jefe**: una vez por forma, al perder ~40 % de la vida de esa forma, el jefe brilla y el disparo rebota; solo el lazo lo daña (hasta quitarle ~22 % de la forma o 20 s).
- Dificultad: hasta 4-5 enemigos a la vez, jefes con mas vida (Chiclon 170/150/125, Rey de Hielo 100/100/90) y menos pausa entre ataques.
- Musica: fundidos de potencia constante (1.8 s entre niveles, 3.2 s entre formas de jefe).

## Rediseño "dulce gótico" del Reino de Chicle
Se bajó el tono infantil del mundo de Chicle (arte 100 % generado por código en `0_3_Art/ArtSource~`):
- **Jefe: La Princesa Chicle** (`boss.py`). Forma 1: princesa elegante (vestido de gominola con burbujas atrapadas, corona de oro, cetro-piruleta). Forma 2: se deforma (la corona se rompe en astillas, la cara se estira, un ojo ácido y un tercer ojo en la frente, el vestido se derrite en goteos, brazos con garras). Forma 3: masa monstruosa de chicle con varios ojos de pupila rasgada, fauces con colmillos, venas y tentáculos. Mismas animaciones y tiempos que antes (`boss_clown.py` conserva los parámetros de animación). Hitboxes ajustadas en `GumBoss.cs`.
- **Mundo** (`env_gothic.py`): Jardín nocturno (luna de chicle, castillo lejano, árboles de regaliz con bulbos que laten, hongos luminosos, torre), nivel de nubes como atardecer ciruela con farolillos, y **Salón del Trono** para el jefe (vitrales, columnas, arañas de luz, cortinas, candelabros con llamas animadas). Plataformas y suelos oscuros con glaseado magenta u oro.
- Enemigos de Chicle con paletas más profundas (oso, jelly bean, piruleta). Los fundidos de partículas ambientales ahora son esporas/brasas en vez de confeti.

### Música del mundo de Chicle (tono gótico)
Menú: vals de caja de música en La menor. Jardín Nocturno (nivel 1): melancólico en La menor con arpegios de pluck. Nubes de Tormenta (nivel 3): Mi menor con campanas. Salón del Trono (jefe): vals en Re menor (3/4) que sube de tono y tempo en cada forma (132 → 150 → 168 bpm). Cueva y Fábrica no cambian.


## Cambios recientes
- Se quitaron el **súper** (5 cartas) y el **parry**; el HUD solo muestra corazones. Los resultados cuentan los **lazos** (enemigos acorazados atrapados).
- **Enemigos de Chicle rediseñados** (menos infantiles): oso remendado con un ojo de botón y costuras, frijol con ojos huecos y cremallera por boca, piruleta de un solo ojo con alas rotas y cañón con cara de calavera.
- **Más acorazados en el mundo de hielo** (solo el lazo los atrapa): muñeco de hierro, **pingüino de hierro** y **búho de hierro** (acero oscuro con remaches naranjas). El disparo rebota con un CLANG.
