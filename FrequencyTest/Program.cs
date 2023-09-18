using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;

namespace FrequencyTest
{
    internal class Program
    {
        private static readonly int subsequenceLength = 3;
        static async Task Main(string[] args)
        {
            var timer = new Stopwatch();
            timer.Start();
            string path = string.Empty;
            if (args.Length == 0)
            {
                path = @"C:\FreqTest\text.txt";
            }
            else
            {
                path = args[0];
            }
            string text = await ReadFromFileAsync(path);
            text = text.Replace(Environment.NewLine, " ").ToLower();
            if (text.Length >= subsequenceLength)
            {
                Dictionary<string,int> words = GetWordsFromText(text);
                var subsequenceDict = new ConcurrentDictionary<string, int>();
                CountSubseqInWords(words, subsequenceDict);
                int toTake = Math.Min(subsequenceDict.Count, 10);
                var result = subsequenceDict.OrderByDescending(n => n.Value).Take(toTake);
                foreach (var seq in result)
                {
                    Console.WriteLine(seq.Key + " " + seq.Value);
                }
            }
            else
            {
                Console.WriteLine("Длина текста меньше искомой длины");
            }
            timer.Stop();
            Console.WriteLine("Время работы программы: " + timer.ElapsedMilliseconds);
        }

        public static Dictionary<string,int> GetWordsFromText(string text)
        {
            var sb = new StringBuilder();
            var words = new Dictionary<string,int>();
            foreach (char ch in text)
            {
                if (Char.IsLetter(ch))
                {
                    sb.Append(ch);
                }
                else if (ch == ' ')
                {
                    AddOrUpdateWithSb();
                    sb.Clear();
                }
            }
            AddOrUpdateWithSb();
            return words;

            void AddOrUpdateWithSb()
            {
                if (sb.Length >= subsequenceLength)
                {
                    var str = sb.ToString();
                    if (words.ContainsKey(str))
                    {
                        words[str]++;
                    }
                    else
                    {
                        words.Add(str, 1);
                    }
                }
            }
        }

        public static void CountSubseqInWords(Dictionary<string, int> words, ConcurrentDictionary<string, int> subsequence)
        {
            Parallel.ForEach(words, word =>
            {
                for (int i = 0; i < word.Key.Length - subsequenceLength + 1; i++)
                {
                    var subseq = word.Key[i..(i + subsequenceLength)];
                    int count = word.Value;
                    subsequence.AddOrUpdate(subseq, count, (key, existingCount) => existingCount + count);
                }
            });
        }

        public static async Task<string> ReadFromFileAsync(string path)
        {
            try
            {
                using (StreamReader sr = new(path))
                {
                    return await sr.ReadToEndAsync();
                }
            }
            catch (IOException ex)
            {
                Console.WriteLine("Не удалось прочитать файл");
                Console.WriteLine(ex.Message);
            }
            return string.Empty;
        }
    }
}