# Reino de Chicle — dirección artística pixel art

La presentación activa del juego usa pixel art de arcade de 16 bits. El primer destino del ejecutable y de Play en el Editor es **Candy_Level_01**, Praderas de Caramelo. El mapa es una pantalla secundaria accesible con M desde el menú. SampleScene y el prototipo antiguo están excluidos de Build Settings; se conservaron sus archivos y los sistemas reutilizados.

## Lenguaje visual

Contornos ciruela, siluetas compactas, grupos de píxeles visibles y luces en bandas discretas. Paleta: crema `#FFF0CE`, rosa `#E879AF`, menta `#94EBC5`, caramelo `#F9CE7A`, cacao `#65405D` y contorno ciruela oscuro. El entorno se atenúa para que los protagonistas, el núcleo luminoso del rayo y las señales de peligro se distingan.

| Personaje | Identidad y lectura |
|---|---|
| Mora | Gota morada de silueta continua, punta curva, ojos grandes y brillo menta. |
| Lima | Gota verde lima, brillo lavanda y las mismas capacidades que Mora. |
| Caminante | Caramelo rosa envuelto; extremos del envoltorio forman una silueta horizontal. |
| Saltarín | Gomita verde con orejas de gelatina; silueta elástica y vertical. |
| Perseguidor | Duende de toffee dorado, orejas angulares y gafas azules. |
| Blindado | Armadura de cacao segmentada, bandas de oblea, ojos de menta y cuerno de caramelo. |
| Volador | Gomita lavanda con alas grandes celestes y antenas de azúcar. |
| Cañón | Criatura de caramelo con glaseado, boca de chocolate y cañón visible. |
| Rey Burbuja | Gran monarca fucsia con corona de azúcar, capa de glaseado y núcleo de menta. |

## Recursos y técnica

Los personajes, enemigos, jefe, fondos y logotipo son imágenes originales producidas mediante la herramienta de generación de imágenes y copiadas al proyecto. Los archivos consumidos están en `0_3_Art/PixelArt/`, con sprites transparentes separados y fondos específicos de los cinco niveles. No hay dependencia de sus rutas de generación externas.

Las plataformas de 32×16 píxeles, iconos de partículas, corazones y alfabeto bitmap de 5×7 son recursos originales dibujados por código. No se atribuyen a generación de imágenes. Las plataformas se repiten horizontalmente en lugar de estirar una textura sobre toda su anchura.

Importación Point, sin mipmaps ni compresión. El mundo se renderiza en una textura de 800×450 y se amplía con muestreo Point, conservando la proporción 16:9 con márgenes cuando es necesario. La interfaz usa tipografía bitmap original y marcos escalonados. Las posiciones visuales se ajustan a una cuadrícula sin alterar cuerpos físicos ni hitboxes.

Los impactos producen un destello blanco mediante un shader URP propio. El dash deja ecos de color del sprite. Los nueve personajes cuentan con **hojas de animación generadas y 96 fotogramas recortados e integrados**: 20 por protagonista y 8 por enemigo/jefe. Los protagonistas tienen respiración/parpadeo, carrera con zancadas, despegue, ascenso, caída, aterrizaje, dash, retroceso y dos poses de muerte. Los enemigos alternan locomoción, anticipación, ataque, daño y derrota. La muerte enemiga se representa antes de eliminar el objeto; durante ella ya no puede dañar ni bloquear.

Las poses se seleccionan según el estado real del controlador, no por una reproducción de vídeo independiente. Se mantienen pequeñas deformaciones cuantizadas como complemento. Las pivotes de cada fotograma se ajustan a su base visible para reducir saltos de posición y pies flotantes. La tipografía y el HUD permanecen en píxeles.

El ambiente incluye nubes lentas, motas de azúcar o burbujas, pequeños movimientos de los caramelos decorativos, engranajes por fotogramas en la fábrica, cintas que cambian de dibujo y gomitas con reflejos animados. Son elementos visuales: sus movimientos no desplazan colliders que deban permanecer estables.

La revisión física utiliza material sin fricción en los actores para evitar quedarse suspendido contra paredes. La detección de suelo y bordes de enemigos se ajusta al collider de cada uno, los giros tienen una breve protección contra oscilaciones y los enemigos de una oleada no se bloquean unos a otros. Las pruebas cubren casos de pared, bordes de islas y el blindado; una sesión humana sigue siendo necesaria para descubrir situaciones no cubiertas.

## Mantener el estilo

`Coop Beam > Apply Pixel Art and Candy Startup` vuelve a asignar los PNG e importadores, la interfaz, las superficies y el arranque; también aplica `Apply Sprite Animation and Living World` cuando están presentes las nueve hojas. `Build Candy World` aplica esta presentación después de reconstruir el mundo. Los PNG generados viven separados de los recursos procedurales anteriores y no se sobrescriben al reconstruir.

Para reemplazar un sprite, mantener su archivo `.meta`, conservar transparencia y una silueta legible, y ejecutar la aplicación de estilo. Los colliders conservan los tamaños de gameplay; revisar visualmente cualquier cambio de proporciones. El jefe recibe daño únicamente en su collider de núcleo durante las ventanas de vulnerabilidad.
