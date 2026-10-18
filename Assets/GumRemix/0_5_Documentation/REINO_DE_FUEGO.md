# Reino de Fuego (mundo 3)

Una forja abandonada, oscura y melancólica, alumbrada por hornos y magma. Misma temática que los otros mundos (tono sombrío, nada infantil). Se elige desde el menú (**MUNDO: < FUEGO >**) o se llega tras vencer al Rey de Hielo. Niveles 3-1 a 3-5.

| Nivel | Nombre | Idea |
|---|---|---|
| 3-1 | Forja Abandonada | Introducción con golems de forja. |
| 3-2 | Ríos de Magma | Costra que se rompe, plataforma móvil y **geiseres** de fuego. |
| 3-3 | Cámara de Engranajes | **Resortes de vapor** que lanzan a las galerías altas, ascensor, cañones brasero. |
| 3-4 | Fundición del Rey | Geiseres + **cenizas ardientes** que caen sobre costra frágil. |
| 3-5 | Salón de las Hoces | Jefe: **Los Reyes Mantis**. |

## Enemigos (reusan los comportamientos de las otras familias, con arte nuevo)
- **Fundidor** (golem de forja que carga) y **Fundidor de bronce** (acorazado: solo el lazo lo atrapa).
- **Magma** (blob de lava saltarín), **Chispa** (chispa viva que salta).
- **Cenizo** (murciélago robótico en picada) y **Cenizo de bronce** (acorazado, vuela).
- **Llama errante** (llama flotante que lanza un abanico de fuego).
- **Brasero** (cañón de horno) y **Escarabajo forjado** (se mete bajo su caparazón).
Al atraparlos quedan en una **bola de ceniza** con grietas brillantes.

## Peligros
- **Geiser**: una grieta brilla ~1.2 s y estalla una columna de fuego (`Geyser`).
- **Ceniza ardiente**: una roca en llamas con marcador en el suelo (`Cinder`).
- **Costra frágil** y **resortes de vapor** (plataformas Brittle/Elastic con arte propio).

## Jefe: Los Reyes Mantis (`GumFireBoss.cs`)
Dos reyes mantis robóticos pequeños (inspirados en los Señores Mantis de Hollow Knight): **Rey Brasa** (óxido, oro y brasa) y **Rey Ceniza** (acero ceniza, plata y oro pálido). **Comparten una barra de vida** y pelean en pareja con un **baile**:
- Forma 1 *Danza de apertura*: carga cruzada, tajos en abanico y hoces boomerang.
- Forma 2 *Tijeras*: tajos en pinza, saltos a las paredes y picadas que se cruzan (ondas de fuego), y el **baile de cargas**.
- Forma 3 *Frenesí*: cargas simultáneas, anillos de fuego y más hoces.
Una vez por forma tienen **guardia** (solo el lazo los daña). Música: vals de combate en La menor que sube de tono y tempo en cada forma (152/168/184 bpm).

## Arte (todo generado por código en `0_3_Art/ArtSource~`)
`fire_env.py` (cielos y capas), `fire_tiles.py` (plataformas, suelos, paredes y props animados), `fire_enemies.py` (criaturas), `fire_fx.py` (disparos, geiser, ceniza), `fire_boss.py` + `fire_boss_anim.py` (reyes: esqueleto de poses y 13 animaciones: idle, baile, paso, carga, tajo, salto, picada, lanzamiento, daño, furia, muerte...).

## Música
Forja Abandonada (Mi frigio, campanas), Ríos de Magma (La frigio, lenta), Cámara de Engranajes (Sol menor, industrial), Fundición del Rey (Si menor, metales) y el tema de los reyes.
