using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using DMIEditor.DmiEX;

namespace DMIEditor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>

    public partial class MainWindow : Window
    {
        private EditorTool _selectedTool;
        public static MainWindow Current;
        private ColorPicker _colorPicker;

        public event EventHandler ToolSelectionChanged;

        public MainWindow()
        {
            if(Current != null) throw new Exception("Mainwindow already exists");

            Current = this;
            
            InitializeComponent();
            
            Button createNewFileBtn = new Button {Content = "New"};
            toolBar.Items.Add(createNewFileBtn);
            createNewFileBtn.Click += CreateNewFile;
            
            Button openFileBtn = new Button {Content = "Open"};
            toolBar.Items.Add(openFileBtn);
            openFileBtn.Click += OpenFileDialog;

            IEnumerable<Type> toolTypes = Assembly.GetAssembly(typeof(EditorTool)).GetTypes().Where<Type>(t => t.BaseType?.BaseType == typeof(EditorTool) && !t.IsAbstract );
            
            foreach (Type toolType in toolTypes)
            {
                EditorTool tool = (EditorTool)Activator.CreateInstance(toolType, this);
                ToolButton btn = new ToolButton(tool);
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
                    try
                    {
                        LoadFile(path);
                    }
                    catch (Exception ex)
                    {
                        ErrorPopupHelper.Create(ex);
                    }
                }
            }
        }

        public void LoadFile(string path)
        {
            string filename = path.Split(@"\").Last();            
            if ((from TabItem editorTab in mainTabControl.Items select (FileEditor) editorTab.Content).Any(fileEditor => fileEditor.Path == path))
            {
                throw new WarningException($"File {filename} is already open");
            }
            
            DmiEX.DmiEX dmiFile = DmiEX.DmiEX.FromDmi(path);

            addEditor(dmiFile, path);
        }

        private void addEditor(DmiEX.DmiEX dmiEx, string path)
        {
            string filename = path.Split(@"\").Last();            
            if ((from TabItem editorTab in mainTabControl.Items select (FileEditor) editorTab.Content).Any(fileEditor => fileEditor.Path == path))
            {
                throw new WarningException($"File {filename} is already open");
            }
            
            FileEditor fE = new FileEditor(dmiEx, this, path);
            TabItem tabItem = new TabItem();
            
            StackPanel sp = new StackPanel();
            TextBlock txt = new TextBlock
            {
                Text = filename, VerticalAlignment = VerticalAlignment.Center
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

        private void CreateNewFile(object sender, EventArgs e)
        {
            DmiEX.DmiEX dmiEx = new DmiEX.DmiEX(1.0f, 32, 32);
            try{
                addEditor(dmiEx, "unnamed.dmi");
            }
            catch (Exception ex)
            {
                ErrorPopupHelper.Create(ex);
            }
        }
        
        private class ToolButton : Button
        {
            private readonly EditorTool _tool;
            public ToolButton(EditorTool tool)
            {
                this._tool = tool;
                this.Content = tool.Name;
                Current.ToolSelectionChanged += UpdatePressed;
            }

            private void UpdatePressed(object sender, EventArgs e)
            {
                IsPressed = Current._selectedTool == _tool;
            }

            protected override void OnClick()
            {
                base.OnClick();
                Current._selectedTool = _tool;
                Current.ToolSelectionChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        private void CloseButtonClicked(object sender, EventArgs e)
        {
            TabCloseButton tcBtn = (TabCloseButton)sender;
            mainTabControl.Items.Remove(tcBtn.tabItem);
        }
    }
}
