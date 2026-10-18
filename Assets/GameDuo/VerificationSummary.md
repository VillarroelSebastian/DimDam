# Verificacion de GameDuo

Resultado: 203 comprobaciones aprobadas, 0 fallos en Unity Play Mode.
El detalle se encuentra en CampaignVerification.txt.

Correcciones:
- Los enemigos conservan su imagen hasta que esta lista la animacion de reemplazo.
- Los enemigos de hielo inicializan su primer sprite desde Awake y su pose antes de ocultar la imagen anterior.
- Las caches de animacion reconstruyen sprites destruidos al volver a jugar.
- Se evita duplicar los controladores visuales y de enemigos.
- Las cinemáticas bloquean controles y botones del juego y mantienen la simulacion pausada.
- Se conserva una imagen visible si no esta disponible el shader de desintegracion.

Cobertura:
- Los cinco capitulos y las tres oleadas de cada arena de combate.
- Movimiento y salto con teclado real de ambos heroes.
- Todos los tipos de enemigos de las arenas, incluidos refuerzos del jefe final.
- Impactos del lazo, nucleo cerrado, cuatro patrones y tres fases del Rey Hielo.
- Mecanismos de neon, recoleccion conjunta y portal que exige ambos jugadores.
- Los 16 pinguinos rescatados y fin de ronda por tiempo agotado.
- Caida de un heroe, rescate por contacto, derrota de ambos y reinicio.
- Los nueve videos: decodificacion de imagenes, fin de reproduccion y orden exacto.
- Reentrada en los cinco capitulos y recuperacion de cache con sprites destruidos.

La prueba acelera las cinematicas despues de decodificar imagenes y coloca los personajes para aislar mecanismos y el refugio. No sustituye una partida manual completa ni una comprobacion auditiva de cada video.

Para repetirla: menu GameDuo > Verificar campana completa y cinematics, con la escena guardada y fuera de Play Mode.
