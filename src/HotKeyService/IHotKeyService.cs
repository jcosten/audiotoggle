using System;
using GlobalHotKey;
using System.Windows.Input;

namespace AudioToggle
{
    public interface IHotKeyService : IDisposable
    {
        void RegisterHotKey(HotKey hotKey, ModifierKeys modifiers1 = ModifierKeys.None, ModifierKeys modifiers2 = ModifierKeys.None);
        void RegisterOutputHotKey(HotKey hotKey);
        void RegisterInputHotKey(HotKey hotKey);
        void RegisterCallback(Action callback);
        void RegisterOutputCallback(Action callback);
        void RegisterInputCallback(Action callback);
        void UnregisterKey();
        void UnregisterOutputHotKey();
        void UnregisterInputHotKey();
        Key? ConvertToGlobalHotKeyKey(Avalonia.Input.Key avaloniaKey);
        (Key?, ModifierKeys) ParseHotkeyString(string hotkeyString);

        string ConvertToString(HotKey hotKey);
    }
}
