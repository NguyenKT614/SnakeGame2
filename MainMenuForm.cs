using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SnakeGame
{
    public partial class MainMenuForm : Form
    {
        public MainMenuForm()
        {
            InitializeComponent();

        }
        private SnakeGame snakeGameForm;

        // Bắt đầu chơi 
        private void PlayButton_Click(object sender, EventArgs e)
        {
            // Ẩn MainMenu form khi ấn Play
            this.Hide();

            if (snakeGameForm != null && !snakeGameForm.IsDisposed)
            {
                snakeGameForm.Close();
            }

            // Tạo một instance mới của form chơi game
            snakeGameForm = new SnakeGame();

            // Gắn sự kiện để hiển thị lại MainMenu form khi trò chơi kết thúc
            snakeGameForm.GameOverEvent += SnakeGameForm_GameOverEvent;

            // Mở form chơi game
            snakeGameForm.Show();
        }

        // Sự kiện xảy ra khi trò chơi kết thúc
        private void SnakeGameForm_GameOverEvent()
        {
            // Hiển thị lại MainMenu form khi trò chơi kết thúc
            this.Show();
        }

        // Tắt game
        private void QuitButton_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }
    }
}
