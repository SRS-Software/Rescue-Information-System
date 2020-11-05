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
        public IList<User> GetAllUser()
        {
            using (var _databaseContext = new DatabaseContext())
            {
                var _query = _databaseContext.Users.Include(u => u.Amss).OrderBy(a => a.Name)
                    .AsNoTracking();

                return _query.ToList();
            }
        }

        public async Task<IList<User>> GetAllUserAsync()
        {
            using (var _databaseContext = new DatabaseContext())
            {
                var query = _databaseContext.Users.Include(u => u.Amss).OrderBy(a => a.Name)
                    .AsNoTracking();

                return await query.ToListAsync().ConfigureAwait(false);
            }
        }

        public User GetUserById(int _id)
        {
            using (var _databaseContext = new DatabaseContext())
            {
                var _query = _databaseContext.Users.Include(u => u.Amss).Where(u => u.Id == _id)
                    .OrderBy(a => a.Name).AsNoTracking();

                return _query.SingleOrDefault();
            }
        }

        public IList<User> GetUserByPagers(IList<Pager> _pagerList)
        {
            using (var _databaseContext = new DatabaseContext())
            {
                var _userList = new List<User>();
                foreach (var _pager in _pagerList)
                foreach (var _ams in _pager.Amss)
                foreach (var _user in _ams.Users)
                    if (_userList.Where(u => u.MailAdresse == _user.MailAdresse).Count() == 0)
                        _userList.Add(_user);

                return _userList;
            }
        }

        public IList<User> GetUserWithFaxMessageServiceMailOn()
        {
            using (var _databaseContext = new DatabaseContext())
            {
                var _query = _databaseContext.Users
                    .Where(u => u.FaxMessageService_MailOn && u.FaxMessageService_FaxOn == false).OrderBy(a => a.Name)
                    .AsNoTracking();

                return _query.ToList();
            }
        }

        public IList<User> GetUserWithFaxMessageServiceFaxOn()
        {
            using (var _databaseContext = new DatabaseContext())
            {
                var _query = _databaseContext.Users.Where(u => u.FaxMessageService_FaxOn)
                    .OrderBy(a => a.Name).AsNoTracking();

                return _query.ToList();
            }
        }

        public int AddOrUpdateUser(User _entity)
        {
            if (_entity == null || !_entity.IsValid) new ArgumentNullException("User");

            using (var _databaseContext = new DatabaseContext())
            {
                _entity = _databaseContext.UpdateGraph(_entity);

                _databaseContext.SaveChanges();
                return _entity.Id;
            }
        }

        public async Task<int> AddOrUpdateUserAsync(User _entity)
        {
            if (_entity == null || !_entity.IsValid) new ArgumentNullException("User");

            using (var _databaseContext = new DatabaseContext())
            {
                _entity = _databaseContext.UpdateGraph(_entity);

                await _databaseContext.SaveChangesAsync();
                return _entity.Id;
            }
        }

        public void DeleteUser(User _entity)
        {
            if (_entity == null || !_entity.IsValid) new ArgumentNullException("User");

            using (var _databaseContext = new DatabaseContext())
            {
                _databaseContext.Users.Attach(_entity);
                _databaseContext.Users.Remove(_entity);
                _databaseContext.SaveChanges();
            }
        }
    }
}