using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Diagnostics;

namespace CardOrganizerWPF
{
    public partial class MainWindow : Window
    {
        #region Initialization
        public int SavedTab { get; set; }
        public List<CardTypeTab> Tabs { get; set; }
        public ICommand ScrollToTop { get; set; }
        public ICommand ScrollToBottom { get; set; }

        private TCPClientManager tcpClientManager;
        private SynchronizationContext uiContext = SynchronizationContext.Current;
        private Properties.Settings settings = Properties.Settings.Default;
        private string markedTab = "";
        private CardTypeTab SelectedTab => Tabs[tabControlMain.SelectedIndex == -1 ? 0 : tabControlMain.SelectedIndex];

        private enum Game
        {
            HoneySelect,
            Koikatu,
            Playhome
        }

        public MainWindow()
        {
            //settings.Reset();
            InitializeComponent();
            DataContext = this;
            SettingsLoad();
            Closing += (x, y) => SettingsSave();

            tcpClientManager = new TCPClientManager(x => uiContext.Send(y => SelectedTab.HandleMessage(x), null));
            ScrollToTop = new DelegateCommand(x => SelectedTab.ScrollToTop());
            ScrollToBottom = new DelegateCommand(x => SelectedTab.ScrollToBottom());

            var args = Environment.GetCommandLineArgs();
            if(args.Length > 1)
            {
                switch(args[1])
                {
                    case "HS":
                        CreateTabs(Game.HoneySelect);
                        break;

                    case "KK":
                        CreateTabs(Game.Koikatu);
                        break;

                    case "PH":
                        CreateTabs(Game.Playhome);
                        break;
                }
            }
            else
            {
                CreateTabs(Game.Koikatu);
            }
        }

        private void CreateTabs(Game game)
        {
            switch(game)
            {
                case Game.HoneySelect:
                {
                    string mainPath = settings.HSPath;
                    Tabs = new List<CardTypeTab>
                    {
                        new CardTypeTab("Scenes", Path.Combine(mainPath, @"studioneo\scene"), settings.SavedScenesCategory, tabControlScenes, MsgObject.Action.SceneSave),
                        new CardTypeTab("Females", Path.Combine(mainPath, @"chara\female"), settings.SavedCharactersFCategory, tabControlCharactersF, MsgObject.Action.CharaSave),
                        new CardTypeTab("Males", Path.Combine(mainPath, @"chara\male"), settings.SavedCharactersMCategory, tabControlCharactersM, MsgObject.Action.CharaSave),
                        new CardTypeTab("Outfits (F)", Path.Combine(mainPath, @"coordinate\female"), settings.SavedOutfitsFCategory, tabControlOutfitsF, MsgObject.Action.OutfitSave),
                        new CardTypeTab("Outfits (M)", Path.Combine(mainPath, @"coordinate\male"), settings.SavedOutfitsMCategory, tabControlOutfitsM, MsgObject.Action.OutfitSave),
                    };
                    break;
                }

                case Game.Koikatu:
                {
                    string mainPath = settings.KKPath;
                    Tabs = new List<CardTypeTab>
                    {
                        new CardTypeTab("Scenes", Path.Combine(mainPath, @"studio\scene"), settings.SavedScenesCategory, tabControlScenes, MsgObject.Action.SceneSave),
                        new CardTypeTab("Females", Path.Combine(mainPath, @"chara\female"), settings.SavedCharactersFCategory, tabControlCharactersF, MsgObject.Action.CharaSave),
                        new CardTypeTab("Males", Path.Combine(mainPath, @"chara\male"), settings.SavedCharactersMCategory, tabControlCharactersM, MsgObject.Action.CharaSave),
                        new CardTypeTab("Outfits", Path.Combine(mainPath, @"coordinate"), settings.SavedOutfitsFCategory, tabControlOutfitsF, MsgObject.Action.OutfitSave),
                        new CardTypeTab("Disabled"),
                    };
                    break;
                }

                case Game.Playhome:
                {
                    Tabs = new List<CardTypeTab>
                    {
                        new CardTypeTab("Scenes"),
                        new CardTypeTab("Females"),
                        new CardTypeTab("Males"),
                        new CardTypeTab("Outfits (F)"),
                        new CardTypeTab("Outfits (M)"),
                    };
                    break;
                }
            }
        }

        private void SettingsLoad()
        {
            Top = settings.Top;
            Left = settings.Left;
            Height = settings.Height;
            Width = settings.Width;
            if(settings.Maximized)
                WindowState = WindowState.Maximized;

            SavedTab = settings.SavedTab != -1 ? settings.SavedTab : 0;
        }

        private void SettingsSave()
        {
            if(WindowState == WindowState.Maximized)
            {
                // Use the RestoreBounds as the current values will be 0, 0 and the size of the screen
                settings.Top = RestoreBounds.Top;
                settings.Left = RestoreBounds.Left;
                settings.Height = RestoreBounds.Height;
                settings.Width = RestoreBounds.Width;
                settings.Maximized = true;
            }
            else
            {
                settings.Top = Top;
                settings.Left = Left;
                settings.Height = Height;
                settings.Width = Width;
                settings.Maximized = false;
            }

            settings.SavedScenesCategory = tabControlScenes.SelectedIndex;
            settings.SavedCharactersFCategory = tabControlCharactersF.SelectedIndex;
            settings.SavedOutfitsFCategory = tabControlOutfitsF.SelectedIndex;
            settings.SavedCharactersMCategory = tabControlCharactersM.SelectedIndex;
            settings.SavedOutfitsMCategory = tabControlOutfitsM.SelectedIndex;
            settings.SavedTab = tabControlMain.SelectedIndex;

            settings.Save();
            foreach(var tab in Tabs) tab.SaveCardData();
            tcpClientManager.SendMessage(MsgObject.QuitMsg());
        }

        private void Grid_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            switch(e.Key)
            {
                case Key.Home:
                    SelectedTab.ScrollToTop();
                    e.Handled = true;
                    break;

                case Key.End:
                    SelectedTab.ScrollToBottom();
                    e.Handled = true;
                    break;
            }
        }
        #endregion

        #region Card Methods
        private void Button_Click_Save(object sender, RoutedEventArgs e)
        {
            tcpClientManager.SendMessage(MsgObject.UseMsg(SelectedTab.saveMsg, SelectedTab.FolderPath));
        }

        private void Scenes_MenuItem_Click_Load(object sender, RoutedEventArgs e)
        {
            bool resolve = Keyboard.Modifiers == ModifierKeys.Shift;
            UseCard(e, resolve ? MsgObject.Action.SceneLoadResolver : MsgObject.Action.SceneLoad);
        }

        private void Scenes_MenuItem_Click_Import(object sender, RoutedEventArgs e)
        {
            bool resolve = Keyboard.Modifiers == ModifierKeys.Shift;
            UseCard(e, resolve ? MsgObject.Action.SceneImportAllResolver : MsgObject.Action.SceneImportAll);
        }

        private void Scenes_MenuItem_Click_ImportCharaOnly(object sender, RoutedEventArgs e)
        {
            bool resolve = Keyboard.Modifiers == ModifierKeys.Shift;
            UseCard(e, resolve ? MsgObject.Action.SceneImportCharaResolver : MsgObject.Action.SceneImportChara);
        }

        private void Characters_MenuItem_Click_LoadF(object sender, RoutedEventArgs e)
        {
            bool resolve = Keyboard.Modifiers == ModifierKeys.Shift;
            UseCard(e, resolve ? MsgObject.Action.CharaLoadFemaleResolver : MsgObject.Action.CharaLoadFemale);
        }

        private void Characters_MenuItem_Click_LoadM(object sender, RoutedEventArgs e)
        {
            bool resolve = Keyboard.Modifiers == ModifierKeys.Shift;
            UseCard(e, resolve ? MsgObject.Action.CharaLoadMaleResolver : MsgObject.Action.CharaLoadMale);
        }

        private void Characters_MenuItem_Click_Replace(object sender, RoutedEventArgs e)
        {
            bool resolve = Keyboard.Modifiers == ModifierKeys.Shift;
            UseCard(e, resolve ? MsgObject.Action.CharaReplaceAllResolver : MsgObject.Action.CharaReplaceAll);
        }

        private void Characters_MenuItem_Click_ReplaceBody(object sender, RoutedEventArgs e)
        {
            bool resolve = Keyboard.Modifiers == ModifierKeys.Shift;
            UseCard(e, resolve ? MsgObject.Action.CharaReplaceBodyResolver : MsgObject.Action.CharaReplaceBody);
        }

        private void Outfits_MenuItem_Click_Load(object sender, RoutedEventArgs e)
        {
            UseCard(e, MsgObject.Action.OutfitLoad);
        }

        private void UseCard(RoutedEventArgs e, MsgObject.Action action)
        {
            var thumb = (Thumbnail)(e.Source as MenuItem).DataContext;
            tcpClientManager.SendMessage(MsgObject.UseMsg(action, thumb.Path));
        }

        private void MenuItem_Click_Delete(object sender, RoutedEventArgs e)
        {
            var thumb = (Thumbnail)(e.Source as MenuItem).DataContext;
            SelectedTab.RemoveImage(thumb);
        }

        private void MenuItem_Click_Explorer(object sender, RoutedEventArgs e)
        {
            var thumb = (Thumbnail)(e.Source as MenuItem).DataContext;

            if(!File.Exists(thumb.Path))
            {
                Console.WriteLine($"File does not exist ({thumb.Path})");
                return;
            }

            // combine the arguments together
            // it doesn't matter if there is a space after ','
            string argument = "/select, \"" + thumb.Path + "\"";
            Process.Start("explorer.exe", argument);
        }
        #endregion

        #region Category Methods
        private void CategoryTab_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(e.OriginalSource == e.Source)
            {
                SelectedTab.ScrollToSavePosition();
                e.Handled = true;
            }
        }

        private void CategoryTab_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if(e.LeftButton == MouseButtonState.Pressed)
            {
                dynamic src = e.OriginalSource;
                dynamic context = src.DataContext;

                if(context is KeyValuePair<string, Category>)
                {
                    if(Keyboard.Modifiers == ModifierKeys.Control)
                    {
                        markedTab = context.Key;
                        e.Handled = true;
                        Console.WriteLine($"{markedTab} was marked");
                    }
                    else
                    {
                        SelectedTab.SaveScrollPosition();
                    }
                }
            }
        }

        private void MenuItem_Click_AddCategory(object sender, RoutedEventArgs e)
        {
            string text = new InputBox("Category Name").ShowDialog();
            if(text != "") SelectedTab.AddCategory(text);
        }

        private void MenuItem_Click_RemoveCategory(object sender, RoutedEventArgs e)
        {
            var target = GetPlacementTarget(e);
            SelectedTab.RemoveCategory(target.Text);
        }

        private void MenuItem_Click_MarkCategory(object sender, RoutedEventArgs e)
        {
            var target = GetPlacementTarget(e);
            markedTab = target.Text;
        }

        private void MenuItem_Click_RenameCategory(object sender, RoutedEventArgs e)
        {
            //var target = GetPlacementTarget(e);
            //string text = new InputBox("Rename Category").ShowDialog();
            //if(text != "")
            //{
                
            //    target.Text = text;
            //}
        }

        private TextBlock GetPlacementTarget(RoutedEventArgs e)
        {
            var menuItem = e.OriginalSource as MenuItem;
            var contextMenu = menuItem.Parent as ContextMenu;
            return contextMenu.PlacementTarget as TextBlock;
        }

        private void MenuItem_Click_MoveAll(object sender, RoutedEventArgs e)
        {
            var target = GetPlacementTarget(e).Text;
            if(target != SelectedTab.GetSelectedCategory().Title)
            {
                SelectedTab.MoveImageAll(target);
            }
        }

        private void MenuItem_Click_SortCategory(object sender, RoutedEventArgs e)
        {
            SelectedTab.GetSelectedCategory().SortImagesByDate();
        }

        private void Image_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if(Keyboard.Modifiers == ModifierKeys.Control && markedTab != "" && markedTab != SelectedTab.GetSelectedCategory().Title)
            {
                var thumb = (Thumbnail)(e.Source as Image).DataContext;
                SelectedTab.MoveImage(thumb, markedTab);
            }
        }

        private void TabControlMain_SelectionChanged(object sender, RoutedEventArgs e)
        {
            markedTab = "";
        }

        private void TabControl_Drop(object sender, DragEventArgs e)
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            foreach(var file in files)
            {
                bool move = Keyboard.Modifiers == ModifierKeys.Shift;
                SelectedTab.AddImageFromOutside(file, move);
            }
        }
        #endregion
    }
}
