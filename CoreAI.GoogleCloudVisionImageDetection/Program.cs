
// Resim dosyasının yolu. Bu dosya, Google Cloud Vision API ile analiz edilecek olan resimdir.
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text;

var apiKey = "YOUR_GOOGLE_VISION_API_KEY";

Console.WriteLine("Resim yolunu giriniz:");
string imagePath = Console.ReadLine();

if (string.IsNullOrWhiteSpace(imagePath) || !File.Exists(imagePath))
{
    Console.WriteLine("Geçerli bir dosya yolu girilmedi.");
    return;
}

try
{
    var detectedObjects = await DetectObjects(apiKey, imagePath);
    Console.WriteLine("📸 Resimde Tespit Edilen Nesneler:\n");
    Console.WriteLine(detectedObjects);
}
catch (Exception ex)
{
    Console.WriteLine($"Bir hata oluştu: {ex.Message}");
}



static async Task<string> DetectObjects(string apiKey, string path)
{
    using var client = new HttpClient();
    string apiUrl = $"https://vision.googleapis.com/v1/images:annotate?key={apiKey}";

    byte[] imageBytes = File.ReadAllBytes(path);
    var base64Image = Convert.ToBase64String(imageBytes);

    var requestData = new
    {
        requests = new[]
        {
            new
            {
                image = new { content = base64Image},
                features = new[]
                {
                    new { type = "OBJECT_LOCALIZATION",
                        maxResults = 10 //Tespit edilen nesneler puanlanır (score değeri).En yüksek puanlı 10 nesne seçilir ve döndürülür.
                    }
                }
            }
        }
    };

    var json = JsonSerializer.Serialize(requestData);
    var content = new StringContent(json, Encoding.UTF8, "application/json");

    var response = await client.PostAsync(apiUrl, content);

    if (response.IsSuccessStatusCode)
    {
        var responseBody = await response.Content.ReadAsStringAsync();
        var result = JsonDocument.Parse(responseBody);

        var sb = new StringBuilder();
        var objects = result.RootElement
            .GetProperty("responses")[0]
            .GetProperty("localizedObjectAnnotations");

        foreach (var obj in objects.EnumerateArray())
        {
            var name = obj.GetProperty("name").GetString();
            var score = obj.GetProperty("score").GetDouble();
            sb.AppendLine($"🔹 {name} ({score:P0})");
        }

        return sb.Length > 0 ? sb.ToString() : "Nesne tespit edilemedi.";
    }
    else
    {
        var error = await response.Content.ReadAsStringAsync();
        Console.WriteLine($"❌ API hatası: {error}");
        return "Hata";
    }
}
