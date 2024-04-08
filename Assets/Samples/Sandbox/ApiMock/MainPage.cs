using System.ComponentModel;
using UnityEngine.UIElements;
using Button = UnityEngine.UIElements.Button;

namespace Sandbox.ApiMock
{
    public class MainPage : VisualElement
    {
        private readonly Label _label;
        private readonly MainViewModel _model;

        public MainPage(MainViewModel model)
        {
            _model = model;
            _model.PropertyChanged += OnPropertyChanged;

            _label = new Label("Hello World");
            var button = new Button
            {
                text = "Click me!"
            };

            var button2 = new Button
            {
                text = "Click me! Send Request"
            };

            button.clicked += _model.SendCommend.Execute;
            button2.clicked += _model.SendRequestCommend.Execute;
            Add(_label);
            Add(button);
            Add(button2);
        }

        private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(MainViewModel.Message)) _label.text = _model.Message;
        }
    }
}