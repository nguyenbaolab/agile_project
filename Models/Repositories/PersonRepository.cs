using Agile_Project.Models.Entities;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Agile_Project.Models.Repositories
{
    public class PersonRepository : IPersonRepository
    {
        public List<Person> GetAll()
        {
            var list = new List<Person>();
            using var conn = DatabaseConnection.GetConnection();
            conn.Open();
            var cmd = new MySqlCommand("SELECT * FROM Persons", conn);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                list.Add(MapFromReader(reader));
            }
            return list;
        }

        public List<Person> GetByProject(int projectId)
        {
            var list = new List<Person>();
            using var conn = DatabaseConnection.GetConnection();
            conn.Open();
            var cmd = new MySqlCommand(@"
                SELECT p.* FROM Persons p
                JOIN ProjectPersons pp ON p.PersonId = pp.PersonId
                WHERE pp.ProjectId = @ProjectId", conn);
            cmd.Parameters.AddWithValue("@ProjectId", projectId);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                list.Add(MapFromReader(reader));
            }
            return list;
        }

        public void Add(Person person)
        {
            using var conn = DatabaseConnection.GetConnection();
            conn.Open();
            var cmd = new MySqlCommand(
                "INSERT INTO Persons (Name, Role, ProfileRole) VALUES (@Name, @Role, @ProfileRole)", conn);
            cmd.Parameters.AddWithValue("@Name", person.Name);
            cmd.Parameters.AddWithValue("@Role", person.Role);
            cmd.Parameters.AddWithValue("@ProfileRole",
                string.IsNullOrWhiteSpace(person.ProfileRole) ? "Developer" : person.ProfileRole);
            cmd.ExecuteNonQuery();
        }

        public bool Delete(int personId)
        {
            using var conn = DatabaseConnection.GetConnection();
            conn.Open();

            var check = new MySqlCommand(
                "SELECT IsSystemAccount FROM Persons WHERE PersonId=@Id", conn);
            check.Parameters.AddWithValue("@Id", personId);
            var result = check.ExecuteScalar();
            if (result != null && result != DBNull.Value && Convert.ToInt32(result) == 1)
                return false;

            var cmd = new MySqlCommand(
                "DELETE FROM Persons WHERE PersonId=@Id", conn);
            cmd.Parameters.AddWithValue("@Id", personId);
            cmd.ExecuteNonQuery();
            return true;
        }

        public void AddToProject(int projectId, int personId)
        {
            using var conn = DatabaseConnection.GetConnection();
            conn.Open();
            var cmd = new MySqlCommand(
                "INSERT IGNORE INTO ProjectPersons (ProjectId, PersonId) VALUES (@ProjectId, @PersonId)", conn);
            cmd.Parameters.AddWithValue("@ProjectId", projectId);
            cmd.Parameters.AddWithValue("@PersonId", personId);
            cmd.ExecuteNonQuery();
        }

        // Cuts every link between this person and this project in one transaction:
        // team memberships in the project, task assignments in the project, and the
        // ProjectPersons row itself. The person then disappears from the project's reports.
        public void RemoveFromProject(int projectId, int personId)
        {
            using var conn = DatabaseConnection.GetConnection();
            conn.Open();
            using var tx = conn.BeginTransaction();

            var delTeamMembers = new MySqlCommand(@"
                DELETE tm FROM TeamMembers tm
                JOIN Teams t ON tm.TeamId = t.TeamId
                WHERE t.ProjectId = @ProjectId AND tm.PersonId = @PersonId", conn, tx);
            delTeamMembers.Parameters.AddWithValue("@ProjectId", projectId);
            delTeamMembers.Parameters.AddWithValue("@PersonId", personId);
            delTeamMembers.ExecuteNonQuery();

            var delTaskPersons = new MySqlCommand(@"
                DELETE tp FROM TaskPersons tp
                JOIN Tasks ta ON tp.TaskId = ta.TaskId
                JOIN UserStories us ON ta.UserStoryId = us.UserStoryId
                WHERE us.ProjectId = @ProjectId AND tp.PersonId = @PersonId", conn, tx);
            delTaskPersons.Parameters.AddWithValue("@ProjectId", projectId);
            delTaskPersons.Parameters.AddWithValue("@PersonId", personId);
            delTaskPersons.ExecuteNonQuery();

            var delProjectPerson = new MySqlCommand(
                "DELETE FROM ProjectPersons WHERE ProjectId=@ProjectId AND PersonId=@PersonId", conn, tx);
            delProjectPerson.Parameters.AddWithValue("@ProjectId", projectId);
            delProjectPerson.Parameters.AddWithValue("@PersonId", personId);
            delProjectPerson.ExecuteNonQuery();

            tx.Commit();
        }

        public void RemoveFromTask(int taskId, int personId)
        {
            using var conn = DatabaseConnection.GetConnection();
            conn.Open();
            var cmd = new MySqlCommand(
                "DELETE FROM TaskPersons WHERE TaskId=@TaskId AND PersonId=@PersonId", conn);
            cmd.Parameters.AddWithValue("@TaskId", taskId);
            cmd.Parameters.AddWithValue("@PersonId", personId);
            cmd.ExecuteNonQuery();
        }

        // Login
        public Person? Login(string username, string password)
        {
            using var conn = DatabaseConnection.GetConnection();
            conn.Open();
            var cmd = new MySqlCommand(
                "SELECT * FROM Persons WHERE Username=@U AND Password=@P", conn);
            cmd.Parameters.AddWithValue("@U", username);
            cmd.Parameters.AddWithValue("@P", password);
            using var reader = cmd.ExecuteReader();
            if (reader.Read()) return MapFromReader(reader);
            return null;
        }

        private Person MapFromReader(MySqlDataReader reader)
        {
            bool isSystem = false;
            try
            {
                var col = reader["IsSystemAccount"];
                if (col != DBNull.Value)
                    isSystem = Convert.ToInt32(col) == 1;
            }
            catch {  }

            return new Person
            {
                PersonId = Convert.ToInt32(reader["PersonId"]),
                Name = reader["Name"].ToString()!,
                Role = reader["Role"] == DBNull.Value ? "" : reader["Role"].ToString()!,
                Username = SafeString(reader, "Username"),
                ProfileRole = SafeString(reader, "ProfileRole"),
                IsSystemAccount = isSystem
            };
        }

        private static string SafeString(MySqlDataReader reader, string column)
        {
            try
            {
                var val = reader[column];
                return val == DBNull.Value ? "" : val.ToString()!;
            }
            catch { return ""; }
        }
    }
}