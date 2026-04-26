using Agile_Project.Models.Entities;
using Agile_Project.Models.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Agile_Project.Controllers
{
    public class UserStoryController : IUserStoryController
    {
        private readonly IUserStoryRepository _storyRepo;
        private readonly ITaskRepository _taskRepo;

        public UserStoryController(IUserStoryRepository storyRepo, ITaskRepository taskRepo)
        {
            _storyRepo = storyRepo;
            _taskRepo = taskRepo;
        }

        public UserStoryController() : this(new UserStoryRepository(), new TaskRepository()) { }

        public bool AddUserStory(int projectId, string title, string description, int priority)
        {
            if (string.IsNullOrWhiteSpace(title)) return false;

            _storyRepo.Add(new UserStory
            {
                ProjectId = projectId,
                Title = title,
                Description = description,
                Priority = priority,
                State = UserStoryState.ProjectBacklog
            });
            return true;
        }

        public void DeleteUserStory(int storyId)
        {
            _storyRepo.Delete(storyId);
        }

        public (bool success, string message) ChangeState(int storyId, UserStoryState newState)
        {
            var story = _storyRepo.GetById(storyId);
            if (story == null) return (false, "User story not found.");

            int current = (int)story.State;
            int target = (int)newState;
            if (Math.Abs(current - target) != 1)
                return (false, "Invalid state transition.");

            if (newState == UserStoryState.InSprint)
            {
                if (!CanMoveToSprint(story))
                    return (false, "Dependencies are not in sprint or done yet.");

                _taskRepo.ResetAllToToBeDone(storyId);
            }

            if (newState == UserStoryState.Done)
            {
                if (!CanMoveToDone(storyId))
                    return (false, "All tasks must be done first.");
            }

            _storyRepo.UpdateState(storyId, newState);
            return (true, "State updated successfully.");
        }

        public List<UserStory> GetByProject(int projectId)
        {
            return _storyRepo.GetByProject(projectId);
        }

        public UserStory? GetById(int storyId)
        {
            return _storyRepo.GetById(storyId);
        }

        private bool CanMoveToSprint(UserStory story)
        {
            var dependencies = _storyRepo.GetDependencies(story.UserStoryId);
            foreach (var depId in dependencies)
            {
                var dep = _storyRepo.GetById(depId);
                if (dep == null) continue;
                if (dep.State != UserStoryState.InSprint && dep.State != UserStoryState.Done)
                    return false;
            }
            return true;
        }

        private bool CanMoveToDone(int storyId)
        {
            var tasks = _taskRepo.GetByUserStory(storyId);
            return tasks.All(t => t.State == TaskState.Done);
        }
    }
}
