using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Agile_Project.Models.Entities
{
    public class Person
    {
        public bool IsSystemAccount { get; set; } = false;
        public int PersonId { get; set; }
        public string Name { get; set; } = "";
        public string Role { get; set; } = "";

        // Auth
        public string Username { get; set; } = "";
        public string Password { get; set; } = "";
        public string ProfileRole { get; set; } = "Developer";

        // Single-line summary used by reports and console output.
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder("Person #" + PersonId + ": " + Name);
            if (!string.IsNullOrEmpty(Role))
                sb.Append(" (" + Role + ")");
            if (!string.IsNullOrEmpty(ProfileRole))
                sb.Append(" [" + ProfileRole + "]");
            return sb.ToString();
        }
    }
}
