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
        private readonly KeyboardHook hook;
        private bool ReadyToBind = false;
        private System.Windows.Controls.Button? SelectedBind = null;

        #region convert scan code to System.Windows.Input.Key object
        private const uint MAPVK_VSC_TO_VK = 0x01;

        [DllImport("user32.dll")]
        public static extern uint MapVirtualKey(uint uCode, uint uMapType);
        #endregion

        public PreferencesWindow()
        {
            InitializeComponent();
            Initialize_Bindings();
            hook = new KeyboardHook();
            KeyboardHook.KeyboardInput += OnKeyboardInput;
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

        private void OnKeyboardInput(object? sender, KeyboardInputEventArgs k) // KeyboardInputEventArgs k
        {
            if (ReadyToBind && SelectedBind != null)
            {
                // Cannot be left click (won't be because it only tracks keyboard)

                uint virtualKey = MapVirtualKey(k.Key, MAPVK_VSC_TO_VK);
                Key key = KeyInterop.KeyFromVirtualKey((int)virtualKey);

                System.Windows.MessageBox.Show($"{key.ToString()}");

                /*
                switch (SelectedBind.Name)
                {
                    case "Canvas_Visibility_Bind":
                        Properties.Settings.Default.canvas_visibility_bind = k.Key;
                        break;
                    case "Menu_Visibility_Bind":
                        Properties.Settings.Default.menu_visibility_bind = k.Key;
                        break;
                    default:
                        System.Windows.MessageBox.Show("Error: button name not found");
                        ReadyToBind = false;
                        SelectedBind = null;
                        return;
                }
                */

                // SelectedBind.Content = key.ToString();
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
