# SimpleQRGenerator
genera un qr, puede ser un texto o una url
retorna un png o un texto (se ve bien desde una terminal ejecutando con curl)


:heavy_exclamation_mark:**IMPORTANTE**:heavy_exclamation_mark:

hay dos variables de entorno que hay que completar:

QRGENERATOR_ENDPOINT -> contiene el endpoint que genera una imagen png con el qr
TEXTQRGENERATOR_ENDPOINT -> contiene el endpoint que genera el texto con el qr

si QRGENERATOR_ENDPOINT tiene el valor "qrgenerator"
```
curl --location 'http://localhost:32770/qrgenerator/hola mundo'
```
si TEXTQRGENERATOR_ENDPOINT tiene el valor "textqrgenerator"
```
curl --location 'http://localhost:32770/textqrgenerator/hola mundo'
```


