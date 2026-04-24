using System;
using EasySave.View;
using EasySave.ViewModel;

namespace EasySave
{
    // Application entry point: wires up the ViewModels + View and starts the UI loop.
    class Program
    {
        static void Main(string[] args)
        {
            Console.Title = "EasySave - Console";
            SaveViewModel saveViewModel = new SaveViewModel();
            LanguageViewModel languageViewModel = new LanguageViewModel();
            ConsoleView view = new ConsoleView(saveViewModel, languageViewModel);
            view.Display();
        }
    }
}