#region

using System;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using CommonServiceLocator;
using RIS.Core;
using RIS.Core.Fax;
using RIS.Properties;
using RIS.ViewModels;
using SRS.Utilities;
using Xceed.Wpf.AvalonDock.Layout;
using Xceed.Wpf.AvalonDock.Layout.Serialization;
using MessageBox = RIS.Core.Helper.MessageBox;

#endregion

namespace RIS.Views
{
    public partial class AlarmWindow : Window
    {
        private readonly IMonitorService monitorService;

        public AlarmWindow(Einsatz _einsatz)
        {
            monitorService = ServiceLocator.Current.GetInstance<IMonitorService>();

            InitializeComponent();

            //Set time when window should be closed
            CloseTime = DateTime.Now.Add(Settings.Default.Monitor_AlarmTime);

            var _viewModel = new AlarmViewModel(_einsatz);
            _viewModel.CloseRequested += (sender, e) => { Close(); };
            DataContext = _viewModel;

            Loaded += AlarmWindow_Loaded;
            Closing += AlarmWindow_Closing;

            //Add to alarmWindowList
            monitorService.AddAlarmWindow(this);
        }

        public DateTime CloseTime { get; set; }

        private void AlarmWindow_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                var serializer = new XmlLayoutSerializer(uxDockManager_Alarm);
                serializer.LayoutSerializationCallback += (s, args) =>
                {
                    if (args.Model != null)
                        switch (args.Model.ContentId)
                        {
                            //Do this for each LayoutAnchorable that has a menu item to show/hide it
                            case "uxMenu_MainVehicles":
                                uxMenu_MainVehicles.DataContext = args.Model;
                                break;
                            case "uxMenu_MainTime":
                                uxMenu_MainTime.DataContext = args.Model;
                                break;
                            case "uxMenu_Vehicles":
                                uxMenu_Vehicles.DataContext = args.Model;
                                break;
                            case "uxMenu_Timer":
                                uxMenu_Timer.DataContext = args.Model;
                                break;
                            case "uxMenu_Data":
                                uxMenu_Data.DataContext = args.Model;
                                break;
                            case "uxMenu_Route":
                                uxMenu_Route.DataContext = args.Model;
                                break;
                            case "uxMenu_AlarmappOverview":
                                uxMenu_AlarmappOverview.DataContext = args.Model;
                                break;
                            case "uxMenu_AlarmappUsers":
                                uxMenu_AlarmappUsers.DataContext = args.Model;
                                break;
                        }
                };

                var _layoutFile = Path.Combine(Settings.Default.WorkingFolder, "LayoutAlarm.config");
                if (File.Exists(_layoutFile)) serializer.Deserialize(_layoutFile);
            }
            catch (Exception ex)
            {
                Logger.WriteError(MethodBase.GetCurrentMethod(), ex);
            }
        }

        private void AlarmWindow_Closing(object sender, CancelEventArgs e)
        {
            var _viewModel = DataContext as AlarmViewModel;
            if (_viewModel != null) _viewModel.Cleanup();

            //Remove from alarmWindowList
            monitorService.RemoveAlarmWindow(this);
        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (Keyboard.IsKeyDown(Key.Escape)) Close();
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
                    var layoutFile = Path.Combine(Settings.Default.WorkingFolder, "LayoutAlarm.config");
                    if (File.Exists(layoutFile)) File.Delete(layoutFile);

                    Logger.WriteDebug(MethodBase.GetCurrentMethod(), "Layout -> Reset");
                    MessageBox.Show("Das Layout wurde erfolgreich zurückgesetzt und das Fenster wird nun geschlossen.",
                        MessageBoxButton.OK, MessageBoxImage.Information);

                    //Remove from alarmWindowList
                    monitorService.RemoveAlarmWindow(this);

                    Closing -= AlarmWindow_Closing;
                    Close();
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
                var serializer = new XmlLayoutSerializer(uxDockManager_Alarm);
                serializer.Serialize(Path.Combine(Settings.Default.WorkingFolder, "LayoutAlarm.config"));

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