#region

using System;
using System.Reflection;
using SRS.Utilities;

#endregion

namespace RIS.Core.Helper
{
    public class Coordinaten
    {
        //Bessel Ellipsoid
        private double aB;
        public double Altitude;

        //Konstanten
        //WGS84 Ellipsoid
        private double aW;

        //Ellepsoid-Koordinaten auf dem Bessel Ellipsoid
        private double B;
        private double bB;

        public int Bezugsmeridian;
        private double bW;

        //Helmert-Parameter
        private double dx;
        private double dy;
        private double dz;
        private double e2B;
        private double e2W;
        private double ex;
        private double ey;
        private double ez;
        private double fW;
        private double Height;
        private double HW;
        private double L;

        public double Latitude;
        public double Longitude;
        private double m;
        private int Meridianneu;
        private double RW;

        //Vektoren in DHDN/Bessel
        private double xB;

        //Vektoren in WGS84
        private double xW;
        private double yB;
        private double yW;
        private double zB;
        private double zW;

        public void GaussToWGS84(double rechtswert, double hochwert)
        {
            try
            {
                RW = rechtswert;
                HW = hochwert;
                Height = 0;

                KonstanteParameter();
                GKnachBL();
                VektorenDNDH();
                ParameterHelmert();
                Helmert();
                Vektorenumrechnung();
            }
            catch (Exception ex)
            {
                Logger.WriteError(MethodBase.GetCurrentMethod(), ex);
            }
        }

        private void KonstanteParameter()
        {
            //WGS84 Ellipsoid
            aW = 6378137; //große Halbachse
            bW = 6356752.3141; //kleine Halbachse
            e2W = (Math.Pow(aW, 2) - Math.Pow(bW, 2)) / Math.Pow(aW, 2); //1.Numerische Exzentrität
            fW = (aW - bW) / aW; //Abplattung 1: fW

            //Bessel Ellipsoid
            aB = 6377397.155;
            bB = 6356078.962;
            e2B = (aB * aB - bB * bB) / (aB * aB);
        }

        private void GKnachBL()
        {
            MeridianUmrechnung();

            var n = (aB - bB) / (aB + bB);
            var alpha = (aB + bB) / 2.0 * (1.0 + 1.0 / 4.0 * n * n + 1.0 / 64.0 * Math.Pow(n, 4));
            var beta = 3.0 / 2.0 * n - 27.0 / 32.0 * Math.Pow(n, 3) + 269.0 / 512.0 * Math.Pow(n, 5);
            var gamma = 21.0 / 16.0 * n * n - 55.0 / 32.0 * Math.Pow(n, 4);
            var delta = 151.0 / 96.0 * Math.Pow(n, 3) - 417.0 / 128.0 * Math.Pow(n, 5);
            var epsilon = 1097.0 / 512.0 * Math.Pow(n, 4);

            var y0 = Meridianneu / 3.0;
            var y = RW - y0 * 1000000 - 500000;

            var B0 = HW / alpha;
            var Bf = B0 + beta * Math.Sin(2 * B0) + gamma * Math.Sin(4 * B0) + delta * Math.Sin(6 * B0) +
                     epsilon * Math.Sin(8 * B0);

            var Nf = aB / Math.Sqrt(1.0 - e2B * Math.Pow(Math.Sin(Bf), 2));
            var ETAf = Math.Sqrt(aB * aB / (bB * bB) * e2B * Math.Pow(Math.Cos(Bf), 2));
            var tf = Math.Tan(Bf);

            var b1 = tf / 2.0 / (Nf * Nf) * (-1.0 - ETAf * ETAf) * (y * y);
            var b2 = tf / 24.0 / Math.Pow(Nf, 4) *
                     (5.0 + 3.0 * (tf * tf) + 6.0 * (ETAf * ETAf) - 6.0 * (tf * tf) * (ETAf * ETAf) -
                      4.0 * Math.Pow(ETAf, 4) - 9.0 * (tf * tf) * Math.Pow(ETAf, 4)) * Math.Pow(y, 4);

            B = (Bf + b1 + b2) * 180 / Math.PI;

            var l1 = 1.0 / Nf / Math.Cos(Bf) * y;
            var l2 = 1.0 / 6.0 / Math.Pow(Nf, 3) / Math.Cos(Bf) * (-1.0 - 2.0 * (tf * tf) - ETAf * ETAf) *
                     Math.Pow(y, 3);

            L = Meridianneu + (l1 + l2) * 180 / Math.PI;
        }

        private void MeridianUmrechnung()
        {
            var a = RW.ToString().Substring(0, 1);
            Meridianneu = Convert.ToInt32(a) * 3;
        }

        private void VektorenDNDH()
        {
            //Querkrümmunsradius
            var N = aB / Math.Sqrt(1.0 - e2B * Math.Pow(Math.Sin(B / 180 * Math.PI), 2));

            // Ergebnis Vektoren	
            xB = (N + Height) * Math.Cos(B / 180 * Math.PI) * Math.Cos(L / 180 * Math.PI);
            yB = (N + Height) * Math.Cos(B / 180 * Math.PI) * Math.Sin(L / 180 * Math.PI);
            zB = (N * (bB * bB) / (aB * aB) + Height) * Math.Sin(B / 180 * Math.PI);
        }

        private void ParameterHelmert()
        {
            dx = 598.1; //Translation in X
            dy = 73.7; //Translation in Y
            dz = 418.2; //Translation in Z
            ex = -0.202; //Drehwinkel in Bogensekunden un die x-Achse
            ey = -0.045; //Drehwinkel in Bogensekunden un die y-Achse
            ez = 2.455; //Drehwinkel in Bogensekunden un die z-Achse
            m = 6.7; //Maßstabsfaktor in ppm 
        }

        private void Helmert()
        {
            //Umrechnung der Drehwinkel in Bogenmaß
            var exRad = ex * Math.PI / 180.0 / 3600.0;
            var eyRad = ey * Math.PI / 180.0 / 3600.0;
            var ezRad = ez * Math.PI / 180.0 / 3600.0;

            //Maßstabsumrechnung
            var mEXP = 1 - m * Math.Pow(10, -6);

            //Drehmatrix
            // 1         Ez    -Ez
            // -Ez       1      Ex 
            // Ey       -Ex     1

            //Rotierende Vektoren
            // = Drehmatrix * Vektoren in WGS84
            var RotVektor1 = 1.0 * xB + ezRad * yB + -1.0 * eyRad * zB;
            var RotVektor2 = -1.0 * ezRad * xB + 1 * yB + exRad * zB;
            var RotVektor3 = eyRad * xB + -1.0 * exRad * yB + 1 * zB;

            //Maßstab berücksichtigen
            var RotVectorM1 = RotVektor1 * mEXP;
            var RotVectorM2 = RotVektor2 * mEXP;
            var RotVectorM3 = RotVektor3 * mEXP;

            //Translation anbringen
            //dxT = Drehmatrix * dx * m
            var dxT = 1.0 * dx * mEXP + ezRad * dy * mEXP + -1.0 * eyRad * dz * mEXP;
            var dyT = -1.0 * ezRad * dx * mEXP + 1.0 * dy * mEXP + exRad * dz * mEXP;
            var dzT = eyRad * dx * mEXP + -1.0 * exRad * dy * mEXP + 1 * dz * mEXP;

            //Vektoren jetzt in WGS84
            xW = RotVectorM1 + dxT;
            yW = RotVectorM2 + dyT;
            zW = RotVectorM3 + dzT;
        }

        private void Vektorenumrechnung()
        {
            var s = Math.Sqrt(xW * xW + yW * yW);
            var T = Math.Atan(zW * aW / (s * bW));
            var Bz = Math.Atan((zW + e2W * (aW * aW) / bW * Math.Pow(Math.Sin(T), 3)) /
                               (s - e2W * aW * Math.Pow(Math.Cos(T), 3)));

            var Lz = Math.Atan(yW / xW);
            var N = aW / Math.Sqrt(1 - e2W * Math.Pow(Math.Sin(Bz), 2));

            Altitude = s / Math.Cos(Bz);
            Latitude = Bz * 180 / Math.PI;
            Longitude = Lz * 180 / Math.PI;
            Bezugsmeridian = Meridianneu;
        }
    }
}