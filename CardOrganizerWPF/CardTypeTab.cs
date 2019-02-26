using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using CardOrganizerWPF.Controls;
using CardOrganizerWPF.Utils;
using DrWPF.Windows.Data;

namespace CardOrganizerWPF
{
    public class CardTypeTab
    {
        public Prop<Visibility> Enabled { get; set; } = new Prop<Visibility>();
        public Prop<string> Header { get; set; } = new Prop<string>("");
        public Prop<int> SavedCategory { get; set; } = new Prop<int>(-1);
        public ObservableSortedDictionary<string, Category> Categories { get; set; }
        public Prop<double> ProgressMax { get; set; } = new Prop<double>(1);
        public Prop<double> ProgressVal { get; set; } = new Prop<double>(0);

        public Prop<double> ImageWidth { get; set; } = new Prop<double>();
        public Prop<double> ImageHeight { get; set; } = new Prop<double>();
        public double ImageMultiplier { get; set; }

        private string folderPath;
        private CardDataManager dataManager;
        private TabControl tabControl;
        private MsgObject.Action saveMsg;
        private FileSystemWatcher watcher;
        private SynchronizationContext uiContext;
        private Settings.Category catData;
        private bool initialized = false;
        private volatile bool stopThread = false;

        public CardTypeTab(TabControl tabControl, MsgObject.Action saveMsg)
        {
            uiContext = SynchronizationContext.Current;
            watcher = new FileSystemWatcher();
            Categories = new ObservableSortedDictionary<string, Category>();
            Enabled.Value = Visibility.Collapsed;
            this.tabControl = tabControl;
            this.saveMsg = saveMsg;
        }

        public void SetGame(Settings.GameData gameData, Settings.Category catData)
        {
            initialized = false;
            ScrollViewer = null;
            watcher.EnableRaisingEvents = false;
            watcher.Created -= FileCreated;
            Categories.Clear();
            ProgressVal.Value = 0;
            dataManager = null;
            folderPath = "";

            if(string.IsNullOrWhiteSpace(catData.Header))
            {
                Header.Value = "Disabled";
                Enabled.Value = Visibility.Collapsed;
                initialized = true;
                return;
            }
            
            this.catData = catData;
            Enabled.Value = Visibility.Visible;
            Header.Value = catData.Header;
            folderPath = Path.Combine(gameData.Path, catData.Path);
            dataManager = new CardDataManager(folderPath);
            SetImageSize(catData.ImageMult);

            GetCategoriesFromData(() =>
            {
                initialized = true;
                ProgressVal.Value = 0;
                SavedCategory.Value = catData.SavedCat != -1 ? catData.SavedCat : 0;
                
                watcher.Path = folderPath;
                watcher.Created += FileCreated;
                watcher.EnableRaisingEvents = true;
            });
        }

        public List<string> FindDuplicatesInData()
        {
            var list = new List<string>();

            foreach(var category in Categories.Values)
            {
                foreach(var image in category.Images)
                {
                    list.Add(image.Path);
                }
            }

            var duplicateKeys = list.GroupBy(x => x)
                        .Where(group => group.Count() > 1)
                        .Select(group => group.Key);

            return duplicateKeys.ToList();
        }

        void FileCreated(object sender, FileSystemEventArgs e)
        {
            string ext = Path.GetExtension(e.Name).ToLower();
            if(ext != ".png") return;

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
                    Console.WriteLine("File is still being written to, retrying.");
                    Thread.Sleep(100);
                }
            }

            uiContext.Post((x) => AddImage(e.FullPath), null);
        }

        public void StopThread()
        {
            if(!initialized)
                stopThread = true;
        }

        private void GetCategoriesFromData(Action callback)
        {
            var thread = new Thread(() =>
            {
                var files = Directory.GetFiles(folderPath, "*.png");
                var sorted = files.Select(x => new KeyValuePair<string, FileInfo>(x, new FileInfo(x))).ToList();
                sorted.Sort((KeyValuePair<string, FileInfo> a, KeyValuePair<string, FileInfo> b) => b.Value.LastWriteTime.CompareTo(a.Value.LastWriteTime));

                if(files.Length > 0) uiContext.Post((x) => ProgressMax.Value = files.Length, null);

                var newCategories = new Dictionary<string, Category>();
                var undefined = new Category(Category.DEFAULT_CATEGORY_NAME);
                newCategories.Add(Category.DEFAULT_CATEGORY_NAME, undefined);
                var dataCategories = dataManager.GetCategories();

                int count = 0;
                foreach(var file in sorted)
                {
                    if(stopThread) break;

                    bool found = false;
                    var thumb = new Thumbnail(file.Key, file.Value.LastWriteTime, file.Value.Length);

                    foreach(var category in dataCategories)
                    {
                        if(found) break;

                        if(!newCategories.TryGetValue(category.categoryName, out Category currentCategory))
                        {
                            currentCategory = new Category(category.categoryName);
                            newCategories.Add(currentCategory.Title, currentCategory);
                        }

                        foreach(var card in category.cards)
                        {
                            if(found) break;

                            if(Path.Combine(folderPath, card) == file.Key)
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

                    ++count;
                    uiContext.Post((x) => ProgressVal.Value = count, null);
                }

                if(!stopThread)
                {
                    uiContext.Post((x) =>
                    {
                        foreach(var cat in newCategories)
                        {
                            Categories.Add(cat.Key, cat.Value);
                        }

                        callback();
                    }, null); 
                }

                stopThread = false;
            });

            thread.IsBackground = true;
            thread.Start();
        }

        public Category GetSelectedCategory()
        {
            if(tabControl.SelectedIndex != -1)
                return Categories.ElementAt(tabControl.SelectedIndex).Value;

            return null;
        }

        public void SaveCardData()
        {
            if(Enabled.Value == Visibility.Visible)
            {
                if(initialized)
                {
                    dataManager?.SaveData(folderPath);
                }
            }
        }

        public void SaveSettingsData()
        {
            if(Enabled.Value == Visibility.Visible)
            {
                catData.ImageMult = ImageMultiplier;

                if(initialized)
                {
                    catData.SavedCat = tabControl.SelectedIndex;
                }
            }
        }

        public void SaveCard(string id)
        {
            ServerPipe.SendMessage(MsgObject.Create(saveMsg, id, folderPath));
        }

        public void SetImageSize(double multiplier)
        {
            ImageMultiplier = multiplier;
            ImageWidth.Value = catData.ImageWidth * multiplier;
            ImageHeight.Value = catData.ImageHeight * multiplier;
        }

        #region Scrolling Methods
        private ExtScrollViewer _scrollViewer;
        private ExtScrollViewer ScrollViewer
        {
            get
            {
                if(_scrollViewer != null) return _scrollViewer;

                try
                {
                    DataTemplate template = tabControl.ContentTemplate;
                    if(template.IsSealed)
                    {
                        ContentPresenter cp = tabControl.Template.FindName("PART_SelectedContentHost", tabControl) as ContentPresenter;
                        return _scrollViewer = template.FindName("scrollViewer", cp) as ExtScrollViewer;
                    }
                }
                catch(InvalidOperationException)
                {
                    //Console.WriteLine("Can't find scrollViewer");
                }

                return null;
            }

            set { _scrollViewer = null; }
        }

        public void SaveScrollPosition()
        {
            if(ScrollViewer != null)
            {
                var selectedCategory = GetSelectedCategory();
                if(selectedCategory != null)
                {
                    selectedCategory.SavedScrollPosition = ScrollViewer.VerticalOffset;
                    //Console.WriteLine($"Scroll position saved: {selectedCategory.Title} = {ScrollViewer.VerticalOffset}"); 
                }
            }
        }

        public void ScrollToSavePosition()
        {
            if(ScrollViewer != null)
            {
                var selectedCategory = GetSelectedCategory();
                if(selectedCategory != null)
                {
                    ScrollViewer.ScrollToVerticalOffset(selectedCategory.SavedScrollPosition);
                    //Console.WriteLine($"Scroll position loaded: {selectedCategory.Title} = {selectedCategory.SavedScrollPosition}");
                }
            }
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
            var fileinfo = new FileInfo(path);
            var thumb = new Thumbnail(path, fileinfo.LastWriteTime, fileinfo.Length);
            category.AddImageFirst(thumb);
            dataManager.AddImage(thumb, category.Title);
        }

        public void AddImageFromOutside(string path, bool move, bool reorganize)
        {
            if(Path.GetExtension(path) == ".png")
            {
                string fileName = Path.GetFileName(path);
                string newPath = Path.Combine(folderPath, fileName);
                bool inAnyCategory = FindThumb(newPath, out Category category, out Thumbnail thumb);
                bool inSelectedCategory = inAnyCategory && category == GetSelectedCategory();
                bool inNotSelectedCategory = inAnyCategory && !inSelectedCategory;

                if(File.Exists(newPath))
                {
                    if(inSelectedCategory)
                    {
                        // do nothing
                    }
                    else if(inNotSelectedCategory)
                    {
                        // ask user if should move the file to the selected category
                        if(reorganize) MoveImageFrom(thumb, category, GetSelectedCategory());
                    }
                }
                else
                {
                    if(inAnyCategory)
                    {
                        // remove image from data and then let FileSystemWatcher handle the rest
                        RemoveImageData(category, thumb);
                    }

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

        public void RemoveImageData(Category category, Thumbnail thumb)
        {
            category.RemoveImage(thumb);
            dataManager.RemoveImage(thumb, category.Title);
        }

        public bool FindThumb(string path, out Category outCategory, out Thumbnail outThumb)
        {
            foreach(var category in Categories.Values)
            {
                foreach(var image in category.Images)
                {
                    if(image.Path == path)
                    {
                        outCategory = category;
                        outThumb = image;
                        return true;
                    }
                }
            }

            outCategory = null;
            outThumb = null;
            return false;
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
                    var newCategory = new Category(toCategory);
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
                var newCategory = new Category(newCategoryName);
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
                Categories.Add(categoryName, new Category(categoryName));
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

        public Category(string title)
        {
            Images = new SortableObservableCollection<Thumbnail>();
            Title = title;
            SavedScrollPosition = 0;
        }

        public void AddImageFirst(Thumbnail thumb)
        {
            Images.Insert(0, thumb);
            //Console.WriteLine($"Adding {thumb.Path} to category {Title}");
        }

        public void AddImage(Thumbnail thumb)
        {
            Images.Add(thumb);
            //Console.WriteLine($"Adding {thumb.Path} to category {Title}");
        }

        public void RemoveImage(Thumbnail thumb)
        {
            Images.Remove(thumb);
            //Console.WriteLine($"Removing {thumb.Path} from category {Title}");
        }

        public void SortImagesByDate()
        {
            
        }
    }

    public class Thumbnail
    {
        public string Path { get; set; }
        public DateTime Date { get; set; }
        public string Size { get; set; }

        public Thumbnail(string path, DateTime date, long size)
        {
            Path = path;
            Date = date;
            Size = FileSizeFormat.FormatBytes(size, 2);
        }
    }
}
