using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RentACarDomein;

namespace RentACarDomein
{
    public class Auto
    {
        public string Model { get; set; }
        public decimal FirstHourPrice { get; set; }
        public decimal NightlifePrice { get; set; }
        public decimal WeddingPrice { get; set; }
        public int Year { get; set; }
        public string Name { get; set; }
        public bool Beschikbaar { get; set; }
        public Arrangement Arrangement { get; set; }

       

        public Auto(string model)
        {
              if (string.IsNullOrEmpty(model))
                throw new ArgumentException("Model moet worden opgegeven.");

            Model = model;
        }

        public Auto(string model, bool beschikbaar) : this(model)
        {
            Beschikbaar = beschikbaar;
        }
        public Auto(string model, decimal firstHourPrice, decimal nightlifePrice, decimal weddingPrice, int year) : this(model)
        {
            FirstHourPrice = firstHourPrice;
            NightlifePrice = nightlifePrice;
            WeddingPrice = weddingPrice;
            Year = year;
        }

        public decimal BerekenPrijs(TimeSpan duur)
        {
            decimal prijs = 0;

            if (duur.TotalHours <= 1)
            {
                prijs = FirstHourPrice;
            }
            else if (duur.TotalHours > 1 && duur.TotalHours <= 5)
            {
                prijs = FirstHourPrice + (NightlifePrice * (decimal)(duur.TotalHours - 1));
            }
            else if (duur.TotalHours > 5)
            {
                prijs = FirstHourPrice + (NightlifePrice * 4) + (WeddingPrice * (decimal)(duur.TotalHours - 5));
            }

            return prijs;
        }

        public static bool ControleerBeschikbaarheid(DateTime startdatum, TimeSpan duur, List<Reservering> reserveringen)
        {
            bool beschikbaar = true;
            DateTime einddatum = startdatum + duur;

            foreach (Reservering reservering in reserveringen)
            {
                DateTime reserveringEinddatum = reservering.Startdatum + reservering.Duur;

                if ((startdatum >= reservering.Startdatum && startdatum <= reserveringEinddatum) ||
                    (einddatum >= reservering.Startdatum && einddatum <= reserveringEinddatum))
                {
                    beschikbaar = false;
                    break;
                }
            }

            return beschikbaar;
        }
    }

}
