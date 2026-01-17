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

        public PreferencesWindow()
        {
            InitializeComponent();
            Initialize_Bindings();
        }

        private void Initialize_Bindings()
        {
            Canvas_Visibility_Bind.Content = ((Key)Properties.Settings.Default.Canvas_Visibility_Bind).ToString();
            Menu_Visibility_Bind.Content = ((Key)Properties.Settings.Default.Menu_Visibility_Bind).ToString();
            if (string.Equals(Settings.Default.PropertyValues["canvas_visibility_bind"].SerializedValue, Settings.Default.Properties["canvas_visibility_bind"].DefaultValue))
                Canvas_Visibility_Reset.IsEnabled = false;
            if (string.Equals(Settings.Default.PropertyValues["menu_visibility_bind"].SerializedValue, Settings.Default.Properties["menu_visibility_bind"].DefaultValue))
                Menu_Visibility_Reset.IsEnabled = false;       
        }

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
                                reset.IsEnabled = true;
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
                
                // Update UI and functionality in Main Window
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
            System.Windows.MessageBox.Show("HI");
            // Settings.Default.PropertyValues["MyPropertyName"].SerializedValue = Settings.Default.Properties["MyPropertyName"].DefaultValue;
            // Settings.Default.PropertyValues["MyPropertyName"].Deserialized = false;
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
