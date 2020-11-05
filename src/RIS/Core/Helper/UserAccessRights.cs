#region

using System;
using System.Collections.Generic;
using System.IO;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Text;

#endregion

namespace RIS.Core.Helper
{
    public class UserAccessRights
    {
        private readonly Dictionary<string, AccessRight> _accessRights;
        private readonly WindowsIdentity _userIdentity;
        private FileInfo _fileInfo;

        /// <summary>
        ///     Initialisiert eine neue Instanz der UserAccessRights-Klasse für den angegebenen Pfad des aktuellen Benutzers.
        /// </summary>
        /// <param name="path">Der Pfad dessen Berechtigungen ermittelt werden sollen.</param>
        /// <exception cref="System.IO.FileNotFoundException">Die Datei kann nicht gefunden werden.</exception>
        /// <exception cref="System.IO.DirectoryNotFoundException">Das Verzeichnis kann nicht gefunden werden.</exception>
        /// <exception cref="System.ArgumentNullException">fileName ist null.</exception>
        /// <exception cref="System.Security.SecurityException">Der Aufrufer verfügt nicht über die erforderliche Berechtigung.</exception>
        /// <exception cref="System.ArgumentException">
        ///     Der Dateiname ist leer, oder er enthält nur Leerräume oder ungültige
        ///     Zeichen.
        /// </exception>
        /// <exception cref="System.UnauthorizedAccessException">Der Zugriff auf fileName wird verweigert.</exception>
        /// <exception cref="System.IO.PathTooLongException">
        ///     Der angegebene Pfad und/oder der Dateiname überschreiten die vom
        ///     System vorgegebene Höchstlänge. Beispielsweise dürfen auf Windows-Plattformen Pfade nicht länger als 247 Zeichen
        ///     und Dateinamen nicht länger als 259 Zeichen sein.
        /// </exception>
        /// <exception cref="System.NotSupportedException">fileName enthält einen Doppelpunkt (:) innerhalb der Zeichenfolge.</exception>
        /// <remarks></remarks>
        public UserAccessRights(string path)
        {
            // Prüfe, ob Datei/Verzeichnis vorhanden
            if (!File.Exists(path) && !Directory.Exists(path))
            {
                // nicht vorhanden -> Prüfe, ob Dateierweiterung fehlt
                if (string.IsNullOrEmpty(System.IO.Path.GetExtension(path)))
                    // true -> Verzeichnis existiert nicht
                    throw new DirectoryNotFoundException();
                throw new FileNotFoundException();
            }

            _fileInfo = new FileInfo(path);
            _userIdentity = WindowsIdentity.GetCurrent();
            _accessRights = new Dictionary<string, AccessRight>();

            // Iteriere durch jede Berechtigung und füge diese dem Dictionary hinzu
            foreach (var r in Enum.GetNames(typeof(FileSystemRights))) _accessRights.Add(r, new AccessRight());

            // Intialisiere die Benutzerrechte für den Pfad
            InitializeRights();
        }

        /// <summary>
        ///     Gibt die Berechtigung an, Daten an das Ende einer Datei anzufügen.
        /// </summary>
        /// <value></value>
        /// <returns>true, wenn der Benutzer diese Berechtigung besitzt, ansonsten false.</returns>
        /// <remarks></remarks>
        public bool CanAppendData =>
            !_accessRights[FileSystemRights.AppendData.ToString()].Deny &&
            _accessRights[FileSystemRights.AppendData.ToString()].Allow;

        /// <summary>
        ///     Gibt die Berechtigung an, die einer Datei zugeordneten Sicherheits- und Überwachungsregeln zu
        ///     ändern.
        /// </summary>
        /// <value></value>
        /// <returns>true, wenn der Benutzer diese Berechtigung besitzt, ansonsten false.</returns>
        /// <remarks></remarks>
        public bool CanChangePermissions =>
            !_accessRights[FileSystemRights.ChangePermissions.ToString()].Deny &&
            _accessRights[FileSystemRights.ChangePermissions.ToString()].Allow;

        /// <summary>
        ///     Gibt die Berechtigung an, einen Ordner zu erstellen.
        /// </summary>
        /// <value></value>
        /// <returns>true, wenn der Benutzer diese Berechtigung besitzt, ansonsten false.</returns>
        /// <remarks></remarks>
        public bool CanCreateDirectories =>
            !_accessRights[FileSystemRights.CreateDirectories.ToString()].Deny &&
            _accessRights[FileSystemRights.CreateDirectories.ToString()].Allow;

        /// <summary>
        ///     Gibt die Berechtigung an, eine Datei zu erstellen.
        /// </summary>
        /// <value></value>
        /// <returns>true, wenn der Benutzer diese Berechtigung besitzt, ansonsten false.</returns>
        /// <remarks></remarks>
        public bool CanCreateFiles =>
            !_accessRights[FileSystemRights.CreateFiles.ToString()].Deny &&
            _accessRights[FileSystemRights.CreateFiles.ToString()].Allow;

        /// <summary>
        ///     Gibt die Berechtigung an, einen Ordner oder eine Datei zu löschen.
        /// </summary>
        /// <value></value>
        /// <returns>true, wenn der Benutzer diese Berechtigung besitzt, ansonsten false.</returns>
        /// <remarks></remarks>
        public bool CanDelete =>
            !_accessRights[FileSystemRights.Delete.ToString()].Deny &&
            _accessRights[FileSystemRights.Delete.ToString()].Allow;

        /// <summary>
        ///     Gibt die Berechtigung an, einen Ordner und sämtliche in diesem Ordner enthaltenen Dateien zu
        ///     löschen.
        /// </summary>
        /// <value></value>
        /// <returns>true, wenn der Benutzer diese Berechtigung besitzt, ansonsten false.</returns>
        /// <remarks></remarks>
        public bool CanDeleteSubdirectoriesAndFiles =>
            !_accessRights[FileSystemRights.DeleteSubdirectoriesAndFiles.ToString()].Deny &&
            _accessRights[FileSystemRights.DeleteSubdirectoriesAndFiles.ToString()].Allow;

        /// <summary>
        ///     Gibt die Berechtigung an, eine Anwendungsdatei auszuführen.
        /// </summary>
        /// <value></value>
        /// <returns>true, wenn der Benutzer diese Berechtigung besitzt, ansonsten false.</returns>
        /// <remarks></remarks>
        public bool CanExecuteFile =>
            !_accessRights[FileSystemRights.ExecuteFile.ToString()].Deny &&
            _accessRights[FileSystemRights.ExecuteFile.ToString()].Allow;

        /// <summary>
        ///     Gibt die Berechtigung für einen Vollzugriff auf eine Datei oder einen Ordner an sowie die
        ///     Berechtigung, die Zugriffs- und Überwachungsregeln zu ändern. Dieser Wert stellt die Berechtigung
        ///     dar, jede mögliche Aktion für diese Datei durchzuführen. Er ist eine Kombination aller
        ///     Berechtigungen.
        /// </summary>
        /// <value></value>
        /// <returns>true, wenn der Benutzer diese Berechtigung besitzt, ansonsten false.</returns>
        /// <remarks></remarks>
        public bool CanFullControl =>
            !_accessRights[FileSystemRights.FullControl.ToString()].Deny &&
            _accessRights[FileSystemRights.FullControl.ToString()].Allow;

        /// <summary>
        ///     Gibt die Berechtigung an, den Inhalt eines Verzeichnisses zu lesen.
        /// </summary>
        /// <value></value>
        /// <returns>true, wenn der Benutzer diese Berechtigung besitzt, ansonsten false.</returns>
        /// <remarks></remarks>
        public bool CanListDirectory =>
            !_accessRights[FileSystemRights.ListDirectory.ToString()].Deny &&
            _accessRights[FileSystemRights.ListDirectory.ToString()].Allow;

        /// <summary>
        ///     Gibt die Berechtigung an, den Inhalt eines Ordners zu lesen, zu schreiben und aufzulisten,
        ///     Dateien und Ordner zu löschen und Anwendungsdateien auszuführen. Diese Berechtigung schließt
        ///     die Berechtigungen CanReadAndExecute, CanWrite und CanDelete ein.
        /// </summary>
        /// <value></value>
        /// <returns>true, wenn der Benutzer diese Berechtigung besitzt, ansonsten false.</returns>
        /// <remarks></remarks>
        public bool CanModify =>
            !_accessRights[FileSystemRights.Modify.ToString()].Deny &&
            _accessRights[FileSystemRights.Modify.ToString()].Allow;

        /// <summary>
        ///     Gibt die Berechtigung an, Ordner oder Dateien schreibgeschützt zu öffnen und zu kopieren. Diese
        ///     Berechtigung schließt die Berechtigungen CanReadData, CanReadExtendedAttributes,
        ///     CanReadAttributes und CanReadPermissions ein.
        /// </summary>
        /// <value></value>
        /// <returns>true, wenn der Benutzer diese Berechtigung besitzt, ansonsten false.</returns>
        /// <remarks></remarks>
        public bool CanRead =>
            !_accessRights[FileSystemRights.Read.ToString()].Deny &&
            _accessRights[FileSystemRights.Read.ToString()].Allow;

        /// <summary>
        ///     Gibt die Berechtigung an, Ordner oder Dateien schreibgeschützt zu öffnen und zu kopieren und
        ///     Anwendungsdateien auszuführen. Diese Berechtigung schließt die CanRead-Berechtigung und die
        ///     CanExecuteFile-Berechtigung ein.
        /// </summary>
        /// <value></value>
        /// <returns>true, wenn der Benutzer diese Berechtigung besitzt, ansonsten false.</returns>
        /// <remarks></remarks>
        public bool CanReadAndExecute =>
            !_accessRights[FileSystemRights.ReadAndExecute.ToString()].Deny &&
            _accessRights[FileSystemRights.ReadAndExecute.ToString()].Allow;

        /// <summary>
        ///     Gibt die Berechtigung an, Dateisystemattribute einer Datei oder eines Ordners zu öffnen und zu
        ///     kopieren. Dieser Wert gibt z. B. die Berechtigung an, das Erstellungsdatum oder das
        ///     Änderungsdatum einer Datei zu lesen. Dies schließt nicht die Berechtigung ein, Daten, erweiterte
        ///     Dateisystemattribute oder Zugriffs- und Überwachungsregeln zu lesen.
        /// </summary>
        /// <value></value>
        /// <returns>true, wenn der Benutzer diese Berechtigung besitzt, ansonsten false.</returns>
        /// <remarks></remarks>
        public bool CanReadAttributes =>
            !_accessRights[FileSystemRights.ReadAttributes.ToString()].Deny &&
            _accessRights[FileSystemRights.ReadAttributes.ToString()].Allow;

        /// <summary>
        ///     Gibt die Berechtigung an, eine Datei oder einen Ordner zu öffnen und zu kopieren. Dies schließt
        ///     nicht die Berechtigung ein, Dateisystemattribute, erweiterte Dateisystemattribute oder Zugriffs-
        ///     und Überwachungsregeln zu lesen.
        /// </summary>
        /// <value></value>
        /// <returns>true, wenn der Benutzer diese Berechtigung besitzt, ansonsten false.</returns>
        /// <remarks></remarks>
        public bool CanReadData =>
            !_accessRights[FileSystemRights.ReadData.ToString()].Deny &&
            _accessRights[FileSystemRights.ReadData.ToString()].Allow;

        /// <summary>
        ///     Gibt die Berechtigung an, erweiterte Dateisystemattribute einer Datei oder eines Ordners zu öffnen
        ///     und zu kopieren. Dieser Wert gibt zum Beispiel die Berechtigung an, den Autor oder
        ///     Inhaltsinformationen anzuzeigen. Dies schließt nicht die Berechtigung ein, Daten,
        ///     Dateisystemattribute oder Zugriffs- und Überwachungsregeln zu lesen.
        /// </summary>
        /// <value></value>
        /// <returns>true, wenn der Benutzer diese Berechtigung besitzt, ansonsten false.</returns>
        /// <remarks></remarks>
        public bool CanReadExtendedAttributes =>
            !_accessRights[FileSystemRights.ReadExtendedAttributes.ToString()].Deny &&
            _accessRights[FileSystemRights.ReadExtendedAttributes.ToString()].Allow;

        /// <summary>
        ///     Gibt die Berechtigung an, Zugriffs- und Überwachungsregeln für eine Datei oder einen Ordner zu
        ///     öffnen und zu kopieren. Dies schließt nicht die Berechtigung ein, Daten, Dateisystemattribute
        ///     oder erweiterte Dateisystemattribute zu lesen.
        /// </summary>
        /// <value></value>
        /// <returns>true, wenn der Benutzer diese Berechtigung besitzt, ansonsten false.</returns>
        /// <remarks></remarks>
        public bool CanReadPermissions =>
            !_accessRights[FileSystemRights.ReadPermissions.ToString()].Deny &&
            _accessRights[FileSystemRights.ReadPermissions.ToString()].Allow;

        /// <summary>
        ///     Gibt an, ob die Anwendung warten kann, bis ein Dateihandle mit dem Abschluss eines E/A-Vorgangs
        ///     synchronisiert ist.
        /// </summary>
        /// <value></value>
        /// <returns>true, wenn der Benutzer diese Berechtigung besitzt, ansonsten false.</returns>
        /// <remarks></remarks>
        public bool CanSynchronize =>
            !_accessRights[FileSystemRights.Synchronize.ToString()].Deny &&
            _accessRights[FileSystemRights.Synchronize.ToString()].Allow;

        /// <summary>
        ///     Gibt die Berechtigung an, den Besitzer eines Ordners oder einer Datei zu ändern. Beachten Sie,
        ///     dass Besitzer einer Ressource über einen Vollzugriff auf diese Ressource verfügen.
        /// </summary>
        /// <value></value>
        /// <returns>true, wenn der Benutzer diese Berechtigung besitzt, ansonsten false.</returns>
        /// <remarks></remarks>
        public bool CanTakeOwnership =>
            !_accessRights[FileSystemRights.TakeOwnership.ToString()].Deny &&
            _accessRights[FileSystemRights.TakeOwnership.ToString()].Allow;

        /// <summary>
        ///     Gibt die Berechtigung an, den Inhalt eines Ordners aufzulisten und in diesem Ordner enthaltene
        ///     Anwendungen auszuführen.
        /// </summary>
        /// <value></value>
        /// <returns>true, wenn der Benutzer diese Berechtigung besitzt, ansonsten false.</returns>
        /// <remarks></remarks>
        public bool CanTraverse =>
            !_accessRights[FileSystemRights.Traverse.ToString()].Deny &&
            _accessRights[FileSystemRights.Traverse.ToString()].Allow;

        /// <summary>
        ///     Gibt die Berechtigung an, Ordner und Dateien zu erstellen, Dateien Daten hinzuzufügen und Daten
        ///     aus Dateien zu entfernen. Diese Berechtigung schließt die Berechtigungen CanWriteData,
        ///     CanAppendData, CanWriteExtendedAttributes und CanWriteAttributes ein.
        /// </summary>
        /// <value></value>
        /// <returns>true, wenn der Benutzer diese Berechtigung besitzt, ansonsten false.</returns>
        /// <remarks></remarks>
        public bool CanWrite =>
            !_accessRights[FileSystemRights.Write.ToString()].Deny &&
            _accessRights[FileSystemRights.Write.ToString()].Allow;

        /// <summary>
        ///     Gibt die Berechtigung an, Dateisystemattribute einer Datei oder eines Ordners zu öffnen und zu
        ///     schreiben. Dies schließt nicht die Berechtigung ein, Daten, erweiterte Attribute oder Zugriffs-
        ///     und Überwachungsregeln zu schreiben.
        /// </summary>
        /// <value></value>
        /// <returns>true, wenn der Benutzer diese Berechtigung besitzt, ansonsten false.</returns>
        /// <remarks></remarks>
        public bool CanWriteAttributes =>
            !_accessRights[FileSystemRights.WriteAttributes.ToString()].Deny &&
            _accessRights[FileSystemRights.WriteAttributes.ToString()].Allow;

        /// <summary>
        ///     Gibt die Berechtigung an, eine Datei oder einen Ordner zu öffnen und in die Datei bzw. den Ordner
        ///     zu schreiben. Dies schließt nicht die Berechtigung ein, Dateisystemattribute, erweiterte
        ///     Dateisystemattribute oder Zugriffs- und Überwachungsregeln zu öffnen und zu schreiben.
        /// </summary>
        /// <value></value>
        /// <returns>true, wenn der Benutzer diese Berechtigung besitzt, ansonsten false.</returns>
        /// <remarks></remarks>
        public bool CanWriteData =>
            !_accessRights[FileSystemRights.WriteData.ToString()].Deny &&
            _accessRights[FileSystemRights.WriteData.ToString()].Allow;

        /// <summary>
        ///     Gibt die Berechtigung an, erweiterte Dateisystemattribute einer Datei oder eines Ordners zu öffnen
        ///     und zu schreiben. Dies schließt nicht die Berechtigung ein, Daten, Attribute oder Zugriffs- und
        ///     Überwachungsregeln zu schreiben.
        /// </summary>
        /// <value></value>
        /// <returns>true, wenn der Benutzer diese Berechtigung besitzt, ansonsten false.</returns>
        /// <remarks></remarks>
        public bool CanWriteExtendedAttributes =>
            !_accessRights[FileSystemRights.WriteExtendedAttributes.ToString()].Deny &&
            _accessRights[FileSystemRights.WriteExtendedAttributes.ToString()].Allow;

        /// <summary>
        ///     Ruft die vollständige Zeichenfolge für den zu prüfenden Pfad ab oder legt diesen fest.
        /// </summary>
        /// <value></value>
        /// <returns>Der vollständige zu prüfende Pfad.</returns>
        /// <exception cref="System.ArgumentNullException">fileName ist null.</exception>
        /// <exception cref="System.Security.SecurityException">Der Aufrufer verfügt nicht über die erforderliche Berechtigung.</exception>
        /// <exception cref="System.ArgumentException">
        ///     Der Dateiname ist leer, oder er enthält nur Leerräume oder ungültige
        ///     Zeichen.
        /// </exception>
        /// <exception cref="System.UnauthorizedAccessException">Der Zugriff auf fileName wird verweigert.</exception>
        /// <exception cref="System.IO.PathTooLongException">
        ///     Der angegebene Pfad und/oder der Dateiname überschreiten die vom
        ///     System vorgegebene Höchstlänge. Beispielsweise dürfen auf Windows-Plattformen Pfade nicht länger als 247 Zeichen
        ///     und Dateinamen nicht länger als 259  Zeichen sein.
        /// </exception>
        /// <exception cref="System.NotSupportedException">fileName enthält einen Doppelpunkt (:) innerhalb der Zeichenfolge.</exception>
        /// <remarks></remarks>
        public string Path
        {
            get => _fileInfo.FullName;
            set
            {
                _fileInfo = new FileInfo(value);

                // Rechte aktualisieren
                InitializeRights();
            }
        }


        /// <summary>
        ///     Prüft, ob die angegebene Berechtigung in der angegebenen FileSystemAccessRule vorhanden ist.
        /// </summary>
        /// <param name="right">Die Berechtigung als Zeichenfolge.</param>
        /// <param name="rule">Die zu prüfende FileSystemAccessRule.</param>
        /// <returns>true, wenn die Berechtigung vorhanden ist, ansonsten false.</returns>
        /// <remarks></remarks>
        private bool Contains(string right, FileSystemAccessRule rule)
        {
            // Zeichenkette (Berechtigung) parsen
            var fsr = (FileSystemRights) Enum.Parse(typeof(FileSystemRights), right);

            // Prüfen, ob diese vorhanden ist
            return (fsr & rule.FileSystemRights) == fsr;
        }

        /// <summary>
        ///     Gibt eine durch Semikolon getrennte Zeichenkette aller erlaubten Zugriffsrechte des Benutzers zurück.
        /// </summary>
        /// <returns>Eine Zeichenkette aller erlaubten Zugriffsrechte.</returns>
        /// <remarks></remarks>
        public string GetAllowedRights()
        {
            return GetRights(true);
        }

        /// <summary>
        ///     Gibt eine durch Semikolon getrennte Zeichenkette aller verweigerten Zugriffsrechte des Benutzers zurück.
        /// </summary>
        /// <returns>Eine Zeichenkette aller verweigerten Zugriffsrechte.</returns>
        /// <remarks></remarks>
        public string GetDeniedRights()
        {
            return GetRights(false);
        }

        /// <summary>
        ///     Gibt eine durch Semikolon getrennte Zeichenkette aller Zugriffsrechte mit dem angegebenen Status zurück.
        /// </summary>
        /// <param name="rightState">Der Status der zurückzugebenden Zugriffsrechte.</param>
        /// <returns>Eine Zeichenkette aller Zugriffsrechte mit dem angegebenen Status.</returns>
        /// <remarks></remarks>
        private string GetRights(bool rightState)
        {
            // StringBuilder verwenden
            var sb = new StringBuilder();

            // Iteriere durch alle Eigenschaften dieser Klasse
            foreach (var propertyInfo in GetType().GetProperties())
                // Prüfe, ob die Eigenschaft mit "Can" beginnt und lesbar ist
                if (propertyInfo.Name.StartsWith("Can") && propertyInfo.CanRead)
                {
                    // Wert der Eigeschaft ermitteln
                    var value = propertyInfo.GetValue(this, null).ToString();
                    var result = false;

                    // Wert der Eigenschaft in Boolean parsen
                    if (bool.TryParse(value, out result))
                        // Prüfe auf Übereinstimmung und füge evtl. den Name der Berechtigung hin
                        if (result == rightState)
                            sb.Append(propertyInfo.Name.Substring(3) + "; ");
                }

            // Prüfe ob länger als 2 Zeichen und schneide die letzten beiden Zeichen ab (Leerzeichen und Semikolon)
            if (sb.Length > 2)
                sb.Remove(sb.Length - 2, 2);

            // Zeichenkette zurückgeben
            return sb.ToString();
        }


        /// <summary>
        ///     Initialisiert die Zugriffsrechte für den aktuellen Pfad des Benutzers.
        /// </summary>
        /// <exception cref="System.IO.IOException">E/A-Fehler beim Öffnen der Datei.</exception>
        /// <exception cref="System.PlatformNotSupportedException">
        ///     Das aktuelle Betriebssystem ist nicht Microsoft Windows 2000
        ///     oder höher.
        /// </exception>
        /// <exception cref="System.Security.AccessControl.PrivilegeNotHeldException">
        ///     Dem aktuellen Systemkonto sind keine
        ///     Administratorrechte zugewiesen.
        /// </exception>
        /// <exception cref="System.SystemException">Die Datei konnte nicht gefunden werden.</exception>
        /// <exception cref="System.UnauthorizedAccessException">
        ///     Dieser Vorgang wird von der aktuellen Plattform nicht
        ///     unterstützt.- oder - Der Aufrufer verfügt nicht über die erforderliche Berechtigung.
        /// </exception>
        /// <remarks></remarks>
        private void InitializeRights()
        {
            // Prüfe, ob Benutzer Nothing ist
            if (_userIdentity.User == null)
                return;
            if (string.IsNullOrEmpty(Path))
            {
                ResetRights();
                return;
            }

            AuthorizationRuleCollection acl = null;

            try
            {
                // Ermittle alle AuthorizationRule-Objekte
                acl = _fileInfo.GetAccessControl().GetAccessRules(true, true, typeof(SecurityIdentifier));
            }
            catch (UnauthorizedAccessException)
            {
                return;
            }

            // Iteriere durch jede enthaltende Rule
            for (var i = 0; i <= acl.Count - 1; i++)
            {
                // FileSystemAccessRule ermitteln
                var rule = (FileSystemAccessRule) acl[i];

                // Prüfe, ob diese Regel für den aktuellen Benutzer gilt
                if (_userIdentity.User.Equals(rule.IdentityReference))
                    // Iteriere durch jede Berechtigung
                    foreach (var r in Enum.GetNames(typeof(FileSystemRights)))
                        // Prüfe, ob die Berechtigung in der aktuellen Rule enthalten ist
                        if (Contains(r, rule))
                        {
                            // Prüfe, ob diese Berechtigung verweigert wird
                            if (AccessControlType.Deny == rule.AccessControlType)
                                // true -> Eigenschaft des Elementes aus dem Dictionary setzen
                                _accessRights[r].Deny = true;
                            else
                                // false -> Eigenschaft des Elementes aus dem Dictionary setzen
                                _accessRights[r].Allow = true;
                        }
            }

            // Ermittle die Namen aller Gruppen, in denen der Benutzer ist
            var groups = _userIdentity.Groups;

            // Iteriere durch jede Gruppe
            for (var j = 0; j <= groups.Count - 1; j++)
                // Iteriere durch jede enthaltende Rule
            for (var i = 0; i <= acl.Count - 1; i++)
            {
                // FileSystemAccessRule ermitteln
                var rule = (FileSystemAccessRule) acl[i];

                // Prüfe, ob diese Regel für die aktuelle Gruppe gilt
                if (groups[j].Equals(rule.IdentityReference))
                    // Iteriere durch jede Berechtigung
                    foreach (var r in Enum.GetNames(typeof(FileSystemRights)))
                        // Prüfe, ob die Berechtigung in der aktuellen Rule enthalten ist
                        if (Contains(r, rule))
                        {
                            // Prüfe, ob diese Berechtigung verweigert wird
                            if (AccessControlType.Deny == rule.AccessControlType)
                                // true -> Eigenschaft des Elementes aus dem Dictionary setzen
                                _accessRights[r].Deny = true;
                            else
                                // false -> Eigenschaft des Elementes aus dem Dictionary setzen
                                _accessRights[r].Allow = true;
                        }
            }
        }

        /// <summary>
        ///     Setzt alle Berechtigungen zurück.
        /// </summary>
        /// <remarks></remarks>
        private void ResetRights()
        {
            // Iteriere durch jeden Eintrag im Dictionary
            foreach (var accessRights in _accessRights)
            {
                // Setze alle Eigenschaften zurück
                var ar = accessRights.Value;
                ar.Allow = false;
                ar.Deny = false;
            }
        }

        /// <summary>
        ///     Stellt die Eigenschaften für die Zugriffsrechte einer Berechtigung bereit.
        /// </summary>
        /// <remarks></remarks>
        private class AccessRight
        {
            /// <summary>
            ///     Ruft einen Wert ab, der angibt, ob die Berechtigung verweigert wird.
            /// </summary>
            /// <value></value>
            /// <returns>true, wenn die Berechtigung verweigert wird, ansonsten false.</returns>
            /// <remarks></remarks>
            public bool Deny { get; set; }

            /// <summary>
            ///     Ruft einen Wert ab, der angibt, ob die Berechtigung erlaubt wird.
            /// </summary>
            /// <value></value>
            /// <returns>true, wenn die Berechtigung erlaubt wird, ansonsten false.</returns>
            /// <remarks></remarks>
            public bool Allow { get; set; }
        }
    }
}