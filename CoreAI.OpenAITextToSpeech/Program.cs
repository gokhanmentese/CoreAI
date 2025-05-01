
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text;

var apiKey = "open-ai-secret-key";

Console.WriteLine("Konuşma metnini girin");
var inputText = Console.ReadLine();

if (!string.IsNullOrEmpty(inputText))
{
    Console.WriteLine("Ses dosyası oluşturuluyor.");

    await GenerateSpeech(apiKey, inputText);

    Console.WriteLine("Ses dosyası oluşturuldu.");

    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
    {
        FileName = "output.mp3",
        UseShellExecute = true
    });
}

static async Task GenerateSpeech(string apiKey, string text)
{
    using var client = new HttpClient();
    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

    var requestData = new
    {
        model = "tts-1",
        input = text,
        voice = "alloy", //fable,echo,shimmer -- ses tonlaması
        response_format = "mp3"
    };

    var json = JsonSerializer.Serialize(requestData);
    var content = new StringContent(json, Encoding.UTF8, "application/json");

    var response = await client.PostAsync("https://api.openai.com/v1/audio/speech", content);

    if (response.IsSuccessStatusCode)
    {
        var audioBytes = await response.Content.ReadAsByteArrayAsync();

        await File.WriteAllBytesAsync("output.mp3", audioBytes);
        Console.WriteLine("Ses dosyası oluşturuldu: output.mp3");
    }
    else
    {
        var error = await response.Content.ReadAsStringAsync();
        Console.WriteLine($"Bir hata oluştu: {error}");
    }
}