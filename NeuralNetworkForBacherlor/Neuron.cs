using System;
using System.Collections.Generic;

namespace NeuralNetworkForBacherlor
{
    public class Neuron
    {
        static Random n = new Random();
        public List<Dendrite> Dendrites { get; set; }
        public double Bias { get; set; }
        public double Delta { get; set; }
        public double Value { get; set; }

        public int DendriteCount
        {
            get
            {
                return Dendrites.Count;
            }
        }

        public Neuron()
        {
            Bias = n.NextDouble();
            Dendrites = new List<Dendrite>();
        }
    }
}
