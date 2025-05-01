
//Aşağıda, OpenAI API kullanarak kullanıcıdan malzemeleri alıp bu malzemelere uygun bir yemek tarifi önerisi yapan basit bir .NET console uygulaması 

using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

var apiKey = "open-ai-secret-key"; // 🔐 OpenAI API Key buraya

Console.WriteLine("Yemek tarif önericisine hoş geldiniz!");
Console.WriteLine("Evde bulunan malzemeleri virgülle ayırarak girin (örnek: domates, peynir, yumurta):");

string ingredients = Console.ReadLine();

if (string.IsNullOrWhiteSpace(ingredients))
{
    Console.WriteLine("Geçerli malzeme listesi girilmedi.");
    return;
}

var prompt = $"Evde şu malzemeler var: {ingredients}. Bu malzemelere uygun yaratıcı ve lezzetli bir yemek tarifi öner. Tarif; malzeme listesi, hazırlanışı ve pişirme süresi gibi bölümleri içermelidir. Türkçe olarak yaz.";

Console.WriteLine("\n👨‍🍳 Tarif oluşturuluyor, lütfen bekleyin...\n");

var recipe = await GetRecipeSuggestion(prompt, apiKey);

if (!string.IsNullOrEmpty(recipe))
{
    Console.WriteLine("\n🍽️ Önerilen Tarif:\n");
    Console.WriteLine(recipe);
}
else
{
    Console.WriteLine("Tarif oluşturulamadı.");
}

static async Task<string> GetRecipeSuggestion(string prompt, string apiKey)
{
    using var client = new HttpClient();
    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

    var requestData = new
    {
        model = "gpt-3.5-turbo",
        messages = new[]
        {
            new { role = "system", content = "You are a professional chef assistant." },
            new { role = "user", content = prompt }
        },
        max_tokens = 800,
        temperature = 0.7 // Uygulamanın tahmin gücünü belirler.
    };

    var json = JsonSerializer.Serialize(requestData);
    var content = new StringContent(json, Encoding.UTF8, "application/json");

    try
    {
        var response = await client.PostAsync("https://api.openai.com/v1/chat/completions", content);
        var responseBody = await response.Content.ReadAsStringAsync();
        if (response.IsSuccessStatusCode)
        {
            var result = JsonDocument.Parse(responseBody);
            return result.RootElement
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString()
                .Trim();
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
        Console.WriteLine($"Hata oluştu: {ex.Message}");
        return null;
    }
}
