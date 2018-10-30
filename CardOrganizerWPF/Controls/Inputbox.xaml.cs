using System.Windows;
using System.Windows.Input;

namespace CardOrganizerWPF
{
    public partial class InputBox : Window
    {
        public string TitleText { get; set; }
        public string InfoText { get; set; }
        public ICommand EnterKeyCommand { get; set; }
        public ICommand EscKeyCommand { get; set; }

        public string ResponseText
        {
            get { return ResponseTextBox.Text; }
            set { ResponseTextBox.Text = value; }
        }

        public InputBox(string titleText, string infoText)
        {
            InitializeComponent();
            DataContext = this;

            EnterKeyCommand = new DelegateCommand((x) => CheckResponse());
            EscKeyCommand = new DelegateCommand((x) => DialogResult = false);

            TitleText = titleText;
            InfoText = infoText;
        }

        private void CheckResponse()
        {
            if(!string.IsNullOrWhiteSpace(ResponseText))
            {
                DialogResult = true;
            }
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            CheckResponse();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
