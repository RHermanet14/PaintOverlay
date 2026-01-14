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

namespace PaintOverlay
{
    /// <summary>
    /// Interaction logic for PreferencesWindow.xaml
    /// </summary>
    public partial class PreferencesWindow : Window
    {
        public PreferencesWindow()
        {
            InitializeComponent();
            Initialize_Bindings();
        }

        private void Initialize_Bindings()
        {
            Canvas_Visibility_Bind.Content = ((Key)Properties.Settings.Default.canvas_visibility_bind).ToString();
            Menu_Visibility_Bind.Content = ((Key)Properties.Settings.Default.menu_visibility_bind).ToString();
            if (string.Equals(Settings.Default.PropertyValues["canvas_visibility_bind"].SerializedValue, Settings.Default.Properties["canvas_visibility_bind"].DefaultValue))
                Canvas_Visibility_Reset.IsEnabled = false;
            if (string.Equals(Settings.Default.PropertyValues["menu_visibility_bind"].SerializedValue, Settings.Default.Properties["menu_visibility_bind"].DefaultValue))
                Menu_Visibility_Reset.IsEnabled = false;       
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
            // Possibly will have to create separate functions for each key bind

            // Get next key press from user
            // Set setting value to input key value
            // Save settings
            // Update UI and functionality in Main Window
        }
    }
}
