# Renovacion de 03_ReyHielo

Se incorporaron los seis PNG proporcionados a Resources/GameDuo/IceRefresh. La escena instala el fondo de caverna y cuatro variantes de plataformas al iniciar. Las superficies visuales se anclan al borde superior de sus colisiones; se mantienen los dos escalones accesibles.

El personaje usa una malla de 625 vertices con UV fijas, respiracion, anticipacion, gestos segun los cuatro ataques, recuperacion, reaccion al dano y desplome/desvanecimiento. Las transiciones son continuas y no cambian el cuerpo fisico ni el nucleo vulnerable. Los cristales giran en un objeto visual separado de la colision, con estela y aviso ambar.

Validacion realizada el 18 de octubre de 2026:

- Compilacion completa de scripts de juego con Roslyn y las referencias de Unity: correcta.
- Compilacion completa de scripts del editor, incluida la nueva prueba: correcta. Una advertencia obsoleta preexistente en IceRescueArtSetup.
- Los seis PNG abren correctamente; personaje y cuatro plataformas tienen canal alfa transparente.
- Unity en batch termina con codigo 1 inmediatamente despues de seleccionar el proyecto, antes de importar o ejecutar el metodo de pruebas. No se ha confirmado la apariencia ni el combate en Play Mode.

En Unity, ejecutar **GameDuo > Probar nuevo Rey Hielo**. La prueba abre la escena real, verifica movimiento de la malla y anclaje de plataformas, recorre los cuatro patrones y las tres fases, aplica dano por las ventanas del nucleo y verifica derrota y limpieza de peligros. Escribe IceRefreshPlayQA.txt. Revisar visualmente ademas encuadre del fondo, pies sobre las plataformas y deformaciones durante ataques. La prueba deja Play Mode al terminar.

Revision: plataformas sin grupos superiores de cristales; margenes transparentes recortados en la superficie de apoyo. Sprint/dash desactivado en las seis escenas de GameDuo. Las barras y paneles heredados de controles no se muestran; la explicacion basica queda solo en el tutorial. Compilacion de juego y editor correcta; revision visual en Play Mode pendiente.

Nueva ampliacion: el jefe usa ahora King.png en reposo y 32 dibujos adicionales para sus cuatro ataques. La malla anterior queda como respaldo si falta la hoja nueva. Se agregan 48 dibujos a los seis enemigos, conservando sus ocho poses originales. GameDuoIcePoseFrames mezcla las poses durante 55 ms y conserva los pies sobre la colision. Compilacion completa de juego/editor correcta y verificacion de 80 celdas distintas, no vacias y con transparencia. Play Mode sigue pendiente por cierre de Unity antes de ejecutar validacion.
