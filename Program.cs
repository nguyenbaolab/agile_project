using System.Runtime.InteropServices;
using Agile_Project.Database;
using Agile_Project.Views.Forms;
using Agile_Project.Views.Console;

namespace Agile_Project
{
    internal static class Program
    {
        [DllImport("kernel32.dll")]
        private static extern bool AllocConsole();

        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            ApplicationConfiguration.Initialize();

            DatabaseInitializer.Initialize();

            bool useGUI = true; // doi thanh false de chay Console

            if (useGUI)
            {
                ApplicationConfiguration.Initialize();
                Application.Run(new MainForm());
            }
            else
            {
                AllocConsole();
                new ConsoleUI().Run();
            }
        }
    }
}