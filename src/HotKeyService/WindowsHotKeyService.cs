using System;
using System.Collections.Generic;
using System.Windows.Input;
using GlobalHotKey;
// using Avalonia.Input;

namespace AudioToggle
{
    public class WindowsHotKeyService : IHotKeyService
    {
        private HotKeyManager manager;
        private HotKey key;
        private HotKey outputKey;
        private HotKey inputKey;
        private Action keyCallback;
        private Action outputKeyCallback;
        private Action inputKeyCallback;


        public void RegisterHotKey(HotKey hotKey, ModifierKeys modifiers1 = ModifierKeys.None, ModifierKeys modifiers2 = ModifierKeys.None)
        {
            if (manager == null)
            {
                manager = new HotKeyManager();
                manager.KeyPressed += KeyPressed;
            }

            // Unregister existing key if any
            UnregisterKey();

            // Use the hotKey parameter for registration, combining with additional modifiers if provided
            var combinedModifiers = hotKey.Modifiers | modifiers1 | modifiers2;
            key = manager.Register(hotKey.Key, combinedModifiers);
            System.Diagnostics.Debug.WriteLine($"Registered hotkey: {hotKey.Key} with modifiers: {combinedModifiers}");
        }

        public void RegisterOutputHotKey(HotKey hotKey)
        {
            if (manager == null)
            {
                manager = new HotKeyManager();
                manager.KeyPressed += Manager_KeyPressed;
            }

            // Unregister existing output key if any
            UnregisterOutputHotKey();

            outputKey = manager.Register(hotKey.Key, hotKey.Modifiers);
            System.Diagnostics.Debug.WriteLine($"Registered output hotkey: {hotKey.Key} with modifiers: {hotKey.Modifiers}");
        }

        public void RegisterInputHotKey(HotKey hotKey)
        {
            if (manager == null)
            {
                manager = new HotKeyManager();
                manager.KeyPressed += Manager_KeyPressed;
            }

            // Unregister existing input key if any
            UnregisterInputHotKey();

            inputKey = manager.Register(hotKey.Key, hotKey.Modifiers);
            System.Diagnostics.Debug.WriteLine($"Registered input hotkey: {hotKey.Key} with modifiers: {hotKey.Modifiers}");
        }

        public void RegisterCallback(Action callback)
        {
            keyCallback = callback;
        }

        public void RegisterOutputCallback(Action callback)
        {
            outputKeyCallback = callback;
        }

        public void RegisterInputCallback(Action callback)
        {
            inputKeyCallback = callback;
        }

        public void UnregisterKey()
        {
            if (manager != null && key != null)
                manager.Unregister(key);
        }

        public void UnregisterOutputHotKey()
        {
            if (manager != null && outputKey != null)
                manager.Unregister(outputKey);
        }

        public void UnregisterInputHotKey()
        {
            if (manager != null && inputKey != null)
                manager.Unregister(inputKey);
        }

        private void KeyPressed(object sender, KeyPressedEventArgs e)
        {
            keyCallback?.Invoke();
            System.Diagnostics.Debug.WriteLine("Hot key pressed!");
        }

        private void Manager_KeyPressed(object sender, KeyPressedEventArgs e)
        {
            if (outputKey != null && e.HotKey.Key == outputKey.Key && e.HotKey.Modifiers == outputKey.Modifiers)
            {
                outputKeyCallback?.Invoke();
                System.Diagnostics.Debug.WriteLine("Output hot key pressed!");
            }
            else if (inputKey != null && e.HotKey.Key == inputKey.Key && e.HotKey.Modifiers == inputKey.Modifiers)
            {
                inputKeyCallback?.Invoke();
                System.Diagnostics.Debug.WriteLine("Input hot key pressed!");
            }
        }

        private void OutputKeyPressed(object sender, KeyPressedEventArgs e)
        {
            outputKeyCallback?.Invoke();
            System.Diagnostics.Debug.WriteLine("Output hot key pressed!");
        }

        private void InputKeyPressed(object sender, KeyPressedEventArgs e)
        {
            inputKeyCallback?.Invoke();
            System.Diagnostics.Debug.WriteLine("Input hot key pressed!");
        }

        public void Dispose()
        {
            UnregisterKey();
            UnregisterOutputHotKey();
            UnregisterInputHotKey();
            if (manager != null)
            {
                manager.Dispose();
                manager = null;
            }
        }

        public (Key?, ModifierKeys) ParseHotkeyString(string hotkeyString)
        {
            if (string.IsNullOrEmpty(hotkeyString))
                return (null, ModifierKeys.None);

            var parts = hotkeyString.Split('+');
            if (parts.Length == 0)
                return (null, ModifierKeys.None);

            ModifierKeys modifiers = ModifierKeys.None;
            Key? mainKey = null;

            foreach (var part in parts)
            {
                var trimmedPart = part.Trim();
                switch (trimmedPart.ToLower())
                {
                    case "ctrl":
                        modifiers |= ModifierKeys.Control;
                        break;
                    case "alt":
                        modifiers |= ModifierKeys.Alt;
                        break;
                    case "shift":
                        modifiers |= ModifierKeys.Shift;
                        break;
                    case "win":
                        modifiers |= ModifierKeys.Windows;
                        break;
                    default:
                        mainKey = ParseKeyString(trimmedPart);
                        break;
                }
            }

            return (mainKey, modifiers);
        }

        public System.Windows.Input.Key? ConvertToGlobalHotKeyKey(Avalonia.Input.Key avaloniaKey)
        {
            switch (avaloniaKey)
            {
                case Avalonia.Input.Key.F1: return System.Windows.Input.Key.F1;
                case Avalonia.Input.Key.F2: return System.Windows.Input.Key.F2;
                case Avalonia.Input.Key.F3: return System.Windows.Input.Key.F3;
                case Avalonia.Input.Key.F4: return System.Windows.Input.Key.F4;
                case Avalonia.Input.Key.F5: return System.Windows.Input.Key.F5;
                case Avalonia.Input.Key.F6: return System.Windows.Input.Key.F6;
                case Avalonia.Input.Key.F7: return System.Windows.Input.Key.F7;
                case Avalonia.Input.Key.F8: return System.Windows.Input.Key.F8;
                case Avalonia.Input.Key.F9: return System.Windows.Input.Key.F9;
                case Avalonia.Input.Key.F10: return System.Windows.Input.Key.F10;
                case Avalonia.Input.Key.F11: return System.Windows.Input.Key.F11;
                case Avalonia.Input.Key.F12: return System.Windows.Input.Key.F12;
                case Avalonia.Input.Key.A: return System.Windows.Input.Key.A;
                case Avalonia.Input.Key.B: return System.Windows.Input.Key.B;
                case Avalonia.Input.Key.C: return System.Windows.Input.Key.C;
                case Avalonia.Input.Key.D: return System.Windows.Input.Key.D;
                case Avalonia.Input.Key.E: return System.Windows.Input.Key.E;
                case Avalonia.Input.Key.F: return System.Windows.Input.Key.F;
                case Avalonia.Input.Key.G: return System.Windows.Input.Key.G;
                case Avalonia.Input.Key.H: return System.Windows.Input.Key.H;
                case Avalonia.Input.Key.I: return System.Windows.Input.Key.I;
                case Avalonia.Input.Key.J: return System.Windows.Input.Key.J;
                case Avalonia.Input.Key.K: return System.Windows.Input.Key.K;
                case Avalonia.Input.Key.L: return System.Windows.Input.Key.L;
                case Avalonia.Input.Key.M: return System.Windows.Input.Key.M;
                case Avalonia.Input.Key.N: return System.Windows.Input.Key.N;
                case Avalonia.Input.Key.O: return System.Windows.Input.Key.O;
                case Avalonia.Input.Key.P: return System.Windows.Input.Key.P;
                case Avalonia.Input.Key.Q: return System.Windows.Input.Key.Q;
                case Avalonia.Input.Key.R: return System.Windows.Input.Key.R;
                case Avalonia.Input.Key.S: return System.Windows.Input.Key.S;
                case Avalonia.Input.Key.T: return System.Windows.Input.Key.T;
                case Avalonia.Input.Key.U: return System.Windows.Input.Key.U;
                case Avalonia.Input.Key.V: return System.Windows.Input.Key.V;
                case Avalonia.Input.Key.W: return System.Windows.Input.Key.W;
                case Avalonia.Input.Key.X: return System.Windows.Input.Key.X;
                case Avalonia.Input.Key.Y: return System.Windows.Input.Key.Y;
                case Avalonia.Input.Key.Z: return System.Windows.Input.Key.Z;
                case Avalonia.Input.Key.D0: return System.Windows.Input.Key.D0;
                case Avalonia.Input.Key.D1: return System.Windows.Input.Key.D1;
                case Avalonia.Input.Key.D2: return System.Windows.Input.Key.D2;
                case Avalonia.Input.Key.D3: return System.Windows.Input.Key.D3;
                case Avalonia.Input.Key.D4: return System.Windows.Input.Key.D4;
                case Avalonia.Input.Key.D5: return System.Windows.Input.Key.D5;
                case Avalonia.Input.Key.D6: return System.Windows.Input.Key.D6;
                case Avalonia.Input.Key.D7: return System.Windows.Input.Key.D7;
                case Avalonia.Input.Key.D8: return System.Windows.Input.Key.D8;
                case Avalonia.Input.Key.D9: return System.Windows.Input.Key.D9;
                case Avalonia.Input.Key.Space: return System.Windows.Input.Key.Space;
                case Avalonia.Input.Key.Enter: return System.Windows.Input.Key.Enter;
                case Avalonia.Input.Key.Escape: return System.Windows.Input.Key.Escape;
                case Avalonia.Input.Key.Tab: return System.Windows.Input.Key.Tab;
                case Avalonia.Input.Key.Back: return System.Windows.Input.Key.Back;
                case Avalonia.Input.Key.Delete: return System.Windows.Input.Key.Delete;
                case Avalonia.Input.Key.Insert: return System.Windows.Input.Key.Insert;
                case Avalonia.Input.Key.Home: return System.Windows.Input.Key.Home;
                case Avalonia.Input.Key.End: return System.Windows.Input.Key.End;
                case Avalonia.Input.Key.PageUp: return System.Windows.Input.Key.PageUp;
                case Avalonia.Input.Key.PageDown: return System.Windows.Input.Key.PageDown;
                case Avalonia.Input.Key.Up: return System.Windows.Input.Key.Up;
                case Avalonia.Input.Key.Down: return System.Windows.Input.Key.Down;
                case Avalonia.Input.Key.Left: return System.Windows.Input.Key.Left;
                case Avalonia.Input.Key.Right: return System.Windows.Input.Key.Right;
                default: return null;
            }
        }

        private System.Windows.Input.Key? ParseKeyString(string keyString)
        {
            if (string.IsNullOrEmpty(keyString))
                return null;

            // Try to parse as a single character
            if (keyString.Length == 1)
            {
                char c = keyString[0];
                if (char.IsLetter(c))
                {
                    return (System.Windows.Input.Key)Enum.Parse(typeof(System.Windows.Input.Key), c.ToString().ToUpper());
                }
                else if (char.IsDigit(c))
                {
                    return (System.Windows.Input.Key)Enum.Parse(typeof(System.Windows.Input.Key), "D" + c);
                }
            }

            // Try to parse as a function key or special key
            switch (keyString.ToLower())
            {
                case "f1": return System.Windows.Input.Key.F1;
                case "f2": return System.Windows.Input.Key.F2;
                case "f3": return System.Windows.Input.Key.F3;
                case "f4": return System.Windows.Input.Key.F4;
                case "f5": return System.Windows.Input.Key.F5;
                case "f6": return System.Windows.Input.Key.F6;
                case "f7": return System.Windows.Input.Key.F7;
                case "f8": return System.Windows.Input.Key.F8;
                case "f9": return System.Windows.Input.Key.F9;
                case "f10": return System.Windows.Input.Key.F10;
                case "f11": return System.Windows.Input.Key.F11;
                case "f12": return System.Windows.Input.Key.F12;
                case "space": return System.Windows.Input.Key.Space;
                case "enter": return System.Windows.Input.Key.Enter;
                case "escape": return System.Windows.Input.Key.Escape;
                case "tab": return System.Windows.Input.Key.Tab;
                case "back": return System.Windows.Input.Key.Back;
                case "delete": return System.Windows.Input.Key.Delete;
                case "insert": return System.Windows.Input.Key.Insert;
                case "home": return System.Windows.Input.Key.Home;
                case "end": return System.Windows.Input.Key.End;
                case "pageup": return System.Windows.Input.Key.PageUp;
                case "pagedown": return System.Windows.Input.Key.PageDown;
                case "up": return System.Windows.Input.Key.Up;
                case "down": return System.Windows.Input.Key.Down;
                case "left": return System.Windows.Input.Key.Left;
                case "right": return System.Windows.Input.Key.Right;
                default: return null;
            }
        }

        public string ConvertToString(HotKey hotKey)
        {
            var modifiers = new List<string>();
            if ((hotKey.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
                modifiers.Add("Ctrl");
            if ((hotKey.Modifiers & ModifierKeys.Alt) == ModifierKeys.Alt)
                modifiers.Add("Alt");
            if ((hotKey.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift)
                modifiers.Add("Shift");
            if ((hotKey.Modifiers & ModifierKeys.Windows) == ModifierKeys.Windows)
                modifiers.Add("Win");

            string mainKey = hotKey.Key.ToString();

            return modifiers.Count > 0 ? string.Join("+", modifiers) + "+" + mainKey : mainKey;
        }
    }
}