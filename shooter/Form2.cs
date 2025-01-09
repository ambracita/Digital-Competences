using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace shooter
{
    public partial class Leaderboard : Form
    {
        [DllImport("Gdi32.dll", EntryPoint = "CreateRoundRectRgn")]
        private static extern IntPtr CreateRoundRectRgn(
            int nLeftRect,
            int nTopRect,
            int nRightRect,
            int nBottomRect,
            int nWidthEllipse,
            int nHeightEllipse
            );

        public Leaderboard()
        {
            InitializeComponent();

            //rounded edges
            this.Region = System.Drawing.Region.FromHrgn(CreateRoundRectRgn(0, 0, Width, Height, 30, 30));

        }

        private void Leaderboard_Load(object sender, EventArgs e)
        {
            LoadComboBox();
            string connectionString = @"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=|DataDirectory|\Leaderboard.mdf;Integrated Security=True";
            string query = "SELECT * FROM leaderboard";

            using (SqlConnection con = new SqlConnection(connectionString))
            {
                con.Open();
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                    DataTable table = new DataTable();
                    adapter.Fill(table);

                    // Afișarea datelor în DataGridView
                    Board.DataSource = table;
                }
            }
        }

        private void LoadComboBox()
        {
            comboBox1.Items.Clear();

            string connectionString = @"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=|DataDirectory|\Leaderboard.mdf;Integrated Security=True";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string sql = "SELECT Username FROM leaderboard";
                SqlCommand command = new SqlCommand(sql, connection);

                connection.Open();
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        comboBox1.Items.Add(reader.GetString(0)); // Assuming "Name" is the first column (index 0)
                    }
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void LoadDataGridView()
        {
            string connectionString = @"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=|DataDirectory|\Leaderboard.mdf;Integrated Security=True";
            string query = "SELECT * FROM leaderboard";

            using (SqlConnection con = new SqlConnection(connectionString))
            {
                con.Open();
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                    DataTable table = new DataTable();
                    adapter.Fill(table);

                    // Display data in the DataGridView
                    Board.DataSource = table;
                }
            }
        }

        private void deluser_Click(object sender, EventArgs e)
        {
            string username = comboBox1.Text;

            string connectionString = @"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=|DataDirectory|\Leaderboard.mdf;Integrated Security=True";
            string query = "DELETE FROM leaderboard WHERE Username = @Username";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Username", username);

                    connection.Open();
                    int rowsAffected = command.ExecuteNonQuery(); // Execute the delete command

                    if (rowsAffected > 0)
                    {
                        LoadComboBox(); // Reload combo box
                        LoadDataGridView(); // Reload data grid view
                    }
                }
            }

        }
    }
}