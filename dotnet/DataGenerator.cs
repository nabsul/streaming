using Newtonsoft.Json;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Server
{
    public class DataGenerator
    {
        private const string DataFile = "data.json";
        private static readonly DateTime StartDate = new DateTime(2021, 09, 1);
        private static readonly DateTime EndDate = new DateTime(2021, 10, 1);

        public static void GenerateData()
        {
            var rand = new Random();
            var date = StartDate.AddSeconds(rand.Next(10));

            Console.WriteLine("Opening output file...");
            using var stream = File.OpenWrite(DataFile);
            using var writer = new StreamWriter(stream);
            using var json = new JsonTextWriter(writer);

            Console.WriteLine("Generating data (this could take a while)...");
            json.WriteStartArray();

            while (date < EndDate)
            {
                json.WriteRawValue(GenerateEntry(date, rand.Next(100), rand.Next(100)));
                date = date.AddSeconds(rand.Next(10));
            }

            json.WriteEndArray();
            Console.WriteLine("Done.");
        }

        private static string GenerateEntry(DateTime date, int userId, int points)
        {
            return JsonConvert.SerializeObject(new
            {
                time = date.ToString("s"),
                userId = $"user{userId}",
                points
            });
        }
    }
}
