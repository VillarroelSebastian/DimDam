# Reino de Fuego y botín visible

## Probar ahora
Abrir `Assets/ReinoDeFuego/0_0_Scenes/Fire_Level_01.unity`, Play y Enter. Los controles anteriores siguen iguales. La campaña normal encadena Chicle → Hielo → Fuego; vencer a Cencellón desbloquea la tercera pestaña del mapa. TAB cambia entre los reinos disponibles. El progreso sigue siendo de sesión.

## Botín: por qué ahora se ve
Antes, el enlace podía recoger un objeto en el mismo instante en que nacía. Ahora cada baja deja un objeto real, que sale hacia arriba y flota con icono grande, halo giratorio y nombre. Tiene 1,2 segundos de aparición durante los cuales NO se puede recoger; dura 18 segundos y hay como máximo seis objetos simultáneos. Al estar aproximadamente un metro por encima de la baja, hace falta saltar, tocarlo o inclinar el enlace para recogerlo. No se exige otra tecla.

La secuencia de bajas rota entre Prisma, Chispa, Burbuja, Anillo y Corazón. Repetir uno renueva su duración, no acumula infinitamente su potencia. El HUD muestra iconos y segundos restantes. Estos cambios funcionan en los tres reinos.

| Objeto | Efecto |
|---|---|
| Prisma | Rayo más ancho y daño x1,5 durante 8 s. La ulti mantiene su propio multiplicador x2,4; no se multiplican entre sí. |
| Chispa Encadenada | Durante 10 s, un impacto efectivo del rayo encadena hasta dos enemigos a 3,5 unidades del primero: 10 de daño por objetivo, con cooldown global de 0,8 s y arco dorado visible. No se propaga recursivamente. |
| Burbuja | Un golpe protegido por jugador, con duración máxima de 15 s. |
| Anillo Solar | Durante 10 s, un pulso cada 1,4 s alrededor del segmento alcanza objetivos a menos de 0,95 unidades del enlace: 18 de daño por objetivo. La onda se dibuja en el mundo. |
| Corazón | +1 corazón a cada jugador vivo, hasta su máximo, y 30 de carga de Resonancia. No resucita. |

Los poderes ofensivos nuevos dependen de ambos jugadores vivos y de un segmento con longitud válida. No hay disparos individuales. El daño derivado de Chispa/Anillo no recarga Resonancia; el daño efectivo directo del rayo sí. Todos los efectos de daño respetan las reglas de vulnerabilidad de los jefes y sus umbrales. Todo reinicia al reintentar o cambiar de nivel. Resonancia sigue requiriendo Q y Shift derecho dentro de dos segundos.

## Las cinco arenas
1. **Huerto de Brasas:** plataformas amplias, primeros hornos y salamandras. El botín enseña a modificar la orientación del enlace.
2. **Puerto de Fundición:** cintas opuestas, ascensor de fuelle, grúas elevadas y respiraderos alternados.
3. **Grutas del Magma Cantor:** cristales elásticos y escalones hacia la cámara alta; fénix, flores y hornos en diferentes alturas.
4. **Bastión de la Ceniza:** basalto quebradizo que reaparece, puente móvil y mezcla de presión terrestre/aérea. Respiraderos con aviso de 1,5 s.
5. **Corazón de Brontoforno:** dos oleadas preparatorias y el jefe final; galerías en dos alturas para cruzar su núcleo.

## Enemigos
- **Horno granadero:** lanza una vasija de magma en arco; el lugar previsto de impacto recibe un aviso independiente antes de arder.
- **Escarabajo de basalto:** reduce al 40 % el daño mientras está blindado, anuncia una embestida y deja un rastro de fuego anunciado. Durante su recuperación de 1,7 s recibe daño completo.
- **Salamandra chispa:** salta y deja tres pequeñas zonas ardientes, cada una con su propio aviso.
- **Fénix ascua:** vuela y fija la posición de un jugador antes de liberar cinco plumas de fuego en abanico.
- **Flor pirotécnica:** abre sus pétalos y dispara una rueda radial; desde el nivel 4 añade un segundo patrón girado.
- **Diablillo de mecha:** se impulsa y anuncia una explosión en la posición fijada del objetivo. Se puede salir de la marca antes del estallido.

Los objetivos alternan; no se corrige instantáneamente un ataque ya anunciado para seguir al jugador. Avisos cian sobre fondos de magma, ataques activos naranja/amarillo. Los proyectiles despejables se pueden eliminar con el enlace durante Resonancia; las zonas de suelo no.

## Brontoforno
Criatura original de fundición: salamandra de basalto, guanteletes de forja, cuernos-chimenea, halo de latón y corazón de magma. 1600 de vida, fases 100–70, 70–35 y 35–0 %. No se puede saltar un umbral con daño excesivo. Transformaciones invulnerables de 2,4 s, con limpieza de peligros anteriores.

Cinco familias de ataques, con variantes de cantidad/velocidad según fase: martillos que levantan columnas marcadas, rueda solar radial con huecos, río de fuego que obliga a buscar refugio, cometas balísticos y aliento en abanico. Tras cada ataque se abre el corazón durante 3,1 s (3,6 en fase final); en fase 2 cambia de altura. La derrota se anima durante 1,92 s antes de liberar el registro de oleada. Límite global compartido de 24 peligros. Estos valores son balance inicial, no una dificultad certificada con personas.

## Arte, herramientas y mantenimiento
Arte generado con la herramienta incorporada `image_gen`: FireEnemies.png (48 poses), Brontoforno.png (16 poses), FireBackgrounds.png (cinco fondos). El jefe pasó por una edición de transparencia; los RGB de píxeles con alpha cero no se muestran en el juego. Texturas Point, sin mipmaps ni compresión. Las filas del atlas de enemigos y los cinco fondos tienen recortes ajustados a la imagen generada; los pies se alinean por alpha. Las animaciones eligen poses según gameplay, como los otros reinos.

Plataformas de obsidiana con luces animadas, llamas, brasas, iconos y arcos de energía son gráficos procedurales. No se atribuyen a image_gen. Los archivos finales y el manifiesto están en `0_3_Art/`.

`FireKingdomBuilder.Build()` crea exclusivamente los recursos nuevos de Fuego y añade sus cinco escenas al build; se niega a sobrescribir niveles ya existentes. No repetirlo para correcciones pequeñas. No se ejecutaron constructores globales de Chicle o Hielo.

Pruebas optativas: `FireExpansionValidation.Run()`, `FirePlayModeQA` y `CampaignTraversalQA`. Los componentes de QA se añaden solo temporalmente en Play Mode. El recorrido usa daño acelerado y protección de jugadores para verificar oleadas, desbloqueos y transiciones; no equivale a jugarlo entre dos personas ni a validar una build standalone.

Sin commits ni pushes.

## Verificación de esta entrega
- `FireAssets.txt`: 54 PASS, 0 FAIL; escenas, datos, prefabs, animaciones y referencias.
- `FirePlayMode.txt`: 56 PASS, 0 FAIL; botín real al morir, espera antes de recogida, recogida por el rayo, cadena, pulsos, seis cerebros enemigos, saltos físicos a ambas galerías y quince combinaciones de fase/patrón del jefe.
- `CampaignTraversal.txt`: 49 PASS, 0 FAIL; las quince arenas de Chicle, Hielo y Fuego alcanzan victoria, desbloquean su continuación y llegan al destino correcto.
- `IceRegression.txt`: 42 PASS, 0 FAIL; regresión de las mecánicas anteriores de Hielo con los nuevos sistemas compartidos.
Se revisaron capturas de juego real y capturas del jefe con actores colocados temporalmente para inspección. Los componentes y objetos de QA no se guardan en las escenas. Falta una partida humana de dos personas para ajustar ritmo y dificultad; no se ha certificado una build standalone.
- `LootBurstLimit.txt`: PASS; doce bajas simultáneas conservan como máximo seis objetos activos. Los rótulos cercanos se reparten en filas para mantenerlos legibles.
- Capturas: [botín durante la partida](../0_6_QA/FireLoot_Final.png), [jefe con actores colocados para revisión](../0_6_QA/Brontoforno_Review.png).
- Procedencia: [prompts completos de image_gen](../0_3_Art/GenerationPrompts.md), [manifiesto de recursos](../0_3_Art/AssetManifest.json).
