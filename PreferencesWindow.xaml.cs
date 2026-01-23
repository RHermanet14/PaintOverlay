using PaintOverlay.Properties;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Runtime.InteropServices;
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
    /// Interaction logic for PreferencesWindow.xaml
    /// </summary>
    public partial class PreferencesWindow : Window
    {
        private bool ReadyToBind = false;
        private System.Windows.Controls.Button? SelectedBind = null;
        private readonly MainWindow _main;

        public PreferencesWindow(MainWindow main)
        {
            InitializeComponent();
            _main = main;
            Initialize_Bindings();
        }

        private void Initialize_Bindings()
        {
            string? temp = null;
            foreach (UIElement element in preferences_grid.Children) // Determined by order of elements in xaml, not position in grid
            {
                if (element is System.Windows.Controls.Button button)
                {
                    switch (Grid.GetColumn(button))
                    {
                        case 1: // Bind button
                            temp = button.Name;
                            button.Content = ((Key)Properties.Settings.Default[temp]);
                            break;
                        case 2: // Reset Button
                            if (!string.IsNullOrEmpty(temp) && Is_Setting_Default(temp))
                                button.IsEnabled = false;
                            break;
                        default:
                            break; // Do nothing
                    }
                }     
            }
        }
        private void test()
        {
            // Enum in which to add attributes (dimensions, fill, stroke, thickness) to either rectangle or ellipse or custom (e.g triangle)
        }

        private static bool Is_Setting_Default(string setting) { return string.Equals(Settings.Default.PropertyValues[setting].SerializedValue, Settings.Default.Properties[setting].DefaultValue); }

        private void Window_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (ReadyToBind && SelectedBind != null)
            {
                Key key;
                if (string.Equals(e.Key.ToString(), "System"))
                    key = e.SystemKey;
                else
                    key = e.Key;
                try
                {
                    Properties.Settings.Default[SelectedBind.Name] = (int)key;
                    SelectedBind.Content = key.ToString();
                    int row = Grid.GetRow(SelectedBind);
                    foreach (UIElement element in preferences_grid.Children)
                    {
                        if (Grid.GetRow(element) == row && Grid.GetColumn(element) == 2)
                        {
                            if (element is System.Windows.Controls.Button reset)
                            {
                                reset.IsEnabled = !Is_Setting_Default(SelectedBind.Name);
                                break;
                            }
                        }
                    }
                } catch (Exception ex)
                {
                    System.Windows.MessageBox.Show($"Error: no setting with name {SelectedBind.Name} found. {ex.Message}");
                }
                ReadyToBind = false;
                SelectedBind = null;
                Properties.Settings.Default.Save();
                // Functionality gets auto updated because it reads binds from settings
            }
        }

        private void Restore_Default(object sender, RoutedEventArgs e)
        {
            string temp = Properties.Settings.Default.SavedColors; // Don't get rid of saved colors!
            Properties.Settings.Default.Reset();
            Properties.Settings.Default.SavedColors = temp;
            Properties.Settings.Default.Save();
            Initialize_Bindings();
        }

        private void Visibility_Reset_Click(object sender, RoutedEventArgs e)
        {
            if (sender is System.Windows.Controls.Button reset_button)
            {
                int row = Grid.GetRow(reset_button);
                foreach (UIElement element in preferences_grid.Children)
                {
                    if (Grid.GetRow(element) == row && Grid.GetColumn(element) == 1 && element is System.Windows.Controls.Button bind_button)
                    {
                        try
                        {
                            if(int.TryParse(Settings.Default.Properties[bind_button.Name].DefaultValue.ToString(), out int default_bind))
                            {
                                Properties.Settings.Default[bind_button.Name] = default_bind;
                                Properties.Settings.Default.Save();
                                reset_button.IsEnabled = false;
                                bind_button.Content = (Key)default_bind;
                                // Functionality gets auto updated because it reads binds from settings
                                break;
                            } else
                            {
                                throw new Exception($"Error: unable to convert {bind_button.Name} default value from string to int.");
                            }
                            
                        } catch(Exception ex)
                        {
                            System.Windows.MessageBox.Show($"{ex.Message}");
                        }
                    }
                }
            }
        }

        private void Visibility_Bind_Click(object sender, RoutedEventArgs e)
        {
            if (sender is System.Windows.Controls.Button button)
            {
                ReadyToBind = true;
                SelectedBind = button;
            }
            // key press is handled in Window_KeyDown
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            _main.CanDraw = true;
        }
    }
}
