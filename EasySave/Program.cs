using System;
using EasySave.Controllers;

namespace EasySave
{
    internal class Program
    {
        static void Main(string[] args)
        {
            // Initialize the Controller which will handle the application flow
            new Controller();

            // Note: The Console.ReadKey() from your original version is no longer needed here
            // because the Controller's Run() method now contains the main application loop
        }
    }
}
