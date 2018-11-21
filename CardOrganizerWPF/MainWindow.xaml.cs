using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Ookii.Dialogs.Wpf;

namespace CardOrganizerWPF
{
    public partial class MainWindow : Window
    {
        #region Initialization
        public Prop<string> WindowTitle { get; set; } = new Prop<string>();
        public Prop<int> SavedTab { get; set; } = new Prop<int>();
        public ICommand ScrollToTop { get; set; }
        public ICommand ScrollToBottom { get; set; }
        public ICommand SetTarget { get; set; }
        public ObservableCollection<string> ProcessList { get; set; } = new ObservableCollection<string>();

        public CardTypeTab TabScene { get; set; }
        public CardTypeTab TabChara1 { get; set; }
        public CardTypeTab TabChara2 { get; set; }
        public CardTypeTab TabOutfit1 { get; set; }
        public CardTypeTab TabOutfit2 { get; set; }

        private CardTypeTab SelectedTab
        {
            get
            {
                switch(tabControlMain.SelectedIndex)
                {
                    case 0: return TabScene;
                    case 1: return TabChara1;
                    case 2: return TabChara2;
                    case 3: return TabOutfit1;
                    case 4: return TabOutfit2;
                    default: return null;
                }
            }
        }

        private string defaultTitle = "CardOrganizer";
        private string currentTarget = "";
        private string markedTab = "";
        private SynchronizationContext uiContext;
        private Settings.GameData gameData;

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
            uiContext = SynchronizationContext.Current;
            WindowTitle.Value = "CardOrganizer";

            Settings.LoadData();
            SettingsLoad();

            ScrollToTop = new DelegateCommand(x => SelectedTab.ScrollToTop());
            ScrollToBottom = new DelegateCommand(x => SelectedTab.ScrollToBottom());

            SetTarget = new DelegateCommand(x =>
            {
                currentTarget = x.ToString();
                WindowTitle.Value = $"{defaultTitle} - {gameData.Name} - {currentTarget}";
            });

            //var args = Environment.GetCommandLineArgs();
            //if(args.Length > 0 && Settings.data.Games.TryGetValue(args[0], out Settings.GameData data))
            //{
            //    gameData = data;
            //}

            TabScene = new CardTypeTab(tabControlScenes, MsgObject.Action.SceneSave);
            TabChara1 = new CardTypeTab(tabControlCharactersF, MsgObject.Action.CharaSave);
            TabChara2 = new CardTypeTab(tabControlCharactersM, MsgObject.Action.CharaSave);
            TabOutfit1 = new CardTypeTab(tabControlOutfitsF, MsgObject.Action.OutfitSave);
            TabOutfit2 = new CardTypeTab(tabControlOutfitsM, MsgObject.Action.OutfitSave);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if(gameData == null)
            {
                var list = new SelectList("Choose a profile", Settings.data.Games.Keys.ToList());
                list.Top = Top + (Height / 2) - (list.Height / 2);
                list.Left = Left + (Width / 2) - (list.Width / 2);
                if(list.ShowDialog() == true)
                {
                    gameData = Settings.data.Games[list.Selected];
                }
            }

            if(gameData != null)
            {
                if(string.IsNullOrWhiteSpace(gameData.Path))
                {
                    var dialog = new VistaFolderBrowserDialog();
                    if(dialog.ShowDialog(this) == true)
                    {
                        gameData.Path = dialog.SelectedPath;
                    }
                }

                // cancel if path is not good

                string serverName = $"CardOrganizerServer.{gameData.Server}";
                int serverPort = 9125;
                new RPCServer(serverName, serverPort);
                RPCClient_UI.Start(serverName, serverPort);

                foreach(var exe in gameData.ProcessList)
                {
                    ProcessList.Add(exe);
                }

                currentTarget = ProcessList.First();
                WindowTitle.Value = $"{defaultTitle} - {gameData.Name} - {currentTarget}";

                TabScene.SetGame(gameData, gameData.Category.Scene);
                TabChara1.SetGame(gameData, gameData.Category.Chara1);
                TabChara2.SetGame(gameData, gameData.Category.Chara2);
                TabOutfit1.SetGame(gameData, gameData.Category.Outfit1);
                TabOutfit2.SetGame(gameData, gameData.Category.Outfit2);

                SavedTab.Value = gameData.Tab != -1 ? gameData.Tab : 0;
                Closing += (x, y) => SettingsSave();

                return;
            }

            Close();
        }

        private void SettingsLoad()
        {
            var data = Settings.data;
            Top = data.Window.Top;
            Left = data.Window.Left;
            Height = data.Window.Height;
            Width = data.Window.Width;
            if(data.Window.Maximized)
                WindowState = WindowState.Maximized;
        }

        private void SettingsSave()
        {
            var data = Settings.data;

            if(WindowState == WindowState.Maximized)
            {
                // Use the RestoreBounds as the current values will be 0, 0 and the size of the screen
                data.Window.Top = RestoreBounds.Top;
                data.Window.Left = RestoreBounds.Left;
                data.Window.Height = RestoreBounds.Height;
                data.Window.Width = RestoreBounds.Width;
                data.Window.Maximized = true;
            }
            else
            {
                data.Window.Top = Top;
                data.Window.Left = Left;
                data.Window.Height = Height;
                data.Window.Width = Width;
                data.Window.Maximized = false;
            }

            gameData.Category.Scene.Save = tabControlScenes.SelectedIndex;
            gameData.Category.Chara1.Save = tabControlCharactersF.SelectedIndex;
            gameData.Category.Chara2.Save = tabControlOutfitsF.SelectedIndex;
            gameData.Category.Outfit1.Save = tabControlCharactersM.SelectedIndex;
            gameData.Category.Outfit2.Save = tabControlOutfitsM.SelectedIndex;
            gameData.Tab = tabControlMain.SelectedIndex;

            Settings.Save();

            TabScene.SaveCardData();
            TabChara1.SaveCardData();
            TabChara2.SaveCardData();
            TabOutfit1.SaveCardData();
            TabOutfit2.SaveCardData();
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
            SelectedTab.SaveCard(currentTarget);
        }

        private void Scenes_MenuItem_Click_Load(object sender, RoutedEventArgs e)
        {
            UseCard(e, MsgObject.Action.SceneLoad);
        }

        private void Scenes_MenuItem_Click_Import(object sender, RoutedEventArgs e)
        {
            UseCard(e, MsgObject.Action.SceneImportAll);
        }

        private void Scenes_MenuItem_Click_ImportCharaOnly(object sender, RoutedEventArgs e)
        {
            UseCard(e, MsgObject.Action.SceneImportChara);
        }

        private void Characters_MenuItem_Click_LoadF(object sender, RoutedEventArgs e)
        {
            UseCard(e, MsgObject.Action.CharaLoadFemale);
        }

        private void Characters_MenuItem_Click_LoadM(object sender, RoutedEventArgs e)
        {
            UseCard(e, MsgObject.Action.CharaLoadMale);
        }

        private void Characters_MenuItem_Click_Replace(object sender, RoutedEventArgs e)
        {
            UseCard(e, MsgObject.Action.CharaReplaceAll);
        }

        private void Characters_MenuItem_Click_ReplaceFace(object sender, RoutedEventArgs e)
        {
            UseCard(e, MsgObject.Action.CharaReplaceFace);
        }

        private void Characters_MenuItem_Click_ReplaceBody(object sender, RoutedEventArgs e)
        {
            UseCard(e, MsgObject.Action.CharaReplaceBody);
        }

        private void Characters_MenuItem_Click_ReplaceHair(object sender, RoutedEventArgs e)
        {
            UseCard(e, MsgObject.Action.CharaReplaceHair);
        }

        private void Characters_MenuItem_Click_ReplaceOutfit(object sender, RoutedEventArgs e)
        {
            UseCard(e, MsgObject.Action.CharaReplaceOutfit);
        }

        private void Outfits_MenuItem_Click_Load(object sender, RoutedEventArgs e)
        {
            UseCard(e, MsgObject.Action.OutfitLoad);
        }

        private void Outfits_MenuItem_Click_LoadAccOnly(object sender, RoutedEventArgs e)
        {
            UseCard(e, MsgObject.Action.OutfitLoadAccOnly);
        }

        private void Outfits_MenuItem_Click_LoadClothOnly(object sender, RoutedEventArgs e)
        {
            UseCard(e, MsgObject.Action.OutfitLoadClothOnly);
        }

        private void UseCard(RoutedEventArgs e, MsgObject.Action action)
        {
            var thumb = (Thumbnail)(e.Source as MenuItem).DataContext;
            RPCClient_UI.SendMessage(MsgObject.Create(action, currentTarget, thumb.Path));
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

                if(!(src is ExtScrollViewer))
                {
                    dynamic context = src.DataContext;

                    if(context is KeyValuePair<string, Category> data)
                    {
                        if(Keyboard.Modifiers == ModifierKeys.Control)
                        {
                            e.Handled = true;
                            markedTab = data.Key;
                            Console.WriteLine($"{markedTab} was marked");
                        }
                        else
                        {
                            SelectedTab.SaveScrollPosition();
                        }
                    } 
                }
            }
        }

        private void MenuItem_Click_AddCategory(object sender, RoutedEventArgs e)
        {
            var inputBox = new InputBox("New category", "Name the new category");
            if(inputBox.ShowDialog() == true)
                SelectedTab.AddCategory(inputBox.ResponseText);
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
            Console.WriteLine($"{markedTab} was marked");
        }

        private void MenuItem_Click_RenameCategory(object sender, RoutedEventArgs e)
        {
            
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
            if(files.Length > 0)
            {
                bool move = Keyboard.Modifiers == ModifierKeys.Shift;
                string process = move ? "Move" : "Copy";
                var confirmBox = new ConfirmBox($"{process} file", $"Move already existing cards\nto the current category (if any)?");
                var reorganize = confirmBox.ShowDialog().Value;

                foreach(var file in files)
                {
                    SelectedTab.AddImageFromOutside(file, move, reorganize);
                }
            }
        }
        #endregion
    }
}
