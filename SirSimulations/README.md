# Simulación SIR Paralela en Grilla 2D

## Descripción general

Este proyecto implementa un modelo epidemiológico SIR sobre una grilla de 1000×1000 celdas, lo que representa una población de exactamente un millón de personas. La idea es bastante directa: cada celda es una persona, cada persona puede estar sana, enferma o recuperada, y cada día el sistema decide su destino lanzando un dado probabilístico. Se simula durante 365 días, primero de forma secuencial y luego dividiendo el trabajo entre múltiples hilos para ver cuánto se puede acelerar el proceso.

La verdad es que este tipo de simulaciones son las que realmente se usan para modelar brotes reales, y lo interesante aquí no es solo que funcione, sino entender por qué el paralelismo ayuda y hasta dónde llega esa ayuda antes de encontrar sus límites naturales.

## Modelo matemático

El modelo SIR divide a la población en tres estados posibles en todo momento: S (Susceptible), que es una persona sana que puede contagiarse, I (Infected), que es una persona enferma que puede contagiar a sus vecinos, y R (Recovered/Dead), que es una persona que salió del ciclo activo ya sea porque se recuperó o porque murió.

Las transiciones entre estados siguen reglas probabilísticas que se evalúan cada día. Un susceptible se infecta dependiendo de cuántos vecinos infectados tenga a su alrededor, usando la vecindad de Moore que son los 8 vecinos que rodean a la celda, y cada uno de esos vecinos representa una oportunidad independiente de contagio con probabilidad β. Un infectado se recupera con probabilidad γ, y si no se recupera, puede morir con probabilidad μ, y si ninguna de las dos ocurre simplemente sigue enfermo otro día más.

Los parámetros usados fueron β = 0.30 como probabilidad de contagio por vecino, γ = 0.05 como probabilidad de recuperación diaria, y μ = 0.01 como probabilidad de muerte diaria. La fracción inicial de infectados fue del 0.1% de la población, con semilla aleatoria 42 para reproducibilidad. Vale la pena notar que β = 0.30 es bastante agresivo, más que el COVID-19 real por ejemplo, lo que explica la velocidad con la que la epidemia barrió la población en los primeros 90 días.

## Arquitectura del proyecto

El código se divide en tres capas principales.

### Domain

Aquí vive todo lo que define el mundo de la simulación sin depender de nada externo. CellState es el enum que define los tres estados posibles de una persona, SimulationConfig agrupa todos los parámetros que controlan la enfermedad como la tasa de contagio o la probabilidad de muerte, y DayStatistics es básicamente una fotografía del estado de la población al final de cada día.

### Application

Acá está la lógica real. Grid maneja la grilla con el sistema de doble buffer, SirSimulator contiene la versión secuencial día a día, y ParallelSirSimulator es la versión paralela con ghost cells.

### Infrastructure

Solo tiene CsvExporter, que se encarga de guardar los resultados en archivos CSV. No sabe nada de la simulación, solo recibe datos y los escribe.

### Doble buffer

Un detalle de implementación que vale la pena mencionar es el uso de doble buffer en la grilla. Si actualizas las celdas en el mismo arreglo donde las lees, una celda que ya se infectó hoy va a contaminar a sus vecinas en el mismo día, cuando en realidad ese cambio debería verse recién mañana. Con doble buffer se lee del arreglo actual y se escribe en el siguiente, y al terminar el día se intercambian. Eso garantiza que todos los vecinos que ve una celda son del estado de ayer, no del estado parcialmente actualizado de hoy.

## Paralelización: ghost cells

La estrategia de paralelización divide la grilla en bloques horizontales, uno por hilo. Con 4 hilos, por ejemplo, cada uno trabaja 250 filas. El problema surge en los bordes: la celda de la fila 249 necesita ver a su vecina en la fila 250 que pertenece al bloque del siguiente hilo.

La solución son las ghost cells: antes de que cada hilo empiece, ya tiene acceso a las filas frontera del bloque vecino porque todos leen del mismo buffer de lectura compartido sin modificarlo. Esto elimina completamente la necesidad de locks o sincronización durante la actualización, porque cada hilo escribe en rangos de filas completamente distintos. Con 4 hilos quedaría así: el hilo 0 trabaja las filas 0 a 249 y lee como ghost la fila 250, el hilo 1 trabaja las filas 250 a 499 y lee como ghost la 249 y la 500, el hilo 2 trabaja las filas 500 a 749 y lee como ghost la 499 y la 750, y el hilo 3 trabaja las filas 750 a 999 y lee como ghost la 749. La sincronización solo ocurre una vez por día al hacer el swap de buffers, que es una operación de nanosegundos.

### Reducción paralela de estadísticas

Para contar infectados, recuperados y muertos al final de cada día, cada hilo acumula sus propios contadores locales mientras recorre su bloque de filas. Al terminar, suma sus resultados al total global usando Interlocked.Add, que garantiza atomicidad sin necesidad de locks explícitos. Esto evita el cuello de botella clásico de tener un único contador compartido al que todos los hilos acceden constantemente.

## Resultados de la simulación

### Evolución de la epidemia

La simulación produjo un brote muy agresivo, lo cual es consistente con los parámetros elegidos. El día 1 había 3,271 infectados sobre 996,673 susceptibles, una chispa pequeña en una población completamente vulnerable. Para el día 30 la situación era completamente distinta: los infectados habían explotado hasta 424,859, casi el 42% de la población enferma al mismo tiempo, mientras los susceptibles caían a 139,735. La epidemia estaba en su punto más violento.

Para el día 60 los susceptibles prácticamente habían desaparecido, quedaban apenas 203 personas sanas, y los infectados empezaban a caer porque ya no había a quién contagiar. El día 90 los susceptibles eran exactamente cero, la epidemia había alcanzado a toda la población en menos de tres meses. A partir de ahí fue solo una cuestión de tiempo hasta que los últimos infectados se recuperaran o murieran, y para el día 240 la epidemia se había extinguido completamente.

El resultado final fue 840,104 recuperados, que representa el 84% de la población, y 159,896 muertos, el 16% restante. Nadie quedó susceptible.

## Experimentos de strong scaling

Se midió el tiempo total de simulación fijando el problema en 1000×1000 celdas y 365 días, variando únicamente el número de hilos. Esto es lo que se conoce como strong scaling: el problema no crece, solo el número de trabajadores.

La versión secuencial tardó 32.08 segundos como referencia base. Con 1 hilo paralelo el tiempo bajó a 25.57 segundos con un speed-up de 1.25x, lo que parece contradictorio pero se explica porque Parallel.For aprovecha algunas optimizaciones del runtime de .NET que el loop secuencial no tiene. Con 2 hilos el tiempo cayó a 18.74 segundos y el speed-up subió a 1.71x, que es una mejora real y esperada. Con 4 hilos llegó a 15.96 segundos y 2.01x, y con 8 hilos prácticamente no hubo cambio: 15.63 segundos y 2.05x.

El estancamiento entre 4 y 8 hilos tiene una explicación clara: la máquina donde se ejecutó tiene 4 núcleos físicos. Con 8 hilos el sistema operativo los distribuye entre los mismos 4 núcleos mediante hyperthreading, y ya no hay ganancia real en velocidad de cómputo. Se llega a lo que se conoce como saturación de recursos, y es completamente normal.

### Ley de Amdahl

La Ley de Amdahl predice que el speed-up máximo teórico está limitado por la fracción del programa que no se puede paralelizar. En este caso esa fracción incluye el swap de buffers diario, la inicialización de la grilla y el overhead de coordinar los hilos. Aunque pequeñas, estas partes establecen un techo que ninguna cantidad de hilos puede romper. Con los resultados obtenidos se estima que aproximadamente el 50% del tiempo corresponde a código paralelizable, lo que es consistente con un speed-up máximo cercano a 2x.

## Cómo ejecutar el proyecto

Para correr el proyecto solo se necesita tener instalado .NET 9.0 SDK. Desde la terminal se entra a la carpeta del proyecto y se ejecuta dotnet run. El programa corre automáticamente la versión secuencial y luego las versiones paralelas con 1, 2, 4 y 8 hilos. Al terminar genera los archivos sequential_results.csv, parallel_results_1threads.csv, parallel_results_2threads.csv, parallel_results_4threads.csv, parallel_results_8threads.csv y scaling_results.csv en la carpeta de salida.

## Conclusiones

La verdad es que este proyecto deja ver de forma muy concreta por qué el paralelismo no es una solución mágica. La ganancia existe y es real, pasar de 32 segundos a 15 no es poca cosa, pero hay un techo que viene determinado por el hardware disponible y por las partes del código que inevitablemente deben ejecutarse de forma secuencial. La estrategia de ghost cells resultó ser elegante y efectiva porque elimina la necesidad de sincronización durante la parte más costosa del cómputo, que es la actualización de cada celda, y deja la coordinación para el único momento donde realmente es necesaria.
