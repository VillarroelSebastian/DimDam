# BubblegumKingdom — Torre de menta

Escena: `0_0_Scenes/Level_BubblegumKingdom.unity`. Abrir en Unity y pulsar Play. Se inicia sin Enter. Reconstrucción reproducible: `Coop Beam > Build Bubblegum Kingdom`.

## Recorrido vertical

Torre compacta de 20 unidades de ancho y unos 34 de alto, en lugar del recorrido anterior de unas 80 unidades de ancho. Se conservan los cuatro retos cooperativos:

1. Piso 0: Pipo conecta el auxiliar del túnel; se acciona la palanca; Mimo empuja la mentita a su marca. Se habilita la subida derecha.
2. Piso 10: Mimo prepara el bombón guía y libera la mentita; Pipo elige el desvío verde. La mentita recorre el carril superior y su collider activa la placa. Se habilita la subida izquierda, separada del caramelo estacionado.
3. Piso 20: Mimo coloca el escalón en la marca; Pipo activa el interruptor del túnel. Aparecen tres burbujas escalonadas con reflejos animados. Sus colliders permanecen estables.
4. Piso 28: ambos se acercan al corazón y, después, a la salida. Se conserva la transición prevista a `Level_TheNeonCore`, con mensaje final si esa escena todavía no está disponible.

Cada piso superior tiene un punto de control que requiere a ambos jugadores. R restaura al equipo y los retos pendientes, preservando los ya confirmados. Una caída o muerte recupera al equipo automáticamente.

## Controles y colisiones

Mimo: A/D, W salto, Shift dash, S usar. Pipo: flechas, arriba salto, Ctrl derecho dash, abajo usar. Caminar contra los caramelos pesados los empuja únicamente si es Mimo; un dash no sirve para empujarlos. M abre el mapa.

S/abajo son exclusivamente interacción en esta escena: ya no ejecutan simultáneamente el descenso a través de plataformas. `interactionOnlyDown` queda desactivado por defecto en el controlador compartido, conservando el comportamiento de las demás escenas.

Los caramelos comprueban el espacio que van a recorrer y los colliders de ambos jugadores antes de moverse. El giro afecta únicamente al dibujo, nunca a la caja física. La mentita fugitiva espera si hay una obstrucción y retorna a su origen si no se despeja; protege también a quien esté en el punto de retorno. Las interacciones validan distancia, línea de visión, rol, progreso y una pequeña pausa entre activaciones. Las marcas comprueban ambos ejes. Reiniciar durante la animación de recompensa conserva correctamente el corazón recogido.

## Arte y animación

Se reutilizan los personajes animados y el paisaje de Praderas de Caramelo del reino existente. Las superficies repiten sus texturas en vez de estirarlas. Cámara vertical compartida, render pixelado y la tipografía bitmap del reino para HUD y señales.

`Art/` contiene 14 PNG originales dibujados por código en la paleta existente: mentita espiral rosa/menta, bombón, marca, placa, puerta, corazón, cuatro reflejos de burbuja y cuatro fotogramas de interruptor. Importación Point, sin mipmaps ni compresión. La mentita rueda con ángulos cuantizados, el corazón y las burbujas ambientales flotan y los mecanismos responden con sonido y partículas. El generador conserva los `.meta` de los recursos al reconstruir.

## Verificación

`QA/BubblegumPlayQA.cs` se adjunta explícitamente en Play, nunca se guarda en la escena. Usa teclado virtual para caminar, empujar y recorrer las tres subidas con ambos personajes; reposiciona actores para aislar los casos de interacciones, rutas y puntos de control. Restaura el foco de entrada al finalizar. Consultar `QA/PlayMode-results.txt` para los resultados de la última ejecución. Estas comprobaciones no sustituyen una sesión humana completa a dos jugadores.

Última verificación: 34 comprobaciones de Play Mode aprobadas, cero fallos. Dos comprobaciones adicionales de retorno ante obstrucción y seguridad del jugador en `QA/Safety-results.txt`. Escena validada sin scripts ausentes ni prefabs rotos. Los prefabs originales conservan el descenso por plataformas.
