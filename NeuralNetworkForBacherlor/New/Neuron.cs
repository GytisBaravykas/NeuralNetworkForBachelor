using System;
using System.Collections.Generic;
using System.Text;

namespace NeuralNetworkForBacherlor.New
{
    class Neuron
    {
        public static int counter { get; set; }
        public int id { get; set; } // auto increment, starts at 0
        private double output { get; set; }

        private List<Connection> connections = new List<Connection> { };
        private Dictionary<int, Connection> connectionLookup = new Dictionary<int, Connection>();

        public Neuron()
        {
            id = counter;
            counter++;
        }

        public void calculateOutput()
        {
            double v = 0;
            foreach(Connection con in connections)
            {
                Neuron leftNeuron = con.getLeftNeuron();
                double weight = con.getWeight();
                double y = leftNeuron.getOutput();
                v += weight * y;
            }
            output = sigmoid(v);
        }

        public double sigmoid(double x)
        {
            return 1.0 / (1.0 + (Math.Exp(-x)));
        }

        public void addConnections(List<Neuron> neurons)
        {
            foreach(Neuron n in neurons)
            {
                Connection con = new Connection(n);
                connections.Add(con);
                connectionLookup[n.id] = con;
            }
        }

        public void addConnections(Neuron[] neurons)
        {
            foreach(Neuron n in neurons)
            {
                Connection con = new Connection(n);
                connections.Add(con);
                connectionLookup[n.id] = con;
            }
        }

        public Connection getConnection(int neuronIndex)
        {
            return connectionLookup[neuronIndex];
        }

        public List<Connection> getAllConnections()
        {
            return connections;
        }

        public double getOutput()
        {
            return output;
        }

        public void setOutput(double o)
        {
            output = o;
        }
    }
}
