using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Windows;

using System;


namespace AngryBirdsPro
{
    public partial class MainWindow : Window
    {
        private Point _startPoint;
        private bool _isDragging = false;
        private readonly DispatcherTimer _timer;
        private Vector _velocity;
        private Point _birdPosition;
        private readonly double _gravity = 0.5;
        private const int trajectoryPointCount = 50; // Количество точек траектории
        private readonly Ellipse[] _trajectoryPoints;

        public MainWindow()
        {
            InitializeComponent();
            gameCanvas.MouseDown += GameCanvas_MouseDown;
            gameCanvas.MouseMove += GameCanvas_MouseMove;
            gameCanvas.MouseUp += GameCanvas_MouseUp;
            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromMilliseconds(20);
            _timer.Tick += Timer_Tick;

            // Инициализация точек траектории
            _trajectoryPoints = new Ellipse[trajectoryPointCount];
            for (int i = 0; i < trajectoryPointCount; i++)
            {
                Ellipse point = new Ellipse
                {
                    Width = 5,
                    Height = 5,
                    Fill = Brushes.Gray,
                    Visibility = Visibility.Hidden
                };
                gameCanvas.Children.Add(point);
                _trajectoryPoints[i] = point;
            }
        }

        private void GameCanvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            _startPoint = e.GetPosition(gameCanvas);
            _birdPosition = new Point(Canvas.GetLeft(bird), Canvas.GetTop(bird));
            if (IsPointInCircle(_startPoint, _birdPosition, 70))
            {
                _isDragging = true;
            }
        }

        private void GameCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (_isDragging)
            {
                Point currentPoint = e.GetPosition(gameCanvas);
                Vector dragVector = _startPoint - currentPoint;
                bird.SetValue(Canvas.LeftProperty, _startPoint.X - bird.Width / 2 - dragVector.X);
                bird.SetValue(Canvas.TopProperty, _startPoint.Y - bird.Height / 2 - dragVector.Y);

                // Обновление траектории
                UpdateTrajectory(dragVector);
            }
        }

        private void GameCanvas_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (_isDragging)
            {
                _isDragging = false;
                Point releasePoint = e.GetPosition(gameCanvas);
                Vector launchVector = _startPoint - releasePoint;
                _velocity = launchVector / 10; // Скорость запуска
                _birdPosition = new Point(Canvas.GetLeft(bird), Canvas.GetTop(bird));
                ClearTrajectoryPoints();
                _timer.Start();
            }
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            _birdPosition += _velocity;
            _velocity.Y += _gravity; // Ускорение свободного падения

            bird.SetValue(Canvas.LeftProperty, _birdPosition.X);
            bird.SetValue(Canvas.TopProperty, _birdPosition.Y);

            CheckCollision();
        }

        private void CheckCollision()
        {
            Rect birdRect = new Rect(_birdPosition.X, _birdPosition.Y, bird.Width / 1.5, bird.Height / 1.5);
            Rect pigRect = new Rect(Canvas.GetLeft(pig), Canvas.GetTop(pig), pig.Width / 1.5, pig.Height / 1.5);
            Rect grass1Rect = new Rect(Canvas.GetLeft(grass1), Canvas.GetTop(grass1), grass1.Width, grass1.Height);
            Rect grass2Rect = new Rect(Canvas.GetLeft(grass2), Canvas.GetTop(grass2), grass2.Width, grass2.Height);
            Rect barnRect = new Rect(Canvas.GetLeft(barn), Canvas.GetTop(barn), barn.Width, barn.Height);
            Rect sunRect = new Rect(Canvas.GetLeft(sun), Canvas.GetTop(sun), sun.Width / 1.5, sun.Height / 1.5);


            if (birdRect.IntersectsWith(pigRect))
            {
                _timer.Stop();
                MessageBox.Show("Вы попали в свинью!");
                ResetGame();
            }
            if ((birdRect.IntersectsWith(grass1Rect)) || (birdRect.IntersectsWith(grass2Rect)))
            {
                _timer.Stop();
                MessageBox.Show("Вы упали(");
                ResetGame();
            }
            if ((birdRect.IntersectsWith(barnRect)) || (birdRect.IntersectsWith(sunRect)))
            {
                ChangeFlightDirection();
            }

            if (_birdPosition.Y > gameCanvas.ActualHeight || _birdPosition.X > gameCanvas.ActualWidth)
            {
                _timer.Stop();
                ResetGame();
            }
        }

        private void ResetGame()
        {
            bird.SetValue(Canvas.LeftProperty, 100.0);
            bird.SetValue(Canvas.TopProperty, 300.0);
        }

        private void UpdateTrajectory(Vector dragVector)
        {
            Point start = new Point(Canvas.GetLeft(bird) + bird.Width / 2, Canvas.GetTop(bird) + bird.Height / 2);
            Vector velocity = dragVector / 10;
            for (int i = 0; i < trajectoryPointCount; i++)
            {
                double t = i * 4;
                double x = start.X + velocity.X * t;
                double y = start.Y + velocity.Y * t + 0.5 * _gravity * t * t;
                _trajectoryPoints[i].SetValue(Canvas.LeftProperty, x - _trajectoryPoints[i].Width / 2);
                _trajectoryPoints[i].SetValue(Canvas.TopProperty, y - _trajectoryPoints[i].Height / 2);
                _trajectoryPoints[i].Visibility = Visibility.Visible;
            }
        }
        private void ChangeFlightDirection()
        {
            _velocity.Y = -_velocity.Y * 0.9;
        }

        private bool IsPointInCircle(Point point, Point circleCenter, double radius)
        {
            double dx = point.X - circleCenter.X;
            double dy = point.Y - circleCenter.Y;
            return (dx * dx + dy * dy) <= (radius * radius);
        }


        private void ClearTrajectoryPoints()
        {
            foreach (var point in _trajectoryPoints)
            {
                point.Visibility = Visibility.Hidden;
            }
        }
    }
}

