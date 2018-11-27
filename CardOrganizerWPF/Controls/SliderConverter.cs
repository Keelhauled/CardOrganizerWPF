using System;
using System.Globalization;
using System.Windows.Data;

namespace CardOrganizerWPF.Controls
{
    public class SliderConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if(value != null && value is double sliderValue)
            {
                return sliderValue.ToString("F2");
            }
            else
            {
                return null;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
