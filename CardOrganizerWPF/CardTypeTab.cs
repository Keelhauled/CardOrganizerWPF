using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using DrWPF.Windows.Data;

namespace CardOrganizerWPF
{
    public class CardTypeTab
    {
        public bool IsEnabled { get; set; }
        public string Header { get; set; }
        public string FolderPath { get; set; }
        public int SavedCategory { get; set; }
        public DataTemplate Template { get; set; }
        public ObservableSortedDictionary<string, Category> Categories { get; set; }
        
        private CardDataManager dataManager;
        private TabControl tabControl;
        public MsgObject.Action saveMsg;

        FileSystemWatcher watcher;
        SynchronizationContext context = SynchronizationContext.Current;

        public CardTypeTab(string header, string folderPath, int savedCategory, TabControl tabControl, MsgObject.Action saveMsg)
        {
            IsEnabled = true;
            Header = header;
            FolderPath = folderPath;
            dataManager = new CardDataManager(folderPath);
            Categories = GetCategoriesFromData();
            SavedCategory = savedCategory != -1 ? savedCategory : 0;
            this.tabControl = tabControl;
            this.saveMsg = saveMsg;

            watcher = new FileSystemWatcher(folderPath);
            watcher.Created += FileCreated;
            watcher.EnableRaisingEvents = true;
        }

        public CardTypeTab(string header)
        {
            IsEnabled = false;
            Header = header;
            Categories = new ObservableSortedDictionary<string, Category>();
            SavedCategory = -1;
        }

        void FileCreated(object sender, FileSystemEventArgs e)
        {
            bool fileIsBusy = true;
            while(fileIsBusy)
            {
                try
                {
                    using(var file = File.Open(e.FullPath, FileMode.Open, FileAccess.Read, FileShare.Read)) { }
                    fileIsBusy = false;
                }
                catch(IOException)
                {
                    //The file is still arriving, give it time to finish copying and check again
                    Thread.Sleep(100);
                }
            }

            context.Post((x) => AddImage(e.FullPath), null);
        }

        private ObservableSortedDictionary<string, Category> GetCategoriesFromData()
        {
            var files = Directory.GetFiles(FolderPath, "*.png");
            var sorted = files.Select(x => new KeyValuePair<string, DateTime>(x, File.GetLastWriteTime(x))).ToList();
            sorted.Sort((KeyValuePair<string, DateTime> a, KeyValuePair<string, DateTime> b) => b.Value.CompareTo(a.Value));

            var categories = new SortedDictionary<string, Category>();
            var undefined = new Category(Category.DEFAULT_CATEGORY_NAME, Template);

            foreach(var file in sorted)
            {
                bool found = false;
                var thumb = new Thumbnail(file.Key, file.Value);

                foreach(var category in dataManager.GetCategories())
                {
                    if(found) break;

                    if(!categories.TryGetValue(category.categoryName, out Category currentCategory))
                    {
                        currentCategory = new Category(category.categoryName, Template);
                        categories.Add(currentCategory.Title, currentCategory);
                    }

                    foreach(var card in category.cards)
                    {
                        if(found) break;

                        if(Path.Combine(FolderPath, card) == file.Key)
                        {
                            currentCategory.AddImage(thumb);
                            found = true;
                        }
                    }
                }

                if(!found)
                {
                    undefined.AddImage(thumb);
                }
            }

            categories.Add(Category.DEFAULT_CATEGORY_NAME, undefined);
            return new ObservableSortedDictionary<string, Category>(categories);
        }

        public Category GetSelectedCategory()
        {
            return Categories.Values.ToList()[tabControl.SelectedIndex == -1 ? 0 : tabControl.SelectedIndex];
        }

        public void SaveCardData()
        {
            dataManager?.SaveData(FolderPath);
        }

        public void HandleMessage(MsgObject message)
        {
            AddImage(message.path);
        }

        #region Scrolling Methods
        private ExtScrollViewer _scrollViewer;
        private ExtScrollViewer ScrollViewer
        {
            get
            {
                if(_scrollViewer != null) return _scrollViewer;

                DataTemplate template = tabControl.ContentTemplate;
                if(template.IsSealed)
                {
                    ContentPresenter cp = tabControl.Template.FindName("PART_SelectedContentHost", tabControl) as ContentPresenter;
                    return _scrollViewer = template.FindName("scrollViewer", cp) as ExtScrollViewer;
                }

                return null;
            }
        }

        public void SaveScrollPosition()
        {
            if(ScrollViewer != null)
            {
                var selectedCategory = GetSelectedCategory();
                selectedCategory.SavedScrollPosition = ScrollViewer.VerticalOffset;
                Console.WriteLine($"Scroll position saved: {selectedCategory.Title} = {ScrollViewer.VerticalOffset}");
            }
        }

        public void ScrollToSavePosition()
        {
            ScrollViewer?.ScrollToVerticalOffset(GetSelectedCategory().SavedScrollPosition);
        }

        public void ScrollToBottom()
        {
            ScrollViewer?.ScrollToBottom();
        }

        public void ScrollToTop()
        {
            ScrollViewer?.ScrollToTop();
        }
        #endregion

        #region Category Methods
        public void AddImage(string path)
        {
            var category = GetSelectedCategory();
            var time = File.GetLastWriteTime(path);
            var thumb = new Thumbnail(path, time);
            category.AddImageFirst(thumb);
            dataManager.AddImage(thumb, category.Title);
        }

        public void AddImageFromOutside(string path, bool move)
        {
            if(Path.GetExtension(path) == ".png")
            {
                string newPath = Path.Combine(FolderPath, Path.GetFileName(path));
                bool exists = File.Exists(newPath);

                if(!exists && FindAndMoveThumb(newPath))
                {
                    if(move)
                        File.Move(path, newPath);
                    else
                        File.Copy(path, newPath);
                }
                else if(exists)
                {
                    FindAndMoveThumb(newPath);
                }
                else
                {
                    if(move)
                        File.Move(path, newPath);
                    else
                        File.Copy(path, newPath);
                }
            }
        }

        public void RemoveImage(Thumbnail thumb)
        {
            FileOperationAPIWrapper.Send(thumb.Path);
            var category = GetSelectedCategory();
            category.RemoveImage(thumb);
            dataManager.RemoveImage(thumb, category.Title);
        }

        public bool FindAndMoveThumb(string path)
        {
            foreach(var category in Categories.Values)
            {
                foreach(var image in category.Images)
                {
                    if(image.Path == path)
                    {
                        MoveImageFrom(image, category, GetSelectedCategory());
                        return true;
                    }
                }
            }

            return false;
        }

        public void MoveImageFrom(Thumbnail thumb, Category from, Category to)
        {
            from.RemoveImage(thumb);
            dataManager.RemoveImage(thumb, from.Title);

            to.AddImageFirst(thumb);
            dataManager.AddImage(thumb, to.Title);
        }

        public void MoveImageFrom(Thumbnail thumb, string fromCategory, string toCategory)
        {
            if(Categories.TryGetValue(fromCategory, out Category from))
            {
                from.RemoveImage(thumb);
                dataManager.RemoveImage(thumb, from.Title);

                if(Categories.TryGetValue(toCategory, out Category to))
                {
                    to.AddImageFirst(thumb);
                }
                else
                {
                    var newCategory = new Category(toCategory, Template);
                    newCategory.AddImageFirst(thumb);
                    Categories.Add(toCategory, newCategory);
                    dataManager.AddCategory(toCategory);
                }

                dataManager.AddImage(thumb, toCategory);
            }
        }

        public void MoveImage(Thumbnail thumb, string newCategoryName)
        {
            var category = GetSelectedCategory();
            category.RemoveImage(thumb);
            dataManager.RemoveImage(thumb, category.Title);

            if(Categories.TryGetValue(newCategoryName, out Category value))
            {
                value.AddImageFirst(thumb);
            }
            else
            {
                var newCategory = new Category(newCategoryName, Template);
                newCategory.AddImageFirst(thumb);
                Categories.Add(newCategoryName, newCategory);
                dataManager.AddCategory(newCategoryName);
            }

            dataManager.AddImage(thumb, newCategoryName);
        }

        public void MoveImageAll(string newCategoryName)
        {
            var list = new List<Thumbnail>(GetSelectedCategory().Images);
            foreach(var thumb in list)
                MoveImage(thumb, newCategoryName);
        }

        public void AddCategory(string categoryName)
        {
            if(!Categories.TryGetValue(categoryName, out _))
            {
                Categories.Add(categoryName, new Category(categoryName, Template));
                dataManager.AddCategory(categoryName);
            }
        }

        public void RemoveCategory(string categoryName)
        {
            if(categoryName != Category.DEFAULT_CATEGORY_NAME)
            {
                var list = new List<Thumbnail>(Categories[categoryName].Images);
                foreach(var thumb in list)
                    MoveImage(thumb, Category.DEFAULT_CATEGORY_NAME);

                Categories.Remove(categoryName);
                dataManager.RemoveCategory(categoryName);
            }
        }
        #endregion
    }

    public class Category
    {
        public const string DEFAULT_CATEGORY_NAME = "undefined";
        public string Title { get; set; }
        public double SavedScrollPosition { get; set; }
        public ObservableCollection<Thumbnail> Images { get; set; }
        public DataTemplate Template { get; set; }

        public Category(string title, DataTemplate template)
        {
            Template = template;
            Images = new SortableObservableCollection<Thumbnail>();
            Title = title;
            SavedScrollPosition = 0;
        }

        public void AddImageFirst(Thumbnail thumb)
        {
            Images.Insert(0, thumb);
            Console.WriteLine($"Adding {thumb.Path} to category {Title}");
        }

        public void AddImage(Thumbnail thumb)
        {
            Images.Add(thumb);
            Console.WriteLine($"Adding {thumb.Path} to category {Title}");
        }

        public void RemoveImage(Thumbnail thumb)
        {
            Images.Remove(thumb);
            Console.WriteLine($"Removing {thumb.Path} from category {Title}");
        }

        public void SortImagesByDate()
        {
            
        }
    }

    public class Thumbnail
    {
        public string Path { get; set; }
        public DateTime Date { get; set; }

        public Thumbnail(string path, DateTime date)
        {
            Path = path;
            Date = date;
        }
    }
}
