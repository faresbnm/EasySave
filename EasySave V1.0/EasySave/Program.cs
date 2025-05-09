using System;
using EasySave.Controllers;
using EasySave.Localization;

namespace EasySave
{
    internal class Program
    {
        static void Main(string[] args)
        {
            // Initialize localization
            var localization = new JsonLocalizationService();

            // Language selection
            Console.WriteLine("Choose language / Choisissez la langue:");
            Console.WriteLine("1. English");
            Console.WriteLine("2. Français");

            var choice = Console.ReadLine();
            localization.SetLanguage(choice == "2" ? "fr" : "en");

            new Controller(localization);
        }
    }
}