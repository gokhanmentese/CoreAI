
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text;

var apiKey = "open-ai-secret-key";

Console.WriteLine("Lütfen analiz edilecek metni girin");
var inputText = Console.ReadLine();

if (!string.IsNullOrEmpty(inputText))
{
    Console.WriteLine("Gelişmiş duygu analizi yapılıyor.");

    var sentimentJson = await AdvancedAnalyzeSentiment(apiKey, inputText);
    // {"Joy": 60, "Sadness": 10, "Anger": 5, "Fear": 10, "Surprise": 10, "Neutral": 5}
    Console.WriteLine("\n🔍 Duygu Yüzdelikleri:\n");

    try
    {
        var emotions = JsonDocument.Parse(sentimentJson).RootElement;

        foreach (var property in emotions.EnumerateObject())
        {
            var emotion = property.Name;
            var value = property.Value.GetDouble();

            Console.ForegroundColor = emotion.ToLower() switch
            {
                "joy" => ConsoleColor.Green,
                "sadness" => ConsoleColor.Blue,
                "anger" => ConsoleColor.Red,
                "fear" => ConsoleColor.DarkYellow,
                "surprise" => ConsoleColor.Cyan,
                "neutral" => ConsoleColor.Gray,
                _ => ConsoleColor.White
            };

            Console.WriteLine($"{emotion,-10}: {value}%");
        }

        Console.ResetColor();
    }
    catch
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine("JSON ayrıştırılamadı. Gelen içerik:");
        Console.WriteLine(sentimentJson);
        Console.ResetColor();
    }
}

static async Task<string> AdvancedAnalyzeSentiment(string apiKey, string text)
{
    using var client = new HttpClient();
    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

    var requestData = new
    {
        model = "gpt-3.5-turbo",
        messages = new[] {
            new {role = "system", content = "You are an advanced AI that analyzes emotions in text.Your response must be in JSON format.Identfy  the sentiment scores (0-100%) for the following emotions: Joy,Sadness,Anger,Fear,Surprise and Neutral.."},
            new {role = "user", content = $"Analyze the sentiment of this text:{text} and return a JSON object with percentages for each emotions."}
        }
    };

    var json = JsonSerializer.Serialize(requestData);
    var content = new StringContent(json, Encoding.UTF8, "application/json");

    var response = await client.PostAsync("https://api.openai.com/v1/chat/completions", content);

    if (response.IsSuccessStatusCode)
    {
        var responseBody = await response.Content.ReadAsStringAsync();
        var result = JsonDocument.Parse(responseBody);

        var sentiment = result.RootElement
            .GetProperty("choices")[0]
            .GetProperty("message")
            .GetProperty("content")
            .GetString();

        return sentiment.Trim();
    }
    else
    {
        var error = await response.Content.ReadAsStringAsync();
        Console.WriteLine($"Bir hata oluştu: {error}");
        return "Hata";
    }
}
