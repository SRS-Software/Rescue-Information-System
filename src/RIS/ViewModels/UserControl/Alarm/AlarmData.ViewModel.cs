#region

using System;
using System.Collections.ObjectModel;
using System.Reflection;
using GalaSoft.MvvmLight;
using RIS.Core.Fax;
using RIS.Properties;
using SRS.Utilities;

#endregion

namespace RIS.ViewModels
{
    public class AlarmDataViewModel : ViewModelBase
    {
        private readonly string einsatzGuid;

        public AlarmDataViewModel(Einsatz _einsatz)
        {
            try
            {
                einsatzGuid = _einsatz.Guid;

                //Stichwort
                if (!string.IsNullOrWhiteSpace(_einsatz.Stichwort)) DataList.Add(_einsatz.Stichwort);

                //Schlagwort
                if (!string.IsNullOrWhiteSpace(_einsatz.Schlagwort)) DataList.Add(_einsatz.Schlagwort);

                //Abschnitt
                if (!string.IsNullOrWhiteSpace(_einsatz.Abschnitt)) DataList.Add(_einsatz.Abschnitt);

                //Kreuzung
                if (!string.IsNullOrWhiteSpace(_einsatz.Kreuzung)) DataList.Add(_einsatz.Kreuzung);

                //Station
                if (!string.IsNullOrWhiteSpace(_einsatz.Station)) DataList.Add(_einsatz.Station);

                //Objekt
                if (!string.IsNullOrWhiteSpace(_einsatz.Objekt)) DataList.Add(_einsatz.Objekt);

                //Straße + Hausnr
                if (!string.IsNullOrWhiteSpace(_einsatz.Straße))
                    DataList.Add(_einsatz.Straße + " " + _einsatz.Hausnummer);

                //Ort
                if (!string.IsNullOrWhiteSpace(_einsatz.Ort)) DataList.Add(_einsatz.Ort);

                //Bemerkung
                if (!string.IsNullOrWhiteSpace(_einsatz.Bemerkung) && Settings.Default.AlarmData_Width > 0)
                {
                    var _bemerkung = string.Empty;
                    var _maxSign = Convert.ToInt32(Settings.Default.AlarmData_Width) / 25;
                    var _signIndex = 0;

                    foreach (var _sign in _einsatz.Bemerkung)
                        if (_signIndex++ >= _maxSign && char.IsWhiteSpace(_sign))
                        {
                            _bemerkung += Environment.NewLine;
                            _signIndex = 0;
                        }
                        else
                        {
                            _bemerkung += _sign;
                        }

                    DataList.Add(_bemerkung);
                }
                else if (!string.IsNullOrWhiteSpace(_einsatz.Bemerkung))
                {
                    DataList.Add(_einsatz.Bemerkung);
                }
            }
            catch (Exception ex)
            {
                Logger.WriteError(MethodBase.GetCurrentMethod(), ex);
            }
        }

        #region Public Properties

        public ObservableCollection<string> DataList { get; } = new ObservableCollection<string>();

        #endregion //Public Properties

        public override void Cleanup()
        {
            base.Cleanup();
        }

        #region Commands

        #endregion //Commands

        #region Events

        #endregion //Events

        #region Validation

        #endregion //Validation

        #region Public Functions

        #endregion //Public Functions

        #region Private Properties

        #endregion //Private Properties

        #region Private Functions

        #endregion //Private Funtions
    }
}