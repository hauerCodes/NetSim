using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

using NetSim.Lib.Simulator;

namespace NetSim.Converter
{
    public class ProtocolTypeToBoolConverter :IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if(value is NetSimProtocolType && parameter is NetSimProtocolType)
            {
                if((NetSimProtocolType)value == (NetSimProtocolType)parameter)
                {
                    return true;
                }

            }

            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if(value is Boolean && (bool)value)
            {
                return parameter;
            }
            return null;
        }
    }
}
