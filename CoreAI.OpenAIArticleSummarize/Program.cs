
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text;

var apiKey = "open-ai-secret-key";

Console.WriteLine("Lütfen makale metnini girin");
var inputText = Console.ReadLine();

if (!string.IsNullOrEmpty(inputText))
{
    Console.WriteLine("Girilen metin AI tarafından özetleniyor...\n");

    var shortSummary = await SummarizeText(apiKey, inputText, "short");
    var mediumSummary = await SummarizeText(apiKey, inputText, "medium");
    var detailedSummary = await SummarizeText(apiKey, inputText, "detailed");

    Console.WriteLine("\n📝 Özetler:");
    Console.WriteLine("\n🔹 Kısa Özet:\n" + shortSummary);
    Console.WriteLine("\n🔸 Orta Uzunlukta Özet:\n" + mediumSummary);
    Console.WriteLine("\n🔶 Detaylı Özet:\n" + detailedSummary);
}

static async Task<string> SummarizeText(string apiKey, string text , string level)
{
    using var client = new HttpClient();
    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

    string summaryLevel = level switch
    {
        "short" => "Summarize this text in 1-2 sentences",
        "medium" => "Summarize this text in 3-5 sentences",
        "detailed" => "Summarize this text in a detailed but concise manner.",
        _ => "Summarize this text"
    };

    var requestData = new
    {
        model = "gpt-3.5-turbo",
        messages = new[] {
            new {role = "system", content = "You are an AI that summarize text info different levels. short,medium and detailed"},
            new {role = "user", content = $"{summaryLevel}\n\n{text}" }
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
