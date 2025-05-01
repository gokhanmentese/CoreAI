
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

Console.WriteLine("Çevirmek istediğiniz metni giriniz.");
var inputText = Console.ReadLine();

var apiKey = "open-ai-secret-key";

var translatedText = await TranslateText(inputText, apiKey);
if (!string.IsNullOrEmpty(translatedText))
{
    Console.WriteLine();
    Console.WriteLine($"Çeviri Sonucu: {translatedText}");
}


static async Task<string> TranslateText(string text, string apiKey)
{
    using var client = new HttpClient();
    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

    var requestData = new
    {
        model = "gpt-3.5-turbo",
        messages = new[] {
            new {role = "system", content = "You are a helpful translator"},
            new {role = "user", content = $"Please translate this text to English {text}"}
        },
        max_tokens = 100, // uygulamanın bize getirdiği içeriğin uzunlugu
        temperature = 0.5
    };

    var json = JsonSerializer.Serialize(requestData);
    var content = new StringContent(json, Encoding.UTF8, "application/json");

    try
    {
        var response = await client.PostAsync("https://api.openai.com/v1/chat/completions", content);
        var responseBody = await response.Content.ReadAsStringAsync();
        if (response.IsSuccessStatusCode)
        {
            var translation = JsonDocument.Parse(responseBody)
                .RootElement.GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString();

            return translation.Trim();
        }
        else
        {
            Console.WriteLine($"API Hatası: {response.StatusCode}");
            Console.WriteLine(responseBody);
            return null;
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Bir hata oluştu: {ex.Message}");
        return null;
    }
}
