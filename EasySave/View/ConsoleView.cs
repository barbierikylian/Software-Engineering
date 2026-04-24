using System;
using EasySave.ViewModel;

namespace EasySave.View
{
    public class ConsoleView
    {
        private LanguageViewModel langVM;

        public ConsoleView()
        {
            langVM = new LanguageViewModel();
        }

        public void Display()
        {
            Console.WriteLine("===================================");
            Console.WriteLine(langVM.GetString("welcome"));
            Console.WriteLine("===================================");
            Console.WriteLine();
            Console.WriteLine("1. " + langVM.GetString("menu_create"));
            Console.WriteLine("2. " + langVM.GetString("menu_execute"));
            Console.WriteLine("3. " + langVM.GetString("menu_list"));
            Console.WriteLine("4. " + langVM.GetString("menu_delete"));
            Console.WriteLine("5. " + langVM.GetString("menu_language"));
            Console.WriteLine("6. " + langVM.GetString("menu_exit"));
            Console.WriteLine();
            Console.Write(langVM.GetString("choose_option"));
        }

        public string UserInput()
        {
            string input = Console.ReadLine();
            return input;
        }

        public void Reset()
        {
            Console.Clear();
        }

        public void Exit()
        {
            Console.WriteLine(langVM.GetString("goodbye"));
        }

        public void ChangeLanguage(string language)
        {
            langVM.UpdateLanguage(language);
        }
    }
}