using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System;

namespace Server
{
    [Controller]
    public class StreamingController : Controller
    {
        private const string DataFile = "data.json";

        [HttpGet("csv")]
        public async Task GenerateCsvAsync()
        {
            Response.ContentType = "text/plain";
            await foreach (var (time, userId, points) in GetEntriesAsync())
            {
                await Response.WriteAsync($"{time},{userId},{points}\n");
            }
        }

        [HttpGet("summary")]
        public async Task GenerateSummaryAsync()
        {
            Response.ContentType = "application/json";
            var day = "";
            var scores = new Dictionary<string, int>();
            var prefix = "[\n\t";

            await foreach (var (time, userId, points) in GetEntriesAsync())
            {
                var currentDay = time.Substring(0, time.IndexOf('T'));
                if (day != currentDay)
                {
                    if (scores.Count > 0)
                    {
                        var best = GetBestScore(scores);
                        var result = new { day, winner = best.Key, totalScore = best.Value };
                        await Response.WriteAsync(prefix + JsonConvert.SerializeObject(result));
                        prefix = ",\n\t";
                        scores.Clear();
                    }

                    day = currentDay;
                }

                if (!scores.TryGetValue(userId, out int score))
                {
                    score = 0;
                }

                scores[userId] = score + points;
            }

            if (scores.Count > 0)
            {
                var best = GetBestScore(scores);
                var result = new { day, winner = best.Key, totalScore = best.Value };
                await Response.WriteAsync(prefix + JsonConvert.SerializeObject(result));
            }

            await Response.WriteAsync("\n]");
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

        private static async IAsyncEnumerable<(string, string, int)> GetEntriesAsync()
        {
            string userId = null;
            string time = null;
            int points = 0;

            using var file = System.IO.File.OpenRead(DataFile);
            using var reader = new StreamReader(file);
            using var json = new JsonTextReader(reader);
            while (await json.ReadAsync())
            {
                if (json.TokenType == JsonToken.PropertyName)
                {
                    switch (json.Value as string)
                    {
                        case "time":
                            time = await json.ReadAsStringAsync();
                            break;
                        case "userId":
                            userId = await json.ReadAsStringAsync();
                            break;
                        case "points":
                            points = (await json.ReadAsInt32Async()).Value;
                            break;
                        default:
                            throw new Exception($"Unknown object entry at: {json.Path}");
                    }

                    continue;
                }

                if (json.TokenType == JsonToken.EndObject)
                {
                    yield return (time, userId, points);
                }
            }
        }
    }
}
