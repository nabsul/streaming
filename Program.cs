using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Server
{
    public class Program
    {
        const string DataFile = "data.json";

        public class Entry
        {
            public string time { get; set; }
            public string userId { get; set; }
            public int points { get; set; }
        }

        public static void Main()
        {
            WebHost.CreateDefaultBuilder()
                .UseKestrel(o => o.Listen(IPAddress.Any, 5000))
                .Configure(app => app.UseRouting().UseEndpoints(endpoints =>
                {
                    endpoints.MapGet("/csv", GenerateCsvAsync);
                    endpoints.MapGet("/summary", GenerateSummaryAsync);
                })).Build().Run();
        }

        private static async Task GenerateCsvAsync(HttpContext context)
        {
            await foreach(var entry in GetEntriesAsync())
            {
                await context.Response.WriteAsync($"{entry.time},{entry.userId},{entry.points}\n");
            }
        }

        private static async Task GenerateSummaryAsync(HttpContext context)
        {
            var day = "";
            var scores = new Dictionary<string, int>();
            var prefix = "[\n\t";

            await foreach (var entry in GetEntriesAsync())
            {
                var time = entry.time;
                var currentDay = time.Substring(0, time.IndexOf('T'));
                if (day != currentDay)
                {
                    if (scores.Count > 0)
                    {
                        var best = GetBestScore(scores);
                        var result = new { day, winner = best.Key, totalScore = best.Value };
                        await context.Response.WriteAsync(prefix + JsonConvert.SerializeObject(result));
                        scores.Clear();
                    }

                    prefix = ",\n\t";
                    day = currentDay;
                }

                var userId = entry.userId;
                if (!scores.TryGetValue(userId, out int score))
                {
                    score = 0;
                }

                var points = entry.points;
                scores[userId] = score + points;
            }

            if (scores.Count > 0)
            {
                var best = GetBestScore(scores);
                var result = new { day, winner = best.Key, totalScore = best.Value };
                await context.Response.WriteAsync(prefix + JsonConvert.SerializeObject(result));
            }

            await context.Response.WriteAsync("\n]");
        }

        private static KeyValuePair<string, int> GetBestScore(Dictionary<string, int> scores)
        {
            KeyValuePair<string, int> best = new KeyValuePair<string, int>(null, -1);
            foreach (var p in scores)
            {
                if (best.Value < p.Value)
                {
                    best = p;
                }
            }
         
            return best;
        }

        private static async IAsyncEnumerable<Entry> GetEntriesAsync()
        {
            using var file = File.OpenRead(DataFile);
            using var reader = new StreamReader(file);
            using var json = new JsonTextReader(reader);
            var ser = new JsonSerializer();
            while (await json.ReadAsync())
            {
                if (json.TokenType == JsonToken.StartObject)
                {
                    yield return ser.Deserialize<Entry>(json);
                }
            }
        }
    }
}
