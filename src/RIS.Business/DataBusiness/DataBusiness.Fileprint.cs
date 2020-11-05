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
        public IList<Fileprint> GetAllFileprint()
        {
            using (var _databaseContext = new DatabaseContext())
            {
                var _query = _databaseContext.Fileprints.Include(f => f.Condition)
                    .Include(f => f.Printer).OrderBy(a => a.Name).AsNoTracking();

                return _query.ToList();
            }
        }

        public async Task<IList<Fileprint>> GetAllFileprintAsync()
        {
            using (var _databaseContext = new DatabaseContext())
            {
                var _query = _databaseContext.Fileprints.Include(f => f.Condition)
                    .Include(f => f.Printer).OrderBy(a => a.Name).AsNoTracking();

                return await _query.ToListAsync().ConfigureAwait(false);
            }
        }

        public Fileprint GetFileprintById(int _id)
        {
            using (var _databaseContext = new DatabaseContext())
            {
                var _query = _databaseContext.Fileprints.Include(f => f.Condition)
                    .Include(f => f.Printer).Where(z => z.Id == _id).OrderBy(a => a.Name).AsNoTracking();

                return _query.SingleOrDefault();
            }
        }

        public IList<FileprintCondition> GetAllFileprintCondition()
        {
            using (var _databaseContext = new DatabaseContext())
            {
                var _query = _databaseContext.FileprintConditions.OrderBy(a => a.Name)
                    .AsNoTracking();

                return _query.ToList();
            }
        }

        public async Task<IList<FileprintCondition>> GetAllFileprintConditionAsync()
        {
            using (var _databaseContext = new DatabaseContext())
            {
                var _query = _databaseContext.FileprintConditions.OrderBy(a => a.Name)
                    .AsNoTracking();

                return await _query.ToListAsync().ConfigureAwait(false);
            }
        }

        public int AddOrUpdateFileprint(Fileprint _entity)
        {
            if (_entity == null || !_entity.IsValid) new ArgumentNullException("Fileprint");

            using (var _databaseContext = new DatabaseContext())
            {
                _entity = _databaseContext.UpdateGraph(_entity,
                    map => map.AssociatedEntity(f => f.Condition).AssociatedEntity(f => f.Printer));

                _databaseContext.SaveChanges();
                return _entity.Id;
            }
        }

        public async Task<int> AddOrUpdateFileprintAsync(Fileprint _entity)
        {
            if (_entity == null || !_entity.IsValid) new ArgumentNullException("Fileprint");

            using (var _databaseContext = new DatabaseContext())
            {
                _entity = _databaseContext.UpdateGraph(_entity,
                    map => map.AssociatedEntity(f => f.Condition).AssociatedEntity(f => f.Printer));

                await _databaseContext.SaveChangesAsync();
                return _entity.Id;
            }
        }

        public void DeleteFileprint(Fileprint _entity)
        {
            if (_entity == null || !_entity.IsValid) new ArgumentNullException("Fileprint");


            using (var _databaseContext = new DatabaseContext())
            {
                _databaseContext.Fileprints.Attach(_entity);
                _databaseContext.Fileprints.Remove(_entity);
                _databaseContext.SaveChanges();
            }
        }
    }
}