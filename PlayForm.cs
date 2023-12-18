using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;

namespace SnakeGame
{

    public partial class SnakeGame : Form
    {

        // Liên kết database
        private string connStr = "Data Source=DESKTOP-B7G8SLV;Initial Catalog=PLAYER;Integrated Security=True;";

        // Danh sách chứa con rắn
        private List<Circle> Snake = new List<Circle>();

        // Thức ăn
        private Circle food = new Circle();

        // Danh sách vật cản
        List<Obstacle> obstacles = new List<Obstacle>();

        // Các biến
        int maxWidth;
        int maxHeight;

        // Điểm số 
        int score;
        int highscore;

        // Thời gian để tạo lại thức ăn
        // phòng trường hợp thức ăn ở vị trí mà các vật cản tạo thành chữ U
        // sẽ khiến người chơi không ăn được nên ta phải tạo lại
        DateTime lastFoodTime;
        int foodTimeoutSeconds = 10;

        // count for pause
        // if count is even press P will pause
        // if count is odd press P will continue 
        int count = 0;

        Random rand = new Random();

        bool goLeft, goRight, goDown, goUp;

        public SnakeGame()
        {
            InitializeComponent();
            new Settings();
            LoadData();
        }

        // Hàm khởi tạo vật cản
        private void InitializeObstacles()
        {
            // Create random obstacles 
            for (int i = 0; i < 20; i++)
            {
                Obstacle newObstacle;
                do
                {
                    newObstacle = new Obstacle(rand.Next(0, maxWidth), rand.Next(0, maxHeight), 16, 16);
                } while (IsObstacleTooCloseToSnake(newObstacle));

                obstacles.Add(newObstacle);
            }
        }

        // Xem vật cản có gần vị trí bắt đầu lúc start game không
        // (vì nếu gần quá sẽ dẫn đến thua ngay khi vừa vào game)
        private bool IsObstacleTooCloseToSnake(Obstacle obstacle)
        {
            var SnakeHead = Snake[0];
            int distanceX = Math.Abs(SnakeHead.X - obstacle.X);

            // Kiểm tra xem vật cản có quá gần đầu rắn hay không
            if (distanceX < 5)
            {
                return true;
            }
            return false;
        }

        // Load thông tin của database vào
        private void LoadData()
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connStr))
                {
                    connection.Open();

                    string query = "SELECT * FROM PLAYER";
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            // Tạo DataTable để lưu trữ dữ liệu
                            var dataTable = new System.Data.DataTable();
                            dataTable.Load(reader);

                            // Gán DataTable làm nguồn dữ liệu cho DataGridView
                            dataGridView1.DataSource = dataTable;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi: {ex.Message}");
            }
        }

        // Khi nhấn 1 nút
        private void KeyIsDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Left && Settings.directions != "right")
                goLeft = true;
            if (e.KeyCode == Keys.Right && Settings.directions != "left")
                goRight = true;
            if (e.KeyCode == Keys.Up && Settings.directions != "down")
                goUp = true;
            if (e.KeyCode == Keys.Down && Settings.directions != "up")
                goDown = true;

            if (e.KeyCode == Keys.P && count % 2 == 0)
            {
                gameTimer.Stop();
                count++;
            }
            else if (e.KeyCode == Keys.P && count % 2 == 1)
            {
                gameTimer.Start();
                count++;
            }
        }

        // Reset status to false to allow the next press to change to true
        private void KeyIsUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Left)
                goLeft = false;
            if (e.KeyCode == Keys.Right)
                goRight = false;
            if (e.KeyCode == Keys.Up)
                goUp = false;
            if (e.KeyCode == Keys.Down)
                goDown = false;
        }

        // Bắt đầu = Restart
        private void StartGame(object sender, EventArgs e)
        {
            RestartGame();
        }

        // Chụp hình 
        private void TakeSnapShot(object sender, EventArgs e)
        {
            Label caption = new Label();
            caption.Text = "I scored: " + score + " and my Highscore is " + highscore;
            caption.ForeColor = Color.White;
            caption.Font = new Font("Arial", 12, FontStyle.Bold);
            caption.AutoSize = false;
            caption.Width = picCanvas.Width;
            caption.Height = 30;
            caption.TextAlign = ContentAlignment.MiddleCenter;
            picCanvas.Controls.Add(caption);

            SaveFileDialog dialog = new SaveFileDialog();
            dialog.FileName = "Snake Game Snapshot";
            dialog.DefaultExt = "jpg";
            dialog.Filter = "JPG Image File | *.jpg";
            dialog.ValidateNames = true;

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                int width = Convert.ToInt32(picCanvas.Width);
                int height = Convert.ToInt32(picCanvas.Height);
                Bitmap bmp = new Bitmap(width, height);
                picCanvas.DrawToBitmap(bmp, new Rectangle(0, 0, width, height));
                bmp.Save(dialog.FileName, ImageFormat.Jpeg);
                picCanvas.Controls.Remove(caption);
            }

        }

        // Bộ đếm thời gian
        private void GameTimerEvent(object sender, EventArgs e)
        {
            // Adjust snake speed
            // The lower the faster
            gameTimer.Interval = 75;


            // Setting the directions
            if (goLeft)
            {
                Settings.directions = "left";
            }
            if (goRight)
            {
                Settings.directions = "right";
            }
            if (goDown)
            {
                Settings.directions = "down";
            }
            if (goUp)
            {
                Settings.directions = "up";
            }

            // End of directions
            for (int i = Snake.Count - 1; i >= 0; i--)
            {
                // For the head
                if (i == 0)
                {
                    switch (Settings.directions)
                    {
                        case "left":
                            Snake[i].X--;
                            break;
                        case "right":
                            Snake[i].X++;
                            break;
                        case "up":
                            Snake[i].Y--;
                            break;
                        case "down":
                            Snake[i].Y++;
                            break;
                    }


                    // Go to the other side
                    // if hit walls
                    if (Snake[i].X < 0)
                    {
                        Snake[i].X = maxWidth;
                    }
                    if (Snake[i].X > maxWidth)
                    {
                        Snake[i].X = 0;
                    }
                    if (Snake[i].Y < 0)
                    {
                        Snake[i].Y = maxHeight;
                    }
                    if (Snake[i].Y > maxHeight)
                    {
                        Snake[i].Y = 0;
                    }


                    // Kiểm tra va chạm với các đối tượng vật cản
                    foreach (var obstacle in obstacles)
                    {
                        // Trục x chạy từ trái qua phải
                        // Trục y chạy từ trên xuống dưới

                        // Kiểm tra xem đầu rắn có nằm trong vùng diện tích của vật cản không
                        if (Snake[0].Y == obstacle.Y && Snake[0].X == obstacle.X)
                        {
                            // Xử lý khi rắn va chạm với vật cản (ví dụ: kết thúc trò chơi)
                            GameOver();
                        }
                    }

                    // Kiểm tra xem đã đủ thời gian chưa để tạo food mới
                    if ((DateTime.Now - lastFoodTime).TotalSeconds >= foodTimeoutSeconds)
                    {
                        // Create new random food 
                        do
                            food = new Circle { X = rand.Next(2, maxWidth), Y = rand.Next(2, maxHeight) };
                        while (FoodOnBody() || FoodOnObstacle() || (FoodOnBody() && FoodOnObstacle()));

                        // Cập nhật thời gian tạo food lần cuối
                        lastFoodTime = DateTime.Now;
                    }

                    // When snake head meet the food
                    if (Snake[i].X == food.X && Snake[i].Y == food.Y)
                    {
                        EatFood();
                    }

                    // When snake head meet its body
                    for (int j = 1; j < Snake.Count; j++)
                    {
                        if (Snake[0].X == Snake[j].X && Snake[0].Y == Snake[j].Y)
                        {
                            GameOver();
                        }
                    }
                }

                // For the body
                // Make the body follow the head
                else if (i > 0)
                {
                    Snake[i].X = Snake[i - 1].X;
                    Snake[i].Y = Snake[i - 1].Y;
                }
            }

            // Delete the drawn frame to move to next frame
            // make things looks like they're moving
            picCanvas.Invalidate();
        }

        private void UpdatePictureBoxGraphics(object sender, PaintEventArgs e)
        {
            Graphics canvas = e.Graphics;

            Brush snakeColour;

            // Fill color for snake
            for (int i = 0; i < Snake.Count; i++)
            {
                if (i == 0)
                {
                    snakeColour = Brushes.LightSalmon;

                }
                else
                {
                    snakeColour = Brushes.Salmon;
                }

                canvas.FillEllipse(snakeColour, new Rectangle
                    (
                    Snake[i].X * Settings.Width,
                    Snake[i].Y * Settings.Height,
                    Settings.Width, Settings.Height
                    ));
            }

            // Fill color for food
            canvas.FillEllipse(Brushes.White, new Rectangle
            (
            food.X * Settings.Width,
            food.Y * Settings.Height,
            Settings.Width, Settings.Height
            ));

            // Vẽ các đối tượng vật cản
            foreach (var obstacle in obstacles)
            {
                canvas.FillRectangle(Brushes.Gray, new Rectangle
                    (
                    obstacle.X * obstacle.Width,
                    obstacle.Y * obstacle.Height,
                    obstacle.Width, obstacle.Height
                    ));
            }
        }


        // Hàm bắt đầu lại trò chơi
        private void RestartGame()
        {

            maxWidth = picCanvas.Width / Settings.Width - 1;
            maxHeight = picCanvas.Height / Settings.Height - 1;

            // Reset con rắn
            Snake.Clear();

            // Nếu Button enabled thì program sẽ auto focus vào button
            // khi đó thì không dùng các phim trên bàn phím dc nên phải tắt
            startButton.Enabled = false;
            snapButton.Enabled = false;
            score = 0;
            txtScore.Text = "Score: " + score;

            // Adding the head part of the snake to the list
            Circle head = new Circle { X = 10, Y = 5 };
            Snake.Add(head);

            // Khởi tạo vật cản
            InitializeObstacles();

            // Create and add the body part of the snake to the list
            for (int i = 0; i < 10; i++)
            {
                Circle body = new Circle();
                Snake.Add(body);
            }

            // Create random food and prevent it to spawn on the snake body
            do
                food = new Circle { X = rand.Next(2, maxWidth), Y = rand.Next(2, maxHeight) };
            while (FoodOnBody());

            // Đặt thời gian tạo food lần cuối
            lastFoodTime = DateTime.Now;

            // Start timer
            gameTimer.Start();
        }

        // Khi rắn đụng vào thức ăn
        private void EatFood()
        {
            // Tăng điểm lên 1
            score += 1;
            txtScore.Text = "Score: " + score;

            // Tạo 1 đơn vị body cho thân rắn
            Circle body = new Circle
            {
                X = Snake[Snake.Count - 1].X,
                Y = Snake[Snake.Count - 1].Y
            };

            // Thêm phần thân vừa tạo vào List rắn
            Snake.Add(body);

            // Create random food 
            do
                food = new Circle { X = rand.Next(2, maxWidth), Y = rand.Next(2, maxHeight) };
            while (FoodOnBody() || FoodOnObstacle() || (FoodOnBody() && FoodOnObstacle()));
        }

        // Kiểm tra xem food có nằm TRÙNG vị trí của body hay không
        bool FoodOnBody()
        {
            for (int i = 0; i < Snake.Count; i++)
            {
                if (food.X == Snake[i].X && food.Y == Snake[i].Y)
                    return true;
            }
            return false;
        }

        bool FoodOnObstacle()
        {
            for (int i = 0; i < 20; i++)
            {
                if (food.X == obstacles[i].X && food.Y == obstacles[i].Y)
                    return true;
            }
            return false;
        }

        // Sự kiện xảy ra khi trò chơi kết thúc
        public event Action GameOverEvent;

        // Xử lí khi game kết thúc
        private void GameOver()
        {
            gameTimer.Stop();

            // Cho phép nút Start và Chụp
            startButton.Enabled = true;
            snapButton.Enabled = true;

            if (score > highscore)
            {
                highscore = score;
                txtHighScore.Text = "Hight Score: " + Environment.NewLine + highscore;
                txtHighScore.TextAlign = ContentAlignment.MiddleCenter;
            }

            // Gọi sự kiện khi trò chơi kết thúc
            GameOverEvent?.Invoke();

        }

    }
}
