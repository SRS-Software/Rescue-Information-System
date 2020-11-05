#region

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Printing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Reporting.WinForms;
using RIS.Business;
using RIS.Core.Fax;
using RIS.Core.Helper;
using RIS.Core.Map;
using RIS.Core.Printer;
using RIS.Model;
using RIS.Properties;
using SRS.Utilities;
using SRS.Utilities.Extensions;

#endregion

namespace RIS.Core
{
    public class PrinterService : IPrinterService
    {
        private readonly IBusiness _business;
        private readonly IMapService _mapService;

        public PrinterService(IBusiness business, IMapService mapService)
        {
            try
            {
                _business = business;
                _mapService = mapService;

                Logger.WriteDebug(MethodBase.GetCurrentMethod(), "Initialize");
            }
            catch (Exception ex)
            {
                Logger.WriteError(MethodBase.GetCurrentMethod(), ex);
            }
        }

        #region Public Properties

        public bool IsRunning { get; private set; }

        #endregion //Public Properties

        #region Events

        public event EventHandler<ExceptionEventArgs> ExceptionOccured;

        #endregion //Events

        #region Public Funtions

        public void Start()
        {
            try
            {
                Logger.WriteDebug(MethodBase.GetCurrentMethod(), "Starting");
                var stopWatch = new Stopwatch();
                stopWatch.Start();

                IsRunning = false;
                registerEvents();
                IsRunning = true;

                stopWatch.Stop();
                Logger.WriteDebug(MethodBase.GetCurrentMethod(), $"Started -> {stopWatch.Elapsed}");
            }
            catch (Exception ex)
            {
                ExceptionOccured.RaiseEvent(this, new ExceptionEventArgs
                {
                    Methode = MethodBase.GetCurrentMethod(),
                    Error = ex
                });
            }
        }

        public void Stop()
        {
            try
            {
                Logger.WriteDebug(MethodBase.GetCurrentMethod(), "Stopping");
                var stopWatch = new Stopwatch();
                stopWatch.Start();

                unregisterEvents();

                stopWatch.Stop();
                Logger.WriteDebug(MethodBase.GetCurrentMethod(), $"Stopped -> {stopWatch.Elapsed}");
            }
            catch (Exception ex)
            {
                ExceptionOccured.RaiseEvent(this, new ExceptionEventArgs
                {
                    Methode = MethodBase.GetCurrentMethod(),
                    Error = ex
                });
            }
            finally
            {
                IsRunning = false;
            }
        }

        #endregion //Public Funtions

        #region Private Properties

        private bool fileprintPrintFinished;

        private int reportPageIndex;
        private List<string> reportFiles;
        private IList<Stream> reportStreams;
        private bool reportPrintFinished;

        private int faxPageIndex;
        private List<Bitmap> faxImages;
        private List<string> faxText;
        private bool faxPrintFinished;

        #endregion //Private Properties

        #region Private Funtions

        private void registerEvents()
        {
            Logger.WriteDebug(MethodBase.GetCurrentMethod(), "Register Events");

            _mapService.Finished += mapService_Finished;
        }

        private void unregisterEvents()
        {
            Logger.WriteDebug(MethodBase.GetCurrentMethod(), "Unregister Events");

            _mapService.Finished -= mapService_Finished;
        }

        private void mapService_Finished(object sender, FinishedEventArgs e)
        {
            if (e == null || e.Einsatz == null) return;

            Task.Factory.StartNew(() =>
            {
                try
                {
                    var printerList = _business.GetAllPrinters();
                    if (printerList == null || printerList.Count == 0)
                    {
                        Logger.WriteDebug(MethodBase.GetCurrentMethod(), "No printers configured");
                        return;
                    }

                    foreach (var printer in printerList)
                    {
                        Logger.WriteDebug(MethodBase.GetCurrentMethod(), "Printer[" + printer.PrinterName + "]: start");

                        //Go throw all installed printers on the system.
                        var isPrinterActiv = false;
                        foreach (string systemPrinterName in PrinterSettings.InstalledPrinters)
                            if (printer.PrinterName == systemPrinterName)
                                isPrinterActiv = true;

                        if (isPrinterActiv == false)
                        {
                            Logger.WriteDebug(MethodBase.GetCurrentMethod(),
                                "Printer[" + printer.PrinterName + "]: not installed on system");
                            continue;
                        }

                        #region Fileprint

                        var fileprintCopies = printer.FileprintCopies;
                        if (printer.FileprintNumberOfVehiclesOn) fileprintCopies += e.Einsatz.Einsatzmittel.Count;

                        if (fileprintCopies == 0)
                            Logger.WriteDebug(MethodBase.GetCurrentMethod(),
                                "Printer[" + printer.PrinterName + "]: Fileprint -> no copies");
                        else
                            Logger.WriteDebug(MethodBase.GetCurrentMethod(),
                                "Printer[" + printer.PrinterName + "]: Fileprint -> " + fileprintCopies + " copies");

                        #endregion //Fileprint  

                        #region Report

                        var reportCopies = printer.ReportCopies;
                        if (printer.ReportNumberOfVehiclesOn) reportCopies += e.Einsatz.Einsatzmittel.Count;

                        if (reportCopies == 0)
                        {
                            Logger.WriteDebug(MethodBase.GetCurrentMethod(),
                                "Printer[" + printer.PrinterName + "]: Report -> no copies");
                        }
                        else if (!printer.ReportVehiclesOn && !reportDataOn(printer) && !printer.ReportRouteImageOn &&
                                 !printer.ReportRouteDescriptionOn)
                        {
                            reportCopies = 0;

                            Logger.WriteDebug(MethodBase.GetCurrentMethod(),
                                "Printer[" + printer.PrinterName + "]: Report -> no content");
                        }
                        else if (printer.ReportDistance > 0 && e.Distance < printer.ReportDistance)
                        {
                            reportCopies = 0;

                            Logger.WriteDebug(MethodBase.GetCurrentMethod(),
                                "Printer[" + printer.PrinterName + "]: Report -> distance too low");
                        }
                        else
                        {
                            Logger.WriteDebug(MethodBase.GetCurrentMethod(),
                                "Printer[" + printer.PrinterName + "]: Report -> " + reportCopies + " copies");
                        }

                        #endregion //Report    

                        #region Fax

                        var faxCopies = printer.FaxCopies;
                        if (printer.FaxNumberOfVehiclesOn) faxCopies += e.Einsatz.Einsatzmittel.Count;

                        if (faxCopies == 0)
                            Logger.WriteDebug(MethodBase.GetCurrentMethod(),
                                "Printer[" + printer.PrinterName + "]: Fax -> no copies");
                        else
                            Logger.WriteDebug(MethodBase.GetCurrentMethod(),
                                "Printer[" + printer.PrinterName + "]: Fax -> " + faxCopies + " copies");

                        #endregion //Fax 

                        int[] copies =
                        {
                            fileprintCopies,
                            reportCopies,
                            faxCopies
                        };

                        Stopwatch timeoutStopwatch;
                        for (var currentCopy = 0; currentCopy < copies.Max(); currentCopy++)
                        {
                            #region Fileprint

                            if (fileprintCopies > 0)
                            {
                                fileprintStart(printer, e);
                                timeoutStopwatch = Stopwatch.StartNew();
                                do
                                {
                                    if (fileprintPrintFinished) break;

                                    Thread.Sleep(250);
                                } while (timeoutStopwatch.Elapsed < TimeSpan.FromSeconds(30));

                                fileprintCopies--;
                                if (fileprintCopies == 0)
                                    Logger.WriteDebug(MethodBase.GetCurrentMethod(),
                                        "Printer[" + printer.PrinterName + "]: Fileprint -> copies finished");
                            }

                            #endregion //Fileprint  

                            #region Report

                            if (reportCopies > 0)
                            {
                                reportStart(printer, e);
                                timeoutStopwatch = Stopwatch.StartNew();
                                do
                                {
                                    if (reportPrintFinished) break;

                                    Thread.Sleep(250);
                                } while (timeoutStopwatch.Elapsed < TimeSpan.FromSeconds(30));

                                reportCopies--;
                                if (reportCopies == 0)
                                    Logger.WriteDebug(MethodBase.GetCurrentMethod(),
                                        "Printer[" + printer.PrinterName + "]: Report -> copies finished");
                            }

                            #endregion //Report    

                            #region Fax

                            if (faxCopies > 0)
                            {
                                faxStart(printer, e);
                                timeoutStopwatch = Stopwatch.StartNew();
                                do
                                {
                                    if (faxPrintFinished) break;

                                    Thread.Sleep(250);
                                } while (timeoutStopwatch.Elapsed < TimeSpan.FromSeconds(30));

                                faxCopies--;
                                if (faxCopies == 0)
                                    Logger.WriteDebug(MethodBase.GetCurrentMethod(),
                                        "Printer[" + printer.PrinterName + "]: Fax -> copies finished");
                            }

                            #endregion //Fax 
                        }
                    }
                }
                catch (Exception ex)
                {
                    Logger.WriteError(MethodBase.GetCurrentMethod(), ex);
                }
            });
        }

        #region Fax

        private void faxStart(Model.Printer printer, FinishedEventArgs e)
        {
            try
            {
                faxPrintFinished = false;
                faxPageIndex = 0;

                var printDoc = new PrintDocument();
                if (Path.GetExtension(e.Einsatz.FaxPath).ToLower() == ".txt")
                {
                    faxText = new List<string>();
                    using (var _streamReader = new StreamReader(e.Einsatz.FaxPath, Encoding.GetEncoding(1252)))
                    {
                        while (_streamReader.Peek() >= 0) faxText.Add(_streamReader.ReadLine());
                    }

                    printDoc.PrintPage += faxPrinter_PrintPageText;
                }
                else
                {
                    faxImages = TiffConverter.TiffToBitmapList(e.Einsatz.FaxPath);
                    printDoc.PrintPage += faxPrinter_PrintPageImages;
                }

                printDoc.PrinterSettings.PrinterName = printer.PrinterName;
                printDoc.EndPrint += faxPrinter_StreamDispose;
                printDoc.Print();
            }
            catch (Exception ex)
            {
                Logger.WriteError(MethodBase.GetCurrentMethod(), ex);
                faxPrintFinished = true;
            }
        }

        private void faxPrinter_PrintPageImages(object sender, PrintPageEventArgs e)
        {
            Image printImage = faxImages[faxPageIndex];
            var ratio = printImage.Width / e.PageSettings.PrintableArea.Width;
            var printRec = new Rectangle((int) e.PageSettings.PrintableArea.X,
                (int) e.PageSettings.PrintableArea.Y, (int) e.PageSettings.PrintableArea.Width,
                (int) (printImage.Height / ratio));
            e.Graphics.DrawImage(printImage, printRec);

            faxPageIndex++;
            if (faxPageIndex < faxImages.Count)
                e.HasMorePages = true;
            else
                e.HasMorePages = false;
        }

        private void faxPrinter_PrintPageText(object sender, PrintPageEventArgs e)
        {
            if (faxText == null || faxText.Count <= 0) return;

            //Declare local variables needed  
            var font = new Font("Arial", 9);
            var leftMargin = e.MarginBounds.Left; //X      
            var topMargin = e.MarginBounds.Top; //Y                     
            var linesPerPage = (int) (e.MarginBounds.Height / font.GetHeight(e.Graphics));

            float topPosition = 0;
            var pageLineIndex = 0;
            //Now read lines one by one, using StreamReader        
            while (pageLineIndex + faxPageIndex * linesPerPage < faxText.Count && pageLineIndex < linesPerPage)
            {
                //Calculate the starting position    
                topPosition = topMargin + pageLineIndex * font.GetHeight(e.Graphics);
                //Draw text    
                e.Graphics.DrawString(faxText[pageLineIndex + faxPageIndex * linesPerPage], font, Brushes.Black,
                    leftMargin, topPosition, new StringFormat());
                //Move to next line   
                pageLineIndex++;
            }

            //If PrintPageEventArgs has more pages to print   
            if (pageLineIndex + faxPageIndex * linesPerPage < faxText.Count)
            {
                e.HasMorePages = true;
                faxPageIndex++;
            }
            else
            {
                e.HasMorePages = false;
            }
        }

        private void faxPrinter_StreamDispose(object sender, PrintEventArgs e)
        {
            faxPrintFinished = true;
        }

        #endregion //Fax

        #region Print Report

        private bool reportDataOn(Model.Printer printer)
        {
            if (printer.ReportDataSchlagwortOn)
                return true;
            if (printer.ReportDataStichwortOn)
                return true;
            if (printer.ReportDataOrtOn)
                return true;
            if (printer.ReportDataStraßeOn)
                return true;
            if (printer.ReportDataObjektOn)
                return true;
            if (printer.ReportDataStationOn)
                return true;
            if (printer.ReportDataKreuzungOn)
                return true;
            if (printer.ReportDataAbschnittOn)
                return true;
            if (printer.ReportDataBemerkungOn)
                return true;

            return false;
        }

        private void reportStart(Model.Printer printer, FinishedEventArgs e)
        {
            try
            {
                reportPrintFinished = false;
                reportPageIndex = 0;

                var report = new LocalReport();
                report.ReportEmbeddedResource = "RIS.Report.rdlc";
                report.EnableExternalImages = true;
                //ReportParameter festlegen
                var reportParammeters = new List<ReportParameter>();

                //Allgemein    
                //reportParammeters.Add(new ReportParameter("Organisation", _organisationService.OrganisationName, true));
                reportParammeters.Add(new ReportParameter("Alarmzeit", e.Einsatz.AlarmTime.ToString(), true));

                //Einsatzdaten    
                if (reportDataOn(printer))
                    reportParammeters.Add(new ReportParameter("EinsatzdatenOn", "True"));
                else
                    reportParammeters.Add(new ReportParameter("EinsatzdatenOn", "False"));

                if ((string.IsNullOrEmpty(e.Einsatz.Schlagwort) || !printer.ReportDataSchlagwortOn) &&
                    (string.IsNullOrEmpty(e.Einsatz.Stichwort) || !printer.ReportDataStichwortOn))
                    reportParammeters.Add(new ReportParameter("Ereignis", "", false));
                else if (string.IsNullOrEmpty(e.Einsatz.Schlagwort) || !printer.ReportDataSchlagwortOn)
                    reportParammeters.Add(new ReportParameter("Ereignis", e.Einsatz.Stichwort));
                else if (string.IsNullOrEmpty(e.Einsatz.Stichwort) || !printer.ReportDataStichwortOn)
                    reportParammeters.Add(new ReportParameter("Ereignis", e.Einsatz.Schlagwort));
                else
                    reportParammeters.Add(new ReportParameter("Ereignis",
                        e.Einsatz.Stichwort + " - " + e.Einsatz.Schlagwort));
                reportParammeters.Add(string.IsNullOrEmpty(e.Einsatz.Ort) || !printer.ReportDataOrtOn
                    ? new ReportParameter("Ort", "", false)
                    : new ReportParameter("Ort", e.Einsatz.Ort));
                reportParammeters.Add(string.IsNullOrEmpty(e.Einsatz.Straße) || !printer.ReportDataStraßeOn
                    ? new ReportParameter("Straße", "", false)
                    : new ReportParameter("Straße", e.Einsatz.Straße + " " + e.Einsatz.Hausnummer));
                reportParammeters.Add(string.IsNullOrEmpty(e.Einsatz.Objekt) || !printer.ReportDataObjektOn
                    ? new ReportParameter("Objekt", "", false)
                    : new ReportParameter("Objekt", e.Einsatz.Objekt));
                reportParammeters.Add(string.IsNullOrEmpty(e.Einsatz.Ort) || !printer.ReportDataOrtOn
                    ? new ReportParameter("Station", "", false)
                    : new ReportParameter("Station", e.Einsatz.Station));
                reportParammeters.Add(string.IsNullOrEmpty(e.Einsatz.Straße) || !printer.ReportDataStraßeOn
                    ? new ReportParameter("Kreuzung", "", false)
                    : new ReportParameter("Kreuzung", e.Einsatz.Kreuzung));
                reportParammeters.Add(string.IsNullOrEmpty(e.Einsatz.Objekt) || !printer.ReportDataObjektOn
                    ? new ReportParameter("Abschnitt", "", false)
                    : new ReportParameter("Abschnitt", e.Einsatz.Abschnitt));
                reportParammeters.Add(string.IsNullOrEmpty(e.Einsatz.Bemerkung) || !printer.ReportDataBemerkungOn
                    ? new ReportParameter("Bemerkung", "", false)
                    : new ReportParameter("Bemerkung", e.Einsatz.Bemerkung));

                //Fahrzeuge                       
                if (printer.ReportVehiclesOn && e.Einsatz.Einsatzmittel != null)
                {
                    var einsatzmittelString = "";
                    foreach (var vehicle in e.Einsatz.Einsatzmittel)
                    {
                        einsatzmittelString += vehicle.Name;

                        //Add new line if not last vehicle
                        if (vehicle != e.Einsatz.Einsatzmittel.Last())
                            einsatzmittelString += "\n";
                    }

                    reportParammeters.Add(new ReportParameter("EinsatzmittelOn", "True"));
                    reportParammeters.Add(new ReportParameter("Einsatzmittel", einsatzmittelString));
                }
                else
                {
                    reportParammeters.Add(new ReportParameter("EinsatzmittelOn", "False"));
                    reportParammeters.Add(new ReportParameter("Einsatzmittel", "", false));
                }

                //Routenimage                                                               
                if (printer.ReportRouteImageOn && e.ImageReport != null && e.ImageReport.Count() > 0)
                {
                    reportParammeters.Add(new ReportParameter("MapImageOn", "True"));
                    reportParammeters.Add(new ReportParameter("MapImage", Convert.ToBase64String(e.ImageReport)));
                }
                else
                {
                    reportParammeters.Add(new ReportParameter("MapImageOn", "False"));
                    reportParammeters.Add(new ReportParameter("MapImage", "", false));
                }

                //Routendescription                                                                    
                if (printer.ReportRouteDescriptionOn && e.Description != null)
                {
                    reportParammeters.Add(new ReportParameter("MapDescriptionOn", "True"));
                    reportParammeters.Add(new ReportParameter("MapDescription", e.Description));
                }
                else
                {
                    reportParammeters.Add(new ReportParameter("MapDescriptionOn", "False"));
                    reportParammeters.Add(new ReportParameter("MapDescription", "", false));
                }

                //Parameter an Bericht übertragen                     
                report.SetParameters(reportParammeters);
                report.Refresh();

                //Exportieren des Reports  zu EMF file       
                Warning[] warnings;
                reportFiles = new List<string>();
                reportStreams = new List<Stream>();
                report.Render("Image", @"<DeviceInfo><OutputFormat>EMF</OutputFormat></DeviceInfo>",
                    reportPrinter_StreamCreate, out warnings);

                foreach (var stream in reportStreams) stream.Position = 0;

                //Start to print report 
                var printDoc = new PrintDocument();
                printDoc.PrintPage += reportPrinter_PrintPage;
                printDoc.EndPrint += reportPrinter_StreamDispose;
                printDoc.PrinterSettings.PrinterName = printer.PrinterName;
                printDoc.Print();
            }
            catch (Exception ex)
            {
                Logger.WriteError(MethodBase.GetCurrentMethod(), ex);
                reportPrintFinished = true;
            }
        }

        private Stream reportPrinter_StreamCreate(string name, string fileNameExtension, Encoding encoding,
            string mimeType, bool willSeek)
        {
            var filePath = Path.Combine(Settings.Default.WorkingFolder, "Temp",
                "REPORT_" + Guid.NewGuid() + "." + fileNameExtension);
            Stream stream = new FileStream(filePath, FileMode.Create);
            reportStreams.Add(stream);
            reportFiles.Add(filePath);
            return stream;
        }

        private void reportPrinter_PrintPage(object sender, PrintPageEventArgs e)
        {
            var printImage = new Metafile(reportStreams[reportPageIndex]);
            var ratio = printImage.Width / e.PageSettings.PrintableArea.Width;
            var printRec = new Rectangle((int) e.PageSettings.PrintableArea.X,
                (int) e.PageSettings.PrintableArea.Y, (int) e.PageSettings.PrintableArea.Width,
                (int) (printImage.Height / ratio));
            e.Graphics.DrawImage(printImage, printRec);

            reportPageIndex++;
            if (reportPageIndex < reportFiles.Count)
                e.HasMorePages = true;
            else
                e.HasMorePages = false;
        }

        private void reportPrinter_StreamDispose(object sender, PrintEventArgs e)
        {
            if (reportStreams != null)
            {
                foreach (var _stream in reportStreams) _stream.Close();

                reportStreams = null;
            }

            if (reportFiles != null)
            {
                foreach (var _file in reportFiles) File.Delete(_file);

                reportFiles = null;
            }

            reportPrintFinished = true;
        }

        #endregion //Report

        #region Printfiles

        private void fileprintStart(Model.Printer printer, FinishedEventArgs e)
        {
            try
            {
                fileprintPrintFinished = false;

                foreach (var fileprint in printer.Fileprints)
                    if (fileprintCheck(e.Einsatz, fileprint))
                    {
                        Logger.WriteDebug(MethodBase.GetCurrentMethod(),
                            "Printer[" + printer.PrinterName + "] Fileprint -> " + fileprint.File);

                        if (!PdfPrinterHelper.Print(printer.PrinterName, fileprint.File)) return;
                    }
            }
            catch (Exception ex)
            {
                Logger.WriteError(MethodBase.GetCurrentMethod(), ex);
            }
            finally
            {
                fileprintPrintFinished = true;
            }
        }

        private bool fileprintCheck(Einsatz einsatz, Fileprint fileprint)
        {
            if (fileprint == null || fileprint.Condition == null || fileprint.Expression == null) return false;

            switch (fileprint.Condition.Id)
            {
                case 1: //Ort ist 
                    if (Text.CheckString(einsatz.Ort, fileprint.Expression)) return true;

                    break;
                case 2: //Straße ist 
                    if (Text.CheckString(einsatz.Straße, fileprint.Expression)) return true;

                    break;
                case 3: //Hausnummer ist 
                    if (Text.CheckString(einsatz.Hausnummer, fileprint.Expression)) return true;

                    break;
                case 4: //Straße + Hausnummer ist 
                    var splitExpression4 = fileprint.Expression.Split('+');
                    if (splitExpression4.Count() != 2) return false;

                    if (Text.CheckString(einsatz.Straße, splitExpression4[0]) &&
                        Text.CheckString(einsatz.Hausnummer, splitExpression4[1]))
                        return true;

                    break;
                case 5: //Objekt ist 
                    if (Text.CheckString(einsatz.Objekt, fileprint.Expression)) return true;

                    break;
                case 6: //Ort + Straße ist   
                    var splitExpression6 = fileprint.Expression.Split('+');
                    if (splitExpression6.Count() != 2) return false;

                    if (Text.CheckString(einsatz.Ort, splitExpression6[0]) &&
                        Text.CheckString(einsatz.Straße, splitExpression6[1]))
                        return true;

                    break;
            }

            return false;
        }

        #endregion //Printfiles

        #endregion //Private Funtions
    }
}