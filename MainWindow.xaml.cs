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
            if (int.TryParse(SizeInput.Text, out int size))
            {
                if (size > 100)
                    size = 100;
                else if (size < 1)
                    size = 1;
                SizeInput.Text = size.ToString();
                PenAttributes.Height = size;
                PenAttributes.Width = size;
            }
        }

        private void Color_Changed(object sender, MouseButtonEventArgs e)
        {
            System.Windows.MessageBox.Show("Hi :3");
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