
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

var apiKey = "open-ai-secret-key";

var genre = ReadInput("Hikaye türünü seçin.(Macera,Korku,Bilim Kurgu,Fantastik,Komedi)");
var mainCharacter = ReadInput("Hikaye için ana karakteri girin:");
var sideCharacter = ReadInput("Hikaye için yan karakteri girin:");
var mainEvents = ReadInput("Hikaye için ana olayları girin:");
var mainLocations = ReadInput("Hikaye için ana mekanları girin:");
var mainThemes = ReadInput("Hikaye için ana temaları girin:");

var prompt = $@"Türü {genre} olan yaratıcı bir hikaye yaz. 
Hikayede giriş, gelişme ve sonuç bölümleri bulunmalı. 
Ana karakter: {mainCharacter}, 
Yan karakter: {sideCharacter}, 
Ana olaylar: {mainEvents}, 
Geçtiği yerler: {mainLocations}, 
Temalar: {mainThemes}.
Hikayeyi Türkçe olarak yaz.";

Console.WriteLine("Hikaye oluşturuluyor...");

if (!string.IsNullOrEmpty(prompt))
{
    var story = await GenerateStory(prompt, apiKey);
    if (!string.IsNullOrEmpty(story))
    {
        Console.WriteLine("\n📖 Hikaye:\n");
        Console.WriteLine(story);

        var fileName = $"hikaye_{DateTime.Now:yyyyMMdd_HHmmss}.txt";
        await File.WriteAllTextAsync(fileName, story, Encoding.UTF8);
        Console.WriteLine($"\n📁 Hikaye '{fileName}' olarak kaydedildi.");
    }
}
else
{
    Console.WriteLine("Geçerli bir hikaye oluşturulamadı.");
}


static async Task<string> GenerateStory(string prompt, string apiKey)
{
    using var client = new HttpClient();
    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

    var requestData = new
    {
        model = "gpt-3.5-turbo",
        messages = new[] {
            new {role = "system", content = "You are a creative story writer."},
            new {role = "user", content =prompt}
        },
        max_tokens = 1000, // uygulamanın bize getirdiği içeriğin uzunlugu
    };

    var json = JsonSerializer.Serialize(requestData);
    var content = new StringContent(json, Encoding.UTF8, "application/json");

    try
    {
        var response = await client.PostAsync("https://api.openai.com/v1/chat/completions", content);
        var responseBody = await response.Content.ReadAsStringAsync();
        if (response.IsSuccessStatusCode)
        {
            var story = JsonDocument.Parse(responseBody)
                .RootElement.GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString();

            return story.Trim();
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

string ReadInput(string label)
{
    string input;
    do
    {
        Console.WriteLine(label);
        input = Console.ReadLine();
    } while (string.IsNullOrWhiteSpace(input));
    return input;
}
