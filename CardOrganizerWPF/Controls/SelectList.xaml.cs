using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;

namespace CardOrganizerWPF.Controls
{
    public partial class SelectList : Window
    {
        public string TitleText { get; set; }
        public List<string> ItemList { get; set; }
        public ICommand EnterKeyCommand { get; set; }
        public ICommand EscKeyCommand { get; set; }

        public string Selected => listBox.SelectedValue.ToString();

        public SelectList(string titleText, IEnumerable<string> itemList)
        {
            InitializeComponent();
            DataContext = this;

            EnterKeyCommand = new DelegateCommand((x) => CheckResult());
            EscKeyCommand = new DelegateCommand((x) => DialogResult = false);

            TitleText = titleText;
            ItemList = new List<string>(itemList);
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
