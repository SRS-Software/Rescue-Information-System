#region

using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using RefactorThis.GraphDiff;
using RIS.Data;
using RIS.Model;

#endregion

namespace RIS.Business
{
    public partial class DataBusiness : IBusiness
    {
        public IList<Vehicle> GetAllVehicle()
        {
            using (var _databaseContext = new DatabaseContext())
            {
                var _query = _databaseContext.Vehicles.OrderBy(a => a.Name).AsNoTracking();

                return _query.ToList();
            }
        }

        public async Task<IList<Vehicle>> GetAllVehicleAsync()
        {
            using (var _databaseContext = new DatabaseContext())
            {
                var _query = _databaseContext.Vehicles.OrderBy(a => a.Name).AsNoTracking();

                return await _query.ToListAsync().ConfigureAwait(false);
            }
        }

        public IList<Vehicle> GetVehiclesAreMain()
        {
            using (var _databaseContext = new DatabaseContext())
            {
                var _query = _databaseContext.Vehicles
                    .Where(v => v.MainRow != null && v.MainColumn != null).OrderBy(a => a.Name).AsNoTracking();

                return _query.ToList();
            }
        }

        public IList<Vehicle> GetVehiclesAreEinsatzmittel()
        {
            using (var _databaseContext = new DatabaseContext())
            {
                var _query = _databaseContext.Vehicles
                    .Where(v => !(v.FaxText == null || v.FaxText.Trim() == string.Empty)).OrderBy(a => a.Name)
                    .AsNoTracking();

                return _query.ToList();
            }
        }

        public Vehicle GetVehicleById(int _id)
        {
            using (var _databaseContext = new DatabaseContext())
            {
                var _query = _databaseContext.Vehicles.Include(v => v.AlarmappGroups)
                    .Where(v => v.Id == _id).AsNoTracking();

                return _query.SingleOrDefault();
            }
        }

        public Vehicle GetVehicleByBosIdentifier(string _bosIdentifier)
        {
            using (var _databaseContext = new DatabaseContext())
            {
                var _query = _databaseContext.Vehicles.Include(v => v.AlarmappGroups)
                    .Where(v => v.BosIdentifier == _bosIdentifier).AsNoTracking();

                return _query.FirstOrDefault();
            }
        }

        public Vehicle GetVehicleByPosition(int _row, int _column)
        {
            using (var _databaseContext = new DatabaseContext())
            {
                var _query = _databaseContext.Vehicles
                    .Where(v => v.MainRow == _row && v.MainColumn == _column).AsNoTracking();

                return _query.FirstOrDefault();
            }
        }

        public void RemoveVehicleByPosition(int _row, int _column)
        {
            using (var _databaseContext = new DatabaseContext())
            {
                var _vehicle = _databaseContext.Vehicles.Where(v => v.MainRow == _row && v.MainColumn == _column)
                    .AsNoTracking().FirstOrDefault();
                if (_vehicle == null) return;

                _vehicle.MainRow = null;
                _vehicle.MainColumn = null;

                AddOrUpdateVehicle(_vehicle);
            }
        }

        public int AddOrUpdateVehicle(Vehicle _entity)
        {
            if (_entity == null || !_entity.IsValid) new ArgumentNullException("Vehicle");

            using (var _databaseContext = new DatabaseContext())
            {
                _entity = _databaseContext.UpdateGraph(_entity);

                _databaseContext.SaveChanges();
                return _entity.Id;
            }
        }

        public async Task<int> AddOrUpdateVehicleAsync(Vehicle _entity)
        {
            if (_entity == null || !_entity.IsValid) new ArgumentNullException("Vehicle");

            using (var _databaseContext = new DatabaseContext())
            {
                _entity = _databaseContext.UpdateGraph(_entity);

                await _databaseContext.SaveChangesAsync();
                return _entity.Id;
            }
        }

        public void DeleteVehicle(Vehicle _entity)
        {
            if (_entity == null || !_entity.IsValid) new ArgumentNullException("Vehicle");

            using (var _databaseContext = new DatabaseContext())
            {
                _databaseContext.Vehicles.Attach(_entity);
                _databaseContext.Vehicles.Remove(_entity);
                _databaseContext.SaveChanges();
            }
        }
    }
}