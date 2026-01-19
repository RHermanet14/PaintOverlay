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

        public PreferencesWindow(MainWindow main)
        {
            InitializeComponent();
            Initialize_Bindings();
        }

        private void Initialize_Bindings()
        {
            Canvas_Visibility_Bind.Content = ((Key)Properties.Settings.Default.Canvas_Visibility_Bind).ToString();
            Menu_Visibility_Bind.Content = ((Key)Properties.Settings.Default.Menu_Visibility_Bind).ToString();
            if (Is_Setting_Default("Canvas_Visibility_bind"))
                Canvas_Visibility_Reset.IsEnabled = false;
            if (Is_Setting_Default("Menu_Visibility_bind"))
                Menu_Visibility_Reset.IsEnabled = false;       
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
            
        }

        private void Save_Changes(object sender, RoutedEventArgs e)
        {

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
    }
}
