using System;

namespace Agile_Project.Models
{
    public static class CurrentSession
    {
        public static int PersonId { get; set; }
        public static string Username { get; set; } = "";
        public static string Role { get; set; } = "";
        public static bool IsLoggedIn => !string.IsNullOrEmpty(Role);
    }
}

