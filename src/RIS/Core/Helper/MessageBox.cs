#region

using System.Windows;

#endregion

namespace RIS.Core.Helper
{
    public static class MessageBox
    {
        public static MessageBoxResult Show(string message, MessageBoxButton button, MessageBoxImage icon)
        {
            var _style = new Style();
            _style.Setters.Add(new Setter(Xceed.Wpf.Toolkit.MessageBox.YesButtonContentProperty, "Ja"));
            _style.Setters.Add(new Setter(Xceed.Wpf.Toolkit.MessageBox.NoButtonContentProperty, "Nein"));
            _style.Setters.Add(new Setter(Xceed.Wpf.Toolkit.MessageBox.CancelButtonContentProperty, "Abbruch"));

            var _result =
                Xceed.Wpf.Toolkit.MessageBox.Show(message, "Rescue-Information-System", button, icon, _style);
            return _result;
        }
    }
}