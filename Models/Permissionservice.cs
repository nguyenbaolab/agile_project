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
                "ChangeTaskState" => role is "Admin" or "Developer",
                "AssignPerson" => role is "Admin" or "ProductOwner",
                "ManagePerson" => role == "Admin",
                "ViewReport" => true,
                "AddTask" => role is "Admin" or "ProductOwner",
                _ => false
            };
        }
    }
}
