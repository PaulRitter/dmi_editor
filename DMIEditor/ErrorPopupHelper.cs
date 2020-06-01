using System;
using System.Windows;

namespace DMIEditor
{
    public class ErrorPopupHelper
    {
        public static void Create(Exception e)
        {
            MessageBox.Show(e.Message, "Exception occured", MessageBoxButton.OK, MessageBoxImage.Error);
            Console.WriteLine(e);
        }

        public static void Create(String e)
        {
            MessageBox.Show(e, "Exception occured", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}