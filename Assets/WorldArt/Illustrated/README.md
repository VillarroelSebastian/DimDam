# Melt Together: mundos ilustrados

Direccion: ilustracion lateral 2D, siluetas claras, contornos oscuros, volumen sencillo y pocos detalles magicos. Los fondos y atlas se generaron con ImageGen integrado; los prompts completos estan en GenerationPrompts.md.

Assets originales nuevos: cuatro fondos y cuatro atlas (112 sprites de biblioteca). Candy, Ice y Fire contienen seis criaturas en dos poses, cuatro poses de jefe y plataformas, decoracion, proyectiles y mecanismos. Neon contiene 16 piezas y estados de mecanismos, incluidas cuatro fases del reactor. Los nuevos personajes y el rescate de pinguinos se conservan.

El instalador conserva los indices y cantidades de fotogramas de las animaciones previas y sus estados de ataque, anticipacion, dano y derrota. Las poses nuevas se combinan con deformacion visual suave; no se anuncian como secuencias dibujadas a mano fotograma por fotograma. Colliders, velocidades, patrones y progresion conservan sus componentes de juego.

Frames contiene las siluetas recortadas. Remapped adapta estos sprites a las dimensiones y pivotes de las referencias anteriores sin editar los PNG originales. Las plataformas usan escalado por secciones para conservar sus bordes al variar el largo. El render de las escenas activas usa resolucion alta, filtro bilineal y no aplica la antigua paleta de 32 colores. Texto, efectos y movimiento ambiental se muestran sin ajuste a la cuadricula de pixeles.

Reaplicar fuera de Play: Coop Beam > Art > Apply Illustrated Worlds. Las escenas activas registradas en Build Settings estan actualizadas; los archivos fuente antiguos se mantienen para recuperacion. El prototipo separado GumRemix conserva su pipeline propio.
