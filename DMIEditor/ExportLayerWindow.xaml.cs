using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using DMI_Parser.Extended;
using DMIEditor.Undo;

namespace DMIEditor
{
    public partial class ExportLayerWindow : Window
    {
        private readonly DmiEXLayer _layer;
        public ExportLayerWindow(DmiEXLayer layer)
        {
            InitializeComponent();
            
            _layer = layer;
            
            //populate dmi selection list
            foreach (TabItem tabItem in MainWindow.Current.mainTabControl.Items)
            {
                FileEditor fe = (FileEditor) tabItem.Content;
                dmi_selector.Items.Add(new ListItem(fe.Path.Split(@"\").Last(), fe.DmiEx));
            }

            dmi_selector.SelectedIndex = 0;
        }

        private void OnButtonClicked(object sender, RoutedEventArgs e)
        {
            if (dmi_selector.SelectedItem == null) return;

            try
            {
                ListItem listItem = (ListItem)dmi_selector.SelectedItem;
                DmiEXState state = _layer.ToDmiExState(listItem.DmiEx, id_editor.Text);
                MainWindow.Current.UndoManager.RegisterUndoItem(new StateNewUndoItem(listItem.DmiEx, state));
                listItem.DmiEx.AddState(state);
                Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        private class ListItem
        {
            public readonly DmiEX DmiEx;
            public readonly string Name;

            public ListItem(string name, DmiEX dmiEx)
            {
                Name = name;
                DmiEx = dmiEx;
            }

            public override string ToString()
            {
                return $"{Name}";
            }
        }
    }
}