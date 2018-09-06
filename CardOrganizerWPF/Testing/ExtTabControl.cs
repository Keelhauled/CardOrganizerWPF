using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Controls;
using System.Windows.Input;

namespace CardOrganizerWPF
{
    public class ExtTabControl : TabControl
    {
        protected override void OnSelectionChanged(SelectionChangedEventArgs e)
        {
            
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            e.Handled = true;
        }
    }
}
