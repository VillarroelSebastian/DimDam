# Sprites de GameDuo

Generados con la herramienta integrada ImageGen, fondo transparente. Los PNG originales se conservan sin retoques; el juego crea los recortes de sprites al cargar los atlas.

## GumRaiders_32.png

Production 2D game sprite animation atlas, eight columns and four rows. Hand drawn polished pixel-inspired fantasy platformer style, dark purple outlines, cel shaded glossy bubblegum, menacing silhouettes rather than babies. Four enemy families: pink elastic raider, purple armored gum guard, magenta winged gum imp, red gum spitter. Idle, breathing, walking, anticipation, elastic attack and hurt poses. Consistent centers and baselines. Transparent alpha, no text, borders or scenery.

Los ataques elasticos de las filas 1, 3 y 4 ocupan dos celdas y se recortan juntos para conservar los brazos completos; su pivote mantiene el torso en su lugar. Las variantes de caminante, saltador y perseguidor comparten el invasor rosa.

## IceKing_32.png

Production boss atlas, eight columns and four rows, 32 full-body poses of one regal adult Ice King. Angular cyan crystal body, long indigo mantle, icicle beard, jagged crown, crystal shoulder armor and frozen scepter. Eight idle/breathing poses; eight anticipation poses gathering power; eight attack poses for shards, avalanche, ice rings and columns; exposed chest core, hurt, kneeling defeat and surrender poses. Illustrated cel shading and blue outlines. Transparent alpha, no labels or scenery.

## GumUncle_32.png

Final bubblegum uncle boss atlas, eight columns and four rows, 32 full-body poses. Adult magenta elastic usurper, sharp eyebrows, pointed gum beard, plum royal cape, gold crown and mint green chest core. Eight idle poses; eight inflated-cheek and raised-arm anticipation poses; eight gum whip, sugar orb, elastic strike and stomp attack poses; exposed core, recoil, crumpling defeat and surrender. Glossy hand-painted candy kingdom style with dark purple outlines. Transparent alpha, no text or scenery.

Los recortes se consumen desde `GameDuoEnemyVisual`. Las animaciones responden a los estados reales del enemigo y los jefes; el arte no crea zonas de dano adicionales.
