using DocumentFormat.OpenXml.Drawing.Diagrams;
using System.Data.SqlClient;
using Xceed.Wpf.Toolkit;

namespace RentACarDomein
{
    public class ReserveringSysteem
    {
        public Auto? auto { get; set; }
        private List<Auto> autoList = new();
        decimal prijs = 0;
        string databaseConnectionString = "Data Source=ANDY-LAUWERS\\SQLEXPRESS;Initial Catalog=RentACar;Integrated Security=True;Pooling=False";

        // Controleer de beschikbaarheid van een voertuig voor de opgegeven startdatum en duur
        public bool ControleerBeschikbaarheid(string startplaats, DateTime startdatum, TimeSpan startuur, string voertuig, List<Reservering> reserveringen)
        {
            if (reserveringen == null)
            {
                return true; // Geen reserveringen, dus beschikbaar
            }
            else
            {
                foreach (Reservering reservering in reserveringen)
                {
                    if (reservering.Voertuig == voertuig)
                    {
                        string rsvStartplaats = reservering.Startplaats;
                        string rsvAankomstplaats = reservering.Aankomstplaats;
                        DateTime rsvStartdatum = reservering.Startdatum;
                        TimeSpan rsvStartuur = reservering.Beginuur;
                        TimeSpan rsvDuur = reservering.Duur;

                        if (rsvStartdatum == startdatum)
                        {
                          if (startplaats == rsvAankomstplaats)
                            {
                              // als de startplaats van de nieuwe reservatie en de aankomstplaats van de bestaande reservatie het zelfde zijn gaan er 4u af van startuur
                                rsvStartuur = rsvStartuur.Add(new TimeSpan(-4, 0, 0));
                            }
                          else
                            {
                                rsvStartuur = rsvStartuur.Add(new TimeSpan(-6, 0, 0));
                            }
                            if (startuur >= rsvStartuur && startuur <= rsvDuur)
                            {
                                return false; // Overlapping gevonden, niet beschikbaar
                            }

                        }

                    }
                }

                return true; // Geen overlappingen of onbeschikbare voertuigen gevonden, beschikbaar
            }

        }

        //Lees het Autopark uit de database
        public List<Auto> LeesAutopark()
        {
            autoList = new();

            // Lees de voertuigen uit de database
            using (SqlConnection connection = new(databaseConnectionString))
            {
                connection.Open();
                SqlCommand command = new("SELECT * FROM Autopark", connection);
                SqlDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    string Auto = reader.GetString(1);
                    decimal EersteUur = reader.GetDecimal(2); // Gebruik GetDecimal om een decimale waarde uit de database te lezen
                    decimal Nightlife = reader.GetDecimal(3);
                    decimal Wedding = reader.GetDecimal(4);
                    int Bouwjaar = reader.GetInt32(5); // Gebruik GetInt32 om een integer-waarde uit de database te lezen

                    Auto auto = new(Auto, EersteUur, Nightlife, Wedding, Bouwjaar);
                    autoList.Add(auto);
                }

                connection.Close();
            }
            return autoList;
        }

        // Bereken de prijs van de reservering op basis van het arrangement, duur en voertuig
        public decimal BerekenPrijs(string arrangement, TimeSpan beginuur, TimeSpan duur, string voertuig, DateTime startdatum)
        {
            autoList = LeesAutopark();

            foreach (Auto auto in autoList)
            {
                if (auto.Model == voertuig)
                {
                    if (arrangement == "Nightlife" || arrangement == "Wedding")
                    {
                        switch (arrangement)
                        {
                            case "Nightlife":
                                prijs = auto.NightlifePrice;
                                break;
                            case "Wedding":
                                prijs = auto.WeddingPrice;
                                break;
                        }
                    }
                    else
                    {
                        // Prijs per uur voor andere arrangementen
                        decimal eersteUurPrijs = auto.FirstHourPrice;
                        decimal overigeUurPrijs = auto.FirstHourPrice * 0.6m;
                        decimal nachtUurPrijs = auto.FirstHourPrice * 1.2m;

                        int totaalUren = (int)duur.TotalHours - (int)beginuur.TotalHours;
                        int nachtUren = 0;
                        TimeSpan uur = beginuur;

                        // Bepaal het aantal nachturen (uren tussen 22:00 en 07:00)
                        for (int i = 0; i < totaalUren; i++)
                        {

                            if (uur.Hours >= 22 || uur.Hours < 7)
                            {
                                if (uur.Hours >= 22)
                                {
                                    uur = uur.Add(new TimeSpan(1, 0, 0));
                                    if (uur.Hours >= 24)
                                    {
                                        uur = uur.Add(new TimeSpan(-24, 0, 0));
                                    }
                                }
                                else
                                {
                                    uur = uur.Add(new TimeSpan(1, 0, 0));
                                }
                                nachtUren++;
                            }
                            else
                            {
                            uur = uur.Add(new TimeSpan(1, 0, 0));
                            }

                        }

                        int overigeUren = totaalUren - nachtUren;
                        prijs = eersteUurPrijs;

                        // Bereken de prijs voor overige uren
                        if (overigeUren > 1)
                        {
                            overigeUurPrijs = Math.Round(overigeUurPrijs / 5) * 5; // Rond af naar veelvoud van € 5
                            prijs += overigeUurPrijs * (overigeUren - 1);
                        }

                        // Bereken de prijs voor nachturen
                        if (nachtUren > 0)
                        {
                            nachtUurPrijs = Math.Round(nachtUurPrijs / 5) * 5; // Rond af naar veelvoud van € 5
                            prijs += nachtUurPrijs * nachtUren;
                        }
                        else if (nachtUren > 0 && overigeUren == 0)
                        {
                            prijs = nachtUurPrijs * nachtUren;
                        }
                        break;
                    }
                }
            }
            return prijs;
        }
       
        public decimal BerekenEersteUur(string arrangement, string voertuig)
        {
            autoList = LeesAutopark();

            // vergelijk de voertuigen met de  lijst van voertuigen
            foreach (Auto auto in autoList)
            {

                    
                if (auto.Model == voertuig)
                {
                    if (arrangement == "Airport" || arrangement == "Business")
                    {
                        prijs = auto.FirstHourPrice;
                    }
                    else
                    {
                        prijs = 0;
                    }
                }
            }
            return prijs;
        }
      
        public TimeSpan BeperkDagUren(TimeSpan duur)
        {
            if (duur.TotalHours > 24)
            {
                // Pas de duur aan naar het maximum van 24 uur
                duur = TimeSpan.FromHours(duur.TotalHours - 24);
            }

            return duur;
        }
    }
}
