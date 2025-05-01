
// Tesseract OCR kütüphanesini kullanarak metin tanıma işlemi yapar
// Dil dosyaları genelde  C:\Program Files\Tesseract-OCR\tessdata\ dizininde bulunur
//Türkçe için tur.traineddata dosyası burada olmalıdır. Yoksa şuradan indirilebilir:
// https://github.com/tesseract-ocr/tessdata

using Tesseract;

Console.WriteLine("Karakter okuması yapılacak resim yolunu giriniz.");
string imagePath = Console.ReadLine(); // "C://Temp//invoice-sample.jpg";

// Tesseract'ın kurulu olduğu dizindeki 'tessdata' klasörünü içeren dizini belirt.Burada traineddata lar tessdata dizininde bulunur.
string tessdataParentPath = @"C:\Program Files\Tesseract-OCR\tessdata";

try
{
    //Türkçe için tur girilmelidir.Çoklu dil kullanımı tur+eng gibi girilmelidir.
    using var engine = new TesseractEngine(tessdataParentPath, "eng", EngineMode.Default);
    using var img = Pix.LoadFromFile(imagePath);
    using var page = engine.Process(img);

    var text = page.GetText();
    Console.WriteLine("Resimde Tespit Edilen Metin:");
    Console.WriteLine(text);

}
catch (Exception ex)
{
    Console.WriteLine($"Hata: {ex.Message}");
}

Console.ReadLine();