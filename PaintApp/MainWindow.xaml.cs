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
        private bool isEraser = false;
        private bool isDrawing = false;
        private Point startPoint;
        private Dictionary<Line, Color> linesColors = new Dictionary<Line, Color>(); //przechowywanie koloru

        public MainWindow()
        {
            InitializeComponent();
            UpdateColorPreview();
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





    }
}
