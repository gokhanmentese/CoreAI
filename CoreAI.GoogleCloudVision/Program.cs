
using Google.Cloud.Vision.V1;

// Google Cloud Vision API'yi kullanabilmek için gerekli olan NuGet paketini yükleyin.

// Google Cloud Vision API için gerekli olan kimlik bilgileri dosyasının yolu.
// Bu dosya, Google Cloud Console'dan indirilmelidir ve projenizin kimlik bilgilerini içerir.
//Oluşturulan servis hesabını enable etmelisiniz.
// Google Cloud Console'da projenizi oluşturduktan sonra, "IAM & Admin" sekmesinden "Service accounts" kısmına gidin.
// Yeni bir servis hesabı oluşturun ve "JSON" formatında kimlik bilgilerini indirin.
var credentialsPath = @"C:\CoreAI\GoogleCloudVision\credentials.json";

if (!File.Exists(credentialsPath))
{
    Console.WriteLine("Kimlik bilgileri dosyası bulunamadı.");
    return;
}

// Resim dosyasının yolu. Bu dosya, Google Cloud Vision API ile analiz edilecek olan resimdir.
Console.WriteLine("Resim yolunu giriniz:");
string imagePath = Console.ReadLine();

if (string.IsNullOrWhiteSpace(imagePath) || !File.Exists(imagePath))
{
    Console.WriteLine("Geçerli bir dosya yolu girilmedi.");
    return;
}

try
{
    var client = new ImageAnnotatorClientBuilder
    {
        CredentialsPath = credentialsPath
    }.Build();

    var image = Google.Cloud.Vision.V1.Image.FromFile(imagePath);
    var response = client.DetectText(image);

    var text = string.Join(Environment.NewLine, response.Select(r => r.Description));

    Console.WriteLine("Resimde Tespit Edilen Metin:");
    Console.WriteLine(text);

    var outputPath = Path.ChangeExtension(imagePath, ".txt");
    File.WriteAllText(outputPath, text);
    Console.WriteLine($"Sonuç şu dosyaya yazıldı: {outputPath}");

}
catch (Exception ex)
{
    Console.WriteLine($"Bir hata oluştu: {ex.Message}");
}
