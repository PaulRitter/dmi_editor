using System.Windows;
using System.Windows.Controls;

namespace DMIEditor
{
    public abstract class PromptWindow : Window
    {
        private TextBlock _prompt;
        private TextBox _textBox;
        private Button _createButton;
        private StackPanel _stackPanel;

        private bool _closeOnConfirm;
        
        public PromptWindow(string prompt, string okBtnText = "Confirm", bool closeOnConfirm = true)
        {
            _closeOnConfirm = closeOnConfirm;
            
            _prompt = new TextBlock{ Text = prompt };            
            _textBox = new TextBox();
            _createButton = new Button{ Content = okBtnText };
            _createButton.Click += (s, e) =>
            {
                promptSent(_textBox.Text);
                if(_closeOnConfirm) Close();
            };
            
            _stackPanel = new StackPanel
            {
                Orientation = Orientation.Vertical,
                Children =
                {
                    _prompt,
                    _textBox,
                    _createButton
                }
            };
            
            Content = _stackPanel;
        }

        protected abstract void promptSent(string prompt);
    }
}