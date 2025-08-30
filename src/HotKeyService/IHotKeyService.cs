using System;
using GlobalHotKey;
using System.Windows.Input;

namespace AudioToggle
{
    public interface IHotKeyService : IDisposable
    {
        void RegisterHotKey(HotKey hotKey, ModifierKeys modifiers1 = ModifierKeys.None, ModifierKeys modifiers2 = ModifierKeys.None);
        void RegisterCallback(Action callback);
        void UnregisterKey();
        Key? ConvertToGlobalHotKeyKey(Avalonia.Input.Key avaloniaKey);
        (Key?, ModifierKeys) ParseHotkeyString(string hotkeyString);

        string ConvertToString(HotKey hotKey);
    }
}
