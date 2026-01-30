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
        private int ShapeHeight = 100, ShapeWidth = 100;

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

        private readonly DrawingAttributes ShapeAttributes = new() // Make sure just the shape is drawn, not any pen
        {
            Color = Colors.Black,
            Height = 2,
            Width = 2,
        };

        private readonly DrawingAttributes EraserAttributes = new() // Just to independently control the height and width of the eraser
        {
            Color = Colors.White,
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
            if (int.TryParse(LengthInput.Text, out int length) && int.TryParse(WidthInput.Text, out int width))
            {
                if (length > 100)
                    length = 100;
                else if (length < 1)
                    length = 1;

                if (width > 100)
                    width = 100;
                else if (width < 1)
                    width = 1;
                if (IsSync.IsChecked == true)
                {
                    // Get which dimension was changed
                    if(sender is System.Windows.Controls.TextBox input)
                    {
                        if (input.Name.Equals("WidthInput"))
                        {
                            length = width;
                        } else
                        {
                            width = length;
                        }
                    }
                }
                LengthInput.Text = length.ToString();
                WidthInput.Text = width.ToString();
                switch((BrushTypes)Brush.SelectedIndex)
                {
                    case BrushTypes.Pen:
                        PenAttributes.Height = length;
                        PenAttributes.Width = width;
                        break;
                    case BrushTypes.Highlighter:
                        HighlighterAttributes.Height = length;
                        HighlighterAttributes.Width = width;
                        break;
                    case BrushTypes.Shape:
                        ShapeHeight = length;
                        ShapeWidth = width;
                        break;
                    case BrushTypes.Eraser:
                        DrawingCanvas.EraserShape = new RectangleStylusShape(length, width);
                        break;
                    default:
                        PenAttributes.Height = length;
                        PenAttributes.Width = width;
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
                case BrushTypes.Shape:
                    return; // Do nothing for now, want to keep track of color of rectangle without drawing dot in middle (two separate attributes for shape?)
                case BrushTypes.Eraser:
                    return; // Do nothing
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
                    DrawingCanvas.EraserShape = new EllipseStylusShape(EraserAttributes.Height, EraserAttributes.Width);
                    ColorPreview.Fill = System.Windows.Media.Brushes.White;
                    return;
                case BrushTypes.Shape:
                    brush_attributes = ShapeAttributes;
                    Shape.IsEnabled = DrawShape = true;
                    break;
                default:
                    brush_attributes = PenAttributes;
                    break;
            }       
            DrawingCanvas.DefaultDrawingAttributes = brush_attributes;
            DrawingCanvas.EditingMode = InkCanvasEditingMode.Ink;
            if((BrushTypes)Brush.SelectedIndex == BrushTypes.Shape)
            {
                LengthInput.Text = ShapeHeight.ToString();
                WidthInput.Text = ShapeWidth.ToString();
            } else
            {
                LengthInput.Text = brush_attributes.Height.ToString();
                WidthInput.Text = brush_attributes.Width.ToString();
            }
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

        private async void DrawingCanvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
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
                        x = cursorPosition.X - (ShapeWidth / 2);
                        y = cursorPosition.Y - (ShapeHeight / 2);
                        points.Add(new StylusPoint(x, y)); // Top

                        x = cursorPosition.X + (ShapeWidth / 2);
                        y = cursorPosition.Y - (ShapeHeight / 2);
                        points.Add(new StylusPoint(x, y)); // Right

                        x = cursorPosition.X + (ShapeWidth / 2);
                        y = cursorPosition.Y + (ShapeHeight / 2);
                        points.Add(new StylusPoint(x, y)); // Bottom

                        x = cursorPosition.X - (ShapeWidth / 2);
                        y = cursorPosition.Y + (ShapeHeight / 2);
                        points.Add(new StylusPoint(x, y)); // Left

                        x = cursorPosition.X - (ShapeWidth / 2);
                        y = cursorPosition.Y - (ShapeHeight / 2);
                        points.Add(new StylusPoint(x, y)); // Top again
                        stroke = new(points)
                        {
                            DrawingAttributes = ShapeAttributes
                        };
                        DrawingCanvas.Strokes.Add(stroke);
                        break;
                    case ShapeTypes.Triangle:
                        x = cursorPosition.X;
                        y = cursorPosition.Y - (ShapeHeight / 2);
                        points.Add(new StylusPoint(x, y)); // Top

                        x = cursorPosition.X + (ShapeWidth / 2);
                        y = cursorPosition.Y + (ShapeHeight / 2);
                        points.Add(new StylusPoint(x, y)); // Right

                        x = cursorPosition.X - (ShapeWidth / 2);
                        y = cursorPosition.Y + (ShapeHeight / 2);
                        points.Add(new StylusPoint(x, y)); // Left

                        x = cursorPosition.X;
                        y = cursorPosition.Y - (ShapeHeight / 2);
                        points.Add(new StylusPoint(x, y)); // Top
                        stroke = new(points)
                        {
                            DrawingAttributes = ShapeAttributes
                        };
                        DrawingCanvas.Strokes.Add(stroke);
                        break;
                    default:
                        break;
                }
            }
        }
    }
}