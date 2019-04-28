using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace NeuralNetworkForBacherlor
{
    class InputMaker
    {
        public List<double> Inputs { get; set; }
        public const int WordCount = 360;

        public InputMaker(string text){
            List<double> inputs = new List<double>();

            inputs = PuntuationCounter(text, inputs);
            inputs = SentenceCounter(text, inputs);

            text = GetPos(text, "LM"); // (LM , M , L) choises

            inputs = AnalyzePos(text, inputs);
            inputs = FunctionWords(text, inputs);
            inputs = Depersonalizer(text, inputs);

            Inputs = inputs;

        }
        public List<double> PuntuationCounter(string text, List<double> inputs)
        {
            text = text.Replace(",", " , ");
            text = text.Replace('?', '.');
            text = text.Replace('!', '.');
            text = text.Replace("...", ".");
            text = text.Replace(" .", "");
            text = text.Replace(".", ". ");
            var words = text.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            var foundPunc = new int[50];
            int lastIndex = 0;
            for (int x = 0; x < words.Length; x++)
            {
                if (words[x].Contains('.') && (x + 1) < words.Length)
                {
                    if (Char.IsUpper(words[x + 1][0]) || Char.IsDigit(words[x + 1][0]) && words[x].Length > 2)
                    {
                        var arr = words.Skip(lastIndex + 1).Take(x-lastIndex).ToArray();
                        foundPunc = FindCommma(arr, foundPunc);
                        lastIndex = x;
                    }
                }
                if (words[x].Contains('.') && (x + 1) == words.Length) //End
                {
                    var arr = words.Skip(lastIndex + 1).Take(x-lastIndex + 1).ToArray();
                    foundPunc = FindCommma(arr, foundPunc);
                }
            }

            foreach (var item in foundPunc)
            {
                var input = (double)item /(WordCount - 1);
                inputs.Add(input);
            }

            return inputs;
            //TODO: finish puntuation - done
        }
        public int[] FindCommma(string[] arr, int[] found) // find commas in sentence adds them to found depending on comma position between words.
        {
            int[] index = new int[arr.Length];
            int last = 0;
            for (int m = 0; m < arr.Length; m++)
            {
                if (arr[m].Contains(','))
                {
                    index[m] = 1;
                    int place = 0;
                    for (int n = last; n < m; n++)
                    {
                        if (index[n] == 0)
                            place++;
                    }
                    last = m;
                    found[place]++;
                }
            }
            return found;
        }
        public List<double> SentenceCounter(string text, List<double> inputs)
        {
            text = text.Replace('?', '.');
            text = text.Replace('!', '.');
            text = text.Replace("...", ".");
            var words = text.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            int lastIndex = 0;
            int shortSen = 0;
            int longSen = 0;
            for (int x = 0; x < words.Length; x++)
            {
                if (words[x].Contains('.') && (x + 1) < words.Length)
                {
                    if (!Char.IsUpper(words[x][0]) && Char.IsUpper(words[x + 1][0]) && words[x].Length > 2)
                    {
                        var arr = words.Skip(lastIndex + 1).Take(x + 1).ToArray();
                        if (arr.Length > 8)
                            longSen++;
                        else
                            shortSen++;

                        lastIndex = x;
                    }
                }
                if (words[x].Contains('.') && (x + 1) == words.Length) //End
                {
                    var arr = words.Skip(lastIndex + 1).Take(x + 1).ToArray();
                    if (arr.Length > 6)
                        longSen++;
                    else
                        shortSen++;
                }
            }

            var input = (double)shortSen / (shortSen + longSen);
            inputs.Add(input);

            input = (double)longSen / (shortSen + longSen);
            inputs.Add(input);

            return inputs;

        }
        public string GetPos(string text, string type)
        {
            var url = "http://donelaitis.vdu.lt/main_helper.php?id=4&nr=7_2";
            var client = new RestClient(url);
            // client.Authenticator = new HttpBasicAuthenticator(username, password);

            var request = new RestRequest("", Method.POST);
            //pateikti LM -> lemma ir Morfa
            //tekstas = visas tekstas atskirtas space
            text = text.Replace(" ", "%20");
            var values = "tekstas=" + text + "&tipas=anotuoti&pateikti=" + type + "&veiksmas=Rezultatas%20puslapyje";
            request.AddParameter("application/x-www-form-urlencoded", values, ParameterType.RequestBody);

            // execute the request
            IRestResponse response = client.Execute(request);
            var content = response.Content;

            content = HttpUtility.HtmlDecode(content);
            //remove <a> content
            int begin = content.IndexOf("<a");
            int end = content.IndexOf("</a>") - begin + 4;
            content = content.Remove(begin, end);

            //remove <form> content
            begin = content.IndexOf("<form");
            end = content.IndexOf("</form>") - begin + 7;
            content = content.Remove(begin, end);

            return content;
        }
        public List<double> AnalyzePos(string text,List<double> inputs)
        {
            var pos = new string[] { "dkt.", "bdv.", "sktv.", "įv.", "vksm.", "prv.", "jst.", "išt.", "dll.", "prl.", "jng." };
            var Pos2N = new int[pos.Length * pos.Length][];
            var found2N = new int[Pos2N.Length];


            // Generating all posible 2-N combinations
            var index = 0;
            for (int x = 0; x < pos.Length; x++)
            {
                for (int y = 0; y < pos.Length; y++)
                {
                    Pos2N[index] = new int[2] { x, y };
                    index++;
                }

            }

            // Cleans the unknown in the text
            var temp = new HashSet<string>();
            var words = text.Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string word in words)
            {
                if (word.Contains("type"))
                    temp.Add(word);
            }
            words = temp.ToArray();


            // Creates an array of pos in text with they're index (type=nezinomas wont be found)
            int[] posInText = new int[words.Length];
            for (int m = 0; m < words.Length; m++)
            {
                for (int n = 0; n < pos.Length; n++)
                {
                    if (words[m].Contains(pos[n]))
                        posInText[m] = n;
                }
            }

            // Counts Pos 2N
            for (int i = 0; i < posInText.Length - 1; i++)
            {
                for (int j = 0; j < Pos2N.Length; j++)
                {
                    if (posInText[i] == Pos2N[j][0] && posInText[i + 1] == Pos2N[j][1])
                    {
                        found2N[j]++;
                    }
                }
            }

            foreach (var item in found2N)
            {
                double input = (double)item / Pos2N.Length;
                inputs.Add(input);
            }
            

            return inputs;
            //TODO: POS-N generate - done
        }

        public List<double> FunctionWords(string text, List<double> inputs)
        {
            var fw = new string[] { "sktv.", "įv.", "prv.", "dll.", "jng." };
            var foundFw = new int[fw.Length];

            // Cleans the unknown in the text
            var temp = new HashSet<string>();
            var words = text.Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string word in words)
            {
                if (word.Contains("type"))
                {
                    for (int x = 0; x < fw.Length; x++)
                    {
                        if (word.Contains(fw[x]))
                            foundFw[x]++;
                    }
                }
            }

            foreach (var item in foundFw)
            {
                var input = (double)item / WordCount;
                inputs.Add(input);
            }

            return inputs;
            //TODO: Function words - done
        }
        public List<double> Depersonalizer(string text, List<double> inputs)
        {
            var found = new List<string>();

            // Cleans the unknown in the text
            var temp = new HashSet<string>();
            var words = text.Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string word in words)
            {
                if (word.Contains("lemma"))
                    temp.Add(word);
            }
            words = temp.ToArray();

            foreach (var word in words)
            {
                if (!found.Any())
                {
                    var begin = word.IndexOf("lemma=") + 7;
                    var end = word.LastIndexOf("type") - begin - 2;
                    var tp = word;
                    tp = tp.Remove(0, begin);
                    tp = tp.Remove(end);
                    found.Add(tp);
                }
                else
                {
                    bool exits = false;
                    var begin = word.IndexOf("lemma=") + 7;
                    var end = word.LastIndexOf("type") - begin - 2;
                    var tp = word;
                    tp = tp.Remove(0, begin);
                    tp = tp.Remove(end);

                    for (int i = 0; i < found.Count; i++)
                    {
                        if (tp == found[i])
                        {
                            exits = true;
                        }
                    }

                    if (!exits)
                    {
                        found.Add(tp);
                    }
                }
            }

            var input = (double)found.Count() / WordCount;
            inputs.Add(input);

            return inputs;

            //TODO: Finish depersonalizer - done
            // All will be divided by word count now its 360 - its always a const
        }
    }
}
