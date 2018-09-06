using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace CardOrganizerWPF
{
    public class ImgConverter : IValueConverter
    {
        static Dictionary<string, BitmapImage> cache = new Dictionary<string, BitmapImage>();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if(value != null)
            {
                string path = value as string;

                if(!cache.TryGetValue(path, out BitmapImage image))
                {
                    image = new BitmapImage();
                    image.BeginInit();
                    image.UriSource = new Uri(value as string);
                    image.CacheOption = BitmapCacheOption.OnLoad;
                    image.EndInit();
                    cache.Add(path, image);
                }

                return image;
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
