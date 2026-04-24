using EasySave.View;

Console.WriteLine("=== Test de ConsoleView ===");
Console.WriteLine();

ConsoleView view = new ConsoleView();
view.Display();

string choix = view.UserInput();
Console.WriteLine();
Console.WriteLine("Tu as choisi : " + choix);

Console.WriteLine();
Console.WriteLine("Appuyez sur une touche pour fermer...");
Console.ReadKey();