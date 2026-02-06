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
        private bool DrawShape = false; // Check if shape brush is selected during left mouse down
        private double ShapeHeight = 100.0, ShapeWidth = 100.0;
        private bool ChangingBrush = false;
        private System.Windows.Media.Color ShapeColor = Colors.Black;

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
            Color = Colors.Transparent,
            Height = 2,
            Width = 2,
        };

        private readonly DrawingAttributes EraserAttributes = new() // Just to independently control the height and width of the eraser
        {
            Color = Colors.White,
            Height = 10,
            Width = 10,
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
            if (Brush == null || ChangingBrush) return;
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
                    ShapeColor = color;
                    break;
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
            ChangingBrush = true; // Prevent Size_Changed from running when text box values are changed
            Shape.IsEnabled = DrawShape = ShapeSettings.IsEnabled = false; // Reset special options when changing from shape type
            DrawingCanvas.EditingMode = InkCanvasEditingMode.Ink; // In case of changing from eraser brush type
            switch ((BrushTypes)Brush.SelectedIndex)
            {
                case BrushTypes.Pen:
                    DrawingCanvas.DefaultDrawingAttributes = PenAttributes;      
                    LengthInput.Text = DrawingCanvas.DefaultDrawingAttributes.Height.ToString();
                    WidthInput.Text = DrawingCanvas.DefaultDrawingAttributes.Width.ToString();
                    ColorPreview.Fill = new SolidColorBrush(DrawingCanvas.DefaultDrawingAttributes.Color);
                    break;
                case BrushTypes.Highlighter:
                    DrawingCanvas.DefaultDrawingAttributes = HighlighterAttributes;
                    LengthInput.Text = DrawingCanvas.DefaultDrawingAttributes.Height.ToString();
                    WidthInput.Text = DrawingCanvas.DefaultDrawingAttributes.Width.ToString();
                    ColorPreview.Fill = new SolidColorBrush(DrawingCanvas.DefaultDrawingAttributes.Color);
                    break;
                case BrushTypes.Shape:
                    DrawingCanvas.DefaultDrawingAttributes = ShapeAttributes;
                    Shape.IsEnabled = ShapeSettings.IsEnabled = DrawShape = true; // Enable shape settings
                    LengthInput.Text = ShapeHeight.ToString(); // Size of shape is different from size of each point that makes up shape
                    WidthInput.Text = ShapeWidth.ToString();
                    ColorPreview.Fill = new SolidColorBrush(ShapeColor); // Use ShapeColor to prevent drawing with pen after drawing shape
                    break;
                case BrushTypes.Eraser:
                    DrawingCanvas.DefaultDrawingAttributes = EraserAttributes;
                    LengthInput.Text = DrawingCanvas.DefaultDrawingAttributes.Height.ToString();
                    WidthInput.Text = DrawingCanvas.DefaultDrawingAttributes.Width.ToString();
                    DrawingCanvas.EditingMode = InkCanvasEditingMode.EraseByPoint; // Set to eraser editing mode
                    DrawingCanvas.EraserShape = new EllipseStylusShape(DrawingCanvas.DefaultDrawingAttributes.Height, DrawingCanvas.DefaultDrawingAttributes.Width);
                    ColorPreview.Fill = System.Windows.Media.Brushes.White;
                    break;
                default: // Same as BrushTypes.Pen
                    DrawingCanvas.DefaultDrawingAttributes = PenAttributes;
                    LengthInput.Text = DrawingCanvas.DefaultDrawingAttributes.Height.ToString();
                    WidthInput.Text = DrawingCanvas.DefaultDrawingAttributes.Width.ToString();                 
                    ColorPreview.Fill = new SolidColorBrush(DrawingCanvas.DefaultDrawingAttributes.Color);
                    break;
            }
            ChangingBrush = false;
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

        private void Thickness_Changed(object sender, TextChangedEventArgs e)
        {
            if ((BrushTypes)Brush.SelectedIndex != BrushTypes.Shape) return;
            if (int.TryParse(ThicknessInput.Text, out int thickness))
            {
                if (thickness > 100) thickness = 100;
                if (thickness < 1) thickness = 1;
                ThicknessInput.Text = thickness.ToString();
                ShapeAttributes.Height = thickness;
                ShapeAttributes.Width = thickness;
            }
        }

        private void Rotation_Changed(object sender, TextChangedEventArgs e)
        {

        }

        private async void DrawingCanvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (DrawShape)
            {
                System.Windows.Point cursorPosition = e.GetPosition(DrawingCanvas);
                StylusPointCollection points = [];
                double x, y;
                Stroke stroke;
                ShapeAttributes.Color = ShapeColor;
                if (IsShapeFilled.IsChecked == true) // Draw filled shapes
                {
                    switch ((ShapeTypes)Shape.SelectedIndex)
                    {
                        case ShapeTypes.Ellipse:
                            ShapeAttributes.StylusTip = StylusTip.Ellipse;
                            break;
                        case ShapeTypes.Rectangle:
                            ShapeAttributes.StylusTip = StylusTip.Rectangle;
                            break;
                        case ShapeTypes.Triangle:
                            ShapeAttributes.StylusTip = StylusTip.Ellipse;
                            break;
                        default: // Draw nothing
                            break;
                    }
                } else
                {
                    switch ((ShapeTypes)Shape.SelectedIndex)
                    {
                        case ShapeTypes.Ellipse: // Still needs to use input params
                            ShapeAttributes.StylusTip = StylusTip.Ellipse;
                            for (int i = 0; i <= 360; i++)
                            {
                                double angle = i * Math.PI / 180;
                                x = cursorPosition.X + (ShapeWidth / 2) * Math.Cos(angle);
                                y = cursorPosition.Y + (ShapeHeight / 2) * Math.Sin(angle);
                                points.Add(new StylusPoint(x, y));
                            }

                            stroke = new(points)
                            {
                                DrawingAttributes = ShapeAttributes.Clone()
                            };
                            DrawingCanvas.Strokes.Add(stroke);
                            break;
                        case ShapeTypes.Rectangle:
                            ShapeAttributes.StylusTip = StylusTip.Rectangle;
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
                                DrawingAttributes = ShapeAttributes.Clone()
                            };
                            DrawingCanvas.Strokes.Add(stroke);
                            break;
                        case ShapeTypes.Triangle:
                            ShapeAttributes.StylusTip = StylusTip.Ellipse;
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
                                DrawingAttributes = ShapeAttributes.Clone()
                            };
                            DrawingCanvas.Strokes.Add(stroke);
                            break;
                        default: // Draw nothing
                            break;
                    }
                }
                ShapeAttributes.Color = Colors.Transparent;
            }
        }
    }
}