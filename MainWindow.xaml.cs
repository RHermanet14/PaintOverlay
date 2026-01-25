using Microsoft.Windows.Themes;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Xml.Linq;

namespace PaintOverlay
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly KeyboardHook hook;
        private enum BrushTypes {Pen, Highlighter, Shape, Eraser };
        private enum ShapeTypes { Ellipse, Rectangle, Triangle }
        public bool CanDraw { get; set; } = true; // Either fix or remove
        private bool DrawShape = false;

        #region Brush Types
        private readonly DrawingAttributes PenAttributes = new()
        {
            Color = Colors.Black,
            Height = 2,
            Width = 2,
        };

        private readonly DrawingAttributes HighlighterAttributes = new()
        {
            Color = Colors.Yellow,
            Height = 10,
            Width = 2,
            IgnorePressure = true,
            IsHighlighter = true,
            StylusTip = StylusTip.Rectangle,
        };

        private readonly DrawingAttributes ShapeAttributes = new()
        {
            Color = Colors.Black,
            Height = 2,
            Width = 2,
        };
        #endregion

        public MainWindow()
        {
            InitializeComponent();
            DrawingCanvas.DefaultDrawingAttributes = PenAttributes;
            hook = new KeyboardHook();
            KeyboardHook.KeyboardInput += OnKeyboardInput;
        }

        private void OnKeyboardInput(object? sender, EventArgs e)
        {
            if (!CanDraw) return;
            if (Keyboard.IsKeyDown((Key)Properties.Settings.Default.Canvas_Visibility_Bind))
            {
                DrawingCanvas.Visibility = DrawingCanvas.Visibility == Visibility.Visible ? Visibility.Hidden : Visibility.Visible;
                this.Activate();
            }
            if (Keyboard.IsKeyDown((Key)Properties.Settings.Default.Menu_Visibility_Bind) && PaintWindow.IsActive)
            {
                DrawingMenu.Visibility = DrawingMenu.Visibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
            }
            if (Keyboard.IsKeyDown(Key.Z) && Keyboard.IsKeyDown(Key.LeftCtrl))
            {
                Undo_Canvas();
            }
        }

        private void Undo_Canvas()
        {
            if (DrawingCanvas.Strokes.Count > 0)
                DrawingCanvas.Strokes.RemoveAt(DrawingCanvas.Strokes.Count - 1);
        }

        private void Size_Changed(object sender, TextChangedEventArgs e)
        {
            if (Brush == null) return;
            if (int.TryParse(SizeInput.Text, out int size))
            {
                if (size > 100)
                    size = 100;
                else if (size < 1)
                    size = 1;
                SizeInput.Text = size.ToString();
                switch((BrushTypes)Brush.SelectedIndex)
                {
                    case BrushTypes.Pen:
                        PenAttributes.Height = size;
                        PenAttributes.Width = size;
                        break;
                    case BrushTypes.Highlighter:
                        HighlighterAttributes.Height = size;
                        HighlighterAttributes.Width = size / 5 > 0 ? size / 5 : 1;
                        break;
                    case BrushTypes.Eraser:
                        DrawingCanvas.EraserShape = new RectangleStylusShape(size, size);
                        break;
                    default:
                        PenAttributes.Height = size;
                        PenAttributes.Width = size;
                        break;
                }
            }
        }

        private void Open_Color_Window(object sender, MouseButtonEventArgs e)
        {
            ColorWindow cw = new(this);
            cw.Show();
        }

        internal void Color_Changed(System.Windows.Shapes.Rectangle rect)
        {
            DrawingCanvas.Visibility = Visibility.Visible;
            System.Windows.Media.Color color = ((SolidColorBrush)rect.Fill).Color;
            switch ((BrushTypes)Brush.SelectedIndex)
            {
                case BrushTypes.Pen:
                    PenAttributes.Color = color;
                    break;
                case BrushTypes.Highlighter:
                    HighlighterAttributes.Color = color;
                    break;
                case BrushTypes.Eraser:
                    return;
                default:
                    PenAttributes.Color = color;
                    break;
            }
            ColorPreview.Fill = rect.Fill;
        }

        private void Brush_Changed(object sender, SelectionChangedEventArgs e)
        {
            if (DrawingCanvas == null)
                return;
            Shape.IsEnabled = DrawShape = false;
            DrawingAttributes brush_attributes;
            switch((BrushTypes)Brush.SelectedIndex)
            {
                case BrushTypes.Pen:
                    brush_attributes = PenAttributes;
                    break;
                case BrushTypes.Highlighter:
                    brush_attributes = HighlighterAttributes;
                    break;
                case BrushTypes.Eraser:
                    DrawingCanvas.EditingMode = InkCanvasEditingMode.EraseByPoint;
                    if (int.TryParse(SizeInput.Text, out int size))
                        DrawingCanvas.EraserShape = new EllipseStylusShape(size,size);
                    else
                        DrawingCanvas.EraserShape = new EllipseStylusShape(5, 5);
                    ColorPreview.Fill = System.Windows.Media.Brushes.White;
                    return;
                case BrushTypes.Shape:
                    Shape.IsEnabled = DrawShape = true;
                    return;
                default:
                    brush_attributes = PenAttributes;
                    break;
            }       
            DrawingCanvas.DefaultDrawingAttributes = brush_attributes;
            DrawingCanvas.EditingMode = InkCanvasEditingMode.Ink;
            SizeInput.Text = brush_attributes.Height.ToString();
            SolidColorBrush preview_color = new(brush_attributes.Color);
            ColorPreview.Fill = preview_color;
        }

        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            DrawingCanvas.Strokes.Clear();
        }

        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            DrawingMenu.Visibility = Visibility.Hidden;
            await Dispatcher.InvokeAsync(() => { }, DispatcherPriority.Render);
            System.Drawing.Rectangle bounds = Screen.GetBounds(System.Drawing.Point.Empty);
            using Bitmap bitmap = new(bounds.Width, bounds.Height);
            using (Graphics g = Graphics.FromImage(bitmap))
            {
                g.CopyFromScreen(System.Drawing.Point.Empty, System.Drawing.Point.Empty, bounds.Size);
            }
            System.Windows.Forms.Clipboard.SetImage(bitmap);
            DrawingMenu.Visibility = Visibility.Visible;
        }

        private void HelpButton_Click(object sender, RoutedEventArgs e)
        {
            PreferencesWindow pw = new(this);
            pw.Show();
        }

        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Application.Current.Shutdown();
        }

        private void DrawingCanvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (DrawShape)
            {
                System.Windows.Point cursorPosition = e.GetPosition(DrawingCanvas);
                StylusPointCollection points = [];
                double x, y;
                Stroke stroke;
                switch ((ShapeTypes)Shape.SelectedIndex)
                {
                    case ShapeTypes.Ellipse:
                        
                        int radiusX = 75;
                        int radiusY = 50;

                        for (int i = 0; i <= 360; i++)
                        {
                            double angle = i * Math.PI / 180;
                            x = cursorPosition.X + radiusX * Math.Cos(angle);
                            y = cursorPosition.Y + radiusY * Math.Sin(angle);
                            points.Add(new StylusPoint(x, y));
                        }

                        stroke = new(points)
                        {
                            DrawingAttributes = ShapeAttributes
                        };
                        DrawingCanvas.Strokes.Add(stroke);
                        break;
                    case ShapeTypes.Rectangle:
                        int length = 100, width = 100;
                        if (length % 2 == 0)
                        {
                            if (width % 2 == 0) // Both even
                            {
                                for (int i = 0; i < length; i++)
                                {
                                    x = cursorPosition.X - (width / 2) + i;
                                    y = cursorPosition.Y - (length / 2);
                                    points.Add(new StylusPoint(x,y)); // Top
                                    y = cursorPosition.Y + (length / 2);
                                    points.Add(new StylusPoint(x, y)); // Bottom
                                }
                            } else // Length even, width odd
                            {

                            }
                        } else
                        {
                            if (width % 2 == 0) // Length odd, width even
                            {

                            } else // Both odd
                            {

                            }
                        }
                        stroke = new(points)
                        {
                            DrawingAttributes = ShapeAttributes
                        };
                        DrawingCanvas.Strokes.Add(stroke);
                        break;
                    case ShapeTypes.Triangle:
                        break;
                    default:
                        break;
                }
                
            }
        }
    }
}