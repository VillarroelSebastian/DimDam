# QA de Chicle Rumble

Ejecucion PlayMode en un proyecto aparte (Unity 6000.6.5f1) y verificada tambien sobre una copia del proyecto con URP.
5 pruebas, 5 correctas. Detalle numerico en `QA_log.txt`; las capturas de esta carpeta son de esa misma ejecucion.

| Prueba | Que comprueba |
|---|---|
| FullPlaythrough | Un bot recorre los 10 niveles de los dos mundos, los ascensos entre pisos, ambos jefes (3 fases cada uno), K.O., resultados y game over. En el nivel 1 gana solo atrapando y pateando. |
| IceMechanics | Hielo desliza 3,66 u frente a 0,19 u en nieve; congelacion y deshielo por botones (0,27 s); iceberg quebradizo se rompe y vuelve; escudo del pinguino solo bloquea de frente; el ascensor transporta (0,8 u); la cama elastica lanza a 24 u/s; 3 impactos levantan el puente; la ventisca empuja al expuesto y no al protegido. |
| MonarchWeakPoint | En las 3 formas el sol es pequeno (1,7 u) y esta dentro del caparazon; el chicle contra el caparazon no hace dano y contra el sol si. |
| MenuAndHelp | La primera partida abre las 3 pantallas de ayuda y despues arranca; el menu cambia de mundo. |
| SceneAssetBoots | La escena guardada arranca el juego (referencia de script correcta). |

Limites: no certifica el balance con jugadores humanos ni la duracion real de los jefes; el audio se verifica por compilacion y ejecucion, no de oido.
