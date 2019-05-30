using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace NeuralNetworkForBacherlor.New
{
    class NeuralNetwork
    {

        public Random rand = new Random();
        public List<Neuron> inputLayer = new List<Neuron>();
        public List<Neuron[]> hiddenLayers = new List<Neuron[]>();
        public List<Neuron> outputLayer = new List<Neuron>();

        public double momentum { get; set; }
        public double learningRate { get; set; }

        public List<double[]> inputs { get; set; }
        public List<double[]> outputKinds { get; set; }

        public NeuralNetwork(List<double[]> inputs, List<double[]> outputKinds, int[] hidden, double momentum,
                             double learningRate, double threshold, double minRange, double maxRange)
        {
            this.inputs = inputs;
            this.outputKinds = outputKinds;
            this.momentum = momentum;
            this.learningRate = learningRate;
            int inputNeuron = inputs[0].Length;
            int[] hiddenNeurons = hidden;
            int outputNeuron = outputKinds[0].Length;
            // input layer
            for (int i = 0; i < inputNeuron; i++)
            {
                Neuron neuron = new Neuron();
                inputLayer.Add(neuron);
            }
            // hidden layers
            for (int i = 0; i < hiddenNeurons.Length; i++)
            {
                Neuron[] neurons = new Neuron[hiddenNeurons[i]];
                if (i == 0)
                {
                    for (int j = 0; j < neurons.Length; j++)
                    {
                        neurons[j] = new Neuron();
                        neurons[j].addConnections(inputLayer);
                    }
                }
                else
                {
                    for (int j = 0; j < neurons.Length; j++)
                    {
                        neurons[j] = new Neuron();
                        neurons[j].addConnections(hiddenLayers[i - 1]);
                    }
                }
                hiddenLayers.Add(neurons);
            }
            // output layer
            for (int i = 0; i < outputNeuron; i++)
            {
                Neuron neuron = new Neuron();
                neuron.addConnections(hiddenLayers[hiddenLayers.Count - 1]);
                outputLayer.Add(neuron);
            }

            // Initialize random weights
            foreach (Neuron[] neurons in hiddenLayers)
            {
                foreach (Neuron neuron in neurons)
                {
                    List<Connection> connections = neuron.getAllConnections();
                    foreach (Connection conn in connections)
                    {
                        double newWeight = getRandomNumber(minRange, maxRange);
                        conn.setWeight(newWeight);
                    }
                    connections[0].setWeight(threshold);
                }
            }
            foreach (Neuron neuron in outputLayer)
            {
                List<Connection> connections = neuron.getAllConnections();
                foreach (Connection conn in connections)
                {
                    double newWeight = getRandomNumber(minRange, maxRange);
                    conn.setWeight(newWeight);
                }
            }

            // Reset id counters
            Neuron.counter = 0;
            Connection.counter = 0;
        }

        public double getRandomNumber(double minRange, double maxRange)
        {
            return minRange + (maxRange - minRange) * rand.NextDouble();
        }

        public void setInput(double[] inputs)
        {
            for (int i = 0; i < inputLayer.Count; i++)
            {
                inputLayer[i].setOutput(inputs[i]);
            }
        }

        public double[] getOutput()
        {
            double[] outputs = new double[outputLayer.Count];
            for (int i = 0; i < outputLayer.Count; i++)
                outputs[i] = outputLayer[i].getOutput();
            return outputs;
        }

        public void activate()
        {
            foreach (Neuron[] neurons in hiddenLayers)
            {
                foreach (Neuron neuron in neurons)
                {
                    neuron.calculateOutput();
                }
            }
            outputLayer.ForEach(x => { x.calculateOutput(); });
        }

        public void applyBackpropagation(double[] expectedOutput)
        {
            int i = 0;
            foreach (Neuron n in outputLayer)
            {
                List<Connection> connections = n.getAllConnections();
                foreach (Connection connection in connections)
                {
                    double pervY = connection.leftNeuron.getOutput();
                    double y = n.getOutput();
                    double dy = expectedOutput[i];
                    double partialDerivative = (dy - y) * y * (1 - y);
                    double deltaWeight = learningRate * partialDerivative * pervY;
                    double newWeight = connection.getWeight() + deltaWeight;
                    connection.setDeltaWeight(deltaWeight);
                    connection.setWeight(newWeight + momentum * connection.getPrevDeltaWeight());
                }
                i++;
            }
            double[] pervPartialDerivatives = new double[0];
            for (int j = hiddenLayers.Count - 1; j >= 0; j--)
            {
                Neuron[] neurons = hiddenLayers[j];
                double[] nowPartialDerivatives = new double[neurons.Length];
                int n = 0;
                foreach (Neuron neuron in neurons)
                {
                    double y = neuron.getOutput();
                    double sumOutputs = 0;
                    if (j == hiddenLayers.Count - 1)
                    {
                        int k = 0;
                        foreach (Neuron outputN in outputLayer)
                        {
                            double wjk = outputN.getConnection(neuron.id).getWeight();
                            double dy = expectedOutput[k];
                            double yk = outputN.getOutput();
                            sumOutputs += (dy - yk) * yk * (1 - yk) * wjk;
                            k++;
                        }
                    }
                    else
                    {
                        int k = 0;
                        foreach (Neuron hiddenN in hiddenLayers[j + 1])
                        {
                            double wjk = hiddenN.getConnection(neuron.id).getWeight();
                            sumOutputs += pervPartialDerivatives[k] * wjk;
                            k++;
                        }
                    }
                    double partialDerivative = y * (1 - y) * sumOutputs;
                    nowPartialDerivatives[n] = partialDerivative;
                    List<Connection> connections = neuron.getAllConnections();
                    foreach (Connection connection in connections)
                    {
                        double pervY = connection.leftNeuron.getOutput();
                        double deltaWeight = learningRate * partialDerivative * pervY;
                        double newWeight = connection.getWeight() + deltaWeight;
                        connection.setDeltaWeight(deltaWeight);
                        connection.setWeight(newWeight + momentum * connection.getPrevDeltaWeight());
                    }
                    n++;
                }
                pervPartialDerivatives = nowPartialDerivatives;
            }
        }

        public string run(int maxSteps, double minError)
        {

            int i;
            // Train neural network until minError reached or maxSteps exceeded
            double squareError = 10;
            double minSquareError = 1000000;
            int correct = 0;
            for (i = 0; i < maxSteps && squareError > minError; i++)
            {
                switch (maxSteps)
                {
                    case 200:
                        learningRate = 0.1;
                        momentum = 0.05;
                        break;
                    case 400:
                        learningRate = 0.05;
                        momentum = 0.025;
                        break;
                    case 600:
                        momentum = 0.0001;
                        break;
                }
                squareError = 0;
                correct = 0;
                for (int x = 0; x < inputs.Count; x++)
                {
                    int ranx = rand.Next(0, inputs.Count);
                    setInput(inputs[ranx]);
                    activate();
                    double[] output = getOutput();
                    double[] expectedOutput = outputKinds[ranx];
                    for (int j = 0; j < expectedOutput.Length; j++)
                    {
                        double err = Math.Pow(expectedOutput[j] - output[j], 2);
                        squareError += err;
                    }
                    squareError /= expectedOutput.Length;
                    //TODO: min distance? to result maybe the right answer
                    //double distance = Math.Abs(1 - output[0]);
                    //int idx = 0;

                    //for (int k = 0; k < output.Length; k++)
                    //{
                    //    double newDistance = Math.Abs(1 - output[k]);
                    //    if (newDistance < distance)
                    //    {
                    //        idx = k;
                    //        distance = newDistance;
                    //    }
                    //}
                    //double y = Array.IndexOf(outputKinds[x], outputKinds[x].Max());
                    //if (y == idx) ++correct;
                    /*var gotOut = Array.IndexOf(output, output.Max());
                    var desOut = Array.IndexOf(outputKinds[x], outputKinds[x].Max());
                    if (gotOut == desOut)
                    {
                        correct++;
                    }*/
                    applyBackpropagation(expectedOutput);
                }
                Console.WriteLine(squareError);
                if (squareError < minSquareError)
                {
                    minSquareError = squareError;
                    foreach (Neuron[] neurons in hiddenLayers)
                    {
                        foreach (Neuron neuron in neurons)
                        {
                            List<Connection> connections = neuron.getAllConnections();
                            foreach (Connection conn in connections)
                            {
                                conn.setBestWeight(conn.getWeight());
                            }
                        }
                    }
                    foreach (Neuron neuron in outputLayer)
                    {
                        List<Connection> connections = neuron.getAllConnections();
                        foreach (Connection conn in connections)
                        {
                            conn.setBestWeight(conn.getWeight());
                        }
                    }
                }
            }
            if (i == maxSteps)
            {
                foreach (Neuron[] neurons in hiddenLayers)
                {
                    foreach (Neuron neuron in neurons)
                    {
                        List<Connection> connections = neuron.getAllConnections();
                        connections.ForEach(x => { x.setWeightAsBest(); });
                    }
                }
                foreach (Neuron neuron in outputLayer)
                {
                    List<Connection> connections = neuron.getAllConnections();
                    connections.ForEach(x => { x.setWeightAsBest(); });
                }
            }
            printAllWeights();
            // result = runTimes + MSE + trainRate
            return i.ToString() + " " + squareError + " " + (double)correct / inputs.Count * 100 + "%";
        }

        public string test(List<double[]> inputs, List<double[]> outputKinds)
        {
            this.outputKinds = outputKinds;
            double correct = 0;
            double howmanyCorrect = 0;

            for (int x = 0; x < inputs.Count; x++)
            {
                int y = Array.IndexOf(outputKinds[x], outputKinds[x].Max());
                setInput(inputs[x]);
                activate();
                double[] output = getOutput();
                //output.ToList().ForEach(sk => { Console.WriteLine(sk); });
                double error = 0;
                int kk = Array.IndexOf(output, output.Max());
                if (kk == y && output[kk] > 0.2)
                {
                    howmanyCorrect++;
                }
                else if(output[kk]<0.2)
                    howmanyCorrect++;

                for (int i = 0; i < output.Length; i++)
                {
                    error += Math.Abs(outputKinds[x][i] - output[i]);
                }

                error /= output.Length;
                correct += error;
            }
            Console.WriteLine(howmanyCorrect/inputs.Count*100+"%");
            return howmanyCorrect / inputs.Count * 100 + "";
            //return 100 - correct / inputs.Count * 100 + "%";
        }

        public void printAllWeights()
        {
            outputLayer.ForEach(this.printWeights);
            Console.WriteLine();
        }

        public void printWeights(Neuron n)
        {
            List<Connection> connections = n.getAllConnections();
            foreach (Connection con in connections)
            {
                double w = con.getWeight();
                Console.WriteLine("NeuronID = " + n.id + ", ConnectionID = " + con.id + ",Weight = " + w);
            }
        }
    }
}
