namespace RentACarDomein
{
    public class Reservering
    {
        private static int nextReserveringsnummer = 1;

        public int Reserveringsnummer { get; set; }
        public string KlantNummer { get; set; }
        public string KlantNaam { get; set; }
        public string Voertuig { get; set; }
        public string Arrangement { get; set; }
        public DateTime Startdatum { get; set; }
        public TimeSpan Beginuur { get; set; }
        public TimeSpan Duur { get; set; }
        public decimal Eenheidsprijs { get; set; }
        public decimal TotaalInclusief { get; set; }
        public decimal TotaalExclusief { get; set; }
        public string Startplaats { get; set; }
        public string Aankomstplaats { get; set; }

        public Reservering()
        {
            Reserveringsnummer = GenerateReserveringsnummer();
        }
        public Reservering(DateTime startDatum, TimeSpan duur, TimeSpan beginUur) : this()
        {
            Startdatum = startDatum;
            Duur = duur;
            Beginuur = beginUur;
        }
        public Reservering(string klantNummer,string klantNaam, string auto, string arrangement,
                           DateTime datum, TimeSpan beginuur, TimeSpan duur, string startPlaats,
                           string eindPlaats) : this(datum, duur, beginuur)
        {
            KlantNummer = klantNummer;
            KlantNaam = klantNaam;
            Voertuig = auto;
            Arrangement = arrangement;
            Startplaats = startPlaats;
            Aankomstplaats = eindPlaats;
        }
        public Reservering(string klantNummer, string klantNaam, string auto, string arrangement, DateTime datum, TimeSpan beginuur, TimeSpan duur, string startPlaats,
                           string eindPlaats, decimal eenheidsprijs, decimal totaalExclusief, decimal totaalInclusief) : this(klantNummer, klantNaam, auto, arrangement, datum, beginuur, duur, startPlaats, eindPlaats)
        {
            Eenheidsprijs = eenheidsprijs;
            TotaalExclusief = totaalExclusief;
            TotaalInclusief = totaalInclusief;

        }

        public static int GenerateReserveringsnummer()
        {
            int reserveringsnummer = nextReserveringsnummer;
            nextReserveringsnummer++;
            return reserveringsnummer;
        }
    }
}
