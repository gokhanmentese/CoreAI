// See https://aka.ms/new-console-template for more information

using System.Net.Http.Headers;
using System.Text.Json;
using System.Text;

var apiKey = "open-ai-secret-key";
var audioFilePath = "audio.mp3";

using var client = new HttpClient();
client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

var form = new MultipartFormDataContent();

var audioContent = new ByteArrayContent(File.ReadAllBytes(audioFilePath));
audioContent.Headers.ContentType = MediaTypeHeaderValue.Parse("audio/mpeg");
form.Add(audioContent,"file",Path.GetFileName(audioFilePath));
form.Add(new StringContent("whisper-1"),"model");

Console.WriteLine("Ses dosyası işleniyor, lütfen bekleyiniz");

var response = await client.PostAsync("https://api.openai.com/v1/audio/transcriptions", form);

if (response.IsSuccessStatusCode)
{
    var responseBody = await response.Content.ReadAsStringAsync();


    Console.WriteLine($"Transkript");
}
else
{
    Console.WriteLine("Bir hata olustu"); 
}


