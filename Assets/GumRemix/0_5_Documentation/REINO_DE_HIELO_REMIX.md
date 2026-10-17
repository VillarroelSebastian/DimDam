# Reino de Hielo (Chicle Rumble)

Reinterpretación, en el estilo run & gun / Snow Bros de Chicle Rumble, del Reino de Hielo original del proyecto
(`Assets/ReinoDeHielo`): las mismas cinco arenas, las mismas criaturas y el mismo jefe, con mecánicas adaptadas.
Se juega desde el menú (**MUNDO: HIELO**) o continuando tras vencer al Gran Chiclón. Los héroes llevan ropa de invierno
(bufanda que ondea, orejeras, manoplas).

## Las cinco arenas
| # | Arena | Qué enseña |
|---|---|---|
| 1 | **Lago de los Susurros** | Suelo con sectores de **hielo** (resbala) y de **nieve** (frena), icebergs **quebradizos**, escalones asimétricos. Noche con aurora y pinos. |
| 2 | **Aldea de Iglús Torcidos** | Tejados, un **ascensor** vertical, callejón de tejas quebradizas. Amanecer con iglús que humean y farolillos. |
| 3 | **Cristales Cantores** | **Camas elásticas** de cristal, dos torres y un **puente alto** que se levanta disparando a los **cristales resonantes**. Caen carámbanos. |
| 4 | **Jardines Congelados** | Rutas quebradizas, un **refugio** central, un transbordador y **ventiscas** con carámbanos. |
| 5 | **Palacio de la Última Nevada** | Arena del jefe: escaleras, galerías, transbordador, puente que se rompe y ventiscas. |

## Mecánicas del mundo
- **Inercia en el hielo:** sobre hielo aceleras y frenas despacio; sobre nieve frenas al instante. Dash y salto no cambian.
- **Congelación:** las escarchas (cristales azules) quitan un corazón y te encierran hasta 2,4 s. Martillea cualquier botón (o acerca a tu compañero) para romper el hielo; después tienes 3 s de inmunidad contra encadenar congelaciones.
- **Cristales resonantes:** 3 impactos seguidos (o una bola de nieve) y el cristal canta, levantando el puente 14 s (parpadea antes de desvanecerse).
- **Plataformas:** *móviles* (llevan a quien va encima), *quebradizas* (tiemblan 0,85 s y desaparecen 4,2 s), *elásticas* (te lanzan alto).
- **Ventisca:** aviso de 1,3 s y luego 3,4 s de viento que empuja a quien no tenga un techo encima. Los carámbanos se anuncian con una sombra en el suelo.

## Criaturas
- **Pingüino Escudero:** su escudo detiene el chicle que llega de frente; dispárale por detrás o desde arriba. Se lanza de barriga (más lejos sobre hielo).
- **Muñeco Remendado:** salta; lanza su cabeza (que al caer deja una Semilla de Nieve) y corre sin cabeza antes de rehacerla.
- **Lechuza de Escarcha:** patrulla en el aire, tiembla para fijar objetivo y se lanza en picado.
- **Medusa Boreal:** flota y lanza un abanico de escarcha; a veces uno de los proyectiles es un copo rosa (parry).
- **Cangrejo Iglú:** su caparazón absorbe cada segundo impacto; se mete en la cúpula y rueda, y tras la embestida queda expuesto.
- **Semilla de Nieve:** pequeña y frágil, siempre salta hacia ti.

## Monarca del Deshielo
Criatura de nieve con un **sol cautivo en el pecho**: solo el sol recibe daño; el cuerpo detiene el chicle (las bolas de nieve sí rompen el caparazón). Tres formas, 520 de vida.
1. **Anfitrión en sillón rodante:** granizo con un pasillo libre, carga con el sillón (esquívala con salto desde una plataforma o dash), volea de escarchas.
2. **Gigante agrietado:** pisotón con ondas de hielo, pilares que emergen de grietas anunciadas, invoca pingüinos y semillas.
3. **Tormenta hueca con corona:** anillos de escarchas con hueco, abanico giratorio y ventisca con lluvia de cristales.
Las transformaciones limpian los proyectiles; durante ellas es invulnerable. La posición del sol sale de la propia animación (un punto por fotograma), así que el punto débil sigue al dibujo.

## Monarca del Deshielo: ventana de ataque
La coraza de nieve detiene el chicle. Tras cada ataque la coraza se agrieta y brilla en dorado durante unos 4-5 s (`IceBoss.openT`): en ese momento el chicle llega al sol y le hace dano. Vida: 80 / 80 / 70 por forma.

## Nota para Unity (Enter Play Mode Options)
Si el proyecto tiene desactivado el "Reload Domain", los estaticos sobreviven entre ejecuciones. Cada clase estatica del juego (`Art`, `Sfx`, `GameCtl`, `Levels`) se reinicia con `RuntimeInitializeOnLoadMethod(SubsystemRegistration)`; sin eso la pantalla quedaba en negro al volver a darle Play.
