﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeuralNetworkForBacherlor
{
    public class NeuralNetwork
    {
        private static int errorWriteLineLimiter = 0;

        public List<Layer> Layers { get; set; }
        public double LearningRate { get; set; }
        public int LayerCount
        {
            get
            {
                return Layers.Count;
            }
        }

        public NeuralNetwork(double learningRate, int[] layers)
        {
            if (layers.Length < 2) return;
            this.LearningRate = learningRate;
            this.Layers = new List<Layer>();

            for (int x = 0; x < layers.Length; x++)
            {
                Layer layer = new Layer(layers[x]);
                this.Layers.Add(layer);

                for (int n = 0; n < layers[x]; n++)
                    layer.Neurons.Add(new Neuron());

                layer.Neurons.ForEach((nn) =>
                {
                    if (x == 0)
                        nn.Bias = 0;
                    else
                        for (int d = 0; d < layers[x - 1]; d++)
                            nn.Dendrites.Add(new Dendrite());
                });
            }
        }
        private double Sigmoid(double x)
        {
            return 1 / (1 + Math.Exp(-x));
        }

        public double[] Run(List<double> input)  // atrodo gerai -> patikrinti praktiskai
        {
            if (input.Count != this.Layers[0].NeuronCount) return null;

            for (int x = 0; x < Layers.Count; x++)
            {
                Layer layer = Layers[x];

                for (int y = 0; y < layer.Neurons.Count; y++)
                {
                    Neuron neuron = layer.Neurons[y];

                    if (x == 0)
                        neuron.Value = input[y];
                    else
                    {
                        neuron.Value = 0;
                        for (int np = 0; np < this.Layers[x - 1].Neurons.Count; np++)
                            neuron.Value = neuron.Value + this.Layers[x - 1].Neurons[np].Value * neuron.Dendrites[np].Weight;

                        neuron.Value = Sigmoid(neuron.Value + neuron.Bias);
                    }
                }
            }

            Layer last = this.Layers[this.Layers.Count - 1];
            int numOutput = last.Neurons.Count;
            double[] output = new double[numOutput];
            for (int i = 0; i < last.Neurons.Count; i++)
                output[i] = last.Neurons[i].Value;

            return output;
        }

        //TODO: pakoreguoti pagal mano varianta - done
        public bool Train(InputData inputData)
        {
            List<double> input = inputData.inputs;
            double[] output = inputData.outputs;

            if ((input.Count != this.Layers[0].Neurons.Count) || (output.Length != this.Layers[this.Layers.Count - 1].Neurons.Count)) return false;

            double[] predictions = Run(input);
            double error = 0;
            for (int i = 0; i < output.Length; i++)
            {
                error += Math.Abs(output[i] - predictions[i]);
            }

            if (errorWriteLineLimiter++ % 5000 == 0)
            {
                Console.WriteLine(error);
                errorWriteLineLimiter = 1;
            }

            for (int i = 0; i < this.Layers[this.Layers.Count - 1].Neurons.Count; i++)
            {
                Neuron neuron = this.Layers[this.Layers.Count - 1].Neurons[i];

                // output[i] desired value
                neuron.Delta = neuron.Value * (1 - neuron.Value) * (output[i] - neuron.Value);

            }
            // paslepto sluoksnio klaidos skaiciavimas
            for (int j = this.Layers.Count - 2; j >= 1; j--)
            {
                for (int k = 0; k < this.Layers[j].Neurons.Count; k++)
                {
                    Neuron n = this.Layers[j].Neurons[k];

                    for (int m = 0; m < this.Layers[j + 1].NeuronCount; m++)
                    {
                        n.Delta += n.Value *
                              (1 - n.Value) *
                              this.Layers[j + 1].Neurons[m].Dendrites[k].Weight *
                              this.Layers[j + 1].Neurons[m].Delta;
                    }
                    /*n.Delta += n.Value *
                              (1 - n.Value) *
                              this.Layers[j + 1].Neurons[i].Dendrites[k].Weight *
                              this.Layers[j + 1].Neurons[i].Delta;*/
                }
            }

            // Svoriu koregavimas
            for (int i = this.Layers.Count - 1; i >= 1; i--)
            {
                for (int j = 0; j < this.Layers[i].Neurons.Count; j++)
                {
                    Neuron n = this.Layers[i].Neurons[j];
                    n.Bias = n.Bias + (this.LearningRate * n.Delta);

                    for (int k = 0; k < n.Dendrites.Count; k++)
                        n.Dendrites[k].Weight = n.Dendrites[k].Weight + (this.LearningRate * this.Layers[i - 1].Neurons[k].Value * n.Delta);
                }
            }

            return true;
        }

    }
}
