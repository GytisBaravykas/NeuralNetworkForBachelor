using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace NeuralNetworkForBacherlor
{
    class Program
    {
        static Random _random = new Random();
        static void Main(string[] args)
        {

            //TODO: add čekuolis files
            var authors = new string[] {"justina maciūnaitė", "edmundas jakilaitis", "viktorija chockevičiūtė", "miglė lopetaitė", "aistė žebrauskienė", "agnė keizikienė", "viktorija vilcanaitė", "neringa mikėnaitė", "žiedūnė juškytė", "arnas mazėtis" };

            var trainPath = @"C:\Users\Gytis\Desktop\DuomenysNN\Train\";
            var testPath = @"C:\Users\Gytis\Desktop\DuomenysNN\Test\";
            string[] trainDir = Directory.GetFiles(trainPath);
            string[] testDir = Directory.GetFiles(testPath);

            // Sumaisomi duomenys NN tinklui
            Shuffle(trainDir);

            NeuralNetwork NeuralNet = new NeuralNetwork(0.01, new int[3]{179,5,authors.Length}); 
            //179 input , 5 hidden , 11 output


            // treniravimas - panasu reikia didinti kiekius arba apsimokymo greiti.
            // treniravimas kol kas letas nes skaito failus -> pagalvoti alternatyvu
            for (int x = 0; x < trainDir.Length; x++)
            {
                var maker = new InputMaker(File.ReadAllLines(trainDir[x])[1]);
                var author = File.ReadAllLines(trainDir[x])[0];
                author = author.Replace(";", "");
                author = author.Replace(",", "");
                var inputs = maker.Inputs;
                double[] desOutputs = new double[authors.Length];
                for (int y = 0; y < authors.Length; y++)
                {
                    if (authors[y] == author)
                        desOutputs[y] = 1; //Neuron index of the right author
                }
                NeuralNet.Train(inputs, desOutputs);

            }

            /*Shuffle(trainDir);
            for (int x = 0; x < trainDir.Length; x++)
            {
                var maker = new InputMaker(File.ReadAllLines(trainDir[x])[1]);
                var author = File.ReadAllLines(trainDir[x])[0];
                author = author.Replace(";", "");
                author = author.Replace(",", "");
                var inputs = maker.Inputs;
                double[] desOutputs = new double[authors.Length];
                for (int y = 0; y < authors.Length; y++)
                {
                    if (authors[y] == author)
                        desOutputs[y] = 1; //Neuron index of the right author
                }

                NeuralNet.Train(inputs, desOutputs);

            }
            Shuffle(trainDir);
            for (int x = 0; x < trainDir.Length; x++)
            {
                var maker = new InputMaker(File.ReadAllLines(trainDir[x])[1]);
                var author = File.ReadAllLines(trainDir[x])[0];
                author = author.Replace(";", "");
                author = author.Replace(",", "");
                var inputs = maker.Inputs;
                double[] desOutputs = new double[authors.Length];
                for (int y = 0; y < authors.Length; y++)
                {
                    if (authors[y] == author)
                        desOutputs[y] = 1; //Neuron index of the right author
                }

                NeuralNet.Train(inputs, desOutputs);

            }*/

            // testavimas - jeigu teisingai atpazista tai paleidziam treniravima.
            for (int m = 0; m < testDir.Length; m++)
            {
                var maker = new InputMaker(File.ReadAllLines(testDir[m])[1]);
                var author = File.ReadAllLines(testDir[m])[0];
                author = author.Replace(";", "");
                author = author.Replace(",", "");
                var inputs = maker.Inputs;

                int index = 999;
                for (int y = 0; y < authors.Length; y++)
                {
                    if (authors[y] == author)
                        index = y; //Neuron index of the right author
                }
                var outputs = NeuralNet.Run(inputs);
                if (index != 999)
                {
                    if (outputs.Max() == outputs[index])
                    {
                        double[] desOutputs = new double[authors.Length];
                        NeuralNet.Train(inputs, desOutputs);
                        Console.WriteLine("Success: {0} -> {1}", outputs[index], authors[index]);
                    }
                    else
                    {
                        Console.WriteLine("Failure: {0} -> {1}", outputs[index], testDir[m]);
                    }
                }
            }

            Console.ReadLine();
        }
        static void Shuffle<T>(T[] array)
        {
            int n = array.Length;
            for (int i = 0; i < n; i++)
            {
                // Use Next on random instance with an argument.
                // ... The argument is an exclusive bound.
                //     So we will not go past the end of the array.
                int r = i + _random.Next(n - i);
                T t = array[r];
                array[r] = array[i];
                array[i] = t;
            }
        }
    }
}
