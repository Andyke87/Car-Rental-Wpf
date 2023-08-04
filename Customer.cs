using DocumentFormat.OpenXml.Wordprocessing;

namespace RentACarDomein
{
    public class Klant
    {
        public string Klantennummer { get; set; }
        public string Voornaam { get; set; }
        public string Achternaam { get; set; }
        public string Straat { get; set; }
        public int Huisnummer { get; set; }
        public string? Busnummer { get; set; }
        public string Stad { get; set; }
        public int Postcode { get; set; }
        public string? BTWnummer { get; set; }
        public string VolledigeNaam => $"{Voornaam} {Achternaam}";

        public Klant() : this("")
        {
        }
        public Klant(string customerNumber)
        {
            Klantennummer = customerNumber;

        }
        public Klant(string customerNumber, string firstName, string lastName, string street, int streetNumber,
            string? busNumber, string city, int postalCode, string? vatNumber)
        {
            Klantennummer = customerNumber;
            Voornaam = firstName;
            Achternaam = lastName;
            Straat = street;
            Huisnummer = streetNumber;
            Busnummer = busNumber;
            Stad = city;
            Postcode = postalCode;
            BTWnummer = vatNumber;
        }
    }

}
