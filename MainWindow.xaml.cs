using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Ink;

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
            MessageBox.Show("Hi :3");
        }

        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            DrawingCanvas.Strokes.Clear();
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Will copy picture of desktop with ink canvas without top bar into clipboard");
        }

        private void HelpButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Will open new window where keybinds can be changed\nRight Alt to toggle canvas\n Right control to hide top bar");
        }
    }
}