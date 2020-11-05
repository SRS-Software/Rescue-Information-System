#region

using System;
using System.Collections.Generic;
using System.Reflection;
using RIS.Core.Helper;
using RIS.Model;
using SRS.Utilities;

#endregion

namespace RIS.Core.Fax
{
    public class Einsatz
    {
        public readonly string Guid = System.Guid.NewGuid().ToString();

        public Einsatz()
        {
            AlarmTime = DateTime.Now;
            Einsatzmittel = new List<Vehicle>();
        }

        public bool AbsenderValid { get; set; }
        public string FaxPath { get; set; }
        public DateTime AlarmTime { get; set; }
        public List<Vehicle> Einsatzmittel { get; set; }
        public string Ort { get; set; }
        public string Straße { get; set; }
        public string Hausnummer { get; set; }
        public string KoordinatenRW { get; set; }
        public string KoordinatenHW { get; set; }
        public string Objekt { get; set; }
        public string Abschnitt { get; set; }
        public string Kreuzung { get; set; }
        public string Station { get; set; }
        public string Schlagwort { get; set; }
        public string Stichwort { get; set; }
        public int Marker { get; set; }
        public string Bemerkung { get; set; }

        public bool IsValid()
        {
            if (string.IsNullOrEmpty(Ort) &&
                string.IsNullOrEmpty(Straße) &&
                string.IsNullOrEmpty(Hausnummer) &&
                string.IsNullOrEmpty(KoordinatenRW) &&
                string.IsNullOrEmpty(KoordinatenHW) &&
                string.IsNullOrEmpty(Objekt) &&
                string.IsNullOrEmpty(Abschnitt) &&
                string.IsNullOrEmpty(Kreuzung) &&
                string.IsNullOrEmpty(Station) &&
                string.IsNullOrEmpty(Schlagwort) &&
                string.IsNullOrEmpty(Stichwort) &&
                string.IsNullOrEmpty(Bemerkung) &&
                Einsatzmittel.Count == 0)
                return false;

            return true;
        }

        public Coordinaten KoordinatenWGS84()
        {
            if (!string.IsNullOrWhiteSpace(KoordinatenRW) && !string.IsNullOrWhiteSpace(KoordinatenHW))
            {
                var _coordinaten = new Coordinaten();
                _coordinaten.GaussToWGS84(Convert.ToDouble(KoordinatenRW), Convert.ToDouble(KoordinatenHW));
                if (_coordinaten.Latitude > 0 && _coordinaten.Longitude > 0)
                    return _coordinaten;
                Logger.WriteDebug(MethodBase.GetCurrentMethod(), "Gauß-Krüger-Koordinaten -> not found");
            }

            return null;
        }

        public override string ToString()
        {
            var _result = Environment.NewLine;

            _result += string.Format("Absender[{0}]\r\n", AbsenderValid ? "pass" : "fail");
            _result += $"AlarmTime[{AlarmTime}]\r\n";
            _result += $"Ort[{Ort}]\r\n";
            _result += $"Straße[{Straße}]\r\n";
            _result += $"Hausnummer[{Hausnummer}]\r\n";
            _result += $"Koordinaten-RW[{KoordinatenRW}]\r\n";
            _result += $"Koordinaten-HW[{KoordinatenHW}]\r\n";
            _result += $"Objekt[{Objekt}]\r\n";
            _result += $"Abschnitt[{Abschnitt}]\r\n";
            _result += $"Kreuzung[{Kreuzung}]\r\n";
            _result += $"Station[{Station}]\r\n";
            _result += $"Schlagwort[{Schlagwort}]\r\n";
            _result += $"Stichwort[{Stichwort}]\r\n";
            _result += $"Bemerkung[{Bemerkung}]\r\n";
            _result += "Einsatzmittel[";
            foreach (var _einsatzmittel in Einsatzmittel) _result += _einsatzmittel.Name + "|";

            _result += "]";

            return _result;
        }
    }
}