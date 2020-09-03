using System.Windows;
using System.Windows.Controls;

namespace DMIEditor
{
    public class TabItemHeader : StackPanel
    {
        public readonly TextBlock Text;
        public readonly TabCloseButton TabCloseButton;
        public readonly TabItem TabItem;
        
        public TabItemHeader(string text, TabItem tabItem)
        {
            
            Text = new TextBlock
            {
                Text = text, VerticalAlignment = VerticalAlignment.Center
            };

            TabItem = tabItem;

            TabCloseButton = new TabCloseButton(TabItem);
            
            Orientation = Orientation.Horizontal;
            Children.Add(Text);
            Children.Add(TabCloseButton);
        }
    }
}