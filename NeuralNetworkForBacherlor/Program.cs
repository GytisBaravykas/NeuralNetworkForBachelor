using RestSharp;
using RestSharp.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace NeuralNetworkForBacherlor
{
    public class Program
    {
        //TODO: add čekuolis files
        public static string[] Authors = new string[] {"justina maciūnaitė",
                                                "edmundas jakilaitis",
                                                "viktorija chockevičiūtė",
                                                "miglė lopetaitė",
                                                "aistė žebrauskienė",
                                                "agnė keizikienė",
                                                "viktorija vilcanaitė",
                                                "neringa mikėnaitė",
                                                "žiedūnė juškytė",
                                                "arnas mazėtis" };
        static Random _random = new Random();

        static void Main(string[] args)
        {
            var trainPath = "Train";
            var testPath = "Test";
            var annoFilePrefix = "Anno";


            InputDataList trainData = new InputDataList(trainPath, annoFilePrefix);
            InputDataList testData = new InputDataList(testPath, annoFilePrefix);

            Shuffle(trainData);

            NeuralNetwork NeuralNet = new NeuralNetwork(0.001, new int[3] { 179, 5, Authors.Length });
            //179 input , 5 hidden , 11 output


            for (int i = 0; i < 1000; i++)
            {
                if (i % 100 == 0)
                {
                    Console.WriteLine("Training set {0}.", i);
                }
                foreach (var item in trainData)
                {
                    NeuralNet.Train(item);
                }
            }

            // testavimas - jeigu teisingai atpazista tai paleidziam treniravima.

            foreach (var item in testData)
            {
                var outputs = NeuralNet.Run(item.inputs);

                Console.WriteLine("-- {0}", Authors[item.outputs.ToList().IndexOf(1)]);
                Dictionary<string, double> outputsDict = new Dictionary<string, double>();


                for (int i = 0; i < outputs.Length; i++)
                {
                    outputsDict.Add(Authors[i], outputs[i]);
                }
                foreach (var item2 in outputsDict.OrderByDescending(x => x.Value))
                {
                    Console.WriteLine("{0}: {1}", item2.Key, item2.Value);
                }

                Console.WriteLine("====================================");
            }



            Console.ReadLine();
        }
        static void Shuffle(List<InputData> array)
        {
            int n = array.Count;
            for (int i = 0; i < n; i++)
            {
                // Use Next on random instance with an argument.
                // ... The argument is an exclusive bound.
                //     So we will not go past the end of the array.
                int r = i + _random.Next(n - i);
                InputData t = array[r];
                array[r] = array[i];
                array[i] = t;
            }
        }

        // Not needed anymore, use data.zip
        static void DownloadAnnotations()
        {
            var trainPath = "Train";
            var testPath = "Test";

            var annoFilePrefix = "Anno";
            int c = 0;

            foreach (var item in Directory.GetFiles(trainPath))
            {
                Annotations.OpenFile_Download_Annotate_SaveFile(item, annoFilePrefix + item);
                Console.Write("{0}| ", c++);
            }
            foreach (var item in Directory.GetFiles(testPath))
            {
                Annotations.OpenFile_Download_Annotate_SaveFile(item, annoFilePrefix + item);
                Console.Write("{0}| ", c++);
            }

        }
    }
}
