using System.Collections.Concurrent;
using System.Text.RegularExpressions;

namespace FrequencyTest
{
    internal class Program
    {
        private static readonly int subsequenceLength = 3;
        static async Task Main(string[] args)
        {
            var startTime = DateTime.Now;
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
                string pattern = "[^a-zA-Zа-яА-Я ]";
                string clearText = Regex.Replace(text, pattern, string.Empty);
                string[] wordsArray = clearText.Split(' ');
                var words = ToConcurrentDictFromArray(wordsArray);
                var subsequenceDict = new ConcurrentDictionary<string, int>();
                CountSubseqInWords(words, subsequenceDict);
                var seqCount = subsequenceDict.Count;
                int toTake = 10;
                var result = subsequenceDict.OrderByDescending(n => n.Value).Take(Math.Min(toTake, seqCount));
                foreach (var seq in result)
                {
                    Console.WriteLine(seq.Key + " " + seq.Value);
                }
            }
            else
            {
                Console.WriteLine("Длина текста меньше искомой длины");
            }
            Console.WriteLine("Время работы программы: " + (DateTime.Now - startTime).TotalMilliseconds);
        }


        public static void CountSubseqInWords(ConcurrentDictionary<string, int> words, ConcurrentDictionary<string, int> subsequence)
        {
            Parallel.ForEach(words, word =>
            {
                if (word.Key.Length >= subsequenceLength)
                {
                    for (int i = 0; i < word.Key.Length - subsequenceLength + 1; i++)
                    {
                        var subseq = word.Key[i..(i + subsequenceLength)];
                        int count = word.Value;
                        subsequence.AddOrUpdate(subseq, count, (key, existingCount) => existingCount + count);
                    }
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

        public static ConcurrentDictionary<string,int> ToConcurrentDictFromArray(string[] array)
        {
            var dict = new ConcurrentDictionary<string, int>();
            Parallel.ForEach(array, word =>
            {
                dict.AddOrUpdate(word, 1, (key, existingCount) => ++existingCount);
            });
            return dict;
        }
    }
}