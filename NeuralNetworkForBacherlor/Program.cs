using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace NeuralNetworkForBacherlor
{
    public class Program
    {
        //TODO: add čekuolis files
        public static string[] Authors = new string[] {"algimantas čekuolis",
                                                "justina maciūnaitė",
                                                "edmundas jakilaitis",
                                                "viktorija chockevičiūtė",
                                                "miglė lopetaitė",
                                                "aistė žebrauskienė",
                                                "agnė keizikienė",
                                                "viktorija vilcanaitė",
                                                "neringa mikėnaitė",
                                                "žiedūnė juškytė",
                                                "arnas mazėtis"
                                                //"other"
        };
        static Random _random = new Random();
        static void Main(string[] args)
        {
            var trainPath = "Train";
            var testPath = "Test";
            var annoFilePrefix = "Anno";
            var unSeenPath = "UnSeen";

            /*var dir = Directory.GetFiles(unSeenPath);
            var anno = Directory.GetFiles(annoFilePrefix+unSeenPath);
            for(int num=0;num<dir.Length;num++)
            {
                Annotations.OpenFile_Download_Annotate_SaveFile(dir[num], anno[num]);
            }*/
            
            InputDataList trainData = new InputDataList(trainPath, annoFilePrefix);
            InputDataList testData = new InputDataList(testPath, annoFilePrefix);
            InputDataList unSeenData = new InputDataList(unSeenPath, annoFilePrefix);


            //TEST
            Shuffle(trainData);
            var inputsD = trainData.Select(x => x.inputs.ToArray()).ToList();
            var outputsD = trainData.Select(x => x.outputs.ToArray()).ToList();
            //var hidden = new List<int> { 5, 10, 20, 30 };
            var hidden = new List<int> {1000,2000,3000};
            foreach (var neurons in hidden)
            {
                var tests = new List<string> { };
                for (int sk = 0; sk < 10; sk++)
                {
                    Shuffle(trainData);
                    var inputsT = trainData.Select(x => x.inputs.ToArray()).ToList();
                    var outputsT = trainData.Select(x => x.outputs.ToArray()).ToList();
                    var network = new New.NeuralNetwork(inputsT, outputsT, new int[1] { 20 }, 0.15,
            0.3, 0.1, -7, 5);
                    var train = network.run(neurons, 0).Split(" ");

                    Shuffle(testData);
                    var inputsV = testData.Select(x => x.inputs.ToArray()).ToList();
                    var outputsV = testData.Select(x => x.outputs.ToArray()).ToList();
                    var test = network.test(inputsV, outputsV);
                    tests.Add(test);
                }
                using (StreamWriter wr = new StreamWriter("IterAcc" + neurons + ".txt"))
                {
                    foreach (var item in tests)
                    {
                        wr.WriteLine(item);
                    }
                }
            }
            Console.WriteLine();
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
