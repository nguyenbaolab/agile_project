using System.Collections.Generic;
using Agile_Project.Models.Entities;

namespace Agile_Project.Models.Repositories
{
    public interface ITaskRepository
    {
        List<ProjectTask> GetByUserStory(int userStoryId);
        ProjectTask? GetById(int taskId);
        void Add(ProjectTask task);
        void Update(ProjectTask task);
        int AddAndGetId(ProjectTask task);
        void UpdateState(int taskId, TaskState newState);
        void UpdatePriority(int taskId, int priority);
        void AssignPerson(int taskId, int personId);
        void RemovePerson(int taskId, int personId);
        void ResetAllToToBeDone(int userStoryId);
        List<Person> GetAssignedPersons(int taskId);
    }
}
