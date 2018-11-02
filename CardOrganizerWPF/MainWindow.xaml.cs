﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using Ookii.Dialogs.Wpf;

namespace CardOrganizerWPF
{
    public partial class MainWindow : Window
    {
        #region Initialization
        public int SavedTab { get; set; }
        public List<CardTypeTab> Tabs { get; set; }
        public ICommand ScrollToTop { get; set; }
        public ICommand ScrollToBottom { get; set; }

        public static Action Rendered { get; set; }
        private CardTypeTab SelectedTab => Tabs[tabControlMain.SelectedIndex == -1 ? 0 : tabControlMain.SelectedIndex];

        private SynchronizationContext uiContext = SynchronizationContext.Current;
        private string markedTab = "";
        private Settings.GameData gameData;

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;

            Settings.LoadData();
            SettingsLoad();

            Rendered = () =>
            {
                if(string.IsNullOrWhiteSpace(gameData.Path))
                {
                    var dialog = new VistaFolderBrowserDialog();
                    if(dialog.ShowDialog(new Window()) == true)
                    {
                        gameData.Path = dialog.SelectedPath;
                    }
                }

                // cancel if path is not good
            };

            Loaded += (sender, e) =>
            {
                Dispatcher.BeginInvoke(Rendered, DispatcherPriority.ContextIdle, null);
            };

            Closing += (sender, e) =>
            {
                SettingsSave();
                //tcpClientManager.SendMessage(MsgObject.QuitMsg());
            };

            //tcpClientManager = new TCPClientManager(x => uiContext.Send(y => SelectedTab.HandleMessage(x), null));
            ScrollToTop = new DelegateCommand(x => SelectedTab.ScrollToTop());
            ScrollToBottom = new DelegateCommand(x => SelectedTab.ScrollToBottom());

            CreateTabs();

            string serverName = "CardOrganizerServer.KK";
            int serverPort = 9125;
            new RPCServer(serverName, serverPort);
            RPCClient_UI.Start(serverName, serverPort);
        }

        private void CreateTabs()
        {
            var args = Environment.GetCommandLineArgs();
            if(args.Length > 1)
            {
                switch(args[1])
                {
                    case "HS":
                        gameData = Settings.data.HS;
                        break;

                    case "KK":
                        gameData = Settings.data.KK;
                        break;

                    default:
                        gameData = Settings.data.KK;
                        break;
                }
            }
            else
            {
                gameData = Settings.data.KK;
            }

            Tabs = new List<CardTypeTab>
            {
                new CardTypeTab(gameData, gameData.Category.Scene, tabControlScenes, MsgObject.Action.SceneSave),
                new CardTypeTab(gameData, gameData.Category.Chara1, tabControlCharactersF, MsgObject.Action.CharaSave),
                new CardTypeTab(gameData, gameData.Category.Chara2, tabControlCharactersM, MsgObject.Action.CharaSave),
                new CardTypeTab(gameData, gameData.Category.Outfit1, tabControlOutfitsF, MsgObject.Action.OutfitSave),
                new CardTypeTab(gameData, gameData.Category.Outfit2, tabControlOutfitsM, MsgObject.Action.OutfitSave),
            };

            SavedTab = gameData.Tab != -1 ? gameData.Tab : 0;
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
            //foreach(var tab in Tabs) tab.SaveCardData();
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
            RPCClient_UI.SendMessage(MsgObject.UseMsg(SelectedTab.saveMsg, SelectedTab.FolderPath));
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

        private void Characters_MenuItem_Click_ReplaceBody(object sender, RoutedEventArgs e)
        {
            UseCard(e, MsgObject.Action.CharaReplaceBody);
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
            RPCClient_UI.SendMessage(MsgObject.UseMsg(action, thumb.Path));
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
