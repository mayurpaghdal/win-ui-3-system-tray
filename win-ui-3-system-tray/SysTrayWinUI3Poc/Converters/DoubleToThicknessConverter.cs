using Microsoft.UI.Xaml.Data;
using System;
using Microsoft.UI.Xaml;

namespace AppUIBasics.Converters
{
    class DoubleToThicknessConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is double?)
            {
                return new Thickness((double)value);
            }
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
