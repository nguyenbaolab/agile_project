using Agile_Project.Models.Entities;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Agile_Project.Models.Repositories
{
    // Data access for Teams and TeamMembers. Cascade rules:
    // delete Project -> Teams; delete Team -> TeamMembers, TaskTeams; delete Person -> TeamMembers.
    public class TeamRepository : ITeamRepository
    {
        public List<Team> GetByProject(int projectId)
        {
            var list = new List<Team>();
            using var conn = DatabaseConnection.GetConnection();
            conn.Open();
            var cmd = new MySqlCommand(
                "SELECT * FROM Teams WHERE ProjectId=@ProjectId ORDER BY Name", conn);
            cmd.Parameters.AddWithValue("@ProjectId", projectId);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
                list.Add(MapFromReader(reader));
            return list;
        }

        public Team? GetById(int teamId)
        {
            using var conn = DatabaseConnection.GetConnection();
            conn.Open();
            var cmd = new MySqlCommand(
                "SELECT * FROM Teams WHERE TeamId=@Id", conn);
            cmd.Parameters.AddWithValue("@Id", teamId);
            using var reader = cmd.ExecuteReader();
            if (reader.Read()) return MapFromReader(reader);
            return null;
        }

        public int AddAndGetId(Team team)
        {
            using var conn = DatabaseConnection.GetConnection();
            conn.Open();
            var cmd = new MySqlCommand(@"
                INSERT INTO Teams (Name, ProjectId)
                VALUES (@Name, @ProjectId);
                SELECT LAST_INSERT_ID();", conn);
            cmd.Parameters.AddWithValue("@Name", team.Name);
            cmd.Parameters.AddWithValue("@ProjectId", team.ProjectId);
            return Convert.ToInt32(cmd.ExecuteScalar());
        }

        public void Update(Team team)
        {
            using var conn = DatabaseConnection.GetConnection();
            conn.Open();
            var cmd = new MySqlCommand(@"
                UPDATE Teams SET
                    Name = @Name
                WHERE TeamId = @TeamId", conn);
            cmd.Parameters.AddWithValue("@Name", team.Name);
            cmd.Parameters.AddWithValue("@TeamId", team.TeamId);
            cmd.ExecuteNonQuery();
        }

        public void Delete(int teamId)
        {
            using var conn = DatabaseConnection.GetConnection();
            conn.Open();
            // TeamMembers rows are removed by ON DELETE CASCADE
            var cmd = new MySqlCommand(
                "DELETE FROM Teams WHERE TeamId=@Id", conn);
            cmd.Parameters.AddWithValue("@Id", teamId);
            cmd.ExecuteNonQuery();
        }

        public List<Person> GetMembers(int teamId)
        {
            var list = new List<Person>();
            using var conn = DatabaseConnection.GetConnection();
            conn.Open();
            var cmd = new MySqlCommand(@"
                SELECT p.* FROM Persons p
                JOIN TeamMembers tm ON p.PersonId = tm.PersonId
                WHERE tm.TeamId = @TeamId", conn);
            cmd.Parameters.AddWithValue("@TeamId", teamId);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
                list.Add(MapPersonFromReader(reader));
            return list;
        }

        public void AddMember(int teamId, int personId)
        {
            using var conn = DatabaseConnection.GetConnection();
            conn.Open();
            var cmd = new MySqlCommand(
                "INSERT IGNORE INTO TeamMembers (TeamId, PersonId) VALUES (@TeamId, @PersonId)", conn);
            cmd.Parameters.AddWithValue("@TeamId", teamId);
            cmd.Parameters.AddWithValue("@PersonId", personId);
            cmd.ExecuteNonQuery();
        }

        public void RemoveMember(int teamId, int personId)
        {
            using var conn = DatabaseConnection.GetConnection();
            conn.Open();
            var cmd = new MySqlCommand(
                "DELETE FROM TeamMembers WHERE TeamId=@TeamId AND PersonId=@PersonId", conn);
            cmd.Parameters.AddWithValue("@TeamId", teamId);
            cmd.Parameters.AddWithValue("@PersonId", personId);
            cmd.ExecuteNonQuery();
        }

        private Team MapFromReader(MySqlDataReader reader)
        {
            return new Team
            {
                TeamId = Convert.ToInt32(reader["TeamId"]),
                ProjectId = reader["ProjectId"] == DBNull.Value ? 0 : Convert.ToInt32(reader["ProjectId"]),
                Name = reader["Name"].ToString()!
            };
        }

        private Person MapPersonFromReader(MySqlDataReader reader)
        {
            return new Person
            {
                PersonId = Convert.ToInt32(reader["PersonId"]),
                Name = reader["Name"].ToString()!,
                Role = reader["Role"] == DBNull.Value ? "" : reader["Role"].ToString()!,
                Username = SafeString(reader, "Username"),
                ProfileRole = SafeString(reader, "ProfileRole")
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
