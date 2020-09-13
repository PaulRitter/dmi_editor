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
using System.IO;
using DMI_Parser.Extended;
using Xceed.Wpf.Toolkit;
using DMIEditor.Undo;

namespace DMIEditor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>

    public partial class MainWindow : Window
    {
        public static MainWindow Current;
        private ColorPicker _colorPicker;
        public readonly UndoManager UndoManager;
        
        private EditorTool _selectedTool;

        public EditorTool SelectedTool
        {
            get => _selectedTool;
            set
            {
                if (value.ShouldBeKept)
                {
                    _selectedTool?.OnDeselected();
                    _selectedTool = value;
                    _selectedTool.OnSelected();
                    ToolSelectionChanged?.Invoke(this, EventArgs.Empty);                    
                }
                else
                {
                    value.OnSelected();
                }
                
            }
        }
        public event EventHandler ToolSelectionChanged;

        public MainWindow()
        {
            if(Current != null) throw new Exception("Mainwindow already exists");

            Current = this;
            UndoManager = new UndoManager();

            InitializeComponent();
            
            Button createNewFileBtn = new Button {Content = "New"};
            toolBar.Items.Add(createNewFileBtn);
            createNewFileBtn.Click += CreateNewFile;
            
            Button openFileBtn = new Button {Content = "Open"};
            toolBar.Items.Add(openFileBtn);
            openFileBtn.Click += OpenFileDialog;
            
            Button saveFileBtn = new Button {Content = "Save"};
            toolBar.Items.Add(saveFileBtn);
            saveFileBtn.Click += DMISave;
            
            Button saveAsFileBtn = new Button {Content = "Save As"};
            toolBar.Items.Add(saveAsFileBtn);
            saveAsFileBtn.Click += DMISaveAs;

            IEnumerable<Type> toolTypes = Assembly.GetAssembly(typeof(EditorTool)).GetTypes().Where<Type>(t => (t.BaseType?.BaseType == typeof(EditorTool) || t.BaseType?.BaseType?.BaseType == typeof(EditorTool)) && !t.IsAbstract );
            
            foreach (Type toolType in toolTypes)
            {
                EditorTool tool = (EditorTool)Activator.CreateInstance(toolType);
                ToolButton btn = new ToolButton(tool);
                toolBar.Items.Add(btn);
            }
            
            _colorPicker = new ColorPicker
            {
                SelectedColor = System.Windows.Media.Colors.Black, ShowDropDownButton = false
            };
            toolBar.Items.Add(_colorPicker);
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            if (Keyboard.Modifiers.HasFlag(ModifierKeys.Control) && e.Key == Key.Z)
            {
                UndoManager.Undo();
            }
        }

        private TabItem SelectedTab
        {
            get
            {
                return mainTabControl.Items.Cast<TabItem>().FirstOrDefault(item => item.IsSelected);
            }
        }

        public StateEditor GetStateEditor(DMIState state)
        {
            return (from TabItem tabItem in mainTabControl.Items select (FileEditor) tabItem.Content into f select f.GetStateEditor(state)).FirstOrDefault();
        }

        public FileEditor SelectedEditor => (FileEditor)SelectedTab?.Content;

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

        public void DMISave(object sender, EventArgs e)
        {
            if (SelectedEditor == null) return;
            if (SelectedEditor.Path == "")
            {
                DMISaveAs(sender, e);
                return;
            }
            
            FileStream stream = new FileStream(SelectedEditor.Path, FileMode.Create);
            SelectedEditor.DmiEx.SaveAsDmi(stream);
            stream.Close();
        }

        public void DMISaveAs(object sender, EventArgs e)
        {
            if (SelectedEditor == null) return;

            SaveFileDialog sfd = new SaveFileDialog
            {
                Filter = "DMI files (*.dmi)|*.dmi",
                InitialDirectory = SelectedEditor.Path
            };
            if (sfd.ShowDialog() == true)
            {
                try
                {
                    Stream fileStream = sfd.OpenFile();
                    SelectedEditor.DmiEx.SaveAsDmi(fileStream);
                    fileStream.Close();

                    SelectedEditor.Path = sfd.FileName;
                    ((TabItemHeader) SelectedTab.Header).Text.Text = sfd.SafeFileName;
                }
                catch (Exception ex)
                {
                    ErrorPopupHelper.Create(ex);
                }
            }
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
            
            DmiEX dmiFile = DmiEX.FromDmi(path);

            AddEditor(dmiFile, path);
        }

        private void AddEditor(DmiEX dmiEx, string path = "")
        {
            string filename = path == "" ? "unsaved file" : path.Split(@"\").Last();            
            if ((from TabItem editorTab in mainTabControl.Items select (FileEditor) editorTab.Content).Any(fileEditor => fileEditor.Path == path))
            {
                throw new WarningException($"File {filename} is already open");
            }
            
            FileEditor fE = new FileEditor(dmiEx, this, path);
            TabItem tabItem = new TabItem();
            
            TabItemHeader header = new TabItemHeader(filename, tabItem);
            header.TabCloseButton.Click += CloseButtonClicked;
            tabItem.Header = header;

            tabItem.Content = fE;
            mainTabControl.Items.Add(tabItem);
            mainTabControl.SelectedIndex = mainTabControl.Items.IndexOf(tabItem);
        }

        private void CreateNewFile(object sender, EventArgs e)
        {
            DmiEX dmiEx = new DmiEX(1.0f, 32, 32);
            try{
                AddEditor(dmiEx, "");
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
                Current.SelectedTool = _tool;
            }
        }

        private void CloseButtonClicked(object sender, EventArgs e)
        {
            TabCloseButton tcBtn = (TabCloseButton)sender;
            mainTabControl.Items.Remove(tcBtn.tabItem);
        }
    }
}
