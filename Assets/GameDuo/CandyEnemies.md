# Enemigos del capitulo 5

La campana configura una copia del nivel en memoria. Las dos oleadas previas al rey presentan oso + avispa + torreta y dona + galleta + torreta. Los otros capitulos conservan sus enemigos.

## Ataques y animacion

- Wasp.png: cuatro poses de vuelo, preparacion, picada a la posicion fijada y recuperacion.
- Donut.png: vuelo, preparacion y abanico de tres caramelos separados 14 grados.
- Bear.png: caminata, brazo levantado, golpe de corto alcance y recuperacion.
- Cookie.png: caminata, pinza levantada, golpe con avance corto y recuperacion.
- Turret.png: orientaciones segun objetivo, disparo y retroceso. Apunta al jugador vivo mas cercano y fija su posicion durante el aviso; el proyectil no lo persigue.

Todos usan EnemyBase y los colliders existentes para recibir dano del lazo. El controlador reemplaza los animadores y las tacticas anteriores solamente en 05_JefeChicle; restablece sprite, escala y orden de dibujo explicitamente.

## Dificultad

Aviso de ataque de 0.9 segundos; como maximo dos enemigos preparan o ejecutan ataques simultaneamente. Los proyectiles viajan a 4.5 unidades/segundo. Tras atacar hay un segundo de recuperacion y una espera adicional. El rey invoca uno por ciclo, dos en fase 3, con un limite de tres vivos. Alterna torreta, avispa, oso, dona y galleta; espera 9/8/7 segundos entre invocaciones segun fase. Los refuerzos duran hasta 24 segundos, dan las recompensas normales al destruirlos y desaparecen junto con sus disparos cuando cae el rey. Durante la invocacion se pausa la lluvia del rey, conservando su nucleo vulnerable.

## Arte

Los cinco atlas estan en Assets/GameDuo/Resources/GameDuo/Final: Wasp.png, Donut.png, Bear.png, Cookie.png, Turret.png. Derivan de las cinco imagenes aportadas mediante la herramienta integrada image_gen. Ocho poses por hoja, cuatro columnas y dos filas, importadas con lectura CPU y alpha habilitados.

Prompt aplicado: extraer las siluetas del fondo a transparencia real, conservar personajes, colores y poses; organizar ocho poses en cuatro columnas y dos filas con separacion transparente. Para la torreta: frente, izquierda, arriba, derecha, perfil izquierdo, carga frontal, disparo derecho y retroceso frontal.

## Comprobacion

Menu Unity: GameDuo > Probar nuevos enemigos de caramelo. Ejecuta comprobaciones de sprites, aviso/ataque por especie, proyectiles, dano del lazo, invocaciones en fase 3, limite de refuerzos y limpieza tras derrotar al rey. Genera CandyEnemyQA.txt y, con renderizador disponible, CandyEnemyEncounter.png junto a este documento.
