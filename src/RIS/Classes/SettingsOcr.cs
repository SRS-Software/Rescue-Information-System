namespace RIS
{
    public class SettingsOcr
    {
        public SettingsOcr()
        {
            Schlagwort = new OcrItem();
            Stichwort1 = new OcrItem();
            Stichwort2 = new OcrItem();
            Stichwort3 = new OcrItem();
            Ort = new OcrItem();
            Straße = new OcrItem();
            Hausnummer = new OcrItem();
            KoordinatenHW = new OcrItem();
            KoordinatenRW = new OcrItem();
            Objekt = new OcrItem();
            Station = new OcrItem();
            Kreuzung = new OcrItem();
            Abschnitt = new OcrItem();
            Bemerkung = new OcrItem();
        }

        public string Absender { get; set; }
        public OcrItem Schlagwort { get; set; }
        public OcrItem Stichwort1 { get; set; }
        public OcrItem Stichwort2 { get; set; }
        public OcrItem Stichwort3 { get; set; }
        public OcrItem Ort { get; set; }
        public OcrItem Straße { get; set; }
        public OcrItem Hausnummer { get; set; }
        public OcrItem KoordinatenHW { get; set; }
        public OcrItem KoordinatenRW { get; set; }
        public OcrItem Objekt { get; set; }
        public OcrItem Station { get; set; }
        public OcrItem Kreuzung { get; set; }
        public OcrItem Abschnitt { get; set; }
        public OcrItem Bemerkung { get; set; }

        public class OcrItem
        {
            public string Start { get; set; }
            public bool Start_FirstWord { get; set; }
            public string Stop { get; set; }
        }
    }
}