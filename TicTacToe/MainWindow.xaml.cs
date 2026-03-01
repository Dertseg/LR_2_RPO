using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace TicTacToe
{
    public partial class MainWindow : Window
    {
        private char currentPlayer = 'X';
        private char[,] board = new char[3, 3];
        private bool gameActive = true;

        // Красивые градиентные кисти для X и O
        private readonly Brush brushX;
        private readonly Brush brushO;

        // Цвет для подсветки победы
        private readonly SolidColorBrush winHighlightColor = new SolidColorBrush(Color.FromRgb(78, 205, 196));

        public MainWindow()
        {
            InitializeComponent();

            var gradientX = new LinearGradientBrush
            {
                StartPoint = new Point(0, 0),
                EndPoint = new Point(1, 1)
            };
            gradientX.GradientStops.Add(new GradientStop(Color.FromRgb(78, 205, 196), 0.0));
            gradientX.GradientStops.Add(new GradientStop(Color.FromRgb(107, 127, 255), 1.0));
            brushX = gradientX;

            var gradientO = new LinearGradientBrush
            {
                StartPoint = new Point(0, 0),
                EndPoint = new Point(1, 1)
            };
            gradientO.GradientStops.Add(new GradientStop(Color.FromRgb(255, 107, 107), 0.0));
            gradientO.GradientStops.Add(new GradientStop(Color.FromRgb(255, 159, 67), 1.0));
            brushO = gradientO;

            InitializeBoard();
        }

        private void InitializeBoard()
        {
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    board[i, j] = ' ';
                }
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (!gameActive) return;

            Button btn = sender as Button;
            if (btn == null) return;

            string[] coords = btn.Tag.ToString().Split(',');
            int row = int.Parse(coords[0]);
            int col = int.Parse(coords[1]);

            if (board[row, col] != ' ') return;

            board[row, col] = currentPlayer;
            btn.Content = currentPlayer.ToString();
            btn.Foreground = (currentPlayer == 'X') ? brushX : brushO;

            PlayAppearAnimation(btn);

            if (CheckWin(currentPlayer))
            {
                StatusText.Text = $"Победил игрок {currentPlayer}!";
                gameActive = false;
                HighlightWinningCells(currentPlayer);
            }
            else if (CheckDraw())
            {
                StatusText.Text = "Ничья!";
                gameActive = false;
            }
            else
            {
                currentPlayer = (currentPlayer == 'X') ? 'O' : 'X';
                UpdateStatusText();
            }
        }

        private void UpdateStatusText()
        {
            StatusText.Text = $"Ход игрока: {currentPlayer}";
            StatusText.Foreground = (currentPlayer == 'X') ? brushX : brushO;
        }

        private void Restart_Click(object sender, RoutedEventArgs e)
        {

            currentPlayer = 'X';
            gameActive = true;
            InitializeBoard();
            UpdateStatusText();

            if (GameGrid != null)
            {
                foreach (var child in GameGrid.Children)
                {
                    if (child is Button btn)
                    {
                        btn.Content = "";
                        btn.ClearValue(Button.ForegroundProperty);
                        btn.Background = new SolidColorBrush(Color.FromRgb(43, 43, 64));
                        btn.Opacity = 1.0;

                        btn.RenderTransform = null;
                    }
                }
            }
        }

        private void PlayAppearAnimation(Button btn)
        {

            ScaleTransform scaleTransform = new ScaleTransform(0.5, 0.5);
            btn.RenderTransform = scaleTransform;
            btn.RenderTransformOrigin = new Point(0.5, 0.5);

            btn.Opacity = 0;

            var opacityAnim = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(300));

            var scaleAnim = new DoubleAnimation(0.5, 1.0, TimeSpan.FromMilliseconds(300))
            {
                EasingFunction = new BackEase { EasingMode = EasingMode.EaseOut }
            };

            btn.BeginAnimation(Button.OpacityProperty, opacityAnim);
            scaleTransform.BeginAnimation(ScaleTransform.ScaleXProperty, scaleAnim);
            scaleTransform.BeginAnimation(ScaleTransform.ScaleYProperty, scaleAnim);
        }

        private bool CheckWin(char player)
        {
            for (int i = 0; i < 3; i++)
            {
                if (board[i, 0] == player && board[i, 1] == player && board[i, 2] == player) return true;
                if (board[0, i] == player && board[1, i] == player && board[2, i] == player) return true;
            }

            if (board[0, 0] == player && board[1, 1] == player && board[2, 2] == player) return true;
            if (board[0, 2] == player && board[1, 1] == player && board[2, 0] == player) return true;

            return false;
        }

        private bool CheckDraw()
        {
            for (int i = 0; i < 3; i++)
                for (int j = 0; j < 3; j++)
                    if (board[i, j] == ' ') return false;
            return true;
        }

        private void HighlightWinningCells(char player)
        {
            // Строки
            for (int i = 0; i < 3; i++)
            {
                if (board[i, 0] == player && board[i, 1] == player && board[i, 2] == player)
                {
                    SetWinBackground(i, 0); SetWinBackground(i, 1); SetWinBackground(i, 2);
                }
            }
            // Столбцы
            for (int i = 0; i < 3; i++)
            {
                if (board[0, i] == player && board[1, i] == player && board[2, i] == player)
                {
                    SetWinBackground(0, i); SetWinBackground(1, i); SetWinBackground(2, i);
                }
            }
            // Диагонали
            if (board[0, 0] == player && board[1, 1] == player && board[2, 2] == player)
            {
                SetWinBackground(0, 0); SetWinBackground(1, 1); SetWinBackground(2, 2);
            }
            if (board[0, 2] == player && board[1, 1] == player && board[2, 0] == player)
            {
                SetWinBackground(0, 2); SetWinBackground(1, 1); SetWinBackground(2, 0);
            }
        }

        private void SetWinBackground(int row, int col)
        {
            string name = $"Btn{row}{col}";
            if (this.FindName(name) is Button btn)
            {
                btn.Background = winHighlightColor;
            }
        }

        private Button GetButton(int row, int col)
        {
            string name = $"Btn{row}{col}";
            return this.FindName(name) as Button;
        }
    }
}