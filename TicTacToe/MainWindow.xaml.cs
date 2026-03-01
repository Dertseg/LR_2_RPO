using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;

namespace TicTacToe
{
    public partial class MainWindow : Window
    {
        private char currentPlayer = 'X';
        private char[,] board = new char[3, 3];
        private bool gameActive = true;

        // Улучшенные градиентные кисти для X и O
        private readonly Brush brushX;
        private readonly Brush brushO;

        // Цвета для подсветки победы
        private readonly SolidColorBrush winHighlightColor = new SolidColorBrush(Color.FromRgb(78, 205, 196));
        private readonly SolidColorBrush winGlowColor = new SolidColorBrush(Color.FromRgb(107, 127, 255));

        public MainWindow()
        {
            InitializeComponent();

            // Инициализация градиента для X (Неоново-голубой → Фиолетовый)
            var gradientX = new LinearGradientBrush
            {
                StartPoint = new Point(0, 0),
                EndPoint = new Point(1, 1)
            };
            gradientX.GradientStops.Add(new GradientStop(Color.FromRgb(78, 205, 196), 0.0));   // #4ECDCC
            gradientX.GradientStops.Add(new GradientStop(Color.FromRgb(107, 127, 255), 1.0));  // #6B7FFF
            brushX = gradientX;

            // Инициализация градиента для O (Розовый → Оранжевый)
            var gradientO = new LinearGradientBrush
            {
                StartPoint = new Point(0, 0),
                EndPoint = new Point(1, 1)
            };
            gradientO.GradientStops.Add(new GradientStop(Color.FromRgb(255, 107, 139), 0.0));  // #FF6B8B
            gradientO.GradientStops.Add(new GradientStop(Color.FromRgb(255, 159, 67), 1.0));   // #FF9F43
            brushO = gradientO;

            InitializeBoard();
            StartStatusPulse();
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

        // Перетаскивание окна за любую область
        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        // Закрытие окна
        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        // Анимация пульсации статуса
        private void StartStatusPulse()
        {
            var storyboard = this.FindResource("PulseAnimation") as Storyboard;
            if (storyboard != null)
            {
                Storyboard.SetTarget(storyboard, StatusBorder);
                storyboard.Begin();
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

            // Анимация появления с эффектом
            PlayAppearAnimation(btn);

            if (CheckWin(currentPlayer))
            {
                StatusText.Text = $"🏆 Победил игрок {currentPlayer}!";
                StatusText.Foreground = new SolidColorBrush(Colors.Gold);
                StopStatusPulse();
                gameActive = false;
                HighlightWinningCells(currentPlayer);
                PlayWinAnimation();
            }
            else if (CheckDraw())
            {
                StatusText.Text = "🤝 Ничья!";
                StatusText.Foreground = new SolidColorBrush(Colors.LightGray);
                StopStatusPulse();
                gameActive = false;
            }
            else
            {
                currentPlayer = (currentPlayer == 'X') ? 'O' : 'X';
                UpdateStatusText();
            }
        }

        private void StopStatusPulse()
        {
            var storyboard = this.FindResource("PulseAnimation") as Storyboard;
            if (storyboard != null)
            {
                storyboard.Stop();
                StatusBorder.Opacity = 1.0;
            }
        }

        private void UpdateStatusText()
        {
            StatusText.Text = $"Ход игрока: {currentPlayer}";
            StatusText.Foreground = (currentPlayer == 'X') ? brushX : brushO;
        }

        private void Restart_Click(object sender, RoutedEventArgs e)
        {
            // Сброс логики
            currentPlayer = 'X';
            gameActive = true;
            InitializeBoard();
            UpdateStatusText();
            StartStatusPulse();

            // Полный сброс визуальной части кнопок
            if (GameGrid != null)
            {
                foreach (var child in GameGrid.Children)
                {
                    if (child is Button btn)
                    {
                        btn.Content = "";
                        btn.ClearValue(Button.ForegroundProperty);
                        btn.Background = new SolidColorBrush(Color.FromRgb(37, 37, 66));
                        btn.Opacity = 1.0;
                        btn.RenderTransform = null;
                        btn.ClearValue(Button.EffectProperty);
                    }
                }
            }
        }

        // Улучшенная анимация появления символа
        private void PlayAppearAnimation(Button btn)
        {
            ScaleTransform scaleTransform = new ScaleTransform(0.3, 0.3);
            btn.RenderTransform = scaleTransform;
            btn.RenderTransformOrigin = new Point(0.5, 0.5);

            btn.Opacity = 0;

            // Анимация прозрачности
            var opacityAnim = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(250));

            // Анимация масштаба с эффектом "отскока"
            var scaleAnim = new DoubleAnimation(0.3, 1.0, TimeSpan.FromMilliseconds(400))
            {
                EasingFunction = new BackEase { Amplitude = 0.3, EasingMode = EasingMode.EaseOut }
            };

            // Анимация свечения при появлении
            var glowAnim = new DoubleAnimation(0, 20, TimeSpan.FromMilliseconds(400))
            {
                AutoReverse = true,
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
            };

            btn.BeginAnimation(Button.OpacityProperty, opacityAnim);
            scaleTransform.BeginAnimation(ScaleTransform.ScaleXProperty, scaleAnim);
            scaleTransform.BeginAnimation(ScaleTransform.ScaleYProperty, scaleAnim);
        }

        // Анимация победы (пульсация победных клеток)
        private void PlayWinAnimation()
        {
            var pulseAnim = new DoubleAnimation(1.0, 1.1, TimeSpan.FromMilliseconds(300))
            {
                AutoReverse = true,
                RepeatBehavior = RepeatBehavior.Forever
            };

            // Применяем анимацию ко всем кнопкам с победным фоном
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    string name = $"Btn{i}{j}";
                    if (this.FindName(name) is Button btn && btn.Background == winHighlightColor)
                    {
                        var scale = new ScaleTransform(1, 1);
                        btn.RenderTransform = scale;
                        btn.RenderTransformOrigin = new Point(0.5, 0.5);
                        scale.BeginAnimation(ScaleTransform.ScaleXProperty, pulseAnim);
                        scale.BeginAnimation(ScaleTransform.ScaleYProperty, pulseAnim);
                    }
                }
            }
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
                // Градиент для победной подсветки
                var winGradient = new LinearGradientBrush
                {
                    StartPoint = new Point(0, 0),
                    EndPoint = new Point(1, 1)
                };
                winGradient.GradientStops.Add(new GradientStop(Color.FromRgb(107, 127, 255), 0.0));
                winGradient.GradientStops.Add(new GradientStop(Color.FromRgb(78, 205, 196), 1.0));

                btn.Background = winGradient;

                // Добавляем эффект свечения
                btn.Effect = new DropShadowEffect
                {
                    Color = Color.FromRgb(107, 127, 255),
                    BlurRadius = 25,
                    ShadowDepth = 0,
                    Opacity = 0.8
                };
            }
        }

        private Button GetButton(int row, int col)
        {
            string name = $"Btn{row}{col}";
            return this.FindName(name) as Button;
        }
    }
}