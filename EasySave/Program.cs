using EasySave.Model;
using EasySave.Services;

class Program
{
    static void Main(string[] args)
    {

        Backup myJob = new Backup("Test_Job", @"C:\test\oui", @"C:\test\azer", "Complete");


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