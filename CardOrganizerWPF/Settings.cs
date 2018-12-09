using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Windows;
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
                    var fileData = JsonConvert.DeserializeObject<Settings>(json);
                    var resourceData = GetResourceData();

                    if(fileData.Version == resourceData.Version)
                    {
                        data = fileData;
                        Console.WriteLine("Loading custom settings.");
                    }
                    else
                    {
                        data = resourceData;
                        Console.WriteLine("Custom settings are for an old version. Loading default settings.");
                    }
                }
                catch(Exception)
                {
                    data = GetResourceData();
                    Console.WriteLine("Failed to deserialize settings data. Loading default settings.");
                }
            }
            else
            {
                data = GetResourceData();
                Console.WriteLine("Loading default settings.");
            }
        }

        static Settings GetResourceData()
        {
            string resourceName = $"{nameof(CardOrganizerWPF)}.{dataFileName}";
            using(var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName))
            {
                using(var reader = new StreamReader(stream))
                {
                    string json = reader.ReadToEnd();
                    return JsonConvert.DeserializeObject<Settings>(json);
                }
            }
        }

        public string Version;
        public string LastProfile;
        public double ScrollSpeed;
        public WindowData Window;
        public Dictionary<string, GameData> Games;

        public class GameData
        {
            public string Name;
            public string Server;
            public string Path;
            public int Tab;
            public CategoryData Category;
            public List<SceneData> SceneList;
            public int SavedScene;
        }

        public class SceneData
        {
            public string Name;
            public Visibility PartialReplaceEnabled;
            public Visibility SpecialLoadEnabled;
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
            public int SavedCat;
            public double ImageMult;
            public double ImageWidth;
            public double ImageHeight;
        }
    }
}
