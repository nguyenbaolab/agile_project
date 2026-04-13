using MySql.Data.MySqlClient;
using Agile_Project.Models;

namespace Agile_Project
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            try
            {
                using var conn = DatabaseConnection.GetConnection();
                conn.Open();
                MessageBox.Show("Connect Database successfully!");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }
    }
}
