

using System.Net.Http.Headers;
using System.Text.Json;
using System.Text;
using HtmlAgilityPack;

var apiKey = "open-ai-secret-key";

Console.WriteLine("Lütfen analiz yapmak istediğiniz web sayfasının URL’sini girin:");
var url = Console.ReadLine();

if (!string.IsNullOrEmpty(url))
{
    Console.WriteLine("Web sayfası içeriği çekiliyor...");
    var webContent = await ExtractTextFromWeb(url);

    if (string.IsNullOrWhiteSpace(webContent))
    {
        Console.WriteLine("Sayfa içeriği alınamadı veya boş.");
        return;
    }

    Console.WriteLine("Web sayfası analiz ediliyor...");

    var analysis = await AnalyzeWithAI(apiKey, "Web sayfası içeriği", webContent);

    Console.WriteLine("\n🧠 Web Sayfası Analizi Sonucu:\n");
    Console.WriteLine(analysis);
}
else
{
    Console.WriteLine("Geçerli bir URL girilmedi.");
}

static async Task<string> ExtractTextFromWeb( string url)
{
    try
    {
        var web = new HtmlWeb();
        var doc = web.Load(url);

        var body = doc.DocumentNode.SelectSingleNode("//body");
        if (body == null)
            return "";

        var text = body.InnerText;

        // Temizle: gereksiz boşlukları sil
        return System.Text.RegularExpressions.Regex.Replace(text, @"\s+", " ").Trim();
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Sayfa içeriği alınırken hata oluştu: {ex.Message}");
        return "";
    }
}

static async Task<string> AnalyzeWithAI(string apiKey,string sourceType, string text)
{
    using var client = new HttpClient();
    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

    var requestData = new
    {
        model = "gpt-3.5-turbo",
        messages = new[] {
            new { role = "system", content = "You are an AI assistant. Analyze and summarize content in a clear and concise way. Also indicate whether the overall tone is Positive, Negative, or Neutral." },
            new { role = "user", content = $"Analyze and summarize the following {sourceType} content:\n\n{text}" }
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