using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Agile_Project.Models.Entities
{
    public enum TaskState
    {
        ToBeDone,
        InProcess,
        Done
    }

    public class ProjectTask
    {
        public int TaskId { get; set; }
        public string Title { get; set; } = "";
        public int Priority { get; set; } = 0;
        public TaskState State { get; set; } = TaskState.ToBeDone;
        public float PlannedTime { get; set; } = 0;
        public float ActualTime { get; set; } = 0;
        public DateTime? PlannedStartDate { get; set; }
        public DateTime? PlannedEndDate { get; set; }
        public DateTime? ActualStartDate { get; set; }
        public DateTime? ActualEndDate { get; set; }
        public int Difficulty { get; set; } = 0;
        public string CategoryLabels { get; set; } = "";
        public int UserStoryId { get; set; }
    }
}
