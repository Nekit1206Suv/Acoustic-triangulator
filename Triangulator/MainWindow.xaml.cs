#nullable disable
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Triangulator
{
    public struct Station
    {
        public int Id;
        public double X;
        public double Y;
        public string Label;
    }

    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            PerformCalculation();
        }

        private void OnParameterChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (IsLoaded) PerformCalculation();
        }

        private void PerformCalculation()
        {
            double sideLen = SlSideLength.Value;
            double el1 = SlEl1.Value, az1 = SlAz1.Value;
            double el2 = SlEl2.Value, az2 = SlAz2.Value;
            double el3 = SlEl3.Value, az3 = SlAz3.Value;

            // 1. Math Calculation
            var result = TriangulationMath.CalculatePosition(sideLen, el1, az1, el2, az2, el3, az3);

            // 2. Update UI Stats
            UpdateStatsUI(result);

            // 3. Draw Map
            var stations = new List<Station>
            {
                new Station { Id=1, X=0, Y=0, Label="S1" },
                new Station { Id=2, X=sideLen, Y=0, Label="S2" },
                new Station { Id=3, X=sideLen / 2, Y=sideLen * Math.Sqrt(3) / 2, Label="S3" }
            };

            DrawMap(stations, result, sideLen, az1, az2, az3);
        }

        private void UpdateStatsUI(TriangulationResult res)
        {
            if (res.IsValid)
            {
                TxtHeight.Text = res.Height.ToString("F2");
                TxtX.Text = res.X.ToString("F2");
                TxtY.Text = res.Y.ToString("F2");

                TxtStatus.Text = "LOCKED";
                TxtStatus.Foreground = (Brush)FindResource("AccentGreen");
                StatusDot.Fill = (Brush)FindResource("AccentGreen");
            }
            else
            {
                TxtHeight.Text = "---";
                TxtX.Text = "---";
                TxtY.Text = "---";
                TxtStatus.Text = "NO FIX";
                TxtStatus.Foreground = (Brush)FindResource("AccentRed");
                StatusDot.Fill = (Brush)FindResource("AccentRed");
            }
        }

        private void DrawMap(List<Station> stations, TriangulationResult result, double sideLen, double az1, double az2, double az3)
        {
            MapCanvas.Children.Clear();

            // Transform: Center the Triangle
            double scale = 380.0 / (sideLen * 2);
            double offsetX = 300 - (sideLen / 2 * scale);
            double offsetY = 420;

            Func<double, double, Point> ToCanvas = (x, y) =>
                new Point(offsetX + (x * scale), offsetY - (y * scale));

            // 1. Draw Triangle Base (Blueish Gray)
            var poly = new Polygon
            {
                Stroke = new SolidColorBrush(Color.FromRgb(50, 70, 100)),
                StrokeThickness = 2,
                Fill = new SolidColorBrush(Color.FromArgb(20, 59, 130, 246))
            };
            foreach (var s in stations) poly.Points.Add(ToCanvas(s.X, s.Y));
            MapCanvas.Children.Add(poly);

            // 2. Calculate Base Angles for Rays
            double cx = sideLen / 2;
            double cy = sideLen * Math.Sqrt(3) / 6;
            double GetBaseAngle(Station s) => Math.Atan2(cy - s.Y, cx - s.X);

            // 3. Draw Rays (Dashed Blue)
            DrawVector(stations[0], GetBaseAngle(stations[0]) - TriangulationMath.ToRad(az1), ToCanvas);
            DrawVector(stations[1], GetBaseAngle(stations[1]) - TriangulationMath.ToRad(az2), ToCanvas);
            DrawVector(stations[2], GetBaseAngle(stations[2]) - TriangulationMath.ToRad(az3), ToCanvas);

            // 4. Draw Stations (Green Dots)
            foreach (var s in stations)
            {
                var pt = ToCanvas(s.X, s.Y);
                var dot = new Ellipse { Width = 14, Height = 14, Fill = (Brush)FindResource("AccentGreen") };
                Canvas.SetLeft(dot, pt.X - 7); Canvas.SetTop(dot, pt.Y - 7);
                MapCanvas.Children.Add(dot);

                var lbl = new TextBlock { Text = s.Label, Foreground = (Brush)FindResource("AccentGreen"), FontSize = 12, FontWeight = FontWeights.Bold };
                Canvas.SetLeft(lbl, pt.X - 8); Canvas.SetTop(lbl, pt.Y + 10);
                MapCanvas.Children.Add(lbl);
            }

            // 5. Draw Result Target
            if (result.IsValid)
            {
                var resPt = ToCanvas(result.X, result.Y);

                // Red Glow
                var glow = new Ellipse { Width = 30, Height = 30, Fill = new SolidColorBrush(Color.FromArgb(60, 239, 68, 68)) };
                Canvas.SetLeft(glow, resPt.X - 15); Canvas.SetTop(glow, resPt.Y - 15);
                MapCanvas.Children.Add(glow);

                // Red Center Dot
                var dot = new Ellipse { Width = 10, Height = 10, Fill = (Brush)FindResource("AccentRed"), Stroke = Brushes.White, StrokeThickness = 2 };
                Canvas.SetLeft(dot, resPt.X - 5); Canvas.SetTop(dot, resPt.Y - 5);
                MapCanvas.Children.Add(dot);

                // Height Label Tag
                var border = new Border
                {
                    Background = new SolidColorBrush(Color.FromArgb(220, 0, 0, 0)),
                    CornerRadius = new CornerRadius(4),
                    Padding = new Thickness(6, 2, 6, 2)
                };
                var txt = new TextBlock
                {
                    Text = $"H: {result.Height:F2}m",
                    Foreground = Brushes.White,
                    FontSize = 11,
                    FontWeight = FontWeights.Bold
                };
                border.Child = txt;

                Canvas.SetLeft(border, resPt.X + 15);
                Canvas.SetTop(border, resPt.Y - 10);
                MapCanvas.Children.Add(border);
            }
        }

        private void DrawVector(Station s, double mathRadians, Func<double, double, Point> transform)
        {
            var start = transform(s.X, s.Y);
            double length = 1000;

            double endX = s.X + length * Math.Cos(mathRadians);
            double endY = s.Y + length * Math.Sin(mathRadians);

            var end = transform(endX, endY);

            var line = new Line
            {
                X1 = start.X,
                Y1 = start.Y,
                X2 = end.X,
                Y2 = end.Y,
                Stroke = (Brush)FindResource("AccentBlue"),
                StrokeThickness = 1.5,
                StrokeDashArray = new DoubleCollection { 4, 4 } // Dashed line
            };
            MapCanvas.Children.Add(line);
        }
    }
}