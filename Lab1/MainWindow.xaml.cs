using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Lab1
{
    // --- 1. Класс Точки ---
    public class CustomPoint
    {
        public double X { get; set; }
        public double Y { get; set; }

        public CustomPoint(double x, double y)
        {
            X = x;
            Y = y;
        }
    }

    // --- 2. Базовый класс для Фигур ---
    public abstract class BaseShape
    {
        protected Canvas CanvasRef { get; set; }
        public UIElement VisualElement { get; protected set; }

        public BaseShape(Canvas canvas)
        {
            CanvasRef = canvas;
        }

        public void Move(double dx, double dy)
        {
            if (VisualElement != null)
            {
                double currentX = Canvas.GetLeft(VisualElement);
                double currentY = Canvas.GetTop(VisualElement);

                if (double.IsNaN(currentX)) currentX = 0;
                if (double.IsNaN(currentY)) currentY = 0;

                Canvas.SetLeft(VisualElement, currentX + dx);
                Canvas.SetTop(VisualElement, currentY + dy);
            }
        }
    }

    // --- 3. Класс Четырехугольника ---
    public class RectangleShape : BaseShape
    {
        public RectangleShape(Canvas canvas, CustomPoint startPoint, double width, double height, Brush fillBrush)
            : base(canvas)
        {
            var rect = new Rectangle
            {
                Width = width,
                Height = height,
                Fill = fillBrush,
                Stroke = Brushes.Black,
                StrokeThickness = 1
            };

            VisualElement = rect;
            Canvas.SetLeft(VisualElement, startPoint.X);
            Canvas.SetTop(VisualElement, startPoint.Y);
            CanvasRef.Children.Add(VisualElement);
        }
    }

    // --- 4. Класс Треугольника ---
    public class TriangleShape : BaseShape
    {
        public TriangleShape(Canvas canvas, CustomPoint p1, CustomPoint p2, CustomPoint p3, Brush fillBrush)
            : base(canvas)
        {
            var polygon = new Polygon
            {
                Fill = fillBrush,
                Stroke = Brushes.Black,
                StrokeThickness = 1
            };

            polygon.Points.Add(new Point(p1.X, p1.Y));
            polygon.Points.Add(new Point(p2.X, p2.Y));
            polygon.Points.Add(new Point(p3.X, p3.Y));

            VisualElement = polygon;
            Canvas.SetLeft(VisualElement, 0);
            Canvas.SetTop(VisualElement, 0);
            CanvasRef.Children.Add(VisualElement);
        }
    }

    // --- 5. Логика Главного Окна ---
    public partial class MainWindow : Window
    {
        // Список, где мы будем хранить ВСЕ созданные фигуры
        private List<BaseShape> allShapes = new List<BaseShape>();

        private BaseShape selectedShape = null;
        private Point lastMousePosition;
        private Random rand = new Random();

        public MainWindow()
        {
            InitializeComponent();
        }

        // --- Кнопки создания фигур ---
        private void BtnCreateRectangle_Click(object sender, RoutedEventArgs e)
        {
            if (ConvertInputs(out double x, out double y, out double w, out double h))
            {
                var shape = new RectangleShape(MainCanvas, new CustomPoint(x, y), w, h, Brushes.LightBlue);
                allShapes.Add(shape); // Просто добавляем в список
            }
        }

        private void BtnCreateSquare_Click(object sender, RoutedEventArgs e)
        {
            if (ConvertInputs(out double x, out double y, out double w, out _))
            {
                var shape = new RectangleShape(MainCanvas, new CustomPoint(x, y), w, w, Brushes.Cyan);
                allShapes.Add(shape);
            }
        }

        private void BtnCreateTriangle_Click(object sender, RoutedEventArgs e)
        {
            if (ConvertInputs(out double x, out double y, out double w, out _))
            {
                var p1 = new CustomPoint(x, y + w);
                var p2 = new CustomPoint(x + w / 2, y);
                var p3 = new CustomPoint(x + w, y + w);

                var shape = new TriangleShape(MainCanvas, p1, p2, p3, Brushes.LightGreen);
                allShapes.Add(shape);
            }
        }

        private void BtnRandomRectangle_Click(object sender, RoutedEventArgs e)
        {
            double x = rand.Next(10, (int)MainCanvas.ActualWidth - 150);
            double y = rand.Next(10, (int)MainCanvas.ActualHeight - 150);
            double w = rand.Next(40, 200);
            double h = rand.Next(40, 200);

            var brush = new SolidColorBrush(Color.FromRgb((byte)rand.Next(150, 255), (byte)rand.Next(150, 255), (byte)rand.Next(150, 255)));
            var shape = new RectangleShape(MainCanvas, new CustomPoint(x, y), w, h, brush);
            allShapes.Add(shape);
        }

        private void BtnRandomTriangle_Click(object sender, RoutedEventArgs e)
        {
            double cx = rand.Next(100, (int)MainCanvas.ActualWidth - 100);
            double cy = rand.Next(100, (int)MainCanvas.ActualHeight - 100);

            var p1 = new CustomPoint(cx + rand.Next(-50, 50), cy + rand.Next(-50, 0));
            var p2 = new CustomPoint(cx + rand.Next(-100, -20), cy + rand.Next(20, 80));
            var p3 = new CustomPoint(cx + rand.Next(20, 100), cy + rand.Next(20, 80));

            var brush = new SolidColorBrush(Color.FromRgb((byte)rand.Next(150, 255), (byte)rand.Next(150, 255), (byte)rand.Next(150, 255)));
            var shape = new TriangleShape(MainCanvas, p1, p2, p3, brush);
            allShapes.Add(shape);
        }

        // --- Логика Перемещения Мышкой ---
        private void MainCanvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            // Смотрим, на какой визуальный элемент нажал юзер
            if (e.OriginalSource is UIElement clickedVisual)
            {
                // Ищем в нашем списке фигуру, у которой совпадает визуальный элемент
                foreach (var shape in allShapes)
                {
                    if (shape.VisualElement == clickedVisual)
                    {
                        selectedShape = shape;
                        lastMousePosition = e.GetPosition(MainCanvas);
                        Mouse.Capture(clickedVisual);
                        break;
                    }
                }
            }
        }

        private void MainCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (selectedShape != null && e.LeftButton == MouseButtonState.Pressed)
            {
                Point currentMousePosition = e.GetPosition(MainCanvas);

                double dx = currentMousePosition.X - lastMousePosition.X;
                double dy = currentMousePosition.Y - lastMousePosition.Y;

                selectedShape.Move(dx, dy);

                lastMousePosition = currentMousePosition;
            }
        }

        private void MainCanvas_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (selectedShape != null)
            {
                Mouse.Capture(null);
                selectedShape = null;
            }
        }

        private void BtnClear_Click(object sender, RoutedEventArgs e)
        {
            MainCanvas.Children.Clear();
            allShapes.Clear(); // Очищаем список
        }

        // Безопасное чтение циферок из полей ввода
        private bool ConvertInputs(out double x, out double y, out double w, out double h)
        {
            x = 0; y = 0; w = 0; h = 0;

            bool isXOk = double.TryParse(TxtX.Text, out x);
            bool isYOk = double.TryParse(TxtY.Text, out y);
            bool isWOk = double.TryParse(TxtWidth.Text, out w);
            bool isHOk = double.TryParse(TxtHeight.Text, out h);

            return isXOk && isYOk && isWOk && isHOk;
        }
    }
}