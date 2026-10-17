# Expansion: Resonancia y Reino de Hielo

## Jugar
Abrir `Assets/ReinoDeHielo/0_0_Scenes/Ice_Level_01.unity`, Play, Enter para probar Hielo directamente. En la campaña normal se abre el segundo reino al vencer al jefe de Chicle. El mapa tiene dos pestañas; TAB cambia de reino. El progreso sigue siendo de sesión, sin guardado permanente.

## Cooperación
El rayo sigue siendo la única fuente de daño del equipo. 240 puntos de daño efectivo aceptado cargan Resonancia; el daño bloqueado no cuenta. Mora pulsa Q y Lima Shift derecho dentro de dos segundos, con ambas vivas, para activar 4,5 segundos de rayo ancho y daño x2,4. La ulti no se recarga a sí misma. El contacto del rayo sobrecargado destruye proyectiles despejables; no elimina columnas ni peligros ambientales. Las ventanas de núcleo y umbrales de jefe se respetan. La activación da 0,75 segundos de protección, no invulnerabilidad durante toda la ulti.

Primer enemigo y después cada tres bajas dejan un objeto durante 13 segundos. Se recoge tocándolo con un jugador vivo o atravesándolo con el enlace mientras ambos viven:
- Prisma: 8 segundos de anchura mejorada y daño x1,5. No se multiplica con la ulti.
- Burbuja: un golpe protegido por cada jugador; caduca a los 15 segundos. No se acumula.
- Corazón: +1 corazón a cada jugador vivo, sin superar su máximo, y 30 puntos de carga. No resucita a un jugador muerto.
Los efectos y la carga reinician por nivel/reintento. Controles anteriores intactos.

## Enemigos de Chicle
Nivel 1 conserva la introducción. Desde nivel 2, caminantes y perseguidores anuncian aceleraciones, saltarines anuncian un impacto de suelo, blindados expulsan fragmentos de cacao y voladores fijan una posición antes de picar y regresar. Los cañones conservan sus disparos anunciados existentes. No se regeneraron escenas ni se ejecutaron los constructores antiguos de Chicle.

## Reino de Hielo
1. Valle de los Copos: arena amplia; granaderos de nieve, duendes saltadores y primera introducción a deslizadores.
2. Puerto a la Deriva: témpanos móviles; pingüinos rápidos y primeras lechuzas/oso.
3. Grutas de Aurora: cristales elásticos, rutas altas, lechuzas y torretas de abanico.
4. Bastión del Ventisquero: hielo que colapsa y reaparece, pasarela móvil y combinaciones de presión.
5. Corona del Rey Cencellón: dos oleadas preparatorias y un jefe de tres fases.

Seis enemigos originales: muñeco granadero (salva balística), oso polar cristalino (embestida fuerte con recuperación larga), pingüino (deslizamiento), lechuza (picado a la posición fijada), torreta (abanico dirigido) y duende de escarcha (salto e impacto anunciado). Los ataques dan avisos de 0,9 segundos o 1,15 para el oso; alternan objetivos. Los ataques no persiguen instantáneamente al jugador tras fijar el objetivo.

## Rey Cencellón
Monarca no humano de cristal con ojos magenta, corona fracturada y corazón de aurora; diseño original, sin cara/barba/túnica del personaje de referencia conceptual. 1500 de vida, tres fases, umbrales 68/34 por ciento que impiden saltarse fases. Cuatro familias de patrones que cambian por fase: agujas en abanico dirigido, avalancha con carril libre, ondas bajas/altas y columnas anunciadas. Núcleo invulnerable durante ataque y transformaciones. Recuperaciones de 3 segundos (3,4 en fase final); núcleo elevado en fase 2 incentiva un enlace diagonal. Transformaciones 2,5 segundos, limpian peligros. Derrota animada de 1,68 segundos antes de liberar el registro de oleada. Límite global de 24 peligros; las fases no superponen todos los ataques.

## Arte
Herramienta incorporada image_gen, no API externa. Fuentes PNG conservadas en `Assets/ReinoDeHielo/0_3_Art`:
- IceEnemies.png: atlas 8x6, seis personajes con ocho poses cada uno: locomoción, anticipación, ataque, daño y muerte.
- KingCencellon.png: atlas 4x4, dieciséis poses: reposo, anticipación, ataques, recuperación, daño y derrota.
- IceBackgrounds.png: cinco panoramas originales: valle, puerto, gruta, bastión y salón del trono.
Las animaciones usan selección de sprites por estado, siguiendo la arquitectura existente. No son animaciones esqueléticas ni clips extensos independientes. Importación Point, sin mipmaps ni compresión. Pivotes por alpha para alinear pies; sprites subassets en PixelAnimationSet. Plataformas, iconos y geometría de efectos son procedurales, no se atribuyen a image_gen. Los PNG originales tienen márgenes de celdas limitados en algunas poses de ataque: revisar visualmente antes de ampliar escala.

## Herramientas y límites
`IceKingdomBuilder.Build` crea exclusivamente recursos nuevos de Hielo. Se niega a sobrescribir escenas existentes y no debe repetirse sobre niveles editados. Conserva el orden de build existente y añade cinco escenas.
`ExpansionValidation.Run` revisa escenas, referencias, oleadas, animaciones y scripts ausentes.
`ExpansionPlayModeQA` se añade solo en memoria en Play Mode, nunca guardar en escena. Usa tiempo acelerado y protección temporal de jugadores para recorrer ataques/umbrales. Los resultados están en `0_6_QA`. No certifica dificultad humana, duración de jefe ni build standalone.
La copia recibida contiene al Rey Burbuja, no el código/arte local de Gomelia descrito en el documento de traspaso. La expansión conserva el jefe que existe realmente; no reconstruye Gomelia.
Sin commits ni pushes.

## Verificación final de esta entrega
- ExpansionAssets.txt: 54 PASS, 0 FAIL (recursos y referencias, cinco escenas nuevas).
- ExpansionPlayMode.txt: 42 PASS, 0 FAIL (mecánicas, colisiones reales, seis roles, fases y limpieza del jefe).
- CampaignTraversal.txt: 31 PASS, 0 FAIL (victorias, desbloqueos y transiciones de las diez arenas).
El recorrido de campaña usa jugadores protegidos y daño del rayo acelerado; no representa una partida humana ni certifica el balance. Las capturas de revisión visual pueden usar protección temporal o colocar actores para inspección. Ningún objeto de QA se guardó en escenas.

## Actualización posterior: Reino de Fuego
La versión actual amplía el botín a cinco objetos, uno por baja, con aparición visible de 1,2 s, duración de 18 s y recogida por contacto/enlace. Añade Chispa Encadenada y Anillo Solar. Sustituye la descripción histórica de tres objetos y una entrega cada tres bajas de este documento. Ver `Assets/ReinoDeFuego/0_5_Documentation/REINO_DE_FUEGO.md` para reglas, campaña de tres reinos y pruebas actuales.
