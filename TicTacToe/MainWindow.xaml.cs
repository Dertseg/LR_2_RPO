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
        // === Игровая логика ===
        private char currentPlayer = 'X';
        private char[,] board = new char[3, 3];
        private bool gameActive = true;
        private bool isBotMode = false;

        // === Счётчик побед ===
        private int scorePlayer = 0;
        private int scoreBot = 0;
        private int scoreDraw = 0;

        // === Кастомизация символов ===
        private string symbolX = "X";
        private string symbolO = "O";
        private Brush brushX;
        private Brush brushO;

        // === Цвета для подсветки ===
        private readonly SolidColorBrush winHighlightColor = new SolidColorBrush(Color.FromRgb(78, 205, 196));

        public MainWindow()
        {
            InitializeComponent();
            InitializeBrushes();
            InitializeBoard();
            UpdateScoreDisplay();
            InitializeGameMode();
        }

        // Инициализация кистей для символов
        private void InitializeBrushes()
        {
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
            gradientO.GradientStops.Add(new GradientStop(Color.FromRgb(255, 107, 139), 0.0));
            gradientO.GradientStops.Add(new GradientStop(Color.FromRgb(255, 159, 67), 1.0));
            brushO = gradientO;
        }

        // Инициализация игрового поля
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

        // Инициализация режима игры
        private void InitializeGameMode()
        {
            if (ModeTwoPlayer != null)
            {
                ModeTwoPlayer.IsChecked = true;
            }
            isBotMode = false;
            UpdateScoreLabels();
        }

        // === Обработчики событий интерфейса ===

        // Перетаскивание окна
        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        // Закрытие окна
        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        // Смена режима игры
        private void GameMode_Changed(object sender, RoutedEventArgs e)
        {
            if (ModeWithBot != null)
            {
                isBotMode = ModeWithBot.IsChecked == true;
            }
            else
            {
                isBotMode = false;
            }

            UpdateScoreLabels();

            scorePlayer = 0;
            scoreBot = 0;
            scoreDraw = 0;
            UpdateScoreDisplay();

            Restart_Game();
        }

        // Обновление подписей счётчика в зависимости от режима
        private void UpdateScoreLabels()
        {
            if (LabelPlayer1 != null && LabelPlayer2 != null)
            {
                if (isBotMode)
                {
                    LabelPlayer1.Text = "Игрок";
                    LabelPlayer2.Text = "Бот";
                }
                else
                {
                    LabelPlayer1.Text = "Игрок 1";
                    LabelPlayer2.Text = "Игрок 2";
                }
            }
        }

        // Изменение цвета символа X
        private void ColorX_Changed(object sender, SelectionChangedEventArgs e)
        {
            if (ColorXCombo != null && ColorXCombo.SelectedItem is ComboBoxItem item && item.Tag is string colorHex)
            {
                if (ColorConverter.ConvertFromString(colorHex) is Color color)
                {
                    brushX = new SolidColorBrush(color);
                    UpdateBoardSymbols();
                }
            }
        }

        // Изменение цвета символа O
        private void ColorO_Changed(object sender, SelectionChangedEventArgs e)
        {
            if (ColorOCombo != null && ColorOCombo.SelectedItem is ComboBoxItem item && item.Tag is string colorHex)
            {
                if (ColorConverter.ConvertFromString(colorHex) is Color color)
                {
                    brushO = new SolidColorBrush(color);
                    UpdateBoardSymbols();
                }
            }
        }

        // Изменение символа X
        private void SymbolX_Changed(object sender, TextChangedEventArgs e)
        {
            if (SymbolXInput != null && !string.IsNullOrEmpty(SymbolXInput.Text))
            {
                symbolX = SymbolXInput.Text.ToUpper();
                UpdateBoardSymbols();
            }
        }

        // Изменение символа O
        private void SymbolO_Changed(object sender, TextChangedEventArgs e)
        {
            if (SymbolOInput != null && !string.IsNullOrEmpty(SymbolOInput.Text))
            {
                symbolO = SymbolOInput.Text.ToUpper();
                UpdateBoardSymbols();
            }
        }

        // Обновление символов на поле
        private void UpdateBoardSymbols()
        {
            if (GameGrid != null)
            {
                foreach (var child in GameGrid.Children)
                {
                    if (child is Button btn)
                    {
                        string content = btn.Content?.ToString();
                        if (content == "X")
                        {
                            btn.Content = symbolX;
                            btn.Foreground = brushX;
                        }
                        else if (content == "O")
                        {
                            btn.Content = symbolO;
                            btn.Foreground = brushO;
                        }
                    }
                }
            }
        }

        // Обновление отображения счёта
        private void UpdateScoreDisplay()
        {
            if (ScorePlayerText != null)
                ScorePlayerText.Text = scorePlayer.ToString();
            if (ScoreDrawText != null)
                ScoreDrawText.Text = scoreDraw.ToString();
            if (ScoreBotText != null)
                ScoreBotText.Text = scoreBot.ToString();
        }

        // === Основная игровая логика ===

        // Обработчик клика по клетке
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (!gameActive) return;

            Button btn = sender as Button;
            if (btn == null) return;

            string[] coords = btn.Tag.ToString().Split(',');
            int row = int.Parse(coords[0]);
            int col = int.Parse(coords[1]);

            if (board[row, col] != ' ') return;

            MakeMove(row, col, currentPlayer);
            PlayAppearAnimation(btn);

            if (CheckWin(currentPlayer))
            {
                HandleWin(currentPlayer);
                return;
            }
            else if (CheckDraw())
            {
                HandleDraw();
                return;
            }

            currentPlayer = (currentPlayer == 'X') ? 'O' : 'X';
            UpdateStatusText();

            if (isBotMode && currentPlayer == 'O' && gameActive)
            {
                System.Threading.Thread.Sleep(300);
                Application.Current.Dispatcher.Invoke(() => BotMove());
            }
        }

        // Выполнение хода
        private void MakeMove(int row, int col, char player)
        {
            board[row, col] = player;
            string name = $"Btn{row}{col}";

            if (this.FindName(name) is Button btn)
            {
                btn.Content = (player == 'X') ? symbolX : symbolO;
                btn.Foreground = (player == 'X') ? brushX : brushO;
            }
        }

        // Ход бота
        private void BotMove()
        {
            if (!gameActive) return;

            if (TryFindWinningMove('O')) return;
            if (TryFindWinningMove('X')) return;

            if (board[1, 1] == ' ')
            {
                MakeBotMove(1, 1);
                return;
            }

            int[] corners = { 0, 2 };
            foreach (int i in corners)
            {
                foreach (int j in corners)
                {
                    if (board[i, j] == ' ')
                    {
                        MakeBotMove(i, j);
                        return;
                    }
                }
            }

            FindRandomMove();
        }

        // Попытка найти выигрышный/блокирующий ход
        private bool TryFindWinningMove(char player)
        {
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    if (board[i, j] == ' ')
                    {
                        board[i, j] = player;
                        if (CheckWin(player))
                        {
                            board[i, j] = 'O';
                            MakeBotMove(i, j);
                            return true;
                        }
                        board[i, j] = ' ';
                    }
                }
            }
            return false;
        }

        // Выполнение хода бота
        private void MakeBotMove(int row, int col)
        {
            MakeMove(row, col, 'O');
            string name = $"Btn{row}{col}";
            if (this.FindName(name) is Button btn)
            {
                PlayAppearAnimation(btn);
            }

            if (CheckWin('O'))
            {
                HandleWin('O');
            }
            else if (CheckDraw())
            {
                HandleDraw();
            }
            else
            {
                currentPlayer = 'X';
                UpdateStatusText();
            }
        }

        // Поиск случайного хода
        private void FindRandomMove()
        {
            Random rand = new Random();
            for (int attempt = 0; attempt < 20; attempt++)
            {
                int row = rand.Next(3);
                int col = rand.Next(3);
                if (board[row, col] == ' ')
                {
                    MakeBotMove(row, col);
                    return;
                }
            }
        }

        // Обработка победы
        private void HandleWin(char winner)
        {
            gameActive = false;
            HighlightWinningCells(winner);

            if (winner == 'X')
            {
                scorePlayer++;
                StatusText.Text = $"🏆 Победил {LabelPlayer1.Text}!";
            }
            else
            {
                scoreBot++;
                StatusText.Text = $"🏆 Победил {LabelPlayer2.Text}!";
            }
            UpdateScoreDisplay();
            PlayWinAnimation();
            UpdateActionButton(true);
        }

        // Обработка ничьей
        private void HandleDraw()
        {
            gameActive = false;
            scoreDraw++;
            StatusText.Text = "🤝 Ничья!";
            StatusText.Foreground = new SolidColorBrush(Colors.Gold);
            UpdateScoreDisplay();
            UpdateActionButton(true);
        }

        // Обновление текста статуса
        private void UpdateStatusText()
        {
            if (StatusText == null) return;

            if (currentPlayer == 'X')
            {
                StatusText.Text = $"Ход: {LabelPlayer1.Text} ({symbolX})";
                StatusText.Foreground = brushX;
            }
            else
            {
                StatusText.Text = $"Ход: {LabelPlayer2.Text} ({symbolO})";
                StatusText.Foreground = brushO;
            }
        }

        // Обновление кнопки действия
        private void UpdateActionButton(bool gameEnded)
        {
            if (ActionButton != null)
            {
                if (gameEnded)
                {
                    ActionButton.Content = "▶️ Продолжить";
                    ActionButton.Background = new SolidColorBrush(Color.FromRgb(78, 205, 196));
                }
                else
                {
                    ActionButton.Content = "🔄 Новая игра";
                    ActionButton.Background = new SolidColorBrush(Color.FromRgb(45, 45, 82));
                }
            }
        }

        // === Методы проверки ===

        // Проверка победы
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

        // Проверка ничьей
        private bool CheckDraw()
        {
            for (int i = 0; i < 3; i++)
                for (int j = 0; j < 3; j++)
                    if (board[i, j] == ' ') return false;
            return true;
        }

        // Подсветка победной комбинации
        private void HighlightWinningCells(char player)
        {
            for (int i = 0; i < 3; i++)
            {
                if (board[i, 0] == player && board[i, 1] == player && board[i, 2] == player)
                {
                    SetWinBackground(i, 0); SetWinBackground(i, 1); SetWinBackground(i, 2);
                }
            }
            for (int i = 0; i < 3; i++)
            {
                if (board[0, i] == player && board[1, i] == player && board[2, i] == player)
                {
                    SetWinBackground(0, i); SetWinBackground(1, i); SetWinBackground(2, i);
                }
            }
            if (board[0, 0] == player && board[1, 1] == player && board[2, 2] == player)
            {
                SetWinBackground(0, 0); SetWinBackground(1, 1); SetWinBackground(2, 2);
            }
            if (board[0, 2] == player && board[1, 1] == player && board[2, 0] == player)
            {
                SetWinBackground(0, 2); SetWinBackground(1, 1); SetWinBackground(2, 0);
            }
        }

        // Установка фона для победной клетки
        private void SetWinBackground(int row, int col)
        {
            string name = $"Btn{row}{col}";
            if (this.FindName(name) is Button btn)
            {
                var winGradient = new LinearGradientBrush
                {
                    StartPoint = new Point(0, 0),
                    EndPoint = new Point(1, 1)
                };
                winGradient.GradientStops.Add(new GradientStop(Color.FromRgb(107, 127, 255), 0.0));
                winGradient.GradientStops.Add(new GradientStop(Color.FromRgb(78, 205, 196), 1.0));

                btn.Background = winGradient;
                btn.Effect = new DropShadowEffect
                {
                    Color = Color.FromRgb(107, 127, 255),
                    BlurRadius = 25,
                    ShadowDepth = 0,
                    Opacity = 0.8
                };
            }
        }

        // === Анимации ===

        // Анимация появления символа
        private void PlayAppearAnimation(Button btn)
        {
            if (btn == null) return;

            ScaleTransform scaleTransform = new ScaleTransform(0.3, 0.3);
            btn.RenderTransform = scaleTransform;
            btn.RenderTransformOrigin = new Point(0.5, 0.5);
            btn.Opacity = 0;

            var opacityAnim = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(250));
            var scaleAnim = new DoubleAnimation(0.3, 1.0, TimeSpan.FromMilliseconds(400))
            {
                EasingFunction = new BackEase { Amplitude = 0.3, EasingMode = EasingMode.EaseOut }
            };

            btn.BeginAnimation(Button.OpacityProperty, opacityAnim);
            scaleTransform.BeginAnimation(ScaleTransform.ScaleXProperty, scaleAnim);
            scaleTransform.BeginAnimation(ScaleTransform.ScaleYProperty, scaleAnim);
        }

        // Анимация победы
        private void PlayWinAnimation()
        {
            var pulseAnim = new DoubleAnimation(1.0, 1.1, TimeSpan.FromMilliseconds(300))
            {
                AutoReverse = true,
                RepeatBehavior = RepeatBehavior.Forever
            };

            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    string name = $"Btn{i}{j}";
                    if (this.FindName(name) is Button btn && btn.Background is LinearGradientBrush)
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

        // === Кнопки управления ===

        // Универсальный обработчик кнопки действия
        private void Action_Click(object sender, RoutedEventArgs e)
        {
            Restart_Game();
        }

        // Перезапуск игры (без сброса счёта)
        private void Restart_Game()
        {
            currentPlayer = 'X';
            gameActive = true;
            InitializeBoard();
            UpdateStatusText();
            UpdateActionButton(false);

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

        // Сброс счёта
        private void ResetScore_Click(object sender, RoutedEventArgs e)
        {
            scorePlayer = 0;
            scoreBot = 0;
            scoreDraw = 0;
            UpdateScoreDisplay();
            Restart_Game();
        }

        // Вспомогательный метод
        private Button GetButton(int row, int col)
        {
            string name = $"Btn{row}{col}";
            return this.FindName(name) as Button;
        }
    }
}