#region

using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Drawing.Printing;
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
        public void LoadPrinters()
        {
            using (var _databaseContext = new DatabaseContext())
            {
                //Go throw all installed printers on the system.
                foreach (string _systemPrinterName in PrinterSettings.InstalledPrinters)
                {
                    var _printer = _databaseContext.Printers.Where(p => p.PrinterName == _systemPrinterName)
                        .SingleOrDefault();

                    //If printer not in db create new
                    if (_printer == null)
                    {
                        _printer = new Printer
                        {
                            PrinterName = _systemPrinterName
                        };
                        _databaseContext.Printers.Add(_printer);
                    }
                }

                _databaseContext.SaveChanges();
            }
        }

        public void DeletePrinters()
        {
            using (var _databaseContext = new DatabaseContext())
            {
                _databaseContext.Printers.RemoveRange(_databaseContext.Printers.ToList());
                _databaseContext.SaveChanges();
            }
        }

        public IList<Printer> GetAllPrinters()
        {
            using (var _databaseContext = new DatabaseContext())
            {
                var _query = _databaseContext.Printers.Include(p => p.Fileprints)
                    .OrderBy(a => a.PrinterName).AsNoTracking();

                return _query.ToList();
            }
        }

        public async Task<IList<Printer>> GetAllPrintersAsync()
        {
            using (var _databaseContext = new DatabaseContext())
            {
                var _query = _databaseContext.Printers.Include(p => p.Fileprints)
                    .OrderBy(a => a.PrinterName).AsNoTracking();

                return await _query.ToListAsync().ConfigureAwait(false);
            }
        }

        public IList<Printer> GetPrintersOverview()
        {
            using (var _databaseContext = new DatabaseContext())
            {
                var _query = _databaseContext.Printers.OrderBy(a => a.PrinterName).AsNoTracking();

                return _query.ToList();
            }
        }

        public async Task<IList<Printer>> GetPrintersOverviewAsync()
        {
            using (var _databaseContext = new DatabaseContext())
            {
                var _query = _databaseContext.Printers.AsNoTracking();

                return await _query.ToListAsync().ConfigureAwait(false);
            }
        }

        public Printer GetPrinterById(int _id)
        {
            using (var _databaseContext = new DatabaseContext())
            {
                var _query = _databaseContext.Printers.Include(p => p.Fileprints)
                    .Where(p => p.Id == _id).AsNoTracking();

                return _query.SingleOrDefault();
            }
        }

        public IList<Printer> GetPrintersForReport()
        {
            using (var _databaseContext = new DatabaseContext())
            {
                var _query = _databaseContext.Printers.Include(p => p.Fileprints)
                    .Where(p => p.ReportCopies > 0 || p.ReportNumberOfVehiclesOn).OrderBy(a => a.PrinterName)
                    .AsNoTracking();

                return _query.ToList();
            }
        }

        public IList<Printer> GetPrintersForFax()
        {
            using (var _databaseContext = new DatabaseContext())
            {
                var _query = _databaseContext.Printers.Include(p => p.Fileprints)
                    .Where(p => p.FaxCopies > 0 || p.FaxNumberOfVehiclesOn).OrderBy(a => a.PrinterName).AsNoTracking();

                return _query.ToList();
            }
        }

        public IList<Printer> GetPrintersForFileprint()
        {
            using (var _databaseContext = new DatabaseContext())
            {
                var _query = _databaseContext.Printers.Include(p => p.Fileprints)
                    .Where(p => p.FileprintCopies > 0 || p.FileprintNumberOfVehiclesOn).OrderBy(a => a.PrinterName)
                    .AsNoTracking();

                return _query.ToList();
            }
        }

        public int AddOrUpdatePrinter(Printer _entity)
        {
            if (_entity == null || !_entity.IsValid) new ArgumentNullException("Printer");

            using (var _databaseContext = new DatabaseContext())
            {
                _entity = _databaseContext.UpdateGraph(_entity, map => map.AssociatedCollection(p => p.Fileprints));

                _databaseContext.SaveChanges();
                return _entity.Id;
            }
        }

        public async Task<int> AddOrUpdatePrinterAsync(Printer _entity)
        {
            if (_entity == null || !_entity.IsValid) new ArgumentNullException("Printer");

            using (var _databaseContext = new DatabaseContext())
            {
                _entity = _databaseContext.UpdateGraph(_entity, map => map.AssociatedCollection(p => p.Fileprints));

                await _databaseContext.SaveChangesAsync();
                return _entity.Id;
            }
        }
    }
}