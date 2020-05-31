using System;
using System.Windows;

namespace DMIEditor
{
    public class ErrorPopupHelper
    {
        public static void Create(Exception e)
        {
            MessageBox.Show(e.ToString(), "Exception occured", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        public static void Create(String e)
        {
            MessageBox.Show(e, "Exception occured", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}