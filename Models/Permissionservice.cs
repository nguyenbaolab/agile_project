using System;

namespace Agile_Project.Models
{
    public static class PermissionService
    {
        public static bool CanDo(string action)
        {
            string role = CurrentSession.Role;
            return action switch
            {
                "ManageProject" => role is "Admin" or "ProductOwner",
                "ManageUserStory" => role is "Admin" or "ProductOwner",
                "ChangeUserStoryState" => role is "Admin" or "ProductOwner" or "Developer",
                "ChangeTaskState" => role is "Admin" or "ProductOwner" or "Developer",
                "AssignPerson" => role is "Admin" or "ProductOwner",
                "ManagePerson" => role == "Admin",
                "ViewReport" => true,
                "AddTask" => role is "Admin" or "ProductOwner",
                "ManageTeam" => role == "Admin",
                "ManageTeamMember" => role is "Admin" or "ProductOwner",
                "ViewTeam" => true,
                _ => false
            };
        }
    }
}
