# GameDuo

Abre `Scenes/01_Tutorial_Chicle.unity` y pulsa Play. Enter inicia el combate.
Las cinco escenas estan al principio de Build Settings, con el tutorial como inicio del ejecutable.

1. Tutorial Chicle: salón del trono de caramelo y tres oleadas, sin llave ni puerta. Incluye controles, salto, esquiva, lazo y rescate.
2. Reino Neon: restaurar nucleo, recoger fragmento y reunir a ambos en el portal.
3. Rey Hielo: completar las oleadas y derrotar al jefe.
4. Rescate de pinguinos: reunir los 16. Perder por tiempo permite reintentar; no avanza.
5. Jefe Chicle (`05_JefeChicle`): salon final.png, jefe rosado y bandadas de murcielagos; completar el combate y mostrar el cierre de campana.

Las victorias avanzan automaticamente tras dos segundos. M abre el menu de campana; R conserva el reinicio propio de cada escena. El progreso desbloqueado se guarda con la clave PlayerPrefs `GameDuo.Unlocked`. Ensayar ultimas en el tutorial no desbloquea capitulos.

Son copias de las escenas existentes y comparten sus recursos. El tutorial adapta la primera arena de chicle y conserva sus lecciones cooperativas. Incluye tres atlas nuevos: invasores de chicle, Rey Hielo y tio usurpador, con poses de reposo, preparacion, ataque, dano y derrota. No incluye cinematicas.

Mimo usa escala 1.35 y Pipro 0.8 tanto en exploracion como combate y pinguinos. Se conserva la escala durante las animaciones y la danza. Salto minimo: 14.5 unidades/s; la corte helada tiene escalones alternativos y avisos visuales de ataques.

El tutorial practica mover, saltar y esquivar con ambos antes de abrir la cerradura. Las lecciones muestran diagramas y teclas. La danza tiene teclas visuales y flechas dibujadas, identidad por color, anillo temporal y respuesta de acierto/fallo. Cada pulso dura 0.85 s y acepta una tecla entre 0.18 y 0.70 s.

`GameDuoCampaign` instala el controlador al cargar una escena de esta carpeta. Centraliza el orden y las condiciones de avance. Los puntos de entrada de mapa/siguiente y la salida del recorrido de mecanismos consultan si hay una campana activa. Las escenas originales conservan su recorrido habitual y reciben tambien la identidad visual comun de los protagonistas y enemigos.

Menu de Unity: GameDuo > Validar progresion. Comprueba referencias, protagonistas, oleadas y registro en el build; no reemplaza una prueba manual del recorrido.

Menu GameDuo > Probar proporciones, saltos y danza ejecuta pruebas en Play Mode con un teclado temporal: cinco capitulos, diferencia de tamano, salto a la segunda plataforma de hielo y doce pulsos de danza. El informe se escribe en PolishPlayModeQA.txt.

El jefe conserva sus tres fases, nucleo de menta movil y lluvia de azucar. Levanta los brazos durante 1.4 segundos antes de invocar una bandada. Las bandadas alternan altura baja y alta y direccion, con 5/6/7 murcielagos segun fase. Saltar o cambiar de plataforma evita su trayectoria. Los murcielagos desaparecen al derrotar al jefe o terminar la partida.

GameDuo > Probar jefe rosado y murcielagos verifica las fases, lluvia de azucar, invocacion, dano por contacto y cierre de campana.
