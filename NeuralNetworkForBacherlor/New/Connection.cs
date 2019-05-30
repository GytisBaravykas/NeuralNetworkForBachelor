using System;
using System.Collections.Generic;
using System.Text;

namespace NeuralNetworkForBacherlor.New
{
    class Connection
    {
        private double weight = 0;
        private double bestWeight = 0;
        private double prevDeltaWeight = 0; // for momentum
        private double deltaWeight = 0;

        public Neuron leftNeuron { get; set; }
        public static int counter { get; set; }
        public int id { get; set; } // auto increment, starts at 0

        public Connection(Neuron fromN)
        {
            leftNeuron = fromN;
            id = counter;
            counter++;
        }

        public double getWeight()
        {
            return weight;
        }

        public void setWeight(double w)
        {
            weight = w;
        }

        public void setBestWeight(double w)
        {
            bestWeight = w;
        }

        public void setWeightAsBest()
        {
            weight = bestWeight;
        }

        public void setDeltaWeight(double w)
        {
            prevDeltaWeight = deltaWeight;
            deltaWeight = w;
        }

        public double getPrevDeltaWeight()
        {
            return prevDeltaWeight;
        }

        public Neuron getLeftNeuron()
        {
            return leftNeuron;
        }
    }
}
