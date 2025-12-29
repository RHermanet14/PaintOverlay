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


namespace PaintOverlay
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly KeyboardHook hook;
        public MainWindow()
        {
            InitializeComponent();
            hook = new KeyboardHook();
            KeyboardHook.KeyboardInput += OnKeyboardInput;
        }
        private void OnKeyboardInput(object? sender, EventArgs e)
        {
            if (Keyboard.IsKeyDown(Key.Oem3))
            {
                DrawingCanvas.Visibility = DrawingCanvas.Visibility == Visibility.Visible ? Visibility.Hidden : Visibility.Visible;
                this.Activate();
            }
        }
    }
}