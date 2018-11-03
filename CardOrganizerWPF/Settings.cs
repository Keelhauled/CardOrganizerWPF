using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Newtonsoft.Json;

namespace CardOrganizerWPF
{
    public class Settings
    {
        public static Settings data;
        static string dataPath;
        const string dataFileName = "SettingsData.json";

        public static void Save()
        {
            if(!string.IsNullOrWhiteSpace(dataPath))
            {
                var json = JsonConvert.SerializeObject(data, Formatting.Indented);
                File.WriteAllText(dataPath, json);
            }
        }

        public static void LoadData()
        {
            var ass = Assembly.GetExecutingAssembly();
            dataPath = Path.Combine(Path.GetDirectoryName(ass.Location), dataFileName);

            if(File.Exists(dataPath))
            {
                try
                {
                    var json = File.ReadAllText(dataPath);
                    data = JsonConvert.DeserializeObject<Settings>(json);
                    Console.WriteLine("Loading custom settings.");
                }
                catch(Exception)
                {
                    Console.WriteLine("Failed to deserialize settings data. Loading default settings.");
                    LoadResourceData();
                }
            }
            else
            {
                Console.WriteLine("Loading default settings.");
                LoadResourceData();
            }
        }

        static void LoadResourceData()
        {
            string resourceName = $"{nameof(CardOrganizerWPF)}.{dataFileName}";
            using(var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName))
            {
                using(var reader = new StreamReader(stream))
                {
                    string json = reader.ReadToEnd();
                    data = JsonConvert.DeserializeObject<Settings>(json);
                }
            }
        }

        public WindowData Window;
        public Dictionary<string, GameData> Games;
        public double ScrollSpeed;

        public class GameData
        {
            public string Server;
            public string Path;
            public int Tab;
            public CategoryData Category;
        }

        public class WindowData
        {
            public double Top;
            public double Left;
            public double Height;
            public double Width;
            public bool Maximized;
        }

        public class CategoryData
        {
            public Category Scene;
            public Category Chara1;
            public Category Chara2;
            public Category Outfit1;
            public Category Outfit2;
        }

        public class Category
        {
            public string Header;
            public string Path;
            public int Save;
        }
    }
}
