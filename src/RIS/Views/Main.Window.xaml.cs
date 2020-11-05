#region

using System;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using RIS.Properties;
using RIS.ViewModels;
using SRS.Utilities;
using Xceed.Wpf.AvalonDock.Layout;
using Xceed.Wpf.AvalonDock.Layout.Serialization;
using MessageBox = RIS.Core.Helper.MessageBox;

#endregion

namespace RIS.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            Loaded += MainWindow_Loaded;
            Closing += MainWindow_Closing;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                var serializer = new XmlLayoutSerializer(uxDockManager_Main);
                serializer.LayoutSerializationCallback += (s, args) =>
                {
                    if (args.Model != null)
                        switch (args.Model.ContentId)
                        {
                            //Do this for each LayoutAnchorable that has a menu item to show/hide it
                            case "uxView_Vehicles":
                                uxMenu_WinVehicles.DataContext = args.Model;
                                break;
                            case "uxView_Pagers":
                                uxMenu_WinPagers.DataContext = args.Model;
                                break;
                            case "uxView_Ticker":
                                uxMenu_WinTicker.DataContext = args.Model;
                                break;
                            case "uxView_Time":
                                uxMenu_WinTime.DataContext = args.Model;
                                break;
                            case "uxView_Riverlevel":
                                uxMenu_WinRiverlevel.DataContext = args.Model;
                                break;
                            case "uxView_Warnweather":
                                uxMenu_WinWarnweather.DataContext = args.Model;
                                break;
                            case "uxView_Webbrowser":
                                //uxMenu_WinWebbrowser.DataContext = args.Model;
                                break;
                        }
                };

                var _layoutFile = Path.Combine(Settings.Default.WorkingFolder, "LayoutMain.config");
                if (File.Exists(_layoutFile)) serializer.Deserialize(_layoutFile);
            }
            catch (Exception ex)
            {
                Logger.WriteError(MethodBase.GetCurrentMethod(), ex);
            }
        }

        private void MainWindow_Closing(object sender, CancelEventArgs e)
        {
            var _viewModel = DataContext as MainViewModel;
            if (_viewModel != null)
            {
                if (_viewModel.IsExit)
                    Application.Current.Shutdown();
                else
                    e.Cancel = true;
            }
        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if ((Keyboard.IsKeyDown(Key.LeftAlt) || Keyboard.IsKeyDown(Key.RightAlt)) && Keyboard.IsKeyDown(Key.F4))
                e.Handled = true;
        }

        private void MenuDockItem_Click(object sender, RoutedEventArgs e)
        {
            var _menuItem = sender as MenuItem;
            if (_menuItem == null) return;

            var _dockItem = _menuItem.DataContext as LayoutAnchorable;
            if (_dockItem == null) return;

            if (_dockItem.IsHidden)
            {
                _dockItem.Show();

                if (_dockItem.FloatingHeight == 0 || _dockItem.FloatingWidth == 0)
                    try
                    {
                        _dockItem.Float();
                    }
                    catch
                    {
                        _dockItem.Dock();
                    }
            }
            else
            {
                _dockItem.Hide();
            }
        }

        private void uxMenu_LayoutReset_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var _result = MessageBox.Show("Wollen Sie wirklich das aktuelle Layout verwerfen?",
                    MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (_result == MessageBoxResult.Yes)
                {
                    var layoutFile = Path.Combine(Settings.Default.WorkingFolder, "LayoutMain.config");
                    if (File.Exists(layoutFile)) File.Delete(layoutFile);

                    Logger.WriteDebug(MethodBase.GetCurrentMethod(), "Layout -> Reset");
                    MessageBox.Show("Das Layout wurde erfolgreich zurückgesetzt und das Programm wird nun beendet.",
                        MessageBoxButton.OK, MessageBoxImage.Information);

                    Closing -= MainWindow_Closing;
                    Application.Current.Shutdown();
                }
            }
            catch (Exception ex)
            {
                Logger.WriteError(MethodBase.GetCurrentMethod(), ex);
            }
        }

        private void uxMenu_LayoutSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var serializer = new XmlLayoutSerializer(uxDockManager_Main);
                serializer.Serialize(Path.Combine(Settings.Default.WorkingFolder, "LayoutMain.config"));

                Logger.WriteDebug(MethodBase.GetCurrentMethod(), "Layout -> Save");
                MessageBox.Show("Das Layout wurde erfolgreich gespeichert.", MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                Logger.WriteError(MethodBase.GetCurrentMethod(), ex);
            }
        }
    }
}