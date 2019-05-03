using RestSharp;
using RestSharp.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;

namespace NeuralNetworkForBacherlor
{
    class InputMaker
    {
        public static readonly int WordCount = 360;

        public static (string, List<double>) makeInputs(string text, Annotations annos)
        {
            List<double> inputs = new List<double>();
            int sentenceLengthsLimit = 8;


            string t2 = Regex.Replace(text, "(\\.\\.\\.)|[!?.] ?([A-Z0-9ĄČĘĖĮŠŲŪŽ])", "\n$2");
            string[] sentences = t2.Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
            sentences = sentences.Select(x => x.Trim()).ToArray();


            List<int> commas = buildCommas(sentences, inputs);
            (int shortSentences, int longSentences) = buildSentenceLengths(sentences, inputs, sentenceLengthsLimit);
            Dictionary<(string, string), int> pos2n = buildPos2N(annos, inputs);
            Dictionary<string, int> functionWords = buildFunctionWords(annos, inputs);
            int uniqueWords = buildUniqueWords(annos, inputs);



            // ========================= START =========================
            // ============ Build pretty inputs file output ============
            // =========================================================

            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("======== Commas ======== [how many words between commas]: [how many instances found]\n");
            for (int i = 0; i < commas.Count; i++)
            {
                sb.AppendFormat("{0}: {1}\n", i, commas[i]);
            }

            sb.AppendFormat("======== SentenceLength (limit: {0}) ========\n", sentenceLengthsLimit);
            sb.AppendFormat("Sentences shorter than {0}: {1}\n", sentenceLengthsLimit, shortSentences);
            sb.AppendFormat("Sentences longer than {0}: {1}\n", sentenceLengthsLimit, longSentences);


            sb.AppendFormat("======== Pos2N ========\n");
            foreach (var item in pos2n)
            {
                sb.AppendFormat("{0}->{1}: {2}\n", item.Key.Item1.Trim('.'), item.Key.Item2.Trim('.'), item.Value);
            }

            sb.AppendFormat("======== FunctionWords ========\n");
            foreach (var item in functionWords)
            {
                sb.AppendFormat("{0}: {1}\n", item.Key, item.Value);
            }

            sb.AppendFormat("======== UniqueWords ========\n");
            sb.AppendFormat("{0}\n", uniqueWords);

            sb.AppendFormat("\n======== Word Count (including unknown) ========\n");
            sb.AppendFormat("{0}\n", annos.Count);

            sb.AppendFormat("\n======== Sentence Count (including unknown) ========\n");
            sb.AppendFormat("{0}\n", sentences.Length);

            sb.AppendFormat("\n======== Sentences ========\n");
            sb.AppendFormat("{0}\n", String.Join("\n", sentences));

            sb.AppendFormat("\n======== Annotations ========\n");
            foreach (var item in annos)
            {
                sb.AppendFormat("{0} - {1} - {2}\n", item.Lemma, item.Pos, item.Morph);
            }


            // ========================= END ===========================
            // ============ Build pretty inputs file output ============
            // =========================================================

            // Commas           (50)    / 50
            // SentenceLength   (2)     / 52
            // POS2N            (121)   / 173
            // FunctionWords    (5)     / 178
            // UniqueWords      (1)     / 179

            return (sb.ToString(), inputs);
        }

        private static int buildUniqueWords(Annotations annos, List<double> inputs)
        {
            int uniqueWords = annos.Select(x => x.Lemma).Distinct().Count();
            inputs.Add(uniqueWords / (double)WordCount);
            return uniqueWords;
        }

        private static Dictionary<string, int> buildFunctionWords(Annotations annos, List<double> inputs)
        {
            var fw = new string[] { "sktv.", "įv.", "prv.", "dll.", "jng." };
            Dictionary<string, int> functionWords = new Dictionary<string, int>();
            for (int i = 0; i < fw.Length; i++)
            {
                int fwCount = annos.Count(x => String.Equals(x.Pos, fw[i]));
                functionWords.Add(fw[i], fwCount);
                inputs.Add(fwCount / (double)WordCount);
            }

            return functionWords;
        }

        private static Dictionary<(string, string), int> buildPos2N(Annotations annos, List<double> inputs)
        {
            Dictionary<(string, string), int> pos2n = annos.Pos2N();
            foreach (var item in pos2n)
            {
                inputs.Add(item.Value / (double)pos2n.Count);
            }
            return pos2n;
        }

        public static (int, int) buildSentenceLengths(IEnumerable<string> sentences, List<double> inputs, int sentenceLength = 8)
        {
            int shortSentences = 0;
            int longSentences = 0;
            List<int> sentenceLengths = new List<int>();
            foreach (var item in sentences)
            {
                int length = item.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).Length;
                sentenceLengths.Add(length);
            }
            shortSentences = sentenceLengths.Count(x => x <= sentenceLength);
            longSentences = sentenceLengths.Count(x => x > sentenceLength);
            int totalSentences = shortSentences + longSentences;

            inputs.Add(shortSentences / (double)totalSentences);
            inputs.Add(longSentences / (double)totalSentences);
            return (shortSentences, longSentences);
        }

        public static List<int> buildCommas(IEnumerable<string> sentences, List<double> inputs)
        {
            for (int i = 0; i < 50; i++)
            {
                inputs.Add(0);
            }
            List<int> commas = new List<int>();

            foreach (string line in sentences)
            {
                var arr = line.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var item in arr.Take(arr.Length - 1))
                {
                    int c = item.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).Length;
                    inputs[c]++;
                }
            }
            for (int i = 0; i < 50; i++)
            {
                commas.Add((int)inputs[i]);
                inputs[i] /= (WordCount - 1);
            }
            return commas;
        }

        #region deprecated
        public static List<double> PuntuationCounter(string text, List<double> inputs)
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
                        var arr = words.Skip(lastIndex).Take(x - lastIndex).ToArray();
                        foundPunc = FindCommma(arr, foundPunc);
                        lastIndex = x + 1;
                    }
                }
                if (words[x].Contains('.') && (x + 1) == words.Length) //End
                {
                    var arr = words.Skip(lastIndex).Take(x - lastIndex).ToArray();
                    foundPunc = FindCommma(arr, foundPunc);
                }
            }

            foreach (var item in foundPunc)
            {
                var input = (double)item / (WordCount - 1);
                inputs.Add(item);
            }

            return inputs;
            //TODO: finish puntuation - done
        }
        public static int[] FindCommma(string[] arr, int[] found) // find commas in sentence adds them to found depending on comma position between words.
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
                    last = m + 1;
                    found[place]++;
                }
            }
            return found;
        }
        public static List<double> SentenceCounter(string text, List<double> inputs)
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
                        var arr = words.Skip(lastIndex).Take(x + 1 - lastIndex).ToArray();
                        if (arr.Length > 8)
                            longSen++;
                        else
                            shortSen++;

                        lastIndex = x + 1;
                        //Console.WriteLine("{0}: {1}", arr.Length, string.Join(" ", arr));
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
            inputs.Add(shortSen);

            input = (double)longSen / (shortSen + longSen);
            inputs.Add(longSen);

            return inputs;

        }
        public static string GetPos(string text, string type)
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
        public static List<double> AnalyzePos(string text, List<double> inputs)
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
                inputs.Add(item);
            }


            return inputs;
            //TODO: POS-N generate - done
        }
        public static List<double> FunctionWords(string text, List<double> inputs)
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
                inputs.Add(item);
            }

            return inputs;
            //TODO: Function words - done
        }
        public static List<double> Depersonalizer(string text, List<double> inputs)
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
            inputs.Add(found.Count());

            return inputs;

            //TODO: Finish depersonalizer - done
            // All will be divided by word count now its 360 - its always a const
        }
        #endregion
    }
}
