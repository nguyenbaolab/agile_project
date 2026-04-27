using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Agile_Project.Models.Entities
{
    public class Team
    {
        public int TeamId { get; set; }
        public int ProjectId { get; set; }
        public string Name { get; set; } = "";

        // Single-line summary used by reports and console output.
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder("Team #" + TeamId + ": " + Name);
            if (ProjectId > 0)
                sb.Append(" (Project " + ProjectId + ")");
            return sb.ToString();
        }
    }
}
