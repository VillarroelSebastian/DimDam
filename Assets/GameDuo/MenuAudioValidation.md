# Menu, audio y transiciones

Se agrego una melodia original de 35.56 segundos en bucle para el menu, con entrada suave y desvanecimiento antes de la introduccion.

Se corrigio la ausencia de AudioListener en el menu, que dejaba sin sonido el primer video. La pista del video se configura explicitamente y comienza a volumen completo; solo se desvanece al terminar.

La comprobacion en Play Mode termino con 0 fallos. Se verificaron los nueve videos con fotogramas decodificados, muestras de audio no silenciosas, una unica escucha activa, preparacion con pantalla negra y activacion de los cinco capitulos detras de negro opaco. Tambien pasaron los controles del menu, la salida de la introduccion al tutorial y la restauracion del tiempo y el audio al final.

La prueba acelera los videos hasta cerca del final despues de comprobar sus primeros segundos, y comprueba el evento real de finalizacion. No constituye una reproduccion manual completa de cada video.

Informe detallado: CampaignVerification.txt. Validacion de referencias y audio codificado: ../../Logs/MenuFlowStaticValidation.txt.
