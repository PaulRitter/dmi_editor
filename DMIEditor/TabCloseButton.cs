using System.Windows;
using System.Windows.Controls;

namespace DMIEditor
{
    public class TabCloseButton : Button
    {
        public TabItem tabItem;
        public TabCloseButton(TabItem tabItem)
        {
            this.tabItem = tabItem;
            this.Margin = new Thickness(2d);
            TextBlock txt = new TextBlock
            {
                Background = System.Windows.Media.Brushes.Red,
                Foreground = System.Windows.Media.Brushes.White,
                Text = "X"
            };
            Content = txt;
        }
    }
}
