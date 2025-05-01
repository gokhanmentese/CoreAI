// See https://aka.ms/new-console-template for more information

using System.Net.Http.Headers;
using System.Text.Json;
using System.Text;

var apiKey = "open-ai-secret-key";

using var client = new HttpClient();
client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

var requestData = new
{
    model = "gpt-3.5-turbo",
    messages = new[] {
        new {role = "system", content = "You are a helpful assistant"},
        new {role = "user", content = "Merhaba, nasılsın?"}
    },
    max_tokens = 100 // uygulamanın bize getirdiği içeriğin uzunlugu
};

var json = JsonSerializer.Serialize(requestData);
var content = new StringContent(json, Encoding.UTF8, "application/json");

var response = await client.PostAsync("https://api.openai.com/v1/chat/completions", content);
var responseBody = await response.Content.ReadAsStringAsync();

if (response.IsSuccessStatusCode)
{
    var result = JsonSerializer.Deserialize<JsonElement>(responseBody);
    var answer = result.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString();

    Console.WriteLine($"Open AI'ın cevabı:{answer}");
}
else
{
    Console.WriteLine("Bir hata olustu");
}
