using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Agile_Project.Models.Entities
{
    public class Project
    {
        public int ProjectId { get; set; }
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
    }
}
