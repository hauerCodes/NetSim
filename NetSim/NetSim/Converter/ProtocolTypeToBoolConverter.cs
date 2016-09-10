// -----------------------------------------------------------------------
// <copyright file="ProtocolTypeToBoolConverter.cs" company="FH Wr.Neustadt">
//      Copyright Christoph Hauer. All rights reserved.
// </copyright>
// <author>Christoph Hauer</author>
// <summary>NetSim - ProtocolTypeToBoolConverter.cs</summary>
// -----------------------------------------------------------------------

namespace NetSim.Converter
{
    using System;
    using System.Globalization;
    using System.Linq;
    using System.Windows.Data;

    using NetSim.Lib.Simulator;

    /// <summary>
    /// The protocol type to boolean converter implementation.
    /// </summary>
    /// <seealso cref="System.Windows.Data.IValueConverter" />
    public class ProtocolTypeToBoolConverter : IValueConverter
    {
        /// <summary>
        /// Converts a value.
        /// </summary>
        /// <param name="value">The value produced by the binding source.</param>
        /// <param name="targetType">The type of the binding target property.</param>
        /// <param name="parameter">The converter parameter to use.</param>
        /// <param name="culture">The culture to use in the converter.</param>
        /// <returns>
        /// A converted value. If the method returns null, the valid null value is used.
        /// </returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is NetSimProtocolType && parameter is NetSimProtocolType)
            {
                if ((NetSimProtocolType)value == (NetSimProtocolType)parameter)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Converts a value.
        /// </summary>
        /// <param name="value">The value that is produced by the binding target.</param>
        /// <param name="targetType">The type to convert to.</param>
        /// <param name="parameter">The converter parameter to use.</param>
        /// <param name="culture">The culture to use in the converter.</param>
        /// <returns>
        /// A converted value. If the method returns null, the valid null value is used.
        /// </returns>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool && (bool)value)
            {
                return parameter;
            }

            return null;
        }
    }
}