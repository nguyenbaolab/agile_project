using System.Collections.Generic;
using Agile_Project.Models.Entities;

namespace Agile_Project.Controllers
{
    public interface IUserStoryController
    {
        bool AddUserStory(int projectId, string title, string description, int priority);
        void DeleteUserStory(int storyId);
        (bool success, string message) ChangeState(int storyId, UserStoryState newState);
        List<UserStory> GetByProject(int projectId);
        UserStory? GetById(int storyId);
    }
}
