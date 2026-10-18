# Mimo y Pipo: sprites 2D nuevos

Atlas transparente de 4 columnas y 4 filas, generado con ImageGen integrado a partir de la referencia visual del usuario.

Orden de lectura: izquierda a derecha, arriba abajo.

1. Mimo rosa: reposo frontal, trasero y dos variantes de perfil derecho.
2. Mimo rosa: cuatro poses de caminata hacia la derecha.
3. Pipo lima: reposo frontal, trasero y dos variantes de perfil derecho.
4. Pipo lima: cuatro poses de caminata hacia la derecha.

Direccion artistica: ilustracion 2D con contorno oscuro, sombreado cel y reflejos de gelatina; ojos negros y mochila marron. Mimo ancho con copete curvado; Pipo mas pequeno con copete erguido. Fondo transparente, celdas iguales y sin texto.

Integrado en los prefabs de los jugadores y las 18 escenas de juego registradas, incluido IceRescue. Se conservan controles, colliders, velocidades, salto, dash, poderes, rescate y reglas cooperativas.

La generacion conserva ambos perfiles mirando a la derecha. Se usa SpriteRenderer.flipX para mirar a la izquierda. El importador recorta las siluetas y normaliza escala y pivote en los 16 assets de Frames/. No modifica el PNG fuente.

Las animaciones reutilizan las poses aprobadas con deformaciones de gelatina: reposo, carrera, salto, caida, aterrizaje, dash, dano, derrota, victoria y danza. Se mantiene el contrato de 20 posiciones del animador anterior; no son 20 dibujos diferentes. La danza alterna poses, orientacion, inclinacion y compresion. El personaje caido permanece aplastado y vuelve a su forma normal al revivir.

Los colores compartidos provienen de CoopPresentation.PlayerColor: rosa (1, .12, .59) y lima (.65, 1, .06). Auras, luces, estelas, particulas de danza, resonancia y HUD usan esa paleta.

Para volver a aplicar: Coop Beam > Characters > Apply Mimo and Pipo. Los constructores antiguos de animacion respetan los conjuntos que ya tienen jellyStyle para no regenerar el modelo anterior.

Validacion: 25 comprobaciones de TogetherPlayModeQA aprobadas; estados de ambos personajes comprobados en Play en AnimationValidation.txt. No se guardan componentes de QA en escenas.
