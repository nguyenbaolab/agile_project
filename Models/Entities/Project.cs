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

        // Single-line summary used by reports and console output.
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder("Project #" + ProjectId + ": " + Name);
            if (!string.IsNullOrEmpty(Description))
                sb.Append(" — " + Description);
            return sb.ToString();
        }
    }
}
