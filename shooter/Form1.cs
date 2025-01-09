using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ListView;

namespace shooter
{
    public partial class Form1 : Form
    {
        private Leaderboard ldrbrd;
        private Game game;
        private string connstr = @"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=|DataDirectory|\Leaderboard.mdf;Integrated Security=True";

        [DllImport("Gdi32.dll", EntryPoint = "CreateRoundRectRgn")]
        private static extern IntPtr CreateRoundRectRgn(
            int nLeftRect,
            int nTopRect,
            int nRightRect,
            int nBottomRect,
            int nWidthEllipse,
            int nHeightEllipse
            );
        public Form1()
        {
            InitializeComponent();

            //rounded edges
            this.Region = System.Drawing.Region.FromHrgn(CreateRoundRectRgn(0, 0, Width, Height, 30, 30));

            startbutt.FlatStyle = FlatStyle.Flat; //minimise start button border 
            exitf1.FlatAppearance.BorderSize = 0; //remove exit button border

            toolTip1.OwnerDraw = true;
            toolTip1.Draw += new DrawToolTipEventHandler(toolTip1_Draw);
            toolTip1.SetToolTip(howto, "Move: arrow keys \nShoot: SPACE \nRestart: ENTER \nExit: Esc");

        }

        private void toolTip1_Draw(object sender, DrawToolTipEventArgs e)
        {
            Font f = new Font("Arial", 9, FontStyle.Bold);
            SizeF textSize = e.Graphics.MeasureString(e.ToolTipText, f);
            e.DrawBackground();
            e.DrawBorder();
            e.Graphics.DrawString(e.ToolTipText, f, Brushes.Black, new Rectangle(e.Bounds.Location, Size.Ceiling(textSize)));

        }

        private void exitf1_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void Leaderboard_FormClosed(object sender, FormClosedEventArgs e)
        {
            // Show Form1 when Leaderboard form is closed
            this.Show();

            // Reload the ComboBox in Form1
            LoadComboBox();
        }

        private void leaderboard_Click(object sender, EventArgs e)
        {

            // Create an instance of Leaderboard and subscribe to its FormClosed event
            ldrbrd = new Leaderboard();
            ldrbrd.FormClosed += Leaderboard_FormClosed;

            // Hide Form1 when Leaderboard form is shown
            this.Hide();

            // Show Leaderboard form
            ldrbrd.Show();


        }

        private void startbutt_Click(object sender, EventArgs e)
        {
            // Check if any item is selected in the ComboBox
            if (comboBox1.SelectedItem == null)
            {
                // If no player is selected, show an error message
                MessageBox.Show("Please select a player to start the game.");
            }
            else
            {
                // If a player is selected, proceed with starting the game
                string user = comboBox1.Text;
                StartSession(user);
                game = new Game();

                // Hide Form1 when game form is shown
                this.Hide();

                // Subscribe to the game form closing event
                game.FormClosed += new FormClosedEventHandler(Game_FormClosed);

                // Show game form
                game.Show();
            }
        }

        private void StartSession(string user)
        {
            var con = new SqlConnection(connstr);
            con.Open();
            int score = 0;
            DateTime dateTime = DateTime.Now;

            string query = "SELECT [High Score],Date FROM leaderboard WHERE username = @username";
            SqlCommand cmd = new SqlCommand(query, con);
            cmd.Parameters.AddWithValue("@username", user);

            using (SqlDataReader reader = cmd.ExecuteReader())
            {
                if (reader.Read())
                {
                    score = int.Parse(reader[0].ToString());
                    dateTime = reader.GetDateTime(1);
                }
            }

            insertintosession(user, score, dateTime);
        }

        private void insertintosession(string user, int score, DateTime dateTime)
        {
            var con = new SqlConnection(connstr);
            con.Open();

            string query = "INSERT INTO Session VALUES(@user,@score,@date)";

            var cmd = new SqlCommand(query, con);
            cmd.Parameters.AddWithValue("@user", user);
            cmd.Parameters.AddWithValue("@score", score);
            cmd.Parameters.AddWithValue("@date", dateTime);

            cmd.ExecuteNonQuery();

        }

        private void Game_FormClosed(object sender, FormClosedEventArgs e)
        {
            // Handle the game form being closed (e.g., show Form1 again)
            this.Show();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            textBox1.Visible = true;
            button2.Visible = true;
            button1.Visible = false;
            comboBox1.Visible = false;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            LoadComboBox();
            textBox1.Visible = false;
            button2.Visible = false;
        }

        private void LoadComboBox()
        {
            comboBox1.Items.Clear(); // Clear existing items
            using (SqlConnection connection = new SqlConnection(connstr))
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

        private void button2_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(textBox1.Text))
            {
                MessageBox.Show("Please enter a name before adding.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return; // Exit the method if the TextBox is empty
            }

            string newname = textBox1.Text;
            int nscore = 0;
            DateTime dt = DateTime.Now;

            var con = new SqlConnection(connstr);
            con.Open();
            string query = "INSERT INTO leaderboard VALUES(@username,@highscore,@date)";
            var cmd = new SqlCommand(query, con);
            cmd.Parameters.AddWithValue("@username", newname);
            cmd.Parameters.AddWithValue("@highscore", nscore);
            cmd.Parameters.AddWithValue("@date", dt);
            cmd.ExecuteNonQuery();

            con.Close();
            cmd.Dispose();
            con.Dispose();

            comboBox1.Items.Add(newname);

            textBox1.Visible = false;
            textBox1.Clear();
            button2.Visible = false;
            button1.Visible = true;
            comboBox1.Visible = true;

        }


    }
}