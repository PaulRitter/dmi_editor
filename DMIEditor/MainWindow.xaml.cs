using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Drawing;
using DMI_Parser;
using System.Linq;
using DMIEditor.Tools;
using System.Reflection;
using Microsoft.Win32;
using System.Windows.Input;
using System.Runtime.InteropServices;
using System.IO;

namespace DMIEditor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>

    public partial class MainWindow : Window
    {
        private List<FileEditor> editors = new List<FileEditor>();
        private EditorTool selectedTool;

        [DllImport("user32.dll")]
        public static extern bool RegisterHotKey(IntPtr hWnd, int id, int fsModifiers, int vlc);
        [DllImport("user32.dll")]
        public static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        public MainWindow()
        {
            InitializeComponent();

            openFileBtn.Click += openFileDialog;

            IEnumerable<Type> toolTypes = Assembly.GetAssembly(typeof(EditorTool)).GetTypes().Where<Type>(t => t.BaseType == typeof(EditorTool) );

            EditorTool[] tools = { new Tools.Pen(this), new Eraser(this), new Fill(this) , new Pipette(this) };

            bool first = true;
            bool left = true;
            foreach (Type toolType in toolTypes)
            {
                EditorTool tool = (EditorTool)Activator.CreateInstance(toolType, this);
                TextBlock txt = new TextBlock();
                txt.Text = tool.ToString();
                ToolButton btn = new ToolButton(tool);
                btn.Content = txt;
                btn.GroupName = "tools";
                btn.Click += toolBtnClicked;
                if (first)
                {
                    btn.IsChecked = true;
                    selectedTool = tool;
                    first = false;
                }

                if (left)
                {
                    toolGridLeft.Children.Add(btn);
                }
                else
                {
                    toolGridRight.Children.Add(btn);
                }
                left = !left;
            }

            //loadFile(@"D:\Workspaces\Github\vgstation13\icons\obj\chempuff.dmi");
            //loadFile(@"D:\Workspaces\Github\vgstation13\icons\mob\animal.dmi");
            //loadFile("D:/Workspaces/Github/vgstation13/icons/effects/alphacolors.dmi");
        }

        private FileEditor selectedEditor
        {
            get
            {
                foreach (TabItem item in mainTabControl.Items)
                {
                    if (item.IsSelected)
                    {
                        return (FileEditor)item.Content;
                    }
                }
                return null;
            }
        }

        private void toolBtnClicked(object sender, EventArgs e)
        {
            ToolButton btn = (ToolButton)sender;
            selectedTool = btn.tool;
        }

        public Color getColor()
        {
            if(!colorPicker.SelectedColor.HasValue)
                return Color.Black;
            System.Windows.Media.Color mc = colorPicker.SelectedColor.Value;
            return Color.FromArgb(mc.A, mc.R, mc.G, mc.B);
        }

        public void setColor(Color c)
        {
            System.Windows.Media.Color mc = System.Windows.Media.Color.FromArgb(c.A, c.R, c.G, c.B);
            colorPicker.SelectedColor = mc;
        }

        public EditorTool getTool()
        {
            return selectedTool;
        }

        public void openFileDialog(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "DMI files (*.dmi)|*.dmi";
            ofd.InitialDirectory = @"D:\Workspaces\Github\vgstation13\icons\";
            ofd.Multiselect = true;
            if(ofd.ShowDialog() == true)
            {
                foreach (string path in ofd.FileNames)
                {
                    loadFile(path);
                }
            }
        }

        public void loadFile(String path)
        {
            //mobs/animal.dmi skips 2nd line
            DMI dmiFile = DMI.fromFile(path);
            FileEditor fE = new FileEditor(dmiFile, this);
            TabItem tabItem = new TabItem();
            
            StackPanel sp = new StackPanel();
            TextBlock txt = new TextBlock();
            txt.Text = path.Split(@"\").Last<String>();
            txt.VerticalAlignment = VerticalAlignment.Center;
            sp.Orientation = Orientation.Horizontal;
            sp.Children.Add(txt);

            TabCloseButton cBtn = new TabCloseButton(tabItem);
            cBtn.Click += closeButtonClicked;
            sp.Children.Add(cBtn);

            tabItem.Header = sp;

            tabItem.Content = fE;
            mainTabControl.Items.Add(tabItem);
            mainTabControl.SelectedIndex = mainTabControl.Items.IndexOf(tabItem);
        }

        private class ToolButton : RadioButton
        {
            public readonly EditorTool tool;
            public ToolButton(EditorTool tool)
            {
                this.tool = tool;
            }
        }

        private void closeButtonClicked(object sender, EventArgs e)
        {
            TabCloseButton tcBtn = (TabCloseButton)sender;
            mainTabControl.Items.Remove(tcBtn.tabItem);
        }

        private class TabCloseButton : Button
        {
            public TabItem tabItem;
            public TabCloseButton(TabItem tabItem)
            {
                this.tabItem = tabItem;
                this.Margin = new Thickness(2d);
                TextBlock txt = new TextBlock();
                txt.Background = System.Windows.Media.Brushes.Red;
                txt.Foreground = System.Windows.Media.Brushes.White;
                txt.Text = "X";
                Content = txt;
            }
        }
    }
}
