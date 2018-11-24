using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;

namespace CardOrganizerWPF.Controls
{
    public partial class SelectList : Window
    {
        public string TitleText { get; set; }
        public List<string> GameList { get; set; }
        public ICommand EnterKeyCommand { get; set; }
        public ICommand EscKeyCommand { get; set; }

        public string Selected => listBox.SelectedValue.ToString();

        public SelectList(string titleText, List<string> gameList)
        {
            InitializeComponent();
            DataContext = this;

            EnterKeyCommand = new DelegateCommand((x) => CheckResult());
            EscKeyCommand = new DelegateCommand((x) => DialogResult = false);

            TitleText = titleText;
            GameList = gameList;
        }

        private void CheckResult()
        {
            if(listBox.SelectedItem != null)
            {
                DialogResult = true;
            }
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            CheckResult();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
