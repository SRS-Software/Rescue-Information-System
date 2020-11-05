#region

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

#endregion

namespace RIS.Model
{
    public class Printer : EntityBase
    {
        public Printer()
        {
            Fileprints = new HashSet<Fileprint>();
        }

        [Required(ErrorMessage = "PrinterName is required")]
        public string PrinterName { get; set; }

        public int FaxCopies { get; set; }
        public bool FaxNumberOfVehiclesOn { get; set; }
        public int ReportCopies { get; set; }
        public bool ReportNumberOfVehiclesOn { get; set; }
        public int ReportDistance { get; set; }
        public bool ReportVehiclesOn { get; set; }
        public bool ReportDataSchlagwortOn { get; set; }
        public bool ReportDataStichwortOn { get; set; }
        public bool ReportDataOrtOn { get; set; }
        public bool ReportDataStraßeOn { get; set; }
        public bool ReportDataObjektOn { get; set; }
        public bool ReportDataStationOn { get; set; }
        public bool ReportDataKreuzungOn { get; set; }
        public bool ReportDataAbschnittOn { get; set; }
        public bool ReportDataBemerkungOn { get; set; }
        public bool ReportRouteImageOn { get; set; }
        public bool ReportRouteDescriptionOn { get; set; }
        public int FileprintCopies { get; set; }
        public bool FileprintNumberOfVehiclesOn { get; set; }
        public virtual ICollection<Fileprint> Fileprints { get; set; }
    }
}