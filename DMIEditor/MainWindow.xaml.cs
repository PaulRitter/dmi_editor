using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Drawing;
using DMI_Parser;
using DMIEditor.Tools;
using System.Reflection;
using Microsoft.Win32;
using System.Windows.Input;
using System.Runtime.InteropServices;
using System.IO;
using Xceed.Wpf.Toolkit;

namespace DMIEditor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>

    public partial class MainWindow : Window
    {
        private List<FileEditor> _editors = new List<FileEditor>();
        private EditorTool _selectedTool;
        public static MainWindow Current;
        private ColorPicker _colorPicker; 

        public MainWindow()
        {
            if(Current != null) throw new Exception("Mainwindow already exists");

            Current = this;
            
            InitializeComponent();
            
            Button openFileBtn = new Button {Content = "Open File"};
            toolBar.Items.Add(openFileBtn);
            openFileBtn.Click += OpenFileDialog;

            IEnumerable<Type> toolTypes = Assembly.GetAssembly(typeof(EditorTool)).GetTypes().Where<Type>(t => t.BaseType?.BaseType == typeof(EditorTool) && !t.IsAbstract );
            
            bool first = true;
            foreach (Type toolType in toolTypes)
            {
                EditorTool tool = (EditorTool)Activator.CreateInstance(toolType, this);
                TextBlock txt = new TextBlock {Text = tool.ToString()};
                ToolButton btn = new ToolButton(tool) {Content = txt, GroupName = "tools"};
                btn.Click += ToolBtnClicked;
                if (first)
                {
                    btn.IsChecked = true;
                    _selectedTool = tool;
                    first = false;
                }

                toolBar.Items.Add(btn);
            }
            
            _colorPicker = new ColorPicker
            {
                SelectedColor = System.Windows.Media.Colors.Black, ShowDropDownButton = false
            };
            toolBar.Items.Add(_colorPicker);
        }

        public FileEditor SelectedEditor
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

        private void ToolBtnClicked(object sender, EventArgs e)
        {
            ToolButton btn = (ToolButton)sender;
            _selectedTool = btn.Tool;
        }

        public Color GetColor()
        {
            if(!_colorPicker.SelectedColor.HasValue)
                return Color.Black;
            System.Windows.Media.Color mc = _colorPicker.SelectedColor.Value;
            return Color.FromArgb(mc.A, mc.R, mc.G, mc.B);
        }

        public void SetColor(Color c)
        {
            System.Windows.Media.Color mc = System.Windows.Media.Color.FromArgb(c.A, c.R, c.G, c.B);
            _colorPicker.SelectedColor = mc;
        }

        public EditorTool GetTool()
        {
            return _selectedTool;
        }

        public void OpenFileDialog(object sender, EventArgs e) 
        {
            OpenFileDialog ofd = new OpenFileDialog
            {
                Filter = "DMI files (*.dmi)|*.dmi",
                Multiselect = true
            };
            if(ofd.ShowDialog() == true)
            {
                foreach (string path in ofd.FileNames)
                {
                    LoadFile(path);
                }
            }
        }

        public void LoadFile(string path)
        {
            DmiEX.DmiEX dmiFile;
            try
            {
                dmiFile = DmiEX.DmiEX.FromDmi(path);
            }
            catch (ParsingException e)
            {
                ErrorPopupHelper.Create(e);
                return;
            }
            
            FileEditor fE = new FileEditor(dmiFile, this);
            TabItem tabItem = new TabItem();
            
            StackPanel sp = new StackPanel();
            TextBlock txt = new TextBlock
            {
                Text = path.Split(@"\").Last(), VerticalAlignment = VerticalAlignment.Center
            };
            sp.Orientation = Orientation.Horizontal;
            sp.Children.Add(txt);

            TabCloseButton cBtn = new TabCloseButton(tabItem);
            cBtn.Click += CloseButtonClicked;
            sp.Children.Add(cBtn);

            tabItem.Header = sp;

            tabItem.Content = fE;
            mainTabControl.Items.Add(tabItem);
            mainTabControl.SelectedIndex = mainTabControl.Items.IndexOf(tabItem);
        }

        private class ToolButton : RadioButton
        {
            public readonly EditorTool Tool;
            public ToolButton(EditorTool tool)
            {
                this.Tool = tool;
            }
        }

        private void CloseButtonClicked(object sender, EventArgs e)
        {
            TabCloseButton tcBtn = (TabCloseButton)sender;
            mainTabControl.Items.Remove(tcBtn.tabItem);
        }
    }
}
