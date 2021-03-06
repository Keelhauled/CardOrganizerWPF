﻿using CardOrganizerWPF.Controls;
using CardOrganizerWPF.Remoting;
using CardOrganizerWPF.Utils;
using Ookii.Dialogs.Wpf;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace CardOrganizerWPF
{
    public partial class MainWindow : Window
    {
        #region Initialization
        public Prop<string> WindowTitle { get; set; } = new Prop<string>();
        public Prop<int> SavedTab { get; set; } = new Prop<int>();
        public Prop<Visibility> PartialReplaceEnabled { get; set; } = new Prop<Visibility>();
        public Prop<Visibility> SpecialLoadEnabled { get; set; } = new Prop<Visibility>();
        public Prop<double> ImageMultiplier { get; set; } = new Prop<double>(1);

        public ObservableCollection<string> ProfileList { get; set; }
        public ObservableCollection<string> ProcessList { get; set; }

        public ICommand ScrollToTop { get; set; }
        public ICommand ScrollToBottom { get; set; }
        public ICommand TargetSwitched { get; set; }
        public ICommand ProfileSwitched { get; set; }

        public CardTypeTab TabScene { get; set; }
        public CardTypeTab TabChara1 { get; set; }
        public CardTypeTab TabChara2 { get; set; }
        public CardTypeTab TabOutfit1 { get; set; }
        public CardTypeTab TabOutfit2 { get; set; }

        CardTypeTab SelectedTab
        {
            get
            {
                if(tabControlMain != null)
                {
                    switch(tabControlMain.SelectedIndex)
                    {
                        case 0: return TabScene;
                        case 1: return TabChara1;
                        case 2: return TabChara2;
                        case 3: return TabOutfit1;
                        case 4: return TabOutfit2;
                    }
                }

                return null;
            }
        }

        string Id
        {
            get { return $"{gameData.Server}_{currentTarget}"; }
        }

        string serverName = "CardOrganizerServer";
        int serverPort = 9125;
        string defaultTitle = "CardOrganizer";
        string currentTarget = "";
        string markedTab = "";
        Settings.GameData gameData;

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
            WindowTitle.Value = defaultTitle;

            Settings.LoadData();
            SettingsLoad();

            ScrollToTop = new DelegateCommand((x) => SelectedTab.ScrollToTop());
            ScrollToBottom = new DelegateCommand((x) => SelectedTab.ScrollToBottom());

            ProcessList = new ObservableCollection<string>();
            TargetSwitched = new DelegateCommand(TargetSwitch);

            ProfileList = new ObservableCollection<string>(Settings.data.Games.Keys);
            ProfileSwitched = new DelegateCommand(ProfileSwitch);

            TabScene = new CardTypeTab(tabControlScene, MsgObject.Action.SceneSave);
            TabChara1 = new CardTypeTab(tabControlChara1, MsgObject.Action.CharaSave);
            TabChara2 = new CardTypeTab(tabControlChara2, MsgObject.Action.CharaSave);
            TabOutfit1 = new CardTypeTab(tabControlOutfit1, MsgObject.Action.OutfitSave);
            TabOutfit2 = new CardTypeTab(tabControlOutfit2, MsgObject.Action.OutfitSave);

            var args = Environment.GetCommandLineArgs();
            if(args.Length > 1)
            {
                var comparer = StringComparer.OrdinalIgnoreCase;
                var caseInsen = new Dictionary<string, Settings.GameData>(Settings.data.Games, comparer);
                caseInsen.TryGetValue(args[1], out gameData);
            }
        }

        void Window_Closing(object sender, CancelEventArgs e)
        {
            SaveSettings(true);
            SaveCardData();
        }

        void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if(!string.IsNullOrWhiteSpace(Settings.data.LastProfile))
            {
                gameData = Settings.data.Games[Settings.data.LastProfile];
            }

            if(gameData == null)
            {
                var list = new SelectList("Choose a profile", Settings.data.Games.Keys);
                list.Top = Top + (Height / 2) - (list.Height / 2);
                list.Left = Left + (Width / 2) - (list.Width / 2);

                if(list.ShowDialog() == true)
                {
                    gameData = Settings.data.Games[list.Selected];
                    Settings.data.LastProfile = list.Selected;
                }
            }

            if(gameData != null)
            {
                if(string.IsNullOrWhiteSpace(gameData.Path))
                {
                    var dialog = new VistaFolderBrowserDialog
                    {
                        Description = $"Select userdata folder for {gameData.Name}",
                        UseDescriptionForTitle = true
                    };

                    if(dialog.ShowDialog(this) == true)
                    {
                        gameData.Path = dialog.SelectedPath;
                    }
                }

                if(!string.IsNullOrWhiteSpace(gameData.Path)) // check if paths in category exist here
                {
                    RPCServer.Start(serverName, serverPort);
                    RPCClient_UI.Start(serverName, serverPort);

                    gameData.SceneList.ForEach((x) => ProcessList.Add(x.Name));
                    var scene = gameData.SceneList[gameData.SavedScene];
                    currentTarget = scene.Name;
                    WindowTitle.Value = $"{defaultTitle} - {gameData.Name} - {currentTarget}";
                    PartialReplaceEnabled.Value = scene.PartialReplaceEnabled;
                    SpecialLoadEnabled.Value = scene.SpecialLoadEnabled;

                    TabScene.SetGame(gameData, gameData.Category.Scene);
                    TabChara1.SetGame(gameData, gameData.Category.Chara1);
                    TabChara2.SetGame(gameData, gameData.Category.Chara2);
                    TabOutfit1.SetGame(gameData, gameData.Category.Outfit1);
                    TabOutfit2.SetGame(gameData, gameData.Category.Outfit2);

                    ImageMultiplier.Value = SelectedTab.ImageMultiplier;
                    SavedTab.Value = gameData.Tab != -1 ? gameData.Tab : 0;

                    Closing += Window_Closing;

                    return;
                }
            }

            Close();
        }

        void ProfileSwitch(object sender)
        {
            string newProfile = sender.ToString();
            SaveSettings(false);

            var newGameData = Settings.data.Games[newProfile];

            if(newGameData.Name != gameData.Name)
            {
                if(string.IsNullOrWhiteSpace(newGameData.Path))
                {
                    var dialog = new VistaFolderBrowserDialog
                    {
                        Description = $"Select userdata folder for {newGameData.Name}",
                        UseDescriptionForTitle = true
                    };
                    if(dialog.ShowDialog(this) == true)
                    {
                        newGameData.Path = dialog.SelectedPath;
                    }
                }

                if(!string.IsNullOrWhiteSpace(newGameData.Path)) // check if paths in category exist here
                {
                    TabScene.StopThread();
                    TabChara1.StopThread();
                    TabChara2.StopThread();
                    TabOutfit1.StopThread();
                    TabOutfit2.StopThread();

                    Settings.data.LastProfile = newProfile;
                    SaveSettings(false);
                    gameData = newGameData;

                    ProcessList.Clear();
                    gameData.SceneList.ForEach((y) => ProcessList.Add(y.Name));
                    var scene = gameData.SceneList[gameData.SavedScene];
                    currentTarget = scene.Name;
                    WindowTitle.Value = $"{defaultTitle} - {gameData.Name} - {currentTarget}";
                    PartialReplaceEnabled.Value = scene.PartialReplaceEnabled;
                    SpecialLoadEnabled.Value = scene.SpecialLoadEnabled;

                    TabScene.SetGame(gameData, gameData.Category.Scene);
                    TabChara1.SetGame(gameData, gameData.Category.Chara1);
                    TabChara2.SetGame(gameData, gameData.Category.Chara2);
                    TabOutfit1.SetGame(gameData, gameData.Category.Outfit1);
                    TabOutfit2.SetGame(gameData, gameData.Category.Outfit2);

                    ImageMultiplier.Value = SelectedTab.ImageMultiplier;
                    SavedTab.Value = gameData.Tab != -1 ? gameData.Tab : 0;
                }
            }
        }

        void TargetSwitch(object sender)
        {
            currentTarget = sender.ToString();
            WindowTitle.Value = $"{defaultTitle} - {gameData.Name} - {currentTarget}";
            var scene = gameData.SceneList.First((x) => x.Name == currentTarget);
            PartialReplaceEnabled.Value = scene.PartialReplaceEnabled;
            SpecialLoadEnabled.Value = scene.SpecialLoadEnabled;
            SaveSettings(false);
        }

        void SettingsLoad()
        {
            var data = Settings.data;
            Top = data.Window.Top;
            Left = data.Window.Left;
            Height = data.Window.Height;
            Width = data.Window.Width;
            if(data.Window.Maximized)
                WindowState = WindowState.Maximized;
        }

        void SaveSettings(bool saveFile)
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

            gameData.Tab = tabControlMain.SelectedIndex;
            gameData.SavedScene = ProcessList.IndexOf(currentTarget);

            TabScene.SaveSettingsData();
            TabChara1.SaveSettingsData();
            TabChara2.SaveSettingsData();
            TabOutfit1.SaveSettingsData();
            TabOutfit2.SaveSettingsData();

            if(saveFile)
                Settings.Save();
        }

        void SaveCardData()
        {
            TabScene.SaveCardData();
            TabChara1.SaveCardData();
            TabChara2.SaveCardData();
            TabOutfit1.SaveCardData();
            TabOutfit2.SaveCardData();
        }

        void Grid_PreviewKeyDown(object sender, KeyEventArgs e)
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
        void Button_Click_Save(object sender, RoutedEventArgs e)
        {
            SelectedTab.SaveCard(Id);
        }

        void Scenes_MenuItem_Click_Load(object sender, RoutedEventArgs e)
        {
            UseCard(e, MsgObject.Action.SceneLoad);
        }

        void Scenes_MenuItem_Click_Import(object sender, RoutedEventArgs e)
        {
            UseCard(e, MsgObject.Action.SceneImportAll);
        }

        void Scenes_MenuItem_Click_ImportCharaOnly(object sender, RoutedEventArgs e)
        {
            UseCard(e, MsgObject.Action.SceneImportChara);
        }

        void Characters_MenuItem_Click_LoadF(object sender, RoutedEventArgs e)
        {
            UseCard(e, MsgObject.Action.CharaLoadFemale);
        }

        void Characters_MenuItem_Click_LoadM(object sender, RoutedEventArgs e)
        {
            UseCard(e, MsgObject.Action.CharaLoadMale);
        }

        void Characters_MenuItem_Click_LoadSpecial(object sender, RoutedEventArgs e)
        {
            UseCard(e, MsgObject.Action.CharaLoadSpecial);
        }

        void Characters_MenuItem_Click_Replace(object sender, RoutedEventArgs e)
        {
            UseCard(e, MsgObject.Action.CharaReplaceAll);
        }

        void Characters_MenuItem_Click_ReplaceFace(object sender, RoutedEventArgs e)
        {
            UseCard(e, MsgObject.Action.CharaReplaceFace);
        }

        void Characters_MenuItem_Click_ReplaceBody(object sender, RoutedEventArgs e)
        {
            UseCard(e, MsgObject.Action.CharaReplaceBody);
        }

        void Characters_MenuItem_Click_ReplaceHair(object sender, RoutedEventArgs e)
        {
            UseCard(e, MsgObject.Action.CharaReplaceHair);
        }

        void Characters_MenuItem_Click_ReplaceOutfit(object sender, RoutedEventArgs e)
        {
            UseCard(e, MsgObject.Action.CharaReplaceOutfit);
        }

        void Outfits_MenuItem_Click_Load(object sender, RoutedEventArgs e)
        {
            UseCard(e, MsgObject.Action.OutfitLoad);
        }

        void Outfits_MenuItem_Click_LoadAccOnly(object sender, RoutedEventArgs e)
        {
            UseCard(e, MsgObject.Action.OutfitLoadAccOnly);
        }

        void Outfits_MenuItem_Click_LoadClothOnly(object sender, RoutedEventArgs e)
        {
            UseCard(e, MsgObject.Action.OutfitLoadClothOnly);
        }

        void UseCard(RoutedEventArgs e, MsgObject.Action action)
        {
            var thumb = (Thumbnail)(e.Source as MenuItem).DataContext;
            RPCClient_UI.SendMessage(MsgObject.Create(action, Id, thumb.Path));
        }

        void MenuItem_Click_Delete(object sender, RoutedEventArgs e)
        {
            var thumb = (Thumbnail)(e.Source as MenuItem).DataContext;
            SelectedTab.RemoveImage(thumb);
        }

        void MenuItem_Click_Explorer(object sender, RoutedEventArgs e)
        {
            var thumb = (Thumbnail)(e.Source as MenuItem).DataContext;

            if(!File.Exists(thumb.Path))
            {
                Debug.WriteLine($"File does not exist ({thumb.Path})");
                return;
            }

            // combine the arguments together
            // it doesn't matter if there is a space after ','
            string argument = "/select, \"" + thumb.Path + "\"";
            Process.Start("explorer.exe", argument);
        }

        void ImageSizeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            SelectedTab?.SetImageSize(e.NewValue);
        }
        #endregion

        #region Category Methods
        void CategoryTab_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(e.OriginalSource == e.Source)
            {
                SelectedTab.ScrollToSavePosition();
                e.Handled = true;
            }
        }

        void CategoryTab_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if(e.LeftButton == MouseButtonState.Pressed)
            {
                if(!(e.OriginalSource is ExtScrollViewer) && e.OriginalSource is FrameworkElement src)
                {
                    if(src.DataContext is KeyValuePair<string, Category> data)
                    {
                        if(Keyboard.Modifiers == ModifierKeys.Control)
                        {
                            e.Handled = true;
                            markedTab = data.Key;
                            Debug.WriteLine($"{markedTab} was marked");
                        }
                        else
                        {
                            SelectedTab.SaveScrollPosition();
                        }
                    }
                }
            }
        }

        void MenuItem_Click_AddCategory(object sender, RoutedEventArgs e)
        {
            var inputBox = new InputBox("New category", "Name the new category");
            if(inputBox.ShowDialog() == true)
                SelectedTab.AddCategory(inputBox.ResponseText);
        }

        void MenuItem_Click_RemoveCategory(object sender, RoutedEventArgs e)
        {
            var target = GetPlacementTarget(e);
            SelectedTab.RemoveCategory(target.Text);
        }

        void MenuItem_Click_MarkCategory(object sender, RoutedEventArgs e)
        {
            var target = GetPlacementTarget(e);
            markedTab = target.Text;
            Debug.WriteLine($"{markedTab} was marked");
        }

        TextBlock GetPlacementTarget(RoutedEventArgs e)
        {
            var menuItem = e.OriginalSource as MenuItem;
            var contextMenu = menuItem.Parent as ContextMenu;
            return contextMenu.PlacementTarget as TextBlock;
        }

        void MenuItem_Click_MoveAll(object sender, RoutedEventArgs e)
        {
            var target = GetPlacementTarget(e).Text;
            if(target != SelectedTab.GetSelectedCategory().Title)
                SelectedTab.MoveImageAll(target);
        }

        void Image_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if(Keyboard.IsKeyDown(Key.Delete))
            {
                var thumb = (Thumbnail)(e.Source as Image).DataContext;
                SelectedTab.RemoveImage(thumb);
            }
            else if(Keyboard.Modifiers == ModifierKeys.Control && markedTab != "" && markedTab != SelectedTab.GetSelectedCategory().Title)
            {
                var thumb = (Thumbnail)(e.Source as Image).DataContext;
                SelectedTab.MoveImage(thumb, markedTab);
            }
        }

        void TabControlMain_SelectionChanged(object sender, RoutedEventArgs e)
        {
            markedTab = "";
            ImageMultiplier.Value = SelectedTab.ImageMultiplier;
        }

        void TabControl_Drop(object sender, DragEventArgs e)
        {
            var files = (string[])e.Data.GetData(DataFormats.FileDrop);
            if(files.Length > 0)
            {
                var move = Keyboard.Modifiers == ModifierKeys.Shift;
                var process = move ? "Move" : "Copy";
                var confirmBox = new ConfirmBox($"{process} file", $"Move already existing cards\nto the current category (if any)?");
                var reorganize = confirmBox.ShowDialog().Value;

                foreach(var file in files)
                    SelectedTab.AddImageFromOutside(file, move, reorganize);
            }
        }
        #endregion
    }
}
