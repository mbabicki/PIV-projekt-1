
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace PaintApp
{
    public partial class MainWindow : Window

    {

        private bool isEraser;
        private bool isDrawing;
        private bool isDragging;
        private Point startPoint;
        private Dictionary<Line, Color> linesColors = new Dictionary<Line, Color>(); //przechowywanie koloru

        public MainWindow()
        {
            InitializeComponent();
            UpdateColorPreview();

            KeyDown += MainWindow_KeyDown;

        }

        private void ColorSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            UpdateColorPreview();
        }

        private void UpdateColorPreview()
        {
            byte red = Convert.ToByte(RedSlider.Value);
            byte green = Convert.ToByte(GreenSlider.Value);
            byte blue = Convert.ToByte(BlueSlider.Value);

            ColorPreview.Fill = new SolidColorBrush(Color.FromRgb(red, green, blue));

        }



        private void EraserButton_Click(object sender, RoutedEventArgs e)
        {
            isEraser = true;
            EraserButton.IsEnabled = false;
            LineThicknessComboBox.SelectedIndex = -1;
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            canvas.Children.Clear();
            linesColors.Clear();
        }

        private void Canvas_MouseDown(object sender, MouseButtonEventArgs e)
        {

            if (isEraser)
            {
                startPoint = e.GetPosition(canvas);
                isDrawing = true;
            }
            else
            {
                startPoint = e.GetPosition(canvas);
                isDrawing = true;
            }
        }

        private void Canvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (isDrawing)
            {
                if (isEraser)
                {
                    Point endPoint = e.GetPosition(canvas);

                    var eraserWidth = 20;
                    Line eraserLine = new Line
                    {
                        Stroke = Brushes.White,
                        StrokeThickness = eraserWidth,
                        X1 = startPoint.X,
                        Y1 = startPoint.Y,
                        X2 = endPoint.X,
                        Y2 = endPoint.Y
                    };
                    canvas.Children.Add(eraserLine);
                    startPoint = endPoint;
                }
                else
                {
                    Point endPoint = e.GetPosition(canvas);
                    var selectedLineThicknessItem = LineThicknessComboBox.SelectedItem as ComboBoxItem;
                    if (selectedLineThicknessItem != null && selectedLineThicknessItem.Tag != null)
                    {
                        var selectedLineThickness = Convert.ToDouble(selectedLineThicknessItem.Tag);
                        Line line = new Line
                        {
                            Stroke = new SolidColorBrush(GetCurrentColor()),
                            StrokeThickness = selectedLineThickness,
                            X1 = startPoint.X,
                            Y1 = startPoint.Y,
                            X2 = endPoint.X,
                            Y2 = endPoint.Y
                        };
                        canvas.Children.Add(line);
                        startPoint = endPoint;
                    }

                }
            }
        }



        private void Canvas_MouseUp(object sender, MouseButtonEventArgs e)
        {
            isDrawing = false;

            if (isEraser)
            {
                isEraser = false;
                EraserButton.IsEnabled = true;
            }
        }

        private Color GetCurrentColor() //pobranie koloru
        {
            byte red = Convert.ToByte(RedSlider.Value);
            byte green = Convert.ToByte(GreenSlider.Value);
            byte blue = Convert.ToByte(BlueSlider.Value);
            return Color.FromRgb(red, green, blue);
        }


        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            // znajdz wszystkie znaki ktore nie sa cyframi
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text); // odrzucenie niepoprawnych znakkow
        }



        private void SaveAsPng(string filePath)
        {

            RenderTargetBitmap renderBitmap = new RenderTargetBitmap((int)canvas.ActualWidth, (int)canvas.ActualHeight, 96d, 96d, PixelFormats.Pbgra32);
            renderBitmap.Render(canvas);


            PngBitmapEncoder pngEncoder = new PngBitmapEncoder();
            pngEncoder.Frames.Add(BitmapFrame.Create(renderBitmap));


            using (FileStream fileStream = new FileStream(filePath, FileMode.Create))
            {
                pngEncoder.Save(fileStream);
            }
        }

        private void LoadFromPng(string filePath)
        {

            BitmapImage bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.UriSource = new Uri(filePath);
            bitmap.EndInit();


            Image image = new Image();
            image.Source = bitmap;
            canvas.Children.Clear();
            canvas.Children.Add(image);
        }


        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
            dlg.FileName = "canvas_image";
            dlg.DefaultExt = ".png";
            dlg.Filter = "Image Files (*.png)|*.png";


            bool? result = dlg.ShowDialog();


            if (result == true)
            {
                SaveAsPng(dlg.FileName);
            }
        }


        private void LoadButton_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.DefaultExt = ".png";
            dlg.Filter = "Image Files (*.png)|*.png";


            bool? result = dlg.ShowDialog();


            if (result == true)
            {
                LoadFromPng(dlg.FileName);
            }
        }

        private void DrawRectangleButton_Click(object sender, RoutedEventArgs e)
        {
            // Tworzenie nowego prostokąta
            Rectangle rectangle = new Rectangle
            {
                Width = 100,
                Height = 50,
                Stroke = Brushes.Black,
                Fill = new SolidColorBrush(GetCurrentColor())

            };


            Canvas.SetLeft(rectangle, 50);
            Canvas.SetTop(rectangle, 50);
            canvas.Children.Add(rectangle);


            rectangle.MouseLeftButtonDown += Rectangle_MouseLeftButtonDown;
            rectangle.MouseLeftButtonUp += Rectangle_MouseLeftButtonUp;
            rectangle.MouseMove += Rectangle_MouseMove;
        }
        private void Rectangle_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            isDragging = true;
            startPoint = e.GetPosition(canvas);
            var rectangle = sender as Rectangle;
            rectangle.CaptureMouse();
        }

        private void Rectangle_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            isDragging = false;
            var rectangle = sender as Rectangle;
            rectangle.ReleaseMouseCapture();
        }

        private void Rectangle_MouseMove(object sender, MouseEventArgs e)
        {
            if (isDragging)
            {
                var rectangle = sender as Rectangle;
                Point currentPoint = e.GetPosition(canvas);

                double offsetX = currentPoint.X - startPoint.X;
                double offsetY = currentPoint.Y - startPoint.Y;

                double newLeft = Canvas.GetLeft(rectangle) + offsetX;
                double newTop = Canvas.GetTop(rectangle) + offsetY;

                Canvas.SetLeft(rectangle, newLeft);
                Canvas.SetTop(rectangle, newTop);

                startPoint = currentPoint;
            }
        }


        private void DrawTriangleButton_Click(object sender, RoutedEventArgs e)
        {

            Polygon triangle = new Polygon
            {
                Fill = new SolidColorBrush(GetCurrentColor()),
                Stroke = Brushes.Black,
            };


            Point point1 = new Point(100, 50);
            Point point2 = new Point(50, 150);
            Point point3 = new Point(150, 150);
            triangle.Points = new PointCollection { point1, point2, point3 };


            canvas.Children.Add(triangle);


            triangle.MouseLeftButtonDown += Triangle_MouseLeftButtonDown;
            triangle.MouseLeftButtonUp += Triangle_MouseLeftButtonUp;
            triangle.MouseMove += Triangle_MouseMove;
        }

        private void Triangle_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            isDragging = true;
            startPoint = e.GetPosition(canvas);
            var triangle = sender as Polygon;
            triangle.CaptureMouse();
        }

        private void Triangle_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            isDragging = false;
            var triangle = sender as Polygon;
            triangle.ReleaseMouseCapture();
        }

        private void Triangle_MouseMove(object sender, MouseEventArgs e)
        {
            if (isDragging)
            {
                var triangle = sender as Polygon;
                Point currentPoint = e.GetPosition(canvas);

                double offsetX = currentPoint.X - startPoint.X;
                double offsetY = currentPoint.Y - startPoint.Y;

                // Przesuwanie trójkąta poprzez zmianę współrzędnych punktów
                for (int i = 0; i < triangle.Points.Count; i++)
                {
                    Point point = triangle.Points[i];
                    triangle.Points[i] = new Point(point.X + offsetX, point.Y + offsetY);
                }

                startPoint = currentPoint;
            }
        }

        private void DrawSquareButton_Click(object sender, RoutedEventArgs e)
        {
            
            Rectangle squareShape = new Rectangle
            {
                Width = 100,
                Height = 100,
                Fill = new SolidColorBrush(GetCurrentColor()),
                Stroke = Brushes.Black,
            };

            
            Canvas.SetLeft(squareShape, 50);
            Canvas.SetTop(squareShape, 50);

            
            canvas.Children.Add(squareShape);

            
            squareShape.MouseLeftButtonDown += SquareShape_MouseLeftButtonDown;
            squareShape.MouseLeftButtonUp += SquareShape_MouseLeftButtonUp;
            squareShape.MouseMove += SquareShape_MouseMove;
        }

        private void SquareShape_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            isDragging = true;
            startPoint = e.GetPosition(canvas);
            var squareShape = sender as Rectangle;
            squareShape.CaptureMouse();
        }

        private void SquareShape_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            isDragging = false;
            var squareShape = sender as Rectangle;
            squareShape.ReleaseMouseCapture();
        }

        private void SquareShape_MouseMove(object sender, MouseEventArgs e)
        {
            if (isDragging)
            {
                var squareShape = sender as Rectangle;
                Point currentPoint = e.GetPosition(canvas);

                double offsetX = currentPoint.X - startPoint.X;
                double offsetY = currentPoint.Y - startPoint.Y;

                double newLeft = Canvas.GetLeft(squareShape) + offsetX;
                double newTop = Canvas.GetTop(squareShape) + offsetY;

                
                Canvas.SetLeft(squareShape, newLeft);
                Canvas.SetTop(squareShape, newTop);

                startPoint = currentPoint;
            }
        }


        private void DrawCircleButton_Click(object sender, RoutedEventArgs e)
        {
            
            Ellipse circle = new Ellipse
            {
                Width = 100,
                Height = 100,
                Fill = new SolidColorBrush(GetCurrentColor()),
                Stroke = Brushes.Black
            };

            
            Canvas.SetLeft(circle, 50);
            Canvas.SetTop(circle, 50);

            
            canvas.Children.Add(circle);

            
            circle.MouseLeftButtonDown += Circle_MouseLeftButtonDown;
            circle.MouseLeftButtonUp += Circle_MouseLeftButtonUp;
            circle.MouseMove += Circle_MouseMove;
        }


        private void Circle_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            isDragging = true;
            startPoint = e.GetPosition(canvas);
            var circle = sender as Ellipse;
            circle.CaptureMouse();
        }

        private void Circle_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            isDragging = false;
            var circle = sender as Ellipse;
            circle.ReleaseMouseCapture();
        }

        private void Circle_MouseMove(object sender, MouseEventArgs e)
        {
            if (isDragging)
            {
                var circle = sender as Ellipse;
                Point currentPoint = e.GetPosition(canvas);

                double offsetX = currentPoint.X - startPoint.X;
                double offsetY = currentPoint.Y - startPoint.Y;

                Canvas.SetLeft(circle, Canvas.GetLeft(circle) + offsetX);
                Canvas.SetTop(circle, Canvas.GetTop(circle) + offsetY);

                startPoint = currentPoint;
            }
        }

        private void MainWindow_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Home)
            {
                RotateRectangle(5);
                RotateSquare(5);
                RotateTriangle(5);
            }
            else if (e.Key == Key.End)
            {
                RotateRectangle(-5);
                RotateSquare(-5);
                RotateTriangle(-5);
            }

        }



        private void RotateRectangle(double angle)
        {
            
            Rectangle rectangle = canvas.Children.OfType<Rectangle>().FirstOrDefault();

            if (rectangle != null)
            {
                
                double newAngle = (double)rectangle.RenderTransform.GetValue(RotateTransform.AngleProperty) + angle;

                
                RotateTransform rotateTransform = new RotateTransform(newAngle, rectangle.Width / 2, rectangle.Height / 2);

                
                rectangle.RenderTransform = rotateTransform;
            }
        }

        private void RotateSquare(double angle)
        {
            
            Rectangle square = canvas.Children.OfType<Rectangle>().FirstOrDefault();

            if (square != null)
            {
                
                double newAngle = (double)square.RenderTransform.GetValue(RotateTransform.AngleProperty) + angle;

                
                RotateTransform rotateTransform = new RotateTransform(newAngle, square.Width / 2, square.Height / 2);

                
                square.RenderTransform = rotateTransform;
            }
        }



        private void RotateTriangle(double angle)
        {
            
            Polygon triangle = canvas.Children.OfType<Polygon>().FirstOrDefault();

            if (triangle != null)
            {
                
                Point center = new Point((triangle.Points[0].X + triangle.Points[1].X + triangle.Points[2].X) / 3,
                                         (triangle.Points[0].Y + triangle.Points[1].Y + triangle.Points[2].Y) / 3);

                
                double newAngle = (double)triangle.RenderTransform.GetValue(RotateTransform.AngleProperty) + angle;

                
                RotateTransform rotateTransform = new RotateTransform(newAngle, center.X, center.Y);

                
                triangle.RenderTransform = rotateTransform;
            }

        }
    }

}