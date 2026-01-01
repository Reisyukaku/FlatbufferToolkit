using System;
using System.Collections.Generic;
using System.Text;

namespace FlatbufferToolkit.UI
{
    public sealed class Settings
    {
        private static Settings? _instance;

        private static readonly object _lock = new();

        public bool ShowTextNumbers = true;
        public bool ShowDataInspector = false;

        public static Settings Instance
        {
            get
            {
                if (_instance == null)
                    lock(_lock) _instance = new Settings();

                return _instance;
            }
        }
    }
}
