# Verificacion en Unity - actualizacion de gotas

Unity 6000.6.5f1, URP 17.6.0, Input System 1.20.0. No se agregaron paquetes.

## Pruebas actuales

- AnimationAndMotion.txt: 21 PASS. Hojas de 20 fotogramas, idle/carrera, salto independiente, dash, impacto, caida contra pared y salida de ella, ocho fotogramas por enemigo, patrulla, blindado, perseguidor, muerte y limpieza.
- VictoryTravel.txt: 11 PASS. Tres oleadas resueltas con consultas reales del rayo; poses de victoria, ascenso, carga automatica del nivel 2, inicio de la nueva partida y recuperacion de controles, fisicas y colliders de ambos jugadores. Dos bucles de escenario asignados.
- WorldIntegration.txt: 46 PASS. Cinco niveles, quince oleadas, plataformas elasticas, cintas, transporte sobre plataforma movil, prensas, malvaviscos, nucleo y tres fases del jefe, victorias, desbloqueos y mapa.

Total de estas tres suites: 78 comprobaciones aprobadas.

Las suites son componentes optativos instanciados solamente en Play Mode. No se guardan en las escenas entregadas. El teclado usado en las pruebas de movimiento es virtual del Input System. La prueba de campana acelera el dano del segmento real y aparta a los jugadores para revisar estados y progresion; no equivale a una partida humana ni certifica balance.

## Evidencias

Results/Droplets_Controls.png muestra los protagonistas y controles.
Results/Droplets_Victory.png y Droplets_Ascent.png muestran la celebracion y viaje.
Results/Level_01.png a Level_05.png y Victory_01.png a Victory_05.png son capturas de la integracion del mundo.

## Historial y limites

Antes de este cambio se aprobaron 20 comprobaciones del rayo y controles, 11 de vida/derrota/recarga, 4 de encuadre/reinicio y 4 de shader/arranque. Esos registros se conservan; no se cuentan como nuevas ejecuciones de esta revision.

No se realizo una partida humana de dos personas, una prueba de ghosting del teclado fisico ni una compilacion standalone. Las animaciones actuales usan sprites generados y estados de gameplay, con deformacion y efectos procedurales complementarios. HUD bitmap mediante IMGUI. Progreso en memoria de la sesion; sin online.

## Cierre

Cinco escenas revisadas sin scripts ausentes ni enlaces faltantes entre GameManager, salida, HUD, animaciones de 20 frames y decoracion. Prefabs revisados sin scripts ausentes. Consola final sin errores ni advertencias.

FinalWorldExit.txt: una comprobacion adicional aprobada de la salida final al mapa y estado de mundo completado. Se inyecto victoria para aislar esa rama; la derrota real del jefe por rayo se comprueba en WorldIntegration.txt.

Editor detenido y Candy_Level_01 abierto, con Play configurado para iniciar ese nivel. No se hicieron commits ni pushes.
