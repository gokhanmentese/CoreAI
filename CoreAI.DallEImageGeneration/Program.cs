// See https://aka.ms/new-console-template for more information

using System.Net.Http.Headers;
using System.Text.Json;
using System.Text;
using System.Diagnostics;

var apiKey = "open-ai-secret-key";

Console.WriteLine("Example prompts."); //draw a yellow car on a raniny day
var prompt=Console.ReadLine();

using var client = new HttpClient();
client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

var requestData = new
{
    prompt = prompt,
    n=1,
    size = "1024x1024"
};

var json = JsonSerializer.Serialize(requestData);
var content = new StringContent(json, Encoding.UTF8, "application/json");

var response = await client.PostAsync("https://api.openai.com/v1/images/generations", content);
var responseBody = await response.Content.ReadAsStringAsync();

if (response.IsSuccessStatusCode)
{
    //Burada dönüş bir link olacak. Trayıcıda açıp resmi görebiliriz.
    var imageUrl = JsonDocument.Parse(responseBody)
                .RootElement.GetProperty("data")[0]
                .GetProperty("url")
                .GetString();
    Console.WriteLine($"Image URL: {imageUrl}");

    //Kodun çıktısını tarayıcıda açmak için de Process.Start("explorer", imageUrl) gibi bir satır ekleyebilirsin (Windows'ta).
    //Process.Start("explorer", imageUrl);
}
else
{
    Console.WriteLine("Bir hata oluştu:");
    Console.WriteLine(responseBody);
}