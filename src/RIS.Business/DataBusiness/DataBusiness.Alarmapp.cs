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
        public IList<AlarmappGroup> GetAllAlarmappGroup()
        {
            using (var _databaseContext = new DatabaseContext())
            {
                var _query = _databaseContext.AlarmappGroups.Include(a => a.Pagers)
                    .Include(a => a.Vehicles).Include(a => a.Department).OrderBy(a => a.GroupName).AsNoTracking();

                return _query.ToList();
            }
        }

        public async Task<IList<AlarmappGroup>> GetAllAlarmappGroupAsync()
        {
            using (var _databaseContext = new DatabaseContext())
            {
                var _query = _databaseContext.AlarmappGroups.Include(a => a.Pagers)
                    .Include(a => a.Vehicles).Include(a => a.Department).OrderBy(a => a.GroupName).AsNoTracking();

                return await _query.ToListAsync().ConfigureAwait(false);
            }
        }

        public IList<AlarmappGroup> GetAlarmappGroupOverview()
        {
            using (var _databaseContext = new DatabaseContext())
            {
                var _query = _databaseContext.AlarmappGroups.Include(a => a.Department)
                    .OrderBy(a => a.GroupName).AsNoTracking();

                return _query.ToList();
            }
        }

        public async Task<IList<AlarmappGroup>> GetAlarmappGroupOverviewAsync()
        {
            using (var _databaseContext = new DatabaseContext())
            {
                var _query = _databaseContext.AlarmappGroups.Include(a => a.Department)
                    .OrderBy(a => a.GroupName).AsNoTracking();

                return await _query.ToListAsync().ConfigureAwait(false);
            }
        }

        public AlarmappGroup GetAlarmappGroupById(int _id)
        {
            using (var _databaseContext = new DatabaseContext())
            {
                var _query = _databaseContext.AlarmappGroups.Include(a => a.Pagers)
                    .Include(a => a.Vehicles).Include(a => a.Department).Where(a => a.Id == _id).AsNoTracking();

                return _query.SingleOrDefault();
            }
        }

        public AlarmappGroup GetAlarmappGroupByGroupId(string groupId)
        {
            using (var _databaseContext = new DatabaseContext())
            {
                var _query = _databaseContext.AlarmappGroups.Include(a => a.Pagers)
                    .Include(a => a.Vehicles).Include(a => a.Department).Where(a => a.GroupId == groupId)
                    .AsNoTracking();

                return _query.SingleOrDefault();
            }
        }

        public IList<AlarmappGroup> GetAlarmappGroupByVehicleIdentifier(string _identifier)
        {
            using (var _databaseContext = new DatabaseContext())
            {
                var _query = _databaseContext.AlarmappGroups
                    .Where(a => a.Vehicles.Any(v => v.BosIdentifier == _identifier)).AsNoTracking();

                return _query.ToList();
            }
        }

        public IList<AlarmappGroup> GetAlarmappGroupByPagerIdentifier(string _identifier)
        {
            using (var _databaseContext = new DatabaseContext())
            {
                var _query = _databaseContext.AlarmappGroups
                    .Where(a => a.Pagers.Any(p => p.Identifier == _identifier)).AsNoTracking();

                return _query.ToList();
            }
        }

        public IList<AlarmappGroup> GetAlarmappGroupWithAlarmfax()
        {
            using (var _databaseContext = new DatabaseContext())
            {
                var _query = _databaseContext.AlarmappGroups.Include(a => a.Vehicles)
                    .Include(a => a.Pagers).Where(a => a.FaxOn).AsNoTracking();

                return _query.ToList();
            }
        }

        public AlarmappDepartment GetAlarmappDepartmentById(string departmentId)
        {
            using (var _databaseContext = new DatabaseContext())
            {
                var _query = _databaseContext.AlarmappDepartments.Include(a => a.Groups)
                    .Where(a => a.DepartmentId == departmentId).AsNoTracking();

                return _query.SingleOrDefault();
            }
        }

        public AlarmappGroup AddOrUpdateAlarmappGroup(AlarmappGroup _entity)
        {
            if (_entity == null || !_entity.IsValid) new ArgumentNullException("AlarmappGroup");

            using (var _databaseContext = new DatabaseContext())
            {
                _entity = _databaseContext.UpdateGraph(_entity,
                    map => map.AssociatedCollection(a => a.Pagers).AssociatedCollection(a => a.Vehicles)
                        .AssociatedEntity(a => a.Department));

                _databaseContext.SaveChanges();
                return _entity;
            }
        }

        public async Task<AlarmappGroup> AddOrUpdateAlarmappGroupAsync(AlarmappGroup _entity)
        {
            if (_entity == null || !_entity.IsValid) new ArgumentNullException("AlarmappGroup");

            using (var _databaseContext = new DatabaseContext())
            {
                _entity = _databaseContext.UpdateGraph(_entity,
                    map => map.AssociatedCollection(a => a.Pagers).AssociatedCollection(a => a.Vehicles)
                        .AssociatedEntity(a => a.Department));

                await _databaseContext.SaveChangesAsync();
                return _entity;
            }
        }

        public AlarmappDepartment AddOrUpdateAlarmappDepartment(AlarmappDepartment _entity)
        {
            if (_entity == null || !_entity.IsValid) new ArgumentNullException("AlarmappDepartment");

            using (var _databaseContext = new DatabaseContext())
            {
                _entity = _databaseContext.UpdateGraph(_entity, map => map.AssociatedCollection(a => a.Groups));

                _databaseContext.SaveChanges();
                return _entity;
            }
        }

        public async Task<AlarmappDepartment> AddOrUpdateAlarmappDepartmentAsync(AlarmappDepartment _entity)
        {
            if (_entity == null || !_entity.IsValid) new ArgumentNullException("AlarmappDepartment");

            using (var _databaseContext = new DatabaseContext())
            {
                _entity = _databaseContext.UpdateGraph(_entity, map => map.AssociatedCollection(a => a.Groups));

                await _databaseContext.SaveChangesAsync();
                return _entity;
            }
        }

        public void DeleteAlarmappAll()
        {
            using (var _databaseContext = new DatabaseContext())
            {
                _databaseContext.AlarmappGroups.RemoveRange(_databaseContext.AlarmappGroups);
                _databaseContext.AlarmappDepartments.RemoveRange(_databaseContext.AlarmappDepartments);
                _databaseContext.SaveChanges();
            }
        }

        public void DeleteAlarmappDepartmentById(string departmentId)
        {
            using (var _databaseContext = new DatabaseContext())
            {
                var _entity = GetAlarmappDepartmentById(departmentId);
                if (_entity == null || !_entity.IsValid) return;

                _entity.Groups.ToList().ForEach(v => _databaseContext.AlarmappGroups.Remove(v));
                _databaseContext.AlarmappDepartments.Remove(_entity);
                _databaseContext.SaveChanges();
            }
        }

        public void DeleteAlarmappGroupById(string groupId)
        {
            using (var _databaseContext = new DatabaseContext())
            {
                var _entity = GetAlarmappGroupByGroupId(groupId);
                if (_entity == null || !_entity.IsValid) return;

                _databaseContext.AlarmappGroups.Remove(_entity);
                _databaseContext.SaveChanges();
            }
        }
    }
}