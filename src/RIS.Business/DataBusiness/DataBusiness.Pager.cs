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
        public IList<Pager> GetAllPager()
        {
            using (var _databaseContext = new DatabaseContext())
            {
                var _query = _databaseContext.Pagers.OrderBy(a => a.Identifier).AsNoTracking();

                return _query.ToList();
            }
        }

        public async Task<IList<Pager>> GetAllPagerAsync()
        {
            using (var _databaseContext = new DatabaseContext())
            {
                var _query = _databaseContext.Pagers.OrderBy(a => a.Identifier).AsNoTracking();

                return await _query.ToListAsync().ConfigureAwait(false);
            }
        }

        public Pager GetPagerById(int _id)
        {
            using (var _databaseContext = new DatabaseContext())
            {
                var _query = _databaseContext.Pagers.Include(z => z.AlarmappGroups)
                    .Include(z => z.Amss.Select(a => a.Users)).Where(z => z.Id == _id).AsNoTracking();

                return _query.SingleOrDefault();
            }
        }

        public Pager GetPagerByIdentifier(string _identifier)
        {
            using (var _databaseContext = new DatabaseContext())
            {
                var _query = _databaseContext.Pagers.Include(z => z.AlarmappGroups)
                    .Include(z => z.Amss.Select(a => a.Users)).Where(z => z.Identifier == _identifier).AsNoTracking();

                return _query.FirstOrDefault();
            }
        }

        public int AddOrUpdatePager(Pager _entity)
        {
            if (_entity == null || !_entity.IsValid) new ArgumentNullException("Pager");

            using (var _databaseContext = new DatabaseContext())
            {
                _entity = _databaseContext.UpdateGraph(_entity);

                _databaseContext.SaveChanges();
                return _entity.Id;
            }
        }

        public async Task<int> AddOrUpdatePagerAsync(Pager _entity)
        {
            if (_entity == null || !_entity.IsValid) new ArgumentNullException("Pager");

            using (var _databaseContext = new DatabaseContext())
            {
                _entity = _databaseContext.UpdateGraph(_entity);

                await _databaseContext.SaveChangesAsync();
                return _entity.Id;
            }
        }

        public void DeletePager(Pager _entity)
        {
            if (_entity == null || !_entity.IsValid) new ArgumentNullException("Pager");

            using (var _databaseContext = new DatabaseContext())
            {
                _databaseContext.Pagers.Attach(_entity);
                _databaseContext.Pagers.Remove(_entity);
                _databaseContext.SaveChanges();
            }
        }
    }
}