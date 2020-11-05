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
        public IList<Aao> GetAllAao()
        {
            using (var _databaseContext = new DatabaseContext())
            {
                var _query = _databaseContext.Aaos.Include(a => a.Aaos).Include(a => a.Condition)
                    .Include(a => a.Combination).Include(a => a.Vehicles.Select(v => v.Vehicle)).OrderBy(a => a.Name)
                    .AsNoTracking();

                return _query.ToList();
            }
        }

        public async Task<IList<Aao>> GetAllAaoAsync()
        {
            using (var _databaseContext = new DatabaseContext())
            {
                var _query = _databaseContext.Aaos.Include(a => a.Aaos).Include(a => a.Condition)
                    .Include(a => a.Combination).Include(a => a.Vehicles.Select(v => v.Vehicle)).OrderBy(a => a.Name)
                    .AsNoTracking();

                return await _query.ToListAsync().ConfigureAwait(false);
            }
        }

        public IList<Aao> GetAaoOverview()
        {
            using (var _databaseContext = new DatabaseContext())
            {
                var _query = _databaseContext.Aaos.Include(a => a.Condition).OrderBy(a => a.Name)
                    .AsNoTracking();

                return _query.ToList();
            }
        }

        public async Task<IList<Aao>> GetAaoOverviewAsync()
        {
            using (var _databaseContext = new DatabaseContext())
            {
                var _query = _databaseContext.Aaos.Include(a => a.Condition).OrderBy(a => a.Name)
                    .AsNoTracking();

                return await _query.ToListAsync().ConfigureAwait(false);
            }
        }

        public Aao GetAaoById(int _id)
        {
            using (var _databaseContext = new DatabaseContext())
            {
                var _query = _databaseContext.Aaos.Include(a => a.Aaos).Include(a => a.Combination)
                    .Include(a => a.Condition).Include(a => a.Vehicles.Select(v => v.Vehicle)).Where(a => a.Id == _id)
                    .AsNoTracking();

                return _query.SingleOrDefault();
            }
        }

        public IList<AaoCondition> GetAllAaoCondition()
        {
            using (var _databaseContext = new DatabaseContext())
            {
                var _query = _databaseContext.AaoConditions.OrderBy(a => a.Name).AsNoTracking();

                return _query.ToList();
            }
        }

        public async Task<IList<AaoCondition>> GetAllAaoConditionAsync()
        {
            using (var _databaseContext = new DatabaseContext())
            {
                var _query = _databaseContext.AaoConditions.OrderBy(a => a.Name).AsNoTracking();

                return await _query.ToListAsync().ConfigureAwait(false);
            }
        }

        public int AddOrUpdateAao(Aao _entity)
        {
            if (_entity == null || !_entity.IsValid) new ArgumentNullException("Aao");

            using (var _databaseContext = new DatabaseContext())
            {
                _entity = _databaseContext.UpdateGraph(_entity,
                    map => map.AssociatedEntity(a => a.Combination).AssociatedEntity(a => a.Condition)
                        .OwnedCollection(a => a.Vehicles, with => with.AssociatedEntity(v => v.Vehicle)));

                _databaseContext.SaveChanges();
                return _entity.Id;
            }
        }

        public async Task<int> AddOrUpdateAaoAsync(Aao _entity)
        {
            if (_entity == null || !_entity.IsValid) new ArgumentNullException("Aao");

            using (var _databaseContext = new DatabaseContext())
            {
                _entity = _databaseContext.UpdateGraph(_entity,
                    map => map.AssociatedEntity(a => a.Combination).AssociatedEntity(a => a.Condition)
                        .OwnedCollection(a => a.Vehicles, with => with.AssociatedEntity(v => v.Vehicle)));
                await _databaseContext.SaveChangesAsync();
                return _entity.Id;
            }
        }

        public void DeleteAao(Aao _entity)
        {
            if (_entity == null || !_entity.IsValid) new ArgumentNullException("Aao");

            using (var _databaseContext = new DatabaseContext())
            {
                _entity = GetAaoById(_entity.Id);
                _entity.Vehicles.ToList().ForEach(v => _databaseContext.AaoVehicles.Remove(v));
                _databaseContext.Aaos.Remove(_entity);

                _databaseContext.SaveChanges();
            }
        }
    }
}