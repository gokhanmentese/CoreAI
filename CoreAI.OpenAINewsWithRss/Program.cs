

using System.Net.Http.Headers;
using System.Text.Json;
using System.Text;
using System.Xml;
using System.Xml.Linq;

var apiKey = "open-ai-secret-key";

Console.WriteLine("Lütfen analiz yapmak istediğiniz web sayfasının rss dosyasının linkini girin:");
var url = Console.ReadLine();

if (!string.IsNullOrEmpty(url))
{
    Console.WriteLine("Haberler sistemden çekiliyor...");

    var articlesByCategory = await GetLatestNewsByCategory(url, 20);

    if (articlesByCategory.Count == 0)
    {
        Console.WriteLine("Haber bulunamadı.");
        return;
    }

    foreach (var category in articlesByCategory.Keys)
    {
        Console.WriteLine($"\n📚 Kategori: {category}");
        var summary = await SummarizeCategoryArticles(apiKey, category, articlesByCategory[category]);
        Console.WriteLine(summary);
    }
}
else
{
    Console.WriteLine("Geçerli bir URL girilmedi.");
}

static async Task<Dictionary<string, List<string>>> GetLatestNewsByCategory(string rssFeedUrl, int count)
{
    var client = new HttpClient();
    var rssContent = await client.GetStringAsync(rssFeedUrl);

    XDocument doc = XDocument.Parse(rssContent);
    var items = doc.Descendants("item").Take(count);

    var newsByCategory = new Dictionary<string, List<string>>();

    foreach (var item in items)
    {
        var title = item.Element("title")?.Value;
        var description = item.Element("description")?.Value;
        var category = item.Element("category")?.Value ?? "Genel";

        // HTML tag'leri temizle
        var cleanDescription = System.Text.RegularExpressions.Regex.Replace(description ?? "", "<.*?>", string.Empty);

        var content = $"Başlık: {title}\nAçıklama: {cleanDescription}";

        if (!newsByCategory.ContainsKey(category))
            newsByCategory[category] = new List<string>();

        newsByCategory[category].Add(content);
    }

    return newsByCategory;
}

static async Task<string> SummarizeCategoryArticles(string apiKey, string category, List<string> articles)
{
    var combinedText = string.Join("\n\n", articles);

    using var client = new HttpClient();
    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

    var requestData = new
    {
        model = "gpt-3.5-turbo",
        messages = new[] {
            new { role = "system", content = "Sen bir haber özetleyicisin. Gönderilen haberleri analiz edip kategori bazlı özet çıkarırsın. Özetleri Türkçe olarak yaz." },
            new { role = "user", content = $"Kategori: {category}\n\nAşağıdaki haberleri analiz edip 3-5 cümleyle özetle:\n\n{combinedText}" }
        },
        max_tokens =800
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