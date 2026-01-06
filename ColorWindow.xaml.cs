using System;
using System.Collections.Generic;
using System.Drawing;
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
        private SolidColorBrush? CustomColor;
        private bool SavingCustomColor = false;
        private List<System.Windows.Shapes.Rectangle> Custom_Color_List = [];
        private byte RColor, GColor, BColor;

        public ColorWindow(MainWindow main)
        {
            InitializeComponent();
            _main = main;
            Set_Custom_Colors();
        }

        private void Set_Custom_Colors()
        {
            //Properties.Settings.Default.Reset();
            //Properties.Settings.Default.Save();
            foreach (System.Windows.Shapes.Rectangle rec in CustomColorsGrid.Children)
            {
                Custom_Color_List.Add(rec);
            }
            RInput.Text = GInput.Text = BInput.Text = "255";
            string temp = Properties.Settings.Default.SavedColors;
            System.Windows.MessageBox.Show(Properties.Settings.Default.SavedColors);
            int i = 0, index;
            List<string> values = [];
            while ((index = temp.IndexOf('_')) != -1)
            {
                values.Add(temp.Substring(0, index));
                temp = temp[(index + 1)..];
                i++;
                if (i % 3 == 0)
                {
                    if (byte.TryParse(values[i - 3], out RColor) && byte.TryParse(values[i - 2], out GColor) && byte.TryParse(values[i - 1], out BColor))
                    {
                        System.Windows.Media.Color color = System.Windows.Media.Color.FromArgb(255, RColor, GColor, BColor);
                        SolidColorBrush PresetColor = new(color);
                        Custom_Color_List[(i / 3) - 1].Fill = PresetColor;
                    }
                }
            }
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

        private void Color_Saved(object sender, RoutedEventArgs e)
        {
            if (byte.TryParse(RInput.Text, out RColor) && byte.TryParse(GInput.Text, out  GColor) && byte.TryParse(BInput.Text, out BColor))
            {
                System.Windows.Media.Color color = System.Windows.Media.Color.FromArgb(255, RColor, GColor, BColor);
                CustomColor = new(color);
                SavingCustomColor = true;
            }
        }

        private void Color_Changed(object sender, TextChangedEventArgs e)
        {
            if (sender is System.Windows.Controls.TextBox input)
            {
                if (int.TryParse(input.Text, out int size))
                {
                    if (size > 255)
                    {
                        size = 255;
                        input.Text = size.ToString();
                    }         
                    else if (size < 0)
                    {
                        size = 0;
                        input.Text = size.ToString();
                    }
                }
            }
        }

        private void Custom_Color_Selected(object sender, MouseButtonEventArgs e)
        {
            if (CustomColor == null)
                return;
            if (e.Source is System.Windows.Shapes.Rectangle rect)
            {
                if (SavingCustomColor == true)
                    rect.Fill = CustomColor;
                _main.Color_Changed(rect);
                string temp = rect.Name;
                temp = temp.Replace('_', ' ');
                ColorName.Text = temp;
                try
                {
                    int.TryParse(rect.Name.Substring(rect.Name.IndexOf('_') + 1), out int index); // DANGEROUS!!!
                    Modify_Custom_Color_Presets(index - 1);
                } catch(Exception ex)
                {
                    System.Windows.MessageBox.Show($"You probably didn't use the correct naming convention: {ex.Message}");
                }           
            }
            SavingCustomColor = false;
        }

        private void Modify_Custom_Color_Presets(int index)
        {
            try
            {
                string temp = RColor.ToString() + "_" + GColor.ToString() + "_" + BColor.ToString();
                int start = 0, end = 0;
                int start_index = index * 3;
                int end_index = start_index + 3;
                //System.Windows.MessageBox.Show($"{index}, {start_index}, {end_index}");
                for (int i = 0; i < end_index; i++) // 3 - 6
                {
                    if (start_index > i)
                        start = Properties.Settings.Default.SavedColors.IndexOf('_', start) + 1; 
                    end = Properties.Settings.Default.SavedColors.IndexOf('_', end) + 1; // adds 2 each time
                }
                //System.Windows.MessageBox.Show($"{end}");
                //System.Windows.MessageBox.Show($"Start: {Properties.Settings.Default.SavedColors.Substring(0, start)}\nmiddle: {temp}\nend: {Properties.Settings.Default.SavedColors.Substring(end - 1)}");
                if (start > 0)
                    Properties.Settings.Default.SavedColors = Properties.Settings.Default.SavedColors.Substring(0, start);
                Properties.Settings.Default.SavedColors += temp;
                if(end_index < Custom_Color_List.Count)
                    Properties.Settings.Default.SavedColors += Properties.Settings.Default.SavedColors.Substring(end - 1);

                System.Windows.MessageBox.Show(Properties.Settings.Default.SavedColors);
                Properties.Settings.Default.Save();
            } catch(Exception ex)
            {
                System.Windows.MessageBox.Show($"Not a number?: {ex.Message}");
            }
            
        }
    }
}
