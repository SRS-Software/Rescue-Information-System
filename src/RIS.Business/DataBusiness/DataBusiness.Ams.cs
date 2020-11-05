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
        public IList<Ams> GetAllAms()
        {
            using (var _databaseContext = new DatabaseContext())
            {
                var _query = _databaseContext.Amss.Include(a => a.Users).Include(a => a.Pagers)
                    .OrderBy(a => a.Name).AsNoTracking();

                return _query.ToList();
            }
        }

        public async Task<IList<Ams>> GetAllAmsAsync()
        {
            using (var _databaseContext = new DatabaseContext())
            {
                var _query = _databaseContext.Amss.Include(a => a.Users).Include(a => a.Pagers)
                    .OrderBy(a => a.Name).AsNoTracking();

                return await _query.ToListAsync().ConfigureAwait(false);
            }
        }

        public IList<Ams> GetAmsOverview()
        {
            using (var _databaseContext = new DatabaseContext())
            {
                var _query = _databaseContext.Amss.OrderBy(a => a.Name).AsNoTracking();

                return _query.ToList();
            }
        }

        public async Task<IList<Ams>> GetAmsOverviewAsync()
        {
            using (var _databaseContext = new DatabaseContext())
            {
                var _query = _databaseContext.Amss.OrderBy(a => a.Name).AsNoTracking();

                return await _query.ToListAsync().ConfigureAwait(false);
            }
        }

        public Ams GetAmsById(int _id)
        {
            using (var _databaseContext = new DatabaseContext())
            {
                var _query = _databaseContext.Amss.Include(a => a.Users).Include(a => a.Pagers)
                    .Where(a => a.Id == _id).AsNoTracking();

                return _query.SingleOrDefault();
            }
        }

        public int AddOrUpdateAms(Ams _entity)
        {
            if (_entity == null || !_entity.IsValid) new ArgumentNullException("Ams");

            using (var _databaseContext = new DatabaseContext())
            {
                _entity = _databaseContext.UpdateGraph(_entity,
                    map => map.AssociatedCollection(a => a.Users).AssociatedCollection(a => a.Pagers));

                _databaseContext.SaveChanges();
                return _entity.Id;
            }
        }

        public async Task<int> AddOrUpdateAmsAsync(Ams _entity)
        {
            if (_entity == null || !_entity.IsValid) new ArgumentNullException("Ams");

            using (var _databaseContext = new DatabaseContext())
            {
                _entity = _databaseContext.UpdateGraph(_entity,
                    map => map.AssociatedCollection(a => a.Users).AssociatedCollection(a => a.Pagers));

                await _databaseContext.SaveChangesAsync();
                return _entity.Id;
            }
        }

        public void DeleteAms(Ams _entity)
        {
            if (_entity == null || !_entity.IsValid) new ArgumentNullException("Ams");


            using (var _databaseContext = new DatabaseContext())
            {
                _databaseContext.Amss.Attach(_entity);
                _databaseContext.Amss.Remove(_entity);
                _databaseContext.SaveChanges();
            }
        }
    }
}