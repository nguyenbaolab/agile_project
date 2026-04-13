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
    }
}
