#region

using System.Collections.Generic;
using System.Threading.Tasks;
using RIS.Model;

#endregion

namespace RIS.Business
{
    public interface IBusiness
    {
        void CheckConnection();

        #region AAO

        IList<Aao> GetAllAao();
        Task<IList<Aao>> GetAllAaoAsync();
        IList<Aao> GetAaoOverview();
        Task<IList<Aao>> GetAaoOverviewAsync();
        IList<AaoCondition> GetAllAaoCondition();
        Task<IList<AaoCondition>> GetAllAaoConditionAsync();
        Aao GetAaoById(int _id);
        int AddOrUpdateAao(Aao _entity);
        Task<int> AddOrUpdateAaoAsync(Aao _entity);
        void DeleteAao(Aao _entity);

        #endregion //AAO

        #region Alarmapp

        IList<AlarmappGroup> GetAllAlarmappGroup();
        Task<IList<AlarmappGroup>> GetAllAlarmappGroupAsync();
        IList<AlarmappGroup> GetAlarmappGroupOverview();
        Task<IList<AlarmappGroup>> GetAlarmappGroupOverviewAsync();
        AlarmappGroup GetAlarmappGroupById(int _id);
        AlarmappGroup GetAlarmappGroupByGroupId(string groupId);
        IList<AlarmappGroup> GetAlarmappGroupByVehicleIdentifier(string _identifier);
        IList<AlarmappGroup> GetAlarmappGroupByPagerIdentifier(string _identifier);
        IList<AlarmappGroup> GetAlarmappGroupWithAlarmfax();
        AlarmappDepartment GetAlarmappDepartmentById(string departmentId);
        AlarmappGroup AddOrUpdateAlarmappGroup(AlarmappGroup _entity);
        Task<AlarmappGroup> AddOrUpdateAlarmappGroupAsync(AlarmappGroup _entity);
        AlarmappDepartment AddOrUpdateAlarmappDepartment(AlarmappDepartment _entity);
        Task<AlarmappDepartment> AddOrUpdateAlarmappDepartmentAsync(AlarmappDepartment _entity);
        void DeleteAlarmappAll();
        void DeleteAlarmappDepartmentById(string departmentId);
        void DeleteAlarmappGroupById(string groupId);

        #endregion //AAO

        #region AMS

        IList<Ams> GetAllAms();
        Task<IList<Ams>> GetAllAmsAsync();
        IList<Ams> GetAmsOverview();
        Task<IList<Ams>> GetAmsOverviewAsync();
        Ams GetAmsById(int _id);
        int AddOrUpdateAms(Ams _entity);
        Task<int> AddOrUpdateAmsAsync(Ams _entity);
        void DeleteAms(Ams _entity);

        #endregion //AMS

        #region User

        IList<User> GetAllUser();
        Task<IList<User>> GetAllUserAsync();
        User GetUserById(int _id);
        IList<User> GetUserByPagers(IList<Pager> _pagerList);
        IList<User> GetUserWithFaxMessageServiceMailOn();
        IList<User> GetUserWithFaxMessageServiceFaxOn();
        int AddOrUpdateUser(User _entity);
        Task<int> AddOrUpdateUserAsync(User _entity);
        void DeleteUser(User _entity);

        #endregion //User

        #region Fileprints

        IList<Fileprint> GetAllFileprint();
        Task<IList<Fileprint>> GetAllFileprintAsync();
        Fileprint GetFileprintById(int _id);
        IList<FileprintCondition> GetAllFileprintCondition();
        Task<IList<FileprintCondition>> GetAllFileprintConditionAsync();
        int AddOrUpdateFileprint(Fileprint _entity);
        Task<int> AddOrUpdateFileprintAsync(Fileprint _entity);
        void DeleteFileprint(Fileprint _entity);

        #endregion //Fileprints

        #region Filter

        IList<Filter> GetAllFilter();
        Task<IList<Filter>> GetAllFilterAsync();
        Filter GetFilterById(int _id);
        IList<FilterField> GetAllFilterField();
        Task<IList<FilterField>> GetAllFilterFieldAsync();
        int AddOrUpdateFilter(Filter _entity);
        Task<int> AddOrUpdateFilterAsync(Filter _entity);
        void DeleteFilter(Filter _entity);

        #endregion //Filter

        #region Printer

        void LoadPrinters();
        void DeletePrinters();
        IList<Printer> GetAllPrinters();
        Task<IList<Printer>> GetAllPrintersAsync();
        IList<Printer> GetPrintersOverview();
        Task<IList<Printer>> GetPrintersOverviewAsync();
        Printer GetPrinterById(int _id);
        IList<Printer> GetPrintersForReport();
        IList<Printer> GetPrintersForFax();
        IList<Printer> GetPrintersForFileprint();
        int AddOrUpdatePrinter(Printer _entity);
        Task<int> AddOrUpdatePrinterAsync(Printer _entity);

        #endregion //Printer

        #region Pager

        IList<Pager> GetAllPager();
        Task<IList<Pager>> GetAllPagerAsync();
        Pager GetPagerById(int _id);
        Pager GetPagerByIdentifier(string _identifier);
        int AddOrUpdatePager(Pager _entity);
        Task<int> AddOrUpdatePagerAsync(Pager _entity);
        void DeletePager(Pager _entity);

        #endregion //Pager

        #region Vehicle

        IList<Vehicle> GetAllVehicle();
        Task<IList<Vehicle>> GetAllVehicleAsync();
        IList<Vehicle> GetVehiclesAreMain();
        IList<Vehicle> GetVehiclesAreEinsatzmittel();

        Vehicle GetVehicleById(int _id);
        Vehicle GetVehicleByBosIdentifier(string _bosIdentifier);
        Vehicle GetVehicleByPosition(int _row, int _column);
        void RemoveVehicleByPosition(int _row, int _column);
        int AddOrUpdateVehicle(Vehicle _entity);
        Task<int> AddOrUpdateVehicleAsync(Vehicle _entity);
        void DeleteVehicle(Vehicle _entity);

        #endregion //Vehicles
    }
}