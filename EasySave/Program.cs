using EasySave.Model;

Console.WriteLine("=== Test du LanguageManager ===");
Console.WriteLine();

LanguageManager lang = new LanguageManager();

// Test en anglais
lang.LoadLanguage("en");
Console.WriteLine("Langue : Anglais");
Console.WriteLine(lang.GetString("welcome"));
Console.WriteLine(lang.GetString("menu_exit"));
Console.WriteLine(lang.GetString("goodbye"));
Console.WriteLine();

// Test en français
lang.LoadLanguage("fr");
Console.WriteLine("Langue : Francais");
Console.WriteLine(lang.GetString("welcome"));
Console.WriteLine(lang.GetString("menu_exit"));
Console.WriteLine(lang.GetString("goodbye"));
Console.WriteLine();

Console.WriteLine("Appuyez sur une touche pour fermer...");
Console.ReadKey();