using PaintOverlay.Properties;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Runtime.InteropServices;

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
            if (string.Equals(Settings.Default.PropertyValues["Canvas_Visibility_Bind"].SerializedValue, Settings.Default.Properties["Canvas_Visibility_Bind"].DefaultValue))
                Canvas_Visibility_Reset.IsEnabled = false;
            if (string.Equals(Settings.Default.PropertyValues["Menu_Visibility_Bind"].SerializedValue, Settings.Default.Properties["Menu_Visibility_Bind"].DefaultValue))
                Menu_Visibility_Reset.IsEnabled = false;       
        }

        private void Window_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (ReadyToBind && SelectedBind != null)
            {
                // Cannot be left click (won't be because it only tracks keyboard)
                try
                {
                    if (string.Equals(e.Key.ToString(), "System"))
                    {
                        System.Windows.MessageBox.Show($"{(int)e.SystemKey}");
                        //Properties.Settings.Default[SelectedBind.Name] = (int)e.SystemKey;
                    }
                    else
                    {
                        System.Windows.MessageBox.Show($"{(int)e.Key}");
                        //Properties.Settings.Default[SelectedBind.Name] = (int)e.Key;
                    }
                } catch (Exception ex)
                {
                    System.Windows.MessageBox.Show($"Error: no setting with name {SelectedBind.Name} found. {ex.Message}");
                }



                //System.Windows.MessageBox.Show($"{e.Key}\n {e.Key.ToString()}\n{Properties.Settings.Default[SelectedBind.Name]}\n{(int)e.SystemKey}");

                ReadyToBind = false;
                SelectedBind = null;
                //Properties.Settings.Default.Save();

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
            // key press is handled in keyboard proc
        }
    }
}
