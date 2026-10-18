# Ice Rescue: arte independiente

Generado con la herramienta integrada ImageGen. Los originales seleccionados se copiaron a esta carpeta; el juego no depende de rutas externas.

- `PolarRefuge.png`: pista helada ilustrada vista desde arriba, con muros nevados, aurora, faroles y una unica puerta inferior central. Sin personajes, texto ni interfaz; centro despejado para jugar.
- `PenguinWaddle.png`: atlas transparente 2 x 2 del mismo pinguino azul marino con vientre crema, patas naranjas y bufanda azul. Cuatro poses de caminata, contorno legible y estilo de ilustracion cartoon.
- `PenguinFrame_0.asset` a `PenguinFrame_3.asset`: sprites persistentes cortados en cuatro celdas iguales a partir del tamano real de la textura, con pivote comun junto a los pies.

Se conservan los personajes Mimo y Pipo del proyecto. El escenario y los pinguinos son assets nuevos exclusivos del minijuego.

Escena: `Assets/Minigames/IceRescue/Scenes/IceRescue.unity`. Se abre directamente y tambien desde el mapa de Hielo con H. Enter inicia; WASD y flechas mueven; Escape pausa; R reinicia; M vuelve al mapa.

Para reaplicar el arte: `Coop Beam > Ice Rescue > Apply Art`, fuera de Play. La escena esta registrada en Build Settings. Editor Play utiliza la escena abierta, sin una escena inicial forzada.
