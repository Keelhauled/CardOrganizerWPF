using System;
using System.Collections.Generic;
using System.IO;
using MessagePack;

namespace CardOrganizerWPF
{
    public class CardDataManager
    {
        private CardData cardData = new CardData();
        private string dataFileName = "CardOrganizerData.bin";
        private bool fileFound = false;

        public List<CardData.CategoryData> GetCategories()
        {
            return new List<CardData.CategoryData>(cardData.categories.Values);
        }

        public CardDataManager(string path)
        {
            LoadData(path);
        }

        public void LoadData(string path)
        {
            string fullPath = Path.Combine(path, dataFileName);
            if(File.Exists(fullPath))
            {
                fileFound = true;
                var data = File.ReadAllBytes(Path.Combine(path, dataFileName));
                cardData = MessagePackSerializer.Deserialize<CardData>(data); 
            }
        }

        public void SaveData(string path)
        {
            if(cardData.categories.Count > 0 || fileFound)
            {
                var data = MessagePackSerializer.Serialize(cardData);
                File.WriteAllBytes(Path.Combine(path, dataFileName), data);
            }
        }

        public void AddImage(Thumbnail thumb, string category)
        {
            if(category != Category.DEFAULT_CATEGORY_NAME)
                cardData.categories[category].cards.Add(Path.GetFileName(thumb.Path));
        }

        public void RemoveImage(Thumbnail thumb, string category)
        {
            if(category != Category.DEFAULT_CATEGORY_NAME)
                cardData.categories[category].cards.Remove(Path.GetFileName(thumb.Path));
        }

        public void AddCategory(string category)
        {
            if(category != Category.DEFAULT_CATEGORY_NAME)
                cardData.categories.Add(category, new CardData.CategoryData(category));
        }

        public void RemoveCategory(string category)
        {
            if(category != Category.DEFAULT_CATEGORY_NAME)
                cardData.categories.Remove(category);
        }
    }

    [MessagePackObject(true)]
    public class CardData
    {
        public Dictionary<string, CategoryData> categories = new Dictionary<string, CategoryData>();

        [MessagePackObject(true)]
        public class CategoryData
        {
            public string categoryName;
            public List<string> cards = new List<string>();

            public CategoryData(string categoryName)
            {
                this.categoryName = categoryName;
            }
        }
    }
}
