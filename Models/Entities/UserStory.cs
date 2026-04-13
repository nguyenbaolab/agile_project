using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Agile_Project.Models.Entities
{
    public enum UserStoryState
    {
        ProjectBacklog,
        InSprint,
        Done
    }
    public class UserStory
    {
        public int UserStoryId { get; set; }
        public string Title { get; set; } = "";
        public string Description { get; set; } = "";
        public int Priority { get; set; } = 0;
        public UserStoryState State { get; set; } = UserStoryState.ProjectBacklog;
        public int ProjectId { get; set; }
        public int? SprintId { get; set; }
    }
}
