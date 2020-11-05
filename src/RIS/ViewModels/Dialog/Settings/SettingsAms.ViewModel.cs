#region

using System;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Windows;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using GalaSoft.MvvmLight.Messaging;
using RIS.Business;
using RIS.Model;
using RIS.Views;
using SRS.Utilities;
using MessageBox = RIS.Core.Helper.MessageBox;

#endregion

namespace RIS.ViewModels
{
    public class SettingsAmsViewModel : ViewModelBase
    {
        private readonly Ams ams;
        private readonly IBusiness business;

        public SettingsAmsViewModel(IBusiness _business, int _id)
        {
            try
            {
                business = _business;

                //Query lists for selection            
                UserList = new ObservableCollection<User>(business.GetAllUserAsync().Result);
                PagerList = new ObservableCollection<Pager>(business.GetAllPagerAsync().Result);

                //Query item with relations
                ams = business.GetAmsById(_id);
                if (ams == null) ams = new Ams();

                //Do selection from list
            }
            catch (Exception ex)
            {
                Logger.WriteError(MethodBase.GetCurrentMethod(), ex);
            }
        }

        #region Commands

        private RelayCommand<object> addUserCommand;

        public RelayCommand<object> AddUserCommand
        {
            get
            {
                if (addUserCommand == null)
                    addUserCommand = new RelayCommand<object>(param => OnAddUser(param), param => CanAddUser(param));

                return addUserCommand;
            }
        }

        private bool CanAddUser(object param)
        {
            if (param == null) return false;

            if (ams.Users == null) return false;

            if (ams.Users.Where(a => a.Id == ((User) param).Id).SingleOrDefault() != null) return false;

            return true;
        }

        private void OnAddUser(object param)
        {
            ams.Users.Add((User) param);
            RaisePropertyChanged(() => AmsUserList);
        }

        private RelayCommand<object> removeUserCommand;

        public RelayCommand<object> RemoveUserCommand
        {
            get
            {
                if (removeUserCommand == null)
                    removeUserCommand =
                        new RelayCommand<object>(param => OnRemoveUser(param), param => CanRemoveUser(param));

                return removeUserCommand;
            }
        }

        private bool CanRemoveUser(object param)
        {
            if (param == null) return false;

            if (ams.Users == null) return false;

            if (ams.Users.Where(a => a.Id == ((User) param).Id).SingleOrDefault() == null) return false;

            return true;
        }

        private void OnRemoveUser(object param)
        {
            ams.Users.Remove((User) param);
            RaisePropertyChanged(() => AmsUserList);
        }

        private RelayCommand<object> addPagerCommand;

        public RelayCommand<object> AddPagerCommand
        {
            get
            {
                if (addPagerCommand == null)
                    addPagerCommand = new RelayCommand<object>(param => OnAddPager(param), param => CanAddPager(param));

                return addPagerCommand;
            }
        }

        private bool CanAddPager(object param)
        {
            if (param == null) return false;

            if (ams.Pagers == null) return false;

            if (ams.Pagers.Where(a => a.Id == ((Pager) param).Id).SingleOrDefault() != null) return false;

            return true;
        }

        private void OnAddPager(object param)
        {
            ams.Pagers.Add((Pager) param);
            RaisePropertyChanged(() => AmsPagerList);
        }

        private RelayCommand<object> removePagerCommand;

        public RelayCommand<object> RemovePagerCommand
        {
            get
            {
                if (removePagerCommand == null)
                    removePagerCommand =
                        new RelayCommand<object>(param => OnRemovePager(param), param => CanRemovePager(param));

                return removePagerCommand;
            }
        }

        private bool CanRemovePager(object param)
        {
            if (param == null) return false;

            if (ams.Pagers == null) return false;

            if (ams.Pagers.Where(a => a.Id == ((Pager) param).Id).SingleOrDefault() == null) return false;

            return true;
        }

        private void OnRemovePager(object param)
        {
            ams.Pagers.Remove((Pager) param);
            RaisePropertyChanged(() => AmsPagerList);
        }

        private RelayCommand saveCommand;

        public RelayCommand SaveCommand
        {
            get
            {
                if (saveCommand == null) saveCommand = new RelayCommand(() => OnSave(), () => CanSave());

                return saveCommand;
            }
        }

        private bool CanSave()
        {
            return true;
        }

        private void OnSave()
        {
            try
            {
                business.AddOrUpdateAms(ams);

                OnClose();
            }
            catch (Exception ex)
            {
                Logger.WriteError(MethodBase.GetCurrentMethod(), ex);
                MessageBox.Show("Leider ist ein Fehler aufgetreten:\r\n" + ex.Message, MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private RelayCommand closeCommand;

        public RelayCommand CloseCommand
        {
            get
            {
                if (closeCommand == null) closeCommand = new RelayCommand(() => OnClose(), () => CanClose());

                return closeCommand;
            }
        }

        private bool CanClose()
        {
            return true;
        }

        private void OnClose()
        {
            Messenger.Default.Send<SettingsAmsDialog>(null);
        }

        #endregion //Commands

        #region Events

        #endregion //Events

        #region Validation

        #endregion //Validation

        #region Public Properties

        public string WindowTitel => "Alarm-Message-Service";

        [Display(Description = "Beschreibung der Liste")]
        public string Name
        {
            get => ams.Name;
            set
            {
                if (ams.Name == value) return;

                ams.Name = value;

                RaisePropertyChanged(() => Name);
            }
        }

        public ObservableCollection<Pager> PagerList { get; }

        public ObservableCollection<Pager> AmsPagerList
        {
            get { return new ObservableCollection<Pager>(ams.Pagers.OrderBy(z => z.Identifier)); }
        }

        public ObservableCollection<User> UserList { get; }

        public ObservableCollection<User> AmsUserList
        {
            get { return new ObservableCollection<User>(ams.Users.OrderBy(z => z.Name)); }
        }

        #endregion //Public Properties

        #region Public Functions

        #endregion //Public Functions

        #region Private Properties

        #endregion //Private Properties

        #region Private Functions

        #endregion //Private Funtions
    }
}