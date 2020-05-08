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
    }
}