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
                list.Add(new Person
                {
                    PersonId = Convert.ToInt32(reader["PersonId"]),
                    Name = reader["Name"].ToString()!,
                    Role = reader["Role"].ToString()!
                });
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
                list.Add(new Person
                {
                    PersonId = Convert.ToInt32(reader["PersonId"]),
                    Name = reader["Name"].ToString()!,
                    Role = reader["Role"].ToString()!
                });
            }
            return list;
        }

        public void Add(Person person)
        {
            using var conn = DatabaseConnection.GetConnection();
            conn.Open();
            var cmd = new MySqlCommand(
                "INSERT INTO Persons (Name, Role) VALUES (@Name, @Role)", conn);
            cmd.Parameters.AddWithValue("@Name", person.Name);
            cmd.Parameters.AddWithValue("@Role", person.Role);
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

        public void RemoveFromProject(int projectId, int personId)
        {
            using var conn = DatabaseConnection.GetConnection();
            conn.Open();
            var cmd = new MySqlCommand(
                "DELETE FROM ProjectPersons WHERE ProjectId=@ProjectId AND PersonId=@PersonId", conn);
            cmd.Parameters.AddWithValue("@ProjectId", projectId);
            cmd.Parameters.AddWithValue("@PersonId", personId);
            cmd.ExecuteNonQuery();
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