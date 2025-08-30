using System;
using System.Runtime.InteropServices;
using GlobalHotKey;
using System.Windows.Input;

namespace AudioToggle
{
    public class HotKeyServiceAdapter : IHotKeyService
    {
        private readonly IHotKeyService _service;

        public HotKeyServiceAdapter()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                _service = new WindowsHotKeyService();
            }
            else
            {
                throw new PlatformNotSupportedException("HotKeyService is only supported on Windows.");
            }
        }

        public void RegisterHotKey(HotKey hotKey, ModifierKeys modifiers1 = ModifierKeys.None, ModifierKeys modifiers2 = ModifierKeys.None)
        {
            _service.RegisterHotKey(hotKey, modifiers1, modifiers2);
        }

        public void RegisterCallback(Action callback)
        {
            _service.RegisterCallback(callback);
        }

        public void UnregisterKey()
        {
            _service.UnregisterKey();
        }

        public void Dispose()
        {
            _service.Dispose();
        }

        public (Key?, ModifierKeys) ParseHotkeyString(string hotkeyString)
        {
            return _service.ParseHotkeyString(hotkeyString);
        }

        public System.Windows.Input.Key? ConvertToGlobalHotKeyKey(Avalonia.Input.Key avaloniaKey)
        {        
                return _service.ConvertToGlobalHotKeyKey(avaloniaKey);              
        }

        public string ConvertToString(HotKey hotKey)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return ((WindowsHotKeyService)_service).ConvertToString(hotKey);
            }
            else
            {
                throw new PlatformNotSupportedException("ConvertToString is only supported on Windows.");
            }
        }
    }
}
