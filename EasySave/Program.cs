using EasySave.View;

ConsoleView view = new ConsoleView();
bool continuer = true;

while (continuer)
{
    view.Reset();
    view.Display();
    string choix = view.UserInput();

    if (choix == "5")
    {
        // Changement de langue
        Console.WriteLine();
        Console.WriteLine("1. Français");
        Console.WriteLine("2. English");
        Console.Write("> ");
        string langueChoix = view.UserInput();

        if (langueChoix == "1")
        {
            view.ChangeLanguage("fr");
        }
        else if (langueChoix == "2")
        {
            view.ChangeLanguage("en");
        }
    }
    else if (choix == "6")
    {
        // Quitter
        view.Exit();
        continuer = false;
    }
    else
    {
        // Option pas encore implémentée
        Console.WriteLine();
        Console.WriteLine("Option " + choix + " pas encore disponible.");
        Console.WriteLine("Appuyez sur une touche...");
        Console.ReadKey();
    }
}

Console.WriteLine();
Console.WriteLine("Appuyez sur une touche pour fermer...");
Console.ReadKey();

using EasySave.Model;
using EasySave.Services;

class Program
{
    static void Main(string[] args)
    {
        string name = "1";
        string source = "C:\test\oui";
        string target = "C:\test\azer";
        string type = "differential";
        BackupService myJob = new BackupService();
        Backup backup = new Backup(name, source, target, type)
        BackupService.CreateJob(backup);
        BackupService.PerformJobs(backup);


        Console.WriteLine("test save");
        ISaveStrategy completeStrategy = new SaveComplete();
        completeStrategy.Save(myJob);

        Console.WriteLine("\nEnter");
        Console.ReadKey();

 
        Console.WriteLine("\ndifferential save test");
        ISaveStrategy diffStrategy = new SaveDifferential();
        diffStrategy.Save(myJob);

        Console.WriteLine("\nfinish");
    }
}

