using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Agile_Project.Models.Entities
{
    public class Person
    {
        public int PersonId { get; set; }
        public string Name { get; set; } = "";
        public string Role { get; set; } = "";

        // Auth
        public string Username { get; set; } = "";
        public string Password { get; set; } = "";
        public string ProfileRole { get; set; } = "Developer";
    }
}
