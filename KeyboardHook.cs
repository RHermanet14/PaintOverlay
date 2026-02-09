using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Input;
#pragma warning disable SYSLIB1054

namespace PaintOverlay
{
    public class KeyboardInputEventArgs : EventArgs
    {
        public ushort Key { get; set; }
        public ushort KeyInputType { get; set; }
    }

    internal partial class KeyboardHook : IDisposable
    {
        #region variables
        public static event EventHandler<KeyboardInputEventArgs>? KeyboardInput;
        private const int WH_KEYBOARD_LL = 13;
        private readonly HOOKPROC _keyboardProc;
        private IntPtr _keyboardHookID = IntPtr.Zero;
        public delegate IntPtr HOOKPROC(int code, IntPtr wParam, IntPtr lParam);
        [DllImport("user32.dll")]
        private static extern IntPtr SetWindowsHookEx(int idHook, HOOKPROC lpfn, IntPtr hMod, uint dwThreadId);
        [DllImport("user32.dll")]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);
        [DllImport("user32.dll")]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);
        [DllImport("kernel32.dll")]
        private static extern IntPtr GetModuleHandle(string lpModuleName);
        [StructLayout(LayoutKind.Sequential)]
        private struct KBDLLHOOKSTRUCT
        {
            public uint vkCode;
            public uint scanCode;
            public uint flags;
            public uint time;
            public UIntPtr dwExtraInfo;
        }
        #endregion

        public KeyboardHook()
        {
            _keyboardProc = KeyboardHookCallback;
            _keyboardHookID = SetKeyboardHook(_keyboardProc);
        }

        private static IntPtr SetKeyboardHook(HOOKPROC proc)
        {
            using Process curProcess = Process.GetCurrentProcess();
            using ProcessModule curModule = curProcess.MainModule!;
            IntPtr hook = SetWindowsHookEx(WH_KEYBOARD_LL, proc,
                GetModuleHandle(curModule.ModuleName!), 0);
            if (hook == IntPtr.Zero)
            {
                int error = Marshal.GetLastWin32Error();
                Console.WriteLine($"Failed to install keyboard hook. Error: {error}");
            }
            return hook;
        }

        private static IntPtr KeyboardHookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            try
            {
                if (nCode >= 0)
                {
                    KBDLLHOOKSTRUCT kb = Marshal.PtrToStructure<KBDLLHOOKSTRUCT>(lParam);
                    KeyboardInput?.Invoke(null, new KeyboardInputEventArgs
                    {
                        Key = (ushort)kb.scanCode,
                        KeyInputType = (ushort)wParam,
                    }); // EventArgs.Empty
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Keyboard hook error: {ex.Message}");
            }
            return CallNextHookEx(IntPtr.Zero, nCode, wParam, lParam);
        }

        public void Dispose()
        {         
            if (_keyboardHookID != IntPtr.Zero)
            {
                UnhookWindowsHookEx(_keyboardHookID);
                _keyboardHookID = IntPtr.Zero;
            }
        }
    }
}
