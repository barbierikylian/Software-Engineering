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
