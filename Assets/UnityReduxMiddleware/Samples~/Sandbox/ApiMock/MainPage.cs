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

            var button3 = new Button
            {
                text = "Click me! Send Request Async"
            };

            button.clicked += _model.SendCommend.Execute;
            button2.clicked += _model.SendRequestCommend.Execute;
            button3.clicked += _model.SendRequestAsyncCommend.Execute;
            Add(_label);
            Add(button);
            Add(button2);
            Add(button3);
        }

        private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(MainViewModel.Message)) _label.text = _model.Message;
        }
    }
}