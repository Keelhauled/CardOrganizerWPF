using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace CardOrganizerWPF
{
    public class InputBox
    {
        Window window;
        TextBox input;
        string exampleText;
        bool clickedOK = false;
        bool inputReset = false;

        public InputBox(Window parent, string titleText, string headerText, string exampletext)
        {
            var font = new FontFamily("Tahoma");
            var elements = new StackPanel();
            double fontSize = 25;
            exampleText = exampletext;
            double windowHeight = 150;
            double windowWidth = 300;

            window = new Window
            {
                Top = parent.Top + 100,
                Left = parent.Left + 100,
                Height = windowHeight,
                Width = windowWidth,
                Title = titleText,
                Content = elements,
                ResizeMode = ResizeMode.CanMinimize,
            };

            var content = new TextBlock
            {
                TextWrapping = TextWrapping.Wrap,
                Background = null,
                HorizontalAlignment = HorizontalAlignment.Center,
                Text = headerText,
                FontFamily = font,
                FontSize = fontSize,
            };

            input = new TextBox
            {
                FontFamily = font,
                FontSize = fontSize,
                HorizontalAlignment = HorizontalAlignment.Center,
                Text = exampleText,
                MinWidth = 200,
            };

            var okButton = new Button
            {
                Width = 70,
                Height = 30,
                Content = "OK",
                HorizontalAlignment = HorizontalAlignment.Center,
            };

            window.Closing += Box_Closing;
            if(exampleText != "") input.MouseEnter += Input_MouseEnter;
            okButton.Click += Ok_Click;

            elements.Children.Add(content);
            elements.Children.Add(input);
            elements.Children.Add(okButton);
        }

        void Box_Closing(object sender, CancelEventArgs e)
        {
            if(!clickedOK)
                input.Text = "";
        }

        void Input_MouseEnter(object sender, MouseEventArgs e)
        {
            if(input.Text == exampleText && !inputReset)
            {
                input.Text = "";
                inputReset = true;
            }
        }

        void Ok_Click(object sender, RoutedEventArgs e)
        {
            clickedOK = true;
            window.Close();
            clickedOK = false;
        }

        public string ShowDialog()
        {
            window.ShowDialog();
            return input.Text;
        }
    }
}
