using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;

namespace shooter
{
    public partial class Game : Form
    {
        private string connstr = @"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=|DataDirectory|\Leaderboard.mdf;Integrated Security=True";

        bool goLeft, goRight, shooting, isGameOver;
        int score;
        int playerSpeed = 6;
        int enemySpeed;
        int bulletSpeed;
        Random rnd = new Random();
        private string username = "";
        int highscore = 0;
        DateTime dt = DateTime.Now;


        public Game()
        {
            InitializeComponent();
            ResetGame();
            this.KeyDown += Game_KeyDown;
            this.DoubleBuffered = true;
        }

        private void Game_KeyDown(object sender, KeyEventArgs e)
        {
            // Check if the pressed key is the Escape key
            if (e.KeyCode == Keys.Escape)
            {
                panel1.Visible = true;

            }
        }

        private void MainGameTimerEvent(object sender, EventArgs e)
        {
            txtScore.Text = score.ToString();

            enemy1.Top += enemySpeed;
            enemy2.Top += enemySpeed;
            enemy3.Top += enemySpeed;

            if(enemy1.Top > 535 || enemy2.Top > 535 || enemy3.Top > 535)
            {
                GameOver();
            }

            /*if (enemy1.Bounds.IntersectsWith(player.Bounds) || enemy2.Bounds.IntersectsWith(player.Bounds) || enemy3.Bounds.IntersectsWith(player.Bounds))
            {
                GameOver();
            }*/

                if (goLeft == true && player.Left > 0)
            {
                player.Left -= playerSpeed;
            }

            if(goRight && player.Right < this.ClientSize.Width)
            {
                player.Left += playerSpeed;
            }

            if (shooting == true)
            {
                bulletSpeed = 20;
                bullet.Top -= bulletSpeed;
            }
            else
            {
                bullet.Left = -300;
                bulletSpeed = 0;
            }

            if (bullet.Top < -41)
            {
                shooting = false;
            }

            if (bullet.Bounds.IntersectsWith(enemy1.Bounds))
            {
                score += 1;
                enemy1.Top = -450;
                enemy1.Left = rnd.Next(20, 600);
                shooting = false;
            }

            if (bullet.Bounds.IntersectsWith(enemy2.Bounds))
            {
                score += 1;
                enemy2.Top = -650;
                enemy2.Left = rnd.Next(20, 600);
                shooting = false;
            }

            if (bullet.Bounds.IntersectsWith(enemy3.Bounds))
            {
                score += 1;
                enemy3.Top = -750;
                enemy3.Left = rnd.Next(20, 600);
                shooting = false;
            }

            if(score==10)
            {
                enemySpeed = 4;
            }
            if(score==20)
            {
                enemySpeed = 6;
            }
        }

        private void KeyisDown(object sender, KeyEventArgs e)
        {
            if(e.KeyCode == Keys.Left)
            {
                goLeft = true;
            }

            if(e.KeyCode == Keys.Right)
            {
                goRight = true;
            }

        }

        private void Game_Load(object sender, EventArgs e)
        {
            GetSessionDetails(ref username,ref highscore,ref dt);
            panel1.Visible = false;

        }

       

        private void GetSessionDetails(ref string username, ref int score, ref DateTime dt)
        {
            using (SqlConnection con = new SqlConnection(connstr))
            {
                con.Open();

                string query = "SELECT Username,[High Score],Date FROM Session";

                SqlCommand cmd = new SqlCommand(query, con);
                SqlDataReader reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    username = reader["Username"].ToString();
                    score = int.Parse(reader[1].ToString());
                    dt = reader.GetDateTime(2);
                }
            }
        }


        private void KeyisUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Left)
            {
                goLeft = false;
            }

            if (e.KeyCode == Keys.Right)
            {
                goRight = false;
            }

            if(e.KeyCode == Keys.Space && shooting==false)
            {
                shooting = true;

                bullet.Top = player.Top - 41;
                bullet.Left = player.Left + (player.Width/2);
            }

            if (e.KeyCode == Keys.Enter && isGameOver == true)
            {
                ResetGame();
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            CloseSession();
            this.Close();
        }

        private void CloseSession()
        {
            var con = new SqlConnection(connstr);
            con.Open();
            string query = "DELETE FROM Session WHERE username = @UserName";
            var cmd = new SqlCommand(query, con);
            cmd.Parameters.AddWithValue("@UserName", username);
            cmd.ExecuteNonQuery();

            con.Close();
            cmd.Dispose();
            con.Dispose();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            panel1.Visible = false;
            this.Focus();
        }

        private void ResetGame()
        {
            playTimer.Start();
            enemySpeed = 3;

            enemy1.Left = rnd.Next(20, 600);
            enemy2.Left = rnd.Next(20, 600);
            enemy3.Left = rnd.Next(20, 600);
            
            enemy1.Top = rnd.Next(0, 200) * -1;
            enemy2.Top = rnd.Next(100, 700) * -1;
            enemy3.Top = rnd.Next(300, 1000) * -1;



            score = 0; 
            
            bulletSpeed = 0;
            bullet.Left = -300;
            shooting = false;

            txtScore.Text=score.ToString();
        }

        private void GameOver()
        {
            isGameOver = true;
            playTimer.Stop();
            if(score > highscore)
                highscore = score;

            DateTime today = DateTime.Now;

            Savesession(username,highscore,today);

            txtScore.Text += Environment.NewLine + "Game Over!";


        }

        private void Savesession(string username, int highscore, DateTime today)
        {
            var con1 = new SqlConnection(connstr);
            con1.Open();
            string query1 = "UPDATE Session SET [High Score] = @score, Date = @date WHERE Username = @username";
            var cmd1 = new SqlCommand(query1, con1);
            cmd1.Parameters.AddWithValue("@username", username);
            cmd1.Parameters.AddWithValue("@score", highscore);
            cmd1.Parameters.AddWithValue("@date", today);
            cmd1.ExecuteNonQuery();

            con1.Close();
            con1.Dispose();
            cmd1.Dispose();

            GetSessionDetails(ref username,ref highscore,ref today);

            var con = new SqlConnection(connstr);
            con.Open();
            string query = "UPDATE leaderboard SET [High Score] = @score, Date = @date WHERE Username = @username";
            var cmd = new SqlCommand(query, con);
            cmd.Parameters.AddWithValue("@username", username);
            cmd.Parameters.AddWithValue("@score", highscore);
            cmd.Parameters.AddWithValue("@date", today);
            cmd.ExecuteNonQuery();

            con.Close();
            con.Dispose();
            cmd.Dispose();
        }
    }
}
