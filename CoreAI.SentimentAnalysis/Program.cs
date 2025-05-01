
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text;

var apiKey = "open-ai-secret-key";

Console.WriteLine("Lütfen analiz edilecek metni girin");
var inputText = Console.ReadLine();

if (!string.IsNullOrEmpty(inputText))
{
    Console.WriteLine("Duygu analizi yapılıyor.");

   var sentiment=  await AnalyzeSentiment(apiKey, inputText);

    Console.Write("Duygu analizi sonucu: ");

    switch (sentiment.ToLower())
    {
        case "positive":
            Console.ForegroundColor = ConsoleColor.Green;
            break;
        case "negative":
            Console.ForegroundColor = ConsoleColor.Red;
            break;
        case "neutral":
            Console.ForegroundColor = ConsoleColor.Yellow;
            break;
        default:
            Console.ForegroundColor = ConsoleColor.Gray;
            break;
    }

    Console.WriteLine(sentiment);
    Console.ResetColor();

}

static async Task<string> AnalyzeSentiment(string apiKey, string text)
{
    using var client = new HttpClient();
    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

    var requestData = new
    {
        model = "gpt-3.5-turbo",
        messages = new[] {
            new {role = "system", content = "You are an AI that analyzes sentiment.You categorize text as Possitive,Negative or Neutral."},
            new {role = "user", content = $"Analyze the sentiment of this text:{text} and return only Possitive,Negative or Neutral."}
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