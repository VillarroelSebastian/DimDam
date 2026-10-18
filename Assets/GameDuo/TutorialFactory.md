# Tutorial: salón del trono de caramelo

La escena `Scenes/01_Tutorial_Chicle.unity` usa el fondo entregado y las cinco poses del rey desde `Resources/GameDuo/Throne`. El rey alterna poses de temor, se cubre la cara y tiembla; al ganar deja de temblar.

Las colisiones del suelo, los tres peldaños del trono, los cinco de la derecha y el pedestal izquierdo usan las coordenadas de la ilustración (1672 × 941). Los escalones permiten saltar desde abajo y descender con S o flecha abajo. La cámara conserva la proporción del fondo.

Una sola explicación aparece al iniciar. Mimo confirma con F y Pipro con Enter; no se exige ejecutar movimientos para cerrarla. Después comienzan las tres oleadas. Las ayudas posteriores del tutorial no abren paneles modales. Este tutorial no crea sellos, llave ni cerradura. Los otros niveles conservan sus mecanismos.

`GameDuo > Guardar fabrica en escena tutorial` guarda la presentación en la escena. `GameDuo > Validar tutorial fabrica completo` verifica movimiento, salto, esquiva, ausencia de llave y puerta, animación del rey, combate, transición, rescate, derrota y reinicio. El informe se escribe en `TutorialFactoryQA.txt` y la captura en `TutorialFactory-preview.png`.

El tutorial agrega cinco plataformas visibles de galleta con colisiones de un solo sentido. Se puede saltar desde abajo y descender con S/Abajo. Las paredes invisibles heredadas quedan desactivadas; el rey conserva su posición.

Los enemigos usan Bill.png con carrera, reposo y dolor; se vuelven rojos al recibir daño y se disuelven durante 0,8 s al morir con partículas de ceniza. El aura se representa mediante un anillo a los pies y se omiten las balizas decorativas. La configuración del editor oculta Gizmos en Scene/Game al abrir o jugar el tutorial.


Actualizacion de aura y ulti: cuatro instrucciones iniciales (controles, rescate, combinacion de auras, ulti), cada una confirmada por Mimo con F y Pipro con Enter. No aparecen lecciones modales posteriores en los reinos. El aura envuelve el cuerpo con borde energetico pulsante. Acerquense 0.6 s y separense para compartir poderes. La ulti carga con 180 de dano o 36 s de tiempo activo; Q y Shift derecho quedan confirmados sin limite de simultaneidad. Comienza directamente la danza y termina en explosion y 8 s de lazo reforzado.
