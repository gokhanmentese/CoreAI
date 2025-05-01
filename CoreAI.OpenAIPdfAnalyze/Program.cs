using System.Net.Http.Headers;
using System.Text.Json;
using System.Text;
using UglyToad.PdfPig;

var apiKey = "open-ai-secret-key";

Console.WriteLine("Lütfen pdf dosya yolunu girin:");
var pdfPath = Console.ReadLine();

if (!string.IsNullOrEmpty(pdfPath))
{
    Console.WriteLine("PDF içeriği çekiliyor...");

    var pdfContent = await ExtractTextFromPdf(pdfPath);

    if (string.IsNullOrWhiteSpace(pdfContent))
    {
        Console.WriteLine("Pdf içeriği alınamadı veya boş.");
        return;
    }

    Console.WriteLine("Pdf analiz ediliyor...");

    var analysis = await AnalyzeWithAI(apiKey, "Pdf", pdfContent);

    Console.WriteLine("\n🧠 Pdf Analizi Sonucu:\n");
    Console.WriteLine(analysis);
}
else
{
    Console.WriteLine("Geçerli bir URL girilmedi.");
}

static async Task<string> ExtractTextFromPdf(string path)
{
    using PdfDocument pdf = PdfDocument.Open(path);
    var text = new StringBuilder();

    foreach (var page in pdf.GetPages())
    {
        text.AppendLine(page.Text);
    }

    return text.ToString();
}

static async Task<string> AnalyzeWithAI(string apiKey, string sourceType, string text)
{
    // Uzun içerikleri kes
    var maxLength = 3500;
    if (text.Length > maxLength)
        text = text.Substring(0, maxLength);

    using var client = new HttpClient();
    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

    var requestData = new
    {
        model = "gpt-3.5-turbo",
        messages = new[] {
            new { role = "system", content = "Sen bir yapay zeka asistanısın.Kullanıcının gönderdiği metni analiz eder ve Türkçe olarak özetlersin. Yanıtlarını sadece Türkçe istiyorum." },
            new { role = "user", content = $"Lütfen aşağıdaki {sourceType} içeriğini analiz edip Türkçe olarak özetle:\n\n{text}" }
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