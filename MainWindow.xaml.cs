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
using System.Xml.Linq;

namespace PaintOverlay
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly KeyboardHook hook;
        private enum BrushTypes {Pen, Highlighter, Eraser};
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
            if (Keyboard.IsKeyDown(Key.RightAlt))
            {
                DrawingCanvas.Visibility = DrawingCanvas.Visibility == Visibility.Visible ? Visibility.Hidden : Visibility.Visible;
                this.Activate();
            }
            if (Keyboard.IsKeyDown(Key.RightCtrl) && PaintWindow.IsActive)
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

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            System.Drawing.Rectangle bounds = Screen.GetBounds(System.Drawing.Point.Empty);
            using Bitmap bitmap = new(bounds.Width, bounds.Height);
            using (Graphics g = Graphics.FromImage(bitmap))
            {
                g.CopyFromScreen(System.Drawing.Point.Empty, System.Drawing.Point.Empty, bounds.Size);
            }
            System.Windows.Forms.Clipboard.SetImage(bitmap);
        }

        private void HelpButton_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.MessageBox.Show("Will open new window where keybinds can be changed\nRight Alt to toggle canvas\n Right control to hide top bar");
        }

        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Application.Current.Shutdown();
        }
    }
}