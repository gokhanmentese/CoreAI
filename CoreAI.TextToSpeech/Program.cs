
using System.Speech.Synthesis;

//System.Speech=>Yapay zeka olarak değerlendirilmez.

SpeechSynthesizer speechSynthesizer = new SpeechSynthesizer();

speechSynthesizer.Volume = 100; // Sesin ses seviyesi (0-100)
speechSynthesizer.Rate = 0; // Sesin hızı (-10 ile 10 arasında)

Console.WriteLine("Lütfen konuşma metnini giriniz:");
string text = Console.ReadLine();

if (!string.IsNullOrEmpty(text))
{
    speechSynthesizer.Speak(text); // Metni sesli olarak oku

    Console.WriteLine("Konuşma tamamlandı.");
}


Console.ReadLine();