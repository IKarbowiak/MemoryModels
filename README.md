# MemoryModels
MemoryModels is a final project for graduating master's program written in c#. It conatins two models: 
1) Hierarchial Temporal Memory - which is a biologically compatible model of intelligence proposed by Jeff Hawkins. The model consist of layers, regions, cells and synapses as the human brain. Layers may contain one or more regions, regions contains cells grouped in columns.  There are two kind of synapses: distal synapses which links column with an input data and proximal synapses which connect two different cells. The model has ability to remember and predict patterns.

2) Personal solution - is a hydraulic model of memory. I allows to create network with use of 3 different types of neurons and run simulation throught it. Durring the simulation declared amount of liquid enter the first element and propagate throught the rest of them, until the end of simulation. The meory is represented by liquid which stay in the main part of neurons - soma, after expired of liquid. The forgetting is simulated by decresing this remaining volume, it depend on strengthening factor and the numbers of finished simulation on this specific netowrk. 

In both of models it is possible to simulate the phenomena such as remembering, forgetting and also cells demage.
