#region

using System.Diagnostics.CodeAnalysis;
using CommonServiceLocator;

#endregion

namespace RIS.ViewModels
{
    public class ViewModelLocator
    {
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic",
            Justification = "This non-static member is needed for data binding purposes.")]
        public MainViewModel MainVM => ServiceLocator.Current.GetInstance<MainViewModel>();

        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic",
            Justification = "This non-static member is needed for data binding purposes.")]
        public MainPagersViewModel MainPagersVM => ServiceLocator.Current.GetInstance<MainPagersViewModel>();

        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic",
            Justification = "This non-static member is needed for data binding purposes.")]
        public MainTickerViewModel MainTickerVM => ServiceLocator.Current.GetInstance<MainTickerViewModel>();

        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic",
            Justification = "This non-static member is needed for data binding purposes.")]
        public MainVehiclesViewModel MainVehiclesVM => ServiceLocator.Current.GetInstance<MainVehiclesViewModel>();

        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic",
            Justification = "This non-static member is needed for data binding purposes.")]
        public MainWarnweatherViewModel MainWarnweatherVM =>
            ServiceLocator.Current.GetInstance<MainWarnweatherViewModel>();

        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic",
            Justification = "This non-static member is needed for data binding purposes.")]
        public MainRiverlevelViewModel MainRiverlevelVM =>
            ServiceLocator.Current.GetInstance<MainRiverlevelViewModel>();

        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic",
            Justification = "This non-static member is needed for data binding purposes.")]
        public MainWebbrowserViewModel MainWebbrowserVM =>
            ServiceLocator.Current.GetInstance<MainWebbrowserViewModel>();
    }
}