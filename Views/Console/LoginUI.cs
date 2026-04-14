using System;
using Agile_Project.Controllers;

namespace Agile_Project.Views.Console
{
    public class LoginUI
    {
        private readonly ProjectController _controller;

        public LoginUI(ProjectController controller)
        {
            _controller = controller;
        }

        public bool Run()
        {
            while (true)
            {
                System.Console.Clear();
                System.Console.WriteLine("=== AGILE PROJECT MANAGER ===");
                System.Console.WriteLine("Please login to continue\n");

                System.Console.Write("Username: ");
                var username = System.Console.ReadLine() ?? "";
                System.Console.Write("Password: ");
                var password = System.Console.ReadLine() ?? "";

                var (success, message) = _controller.Login(username, password);
                System.Console.WriteLine(message);

                if (success) return true;

                System.Console.Write("\nTry again? [Y/N]: ");
                if (System.Console.ReadLine()?.ToUpper() != "Y") return false;
            }
        }
    }
}
