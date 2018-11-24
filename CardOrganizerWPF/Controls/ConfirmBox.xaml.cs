using System.Windows;
using System.Windows.Input;

namespace CardOrganizerWPF.Controls
{
    public partial class ConfirmBox : Window
    {
        public string TitleText { get; set; }
        public string InfoText { get; set; }
        public ICommand EscKeyCommand { get; set; }

        public ConfirmBox(string titleText, string infoText)
        {
            InitializeComponent();
            DataContext = this;
            
            EscKeyCommand = new DelegateCommand((x) => DialogResult = false);

            TitleText = titleText;
            InfoText = infoText;
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
