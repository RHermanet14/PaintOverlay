using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace PaintOverlay
{
    /// <summary>
    /// Interaction logic for ColorWindow.xaml
    /// </summary>
    public partial class ColorWindow : Window
    {
        private readonly MainWindow _main;
        public ColorWindow(MainWindow main)
        {
            InitializeComponent();
            _main = main;
        }

        private void Color_Selected(object sender, MouseButtonEventArgs e)
        {
            if (e.Source == null) return;
            if (e.Source is System.Windows.Shapes.Rectangle rect)
            {
                _main.Color_Changed(rect);
                string temp = rect.Name;
                temp = temp.Replace('_', ' ');
                ColorName.Text = temp;
            }
                
        }
    }
}
