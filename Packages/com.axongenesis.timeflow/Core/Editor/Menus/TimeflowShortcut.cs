using System;

namespace AxonGenesis
{
    [Serializable]
    public class TimeflowShortcut
    {
        public readonly string Category;
        public readonly string Name;
        public readonly string Path;
        public string Binding;

        public enum ShortcutTypes
        {
            Global,
            View,
            Builtin
        }
        public ShortcutTypes ShortcutType = ShortcutTypes.Global;

        public TimeflowShortcut(string category, string name, string path)
        {
            Category = category;
            Name = name;
            Path = path;
            ShortcutType = TimeflowShortcut.ShortcutTypes.Global;
        }
        public TimeflowShortcut(string category, string name, string path, ShortcutTypes type)
        {
            Category = category;
            Name = name;
            Path = path;
            ShortcutType = type;
        }

        public TimeflowShortcut(string category, string name, string path, string binding, ShortcutTypes type = TimeflowShortcut.ShortcutTypes.Global)
        {
            Category = category;
            Name = name;
            Path = path;
            Binding = binding;
            ShortcutType = type;
        }
    }
}