using System;
using System.Windows.Input;
using GlobalHotKey;

namespace AudioToggle
{
    public static class HotKeyService
    {
        private static HotKeyManager manager;
        private static HotKey key;
        private static Action keyCallback;

        public static void RegisterHotKey(HotKey hotKey, ModifierKeys modifiers1 = ModifierKeys.None, ModifierKeys modifiers2 = ModifierKeys.None)
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
        public static void RegisterCallback(Action callback)
        {
            keyCallback = callback;
        }

        public static void UnregisterKey()
        {
            if (manager != null && key != null)
                manager.Unregister(key);
        }

        private static void KeyPressed(object sender, KeyPressedEventArgs e)
        {
            keyCallback?.Invoke();
            System.Diagnostics.Debug.WriteLine("Hot key pressed!");
        }

        public static void Dispose()
        {
            UnregisterKey();
            if (manager != null)
            {
                manager.Dispose();
                manager = null;
            }
        }
    }
}