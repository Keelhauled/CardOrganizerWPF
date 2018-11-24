using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace CardOrganizerWPF.Controls
{
    public class ExtScrollViewer : ScrollViewer
    {
        private double scrollSpeed = Settings.data.ScrollSpeed;
        private ScrollBar verticalScrollbar;

        public override void OnApplyTemplate()
        {
            // Call base class
            base.OnApplyTemplate();

            // Obtain the vertical scrollbar
            verticalScrollbar = GetTemplateChild("PART_VerticalScrollBar") as ScrollBar;
        }

        protected override void OnMouseWheel(MouseWheelEventArgs e)
        {
            // Only handle this message if the vertical scrollbar is in use
            if(verticalScrollbar != null && verticalScrollbar.Visibility == Visibility.Visible && verticalScrollbar.IsEnabled)
            {
                if(ScrollInfo != null)
                {
                    if(e.Delta < 0)
                    {
                        ScrollToVerticalOffset(VerticalOffset + scrollSpeed);
                    }
                    else
                    {
                        ScrollToVerticalOffset(VerticalOffset - scrollSpeed);
                    }
                }
            }
        }
    } 
}
