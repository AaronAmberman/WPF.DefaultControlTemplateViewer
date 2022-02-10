using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace DefaultControlTemplateViewer
{
    /// <summary>Converter that can be used to calculate arithmetic during binding.</summary>
    /// <seealso cref="IValueConverter"/>
    public class ArithmeticConverter : IValueConverter
    {
        /// <summary>Converters the value using the specified arithmetic operations.</summary>
        /// <param name="value">The value produced by the binding source.</param>
        /// <param name="targetType">The type of the binding target property.</param>
        /// <param name="parameter">The converter parameter to use.</param>
        /// <param name="culture">The culture to use in the converter.</param>
        /// <returns>A converted value. If the method returns null, the valid null value is used.</returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double)
            {
                double val = (double)value;

                string[] arithmeticOperations = parameter == null ? null : parameter?.ToString().Split('|');

                if (arithmeticOperations == null) return val;

                foreach (var operation in arithmeticOperations)
                {
                    // get the symbol for the operation (supported symbols: +, -, *, /, %) (always the first character)
                    char symbol = operation[0];

                    string arithmeticValue = operation.Substring(1, operation.Length - 1);

                    double arithmeticVal;
                    bool success = double.TryParse(arithmeticValue, out arithmeticVal);

                    // if successful, do arithmetic operation
                    if (success)
                    {
                        switch (symbol)
                        {
                            case '+':
                                val += arithmeticVal;
                                break;
                            case '-':
                                val -= arithmeticVal;
                                break;
                            case '*':
                                val *= arithmeticVal;
                                break;
                            case '/':
                                val /= arithmeticVal;
                                break;
                            case '%':
                                val %= arithmeticVal;
                                break;
                        }
                    }
                }

                return val;
            }

            return DependencyProperty.UnsetValue;
        }

        /// <summary>Converters the value using the specified arithmetic operations.</summary>
        /// <param name="value">The value produced by the binding source.</param>
        /// <param name="targetType">The type of the binding target property.</param>
        /// <param name="parameter">The converter parameter to use.</param>
        /// <param name="culture">The culture to use in the converter.</param>
        /// <returns>A converted value. If the method returns null, the valid null value is used.</returns>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return DependencyProperty.UnsetValue;
        }
    }
}
