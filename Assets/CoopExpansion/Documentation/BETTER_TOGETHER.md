# Better Together: mecánicas actuales

Proyecto: C:/GameJamWinner/DimDam. Esta guía sustituye los valores anteriores de botín y resonancia en las guías de las expansiones.

## Rescate estable
Al morir, el cuerpo cae por gravedad manual hasta suelo o una plataforma estática suficientemente ancha. No se queda suspendido ni viaja sobre muelles, cintas, plataformas móviles o desmoronables: las atraviesa hasta un apoyo estable inferior. Si cae fuera del escenario, busca apoyo desde el punto inicial. Acercarse al cuerpo ya asentado durante 1.5 segundos rescata al compañero. No hay reaparición automática; ambos muertos terminan la partida.

## Power-ups escasos, sin acumulación
- Máximo un pickup en el escenario y un poder pendiente de compartir.
- Tras tres bajas elegibles aparece uno. Las bajas durante un efecto activo o con un pickup esperando no generan una reserva de botín.
- Recoger, acercarse 0.6 segundos y separarse más de 2.8 unidades activa el poder para ambos.
- Máximo un efecto activo. No se superponen Prisma, Chispa, Anillo o Burbuja.
- Prisma dura 8 s; Chispa y Anillo, 10 s; Burbuja dura hasta 15 s. Hay al menos 7 s de descanso tras esas duraciones antes de volver a contar bajas. Corazón aplica su cura una vez y deja 10 s de descanso. Un escudo consumido pronto mantiene el descanso hasta su ventana prevista.
- Un pickup sin recoger expira después de 18 s. La pausa inicial del nivel es de 4 s.

## Tres ultis
La barra pide 360 puntos de daño directo aceptado. La carga se conserva entre niveles de la sesión. Elegir una ulti no cuesta carga: 1 Danza, 2 Tirachinas, 3 Constelación. Mora propone con Q y Lima confirma con Shift derecho dentro de 2 s. Cambiar de elección cancela las propuestas previas. Los tutoriales se cierran con Esc.

### Danza del enlace
Ambos levitan y quedan protegidos. Doce pulsos alternan entre Mora (W/A/S/D) y Lima (flechas). Cada pulso dura 0.65 s, con ventana válida de 0.12 a 0.53 s. Una sola tecla nueva correcta suma potencia para ambos; anticiparse o pulsar varias falla. La explosión siempre ocurre: 65 + 12 de daño por acierto. Aura, poses, partículas y oscuridad acompañan la secuencia.

### Tirachinas
Mora mantiene A para tensar durante hasta 2.2 s. Lima apunta con arriba/abajo y elige izquierda/derecha. Mora pulsa D y Lima Ctrl derecho dentro de 0.6 s para soltar; antes de un segundo no se puede disparar. A los 6.5 s se libera de todos modos. Una onda de 100 a 210 de daño recorre la arena y rebota hasta tres veces. Cada enemigo solo puede recibir un impacto por onda. El control normal vuelve al disparar.

### Constelación
Cada jugador visita la estrella de su color usando sus controles habituales. Ambos deben ocupar su par durante 0.35 s. Tres pares, ocho segundos. Cada par agrega 60 de daño al remate base de 80. Los jugadores quedan protegidos y pueden moverse; el lazo deja de hacer daño durante la prueba. El remate ocurre aunque no logren todos los pares.

Las tres ultis respetan núcleos cerrados y los límites de fase. Tirachinas y Constelación limitan cada impacto contra jefes a un 24% de su vida máxima antes de esos filtros. Danza mantiene ese mismo límite. Los efectos secundarios no cargan otra ulti.

## Ensayar sin esperar
En el inicio del nivel, pulsar P o el botón «ENSAYAR ULTIS». Es un entrenamiento sin oleadas ni progreso de campaña, con recarga automática entre pruebas. Elegir 1/2/3 y confirmar Q + Shift derecho. R reinicia el nivel y sale del entrenamiento. La carga del ensayo no se guarda en campaña.

## Luz y aura
Cinco balizas se colocan sobre apoyos estáticos del escenario. Tienen luces 2D y halos con shader de emisión aditiva, visibles incluso sobre sprites que no reciben iluminación. Colores según reino. Cada jugador tiene aura emisiva; aparece al completar la barra, durante la ulti y en su remate. Los brillos son procedurales de píxeles, no nuevas ilustraciones generadas.

## Validación
CoopPolishQA es un componente de pruebas optativo; no se guarda en escenas. Su informe está en CoopPolishQA.txt. Usa plataformas temporales y jugadores protegidos para aislar casos de muerte, superficies inestables, rescate, límites de botín y las tres ultis. El límite contra jefes se prueba con un enemigo configurado como jefe; no sustituye jugar los tres combates completos. TogetherPlayModeQA cubre el objetivo inicial y el flujo cooperativo anterior. Los informes históricos de Hielo/Fuego describen reglas previas de botín y no certifican estos cambios. No se ha certificado todavía el equilibrio mediante una partida humana completa ni una build independiente.
