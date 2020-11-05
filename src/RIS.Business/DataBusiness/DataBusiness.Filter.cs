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
        public IList<Filter> GetAllFilter()
        {
            using (var _databaseContext = new DatabaseContext())
            {
                var _query = _databaseContext.Filters.Include(f => f.Field).AsNoTracking();

                return _query.ToList();
            }
        }

        public async Task<IList<Filter>> GetAllFilterAsync()
        {
            using (var _databaseContext = new DatabaseContext())
            {
                var _query = _databaseContext.Filters.Include(f => f.Field).AsNoTracking();

                return await _query.ToListAsync().ConfigureAwait(false);
            }
        }

        public IList<FilterField> GetAllFilterField()
        {
            using (var _databaseContext = new DatabaseContext())
            {
                var _query = _databaseContext.FilterFields.AsNoTracking();

                return _query.ToList();
            }
        }

        public async Task<IList<FilterField>> GetAllFilterFieldAsync()
        {
            using (var _databaseContext = new DatabaseContext())
            {
                var _query = _databaseContext.FilterFields.AsNoTracking();

                return await _query.ToListAsync().ConfigureAwait(false);
            }
        }

        public Filter GetFilterById(int _id)
        {
            using (var _databaseContext = new DatabaseContext())
            {
                var _query = _databaseContext.Filters.Include(f => f.Field).Where(f => f.Id == _id)
                    .AsNoTracking();

                return _query.SingleOrDefault();
            }
        }

        public int AddOrUpdateFilter(Filter _entity)
        {
            if (_entity == null || !_entity.IsValid) new ArgumentNullException("Filter");

            using (var _databaseContext = new DatabaseContext())
            {
                _entity = _databaseContext.UpdateGraph(_entity, map => map.AssociatedEntity(f => f.Field));

                _databaseContext.SaveChanges();
                return _entity.Id;
            }
        }

        public async Task<int> AddOrUpdateFilterAsync(Filter _entity)
        {
            if (_entity == null || !_entity.IsValid) new ArgumentNullException("Filter");

            using (var _databaseContext = new DatabaseContext())
            {
                _entity = _databaseContext.UpdateGraph(_entity, map => map.AssociatedEntity(f => f.Field));

                await _databaseContext.SaveChangesAsync();
                return _entity.Id;
            }
        }

        public void DeleteFilter(Filter _entity)
        {
            if (_entity == null || !_entity.IsValid) new ArgumentNullException("Filter");

            using (var _databaseContext = new DatabaseContext())
            {
                _databaseContext.Filters.Attach(_entity);
                _databaseContext.Filters.Remove(_entity);
                _databaseContext.SaveChanges();
            }
        }
    }
}