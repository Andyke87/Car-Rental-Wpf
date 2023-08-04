using System;
using System.Collections.Generic;
using System.Windows;
using RentACarDomein;
using System.Collections.ObjectModel;
using RentACarDatabank;
using System.Windows.Controls;
using System.Data.SqlClient;
using DocumentFormat.OpenXml.Spreadsheet;

namespace RentACar
{
    public partial class MainWindow : Window
    {

        #region ConnectieStringen
        string excelFilePath = @"C:\Users\andy\OneDrive - Hogeschool Gent\Bureaublad\EindOpdrachtProgrammerenGevorderd\KlantenLijst.csv";
        string databaseConnectionString = "Data Source=ANDY-LAUWERS\\SQLEXPRESS;Initial Catalog=RentACar;Integrated Security=True;Pooling=False";
        #endregion

        #region lijstWeergave
        private int currentPage = 1;
        private readonly int pageSize = 10;
        private int nextReservatieNummer = 1;
        private int index;
        #endregion

        public List<Klant> Customer = new();
        public List<Reservering> Reservatie= new();

        private readonly DataManager dataManager;
        private readonly ReserveringSysteem rvs = new();

        public ObservableCollection<Klant> Customers { get; set; }
        public List<Reservering> Reservaties { get; set; }
        public int reservatieNummer { get; set; }

        public MainWindow()
        {
            InitializeComponent();

            // Maak een nieuwe instantie van de DataManager klasse
            dataManager = new(excelFilePath, databaseConnectionString); // Maak een nieuwe instantie van de DataManager klasse

            // Laad de klanten in de lijst
            Customers = new(dataManager.GeefKlantenTerug());

            // Laad de eerste pagina van klanten
            LoadCustomersList(currentPage);

            GeefKlantnummersWeerReservatie();
            HaalArrangementenFromDatabase();
            HaalAutosFromDatabase();
            HaalStartPlaatsenFromDatabase();
            HaalAankomstPlaatsenFromDatabase();
            HaalReserveringenVanDatabase();

            if (Reservaties != null)
            {
                VulReserveringenGegevens(index);
            }
        }

        private void ExitClick(object sender, RoutedEventArgs e)
        {
            //System.Environment.Exit(0);
            Application.Current.Shutdown();
        }
        private void NewClick(object sender, RoutedEventArgs e)
        {
            var info = GC.GetGCMemoryInfo();
            System.Diagnostics.Debug.WriteLine("Heap size: " + info.HeapSizeBytes);
            var totalMemoryBeforeCleanup = GC.GetTotalMemory(false);
            System.Diagnostics.Debug.WriteLine("Total memory before cleanup: " + totalMemoryBeforeCleanup);
            var totalMemoryAfterCleanup = GC.GetTotalMemory(true); // effectief zoveel mogelijk vrijgeven naar het OS met true
            System.Diagnostics.Debug.WriteLine("Total memory after cleanup: " + totalMemoryAfterCleanup);

            StatusBarTxt.Text = "Memory cleanup: from " + totalMemoryBeforeCleanup + " to " + totalMemoryAfterCleanup + " bytes";
        }

        #region GegevensDatabankHalen
        private void HaalReserveringenVanDatabase()
        {
            Reservaties = new();
            using (SqlConnection connection = new SqlConnection(databaseConnectionString))
            {
                connection.Open();

                string query = "SELECT Reserveringnummer, Klantennummer, Klantennaam, Voertuig, Arrangement, Datum, Beginuur, Einduur, PrijsEersteUur, TotaalExclusief, TotaalInclusief, Startplaats, Aankomstplaats FROM Reservering";
                SqlCommand command = new SqlCommand(query, connection);

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        int reserveringnummer = reader.GetInt32(0);
                        string klantennummer = reader.GetString(1);
                        string klantennaam = reader.GetString(2);
                        string voertuig = reader.GetString(3);
                        string arrangement = reader.GetString(4);
                        DateTime datum = reader.GetDateTime(5).Date;
                        TimeSpan beginuur = reader.GetTimeSpan(6);
                        TimeSpan einduur = reader.GetTimeSpan(7);
                        decimal prijsEersteUur = reader.GetDecimal(8);
                        decimal totaalExclusief = reader.GetDecimal(9);
                        decimal totaalInclusief = reader.GetDecimal(10);
                        string startplaats = reader.GetString(11);
                        string aankomstplaats = reader.GetString(12);

                        Reservering reservering = new(klantennummer, klantennaam, voertuig, arrangement, datum, beginuur, einduur, startplaats, aankomstplaats, prijsEersteUur, totaalExclusief, totaalInclusief);
                        Reservaties.Add(reservering);
                    }
                }
            }
        }
        private void GeefKlantnummersWeerReservatie()
        {
            using (SqlConnection connection = new(databaseConnectionString))
            {
                connection.Open();

                string query = "SELECT KlantNummer FROM Klanten";

                using (SqlCommand command = new(query, connection))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string klantNummer = reader.GetString(0);
                            klantNummerComboBox.Items.Add(klantNummer);
                        }
                    }
                }
            }
        }
        public void HaalArrangementenFromDatabase()
        {
            ObservableCollection<Arrangement> arrangementen = new();

            using (SqlConnection connection = new(databaseConnectionString))
            {
                connection.Open();

                string query = "SELECT Formule FROM Arrangementen";

                using (SqlCommand command = new(query, connection))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string formule = reader.GetString(0);  // Haal de waarde van het veld "Formule" op
                            ArrangementComboBox.Items.Add(formule);
                        }
                    }
                }
            }
        }
        public void HaalAutosFromDatabase()
        {
            ObservableCollection<Auto> autos = new();

            using (SqlConnection connection = new(databaseConnectionString))
            {
                connection.Open();

                string query = "SELECT Auto FROM Autopark";

                using (SqlCommand command = new(query, connection))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string auto = reader.GetString(0);  // Haal de waarde van het veld "Formule" op
                            autoComboBox.Items.Add(auto);
                        }
                    }
                }
            }
        }
        public void HaalStartPlaatsenFromDatabase()
        {
            ObservableCollection<Locatie> startPlaatsen = new();

            using (SqlConnection connection = new(databaseConnectionString))
            {
                connection.Open();

                string query = "SELECT Stad FROM Locatie";

                using (SqlCommand command = new(query, connection))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string startPlaats = reader.GetString(0);
                            startPlaatsComboBox.Items.Add(startPlaats);
                        }
                    }
                }
            }
        }
        public void HaalAankomstPlaatsenFromDatabase()
        {
            ObservableCollection<Locatie> aankomstPlaatsen = new();

            using (SqlConnection connection = new(databaseConnectionString))
            {
                connection.Open();

                string query = "SELECT Stad FROM Locatie";

                using (SqlCommand command = new(query, connection))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string aankomstPlaats = reader.GetString(0);  // Haal de waarde van het veld "Formule" op
                            eindPlaatsComboBox.Items.Add(aankomstPlaats);
                        }
                    }
                }
            }
        }

        #endregion

        #region KlantLijstWeergave
        private void LoadCustomersList(int page)
        {
            int offset = (page - 1) * pageSize;

            using (SqlConnection connection = new(databaseConnectionString))
            {
                connection.Open();

                using (SqlCommand command = new(
                    $"SELECT Klantnummer, Voornaam, Naam, Straat, Straatnummer, Busnummer, Plaats, Postcode, BTWnummer FROM " +
                    $"(SELECT ROW_NUMBER() OVER(ORDER BY Klantnummer) AS RowNum, Klantnummer, Voornaam, Naam, Straat, Straatnummer, Busnummer, Plaats, Postcode, BTWnummer FROM Klanten) AS Sub " +
                    $"WHERE RowNum > {offset} AND RowNum <= {offset + pageSize}", connection))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        List<Klant> klanten = new();

                        while (reader.Read())
                        {
                            Klant klant = new()
                            {
                                Klantennummer = reader.IsDBNull(0) ? string.Empty : reader.GetString(0),
                                Voornaam = reader.IsDBNull(1) ? string.Empty : reader.GetString(1),
                                Achternaam = reader.IsDBNull(2) ? string.Empty : reader.GetString(2),
                                Straat = reader.IsDBNull(3) ? string.Empty : reader.GetString(3),
                                Huisnummer = reader.IsDBNull(4) ? 0 : reader.GetInt32(4),
                                Busnummer = reader.IsDBNull(5) ? string.Empty : reader.GetString(5),
                                Stad = reader.IsDBNull(6) ? string.Empty : reader.GetString(6),
                                Postcode = reader.IsDBNull(7) ? 0 : reader.GetInt32(7),
                                BTWnummer = reader.IsDBNull(8) ? string.Empty : reader.GetString(8)
                            };


                            klanten.Add(klant);
                        }

                        klantenListView.ItemsSource = klanten;
                    }
                }
            }
        }

        private void Vorige10_Click(object sender, RoutedEventArgs e)
        {
            if (currentPage > 1)
            {
                currentPage--;
                LoadCustomersList(currentPage);
            }
        }
        private void Volgende10_Click(object sender, RoutedEventArgs e)
        {
            if (currentPage < 1)
            {
                currentPage = 1;
            }
            if (currentPage < Math.Ceiling((double)Customers.Count / pageSize))
            {
                currentPage++;
                LoadCustomersList(currentPage);
            }
            LoadCustomersList(currentPage);
        }

        #endregion

        #region KlantWeergave
        private void klantenListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (klantenListView.SelectedItem != null)
            {
                Klant selectedKlant = (Klant)klantenListView.SelectedItem;
                naamTextBox.Text = selectedKlant.Voornaam + " " + selectedKlant.Achternaam;
                adresTextBox.Text = selectedKlant.Straat + " " + selectedKlant.Huisnummer + " " + selectedKlant.Busnummer;
                stadTextBox.Text = selectedKlant.Stad;
                btwNummerTextBox.Text = selectedKlant.BTWnummer;
            }
        }
        #endregion

        #region MaakReservatie
        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            if (klantNummerComboBox.Text != "" || klantNaamTextBox.Text != "" || autoComboBox.Text != "" || timeSlider.Value != 0 || huurTijdSlider.Value != 0 || ArrangementComboBox.Text != "" || startPlaatsComboBox.Text != "" || eindPlaatsComboBox.Text != "" || datumPicker.SelectedDate != null)
            {
                // Reset alle velden in de "MaakReservering" tab
                klantNummerComboBox.Text = "";
                klantNaamTextBox.Text = "";
                autoComboBox.Text = "";
                timeSlider.Value = 0;
                huurTijdSlider.Value = 0;
                ArrangementComboBox.Text = "";
                startPlaatsComboBox.Text = "";
                eindPlaatsComboBox.Text = "";
                datumPicker.SelectedDate = null;
            }
        }
        public void ReserveerButton_Click(object sender, RoutedEventArgs e)
        {
            string klantNummer = klantNummerComboBox.Text;
            string klantNaam = klantNaamTextBox.Text;
            string auto = autoComboBox.Text;
            string arrangement = ArrangementComboBox.Text;

            DateTime startDatum = ControleerDatum(datumPicker.SelectedDate.Value);

            string[] beginuurParts = timeLabel.Content.ToString().Split(':');
            int uren = int.Parse(beginuurParts[0]);
            int minuten = int.Parse(beginuurParts[1]);
            TimeSpan beginuur = new TimeSpan(uren, minuten, 0);

            int duurUren = int.Parse(huurTijdLabel.Content.ToString());
            int totaalUren = uren + duurUren;
            TimeSpan eindtijd = new TimeSpan(totaalUren, minuten, 0);

            decimal eenheidsprijs = rvs.BerekenEersteUur(arrangement, auto);
            decimal prijsExclusief = rvs.BerekenPrijs(arrangement, beginuur, eindtijd, auto,startDatum);
            decimal prijs = rvs.BerekenPrijs(arrangement, beginuur, eindtijd, auto, startDatum);
            decimal prijsInclusief = prijs * 1.21m;



            string startPlaats = startPlaatsComboBox.Text;
            string aankomstPlaats = eindPlaatsComboBox.Text;

            if (klantNummer == "" || klantNaam == "" || auto == "" || arrangement == "" || startPlaats == "" || aankomstPlaats == "" || datumPicker.SelectedDate == null)
            {
                MessageBox.Show("Gelieve alle velden in te vullen");
                return;
            }
            else
            {

                bool check = rvs.ControleerBeschikbaarheid(startPlaats, startDatum, beginuur, auto, Reservaties);
                if(check == false)
                {
                    MessageBox.Show("Deze auto is niet beschikbaar op deze datum en tijd");
                    return;
                }
                else
                {
                    Reservering reservering = new(klantNummer, klantNaam, auto, arrangement, startDatum, beginuur, eindtijd, startPlaats, aankomstPlaats, eenheidsprijs, prijsExclusief, prijsInclusief);
                    SaveReservering(reservering);
                    ClearButton_Click(sender, e);
                    MessageBox.Show("Deze auto is gereserveerd");
                }

            }
        }
        public void SaveReservering(Reservering reservering)
        {
            ReserveringSysteem rvs = new();

            using (SqlConnection connection = new(databaseConnectionString))
            {
                connection.Open();

                // Query om de reservering in te voegen
                string insertQuery = @"INSERT INTO Reservering (Reserveringnummer,Klantennummer, Klantennaam, Voertuig, Arrangement, Datum, Beginuur, Einduur, PrijsEersteUur, TotaalExclusief, TotaalInclusief, Startplaats, Aankomstplaats)
                               VALUES (@Reserveringnummer,@Klantennummer,@Naam, @Auto, @Arrangement, @Datum, @Startuur, @Duur, @PrijsEersteUur, @TotaalExclusief, @TotaalInclusief, @Startplaats, @Aankomstplaats);
                               SELECT SCOPE_IDENTITY();";

                using (SqlCommand command = new(insertQuery, connection))
                {
                    int getal = Reservaties.Count;
                    reservatieNummer = MaakReservatieNummerAan(getal);
                    command.Parameters.AddWithValue("@Reserveringnummer", reservatieNummer);

                    command.Parameters.AddWithValue("@Klantennummer", reservering.KlantNummer);
                    command.Parameters.AddWithValue("@Naam", reservering.KlantNaam);
                    command.Parameters.AddWithValue("@Auto", reservering.Voertuig);
                    command.Parameters.AddWithValue("@Arrangement", reservering.Arrangement);
                    command.Parameters.AddWithValue("@Datum", reservering.Startdatum);
                    command.Parameters.AddWithValue("@Startuur", reservering.Beginuur);
                    
                    TimeSpan eindtijd = rvs.BeperkDagUren(reservering.Duur);
                    command.Parameters.AddWithValue("@Duur", eindtijd);

                    command.Parameters.AddWithValue("@PrijsEersteUur", reservering.Eenheidsprijs);
                    command.Parameters.AddWithValue("@TotaalExclusief", reservering.TotaalExclusief);
                    command.Parameters.AddWithValue("@TotaalInclusief", reservering.TotaalInclusief);

                    command.Parameters.AddWithValue("@Startplaats", reservering.Startplaats);
                    command.Parameters.AddWithValue("@Aankomstplaats", reservering.Aankomstplaats);


                    command.ExecuteNonQuery();
                }
            }
        }
        public DateTime ControleerDatum(DateTime datum)
        {
            DateTime vandaag = DateTime.Today;
            if (datum < vandaag)
            {
                MessageBox.Show("De datum mag niet in het verleden liggen.", "Foutmelding", MessageBoxButton.OK, MessageBoxImage.Error);
                datumPicker.Text = string.Empty;
            }
            else
            {
                return datum;
            }
            return datum;
        }
        public int MaakReservatieNummerAan(int getal)
        {
           if (getal == 0)
            {
                reservatieNummer = 1;
            }
            else
            {
                reservatieNummer = getal + 1;
            }
            return reservatieNummer;
        }

        #endregion

        #region MaakReservatieWeergave

        private void timeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            // Logica voor het verwerken van de gewijzigde tijdwaarde
            int minutes = (int)timeSlider.Value;
            int hours = minutes / 60;
            int remainingMinutes = minutes % 60;

            // Bijwerken van de tijdweergave in het label
            timeLabel.Content = hours.ToString("00") + " : " + remainingMinutes.ToString("00");
        }
        private void huurTijdSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            // Logica voor het verwerken van de gewijzigde uurwaarde
            int hours = (int)huurTijdSlider.Value;

            // Bijwerken van de tekst in het label
            huurTijdLabel.Content = hours.ToString("00");
        } 
        private void klantNummerComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            object geselecteerdeWaarde = klantNummerComboBox.SelectedValue;
            if (geselecteerdeWaarde != null)
            {
                string? geselecteerdKlantNummer = geselecteerdeWaarde.ToString();
                foreach (Klant klantInLijst in Customers)
                {
                    if (klantInLijst.Klantennummer == geselecteerdKlantNummer)
                    {
                        klantNaamTextBox.Text = klantInLijst.VolledigeNaam;
                        break;
                    }
                    else
                    {
                        klantNaamTextBox.Text = string.Empty;
                    }
                }
            }
        }
        private void ArrangementComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ReserveringSysteem rvs = new();
            object geselecteerdeWaarde = ArrangementComboBox.SelectedValue;
            object geselecteerdeAuto = autoComboBox.SelectedValue;
            
            DateTime dag = DateTime.Parse(datumPicker.Text);

            string beginuurString = timeLabel.Content.ToString().Replace(" ", ""); // Verwijder spaties uit de string

            TimeSpan beginuur;
            if (TimeSpan.TryParse(beginuurString, out beginuur))
            {
                // Het parsen van de TimeSpan is gelukt
                TimeSpan einduur = beginuur.Add(TimeSpan.FromHours(huurTijdSlider.Value)); // Bereken de eindtijd door de duur toe te voegen

                if (geselecteerdeWaarde != null)
                {
                    StelSlidersIn(geselecteerdeWaarde.ToString());

                    decimal waarde = 0;
                    if (geselecteerdeWaarde != null)
                    {
                        waarde = rvs.BerekenEersteUur(geselecteerdeWaarde.ToString(), geselecteerdeAuto.ToString());
                    }
                    eersteUurTextBlock.Text = waarde.ToString();

                    decimal waarde2 = 0;
                    if (geselecteerdeWaarde != null)
                    {
                        waarde2 = rvs.BerekenPrijs(geselecteerdeWaarde.ToString(), beginuur, einduur, geselecteerdeAuto.ToString(), dag);
                        totaalKostTextBlock.Text = waarde2.ToString();
                    }
                }
            }
        }
        public void StelSlidersIn(string arrangement)
        {
            // Pas de beperkingen op de timeSlider aan op basis van het gekozen arrangement
            if (arrangement == "Wedding")
            {
                // timeSlider.Minimum = 7 uur
                // timeSlider.Maximum = 15 uur
                timeSlider.Minimum = 7 * 60; // Minimum uur: 7
                timeSlider.Maximum = 15 * 60; // Maximum uur: 15
                timeSlider.Value = 0;
                huurTijdSlider.Value = 7;
            }
            else if (arrangement == "Nightlife")
            {
                timeSlider.Minimum = 20 * 60; // Minimum uur: 20
                timeSlider.Maximum = 24 * 60; // Maximum uur: 24
                huurTijdSlider.Value = 7;
            }
            else
            {
                // Geen beperkingen, herstel de standaardwaarden van de timeSlider
                timeSlider.Minimum = 0;
                timeSlider.Maximum = 24 * 60;
            }
        }

        #endregion

        #region OverzichtReservaties
        private void VulReserveringenGegevens(int index)
        {
            Reservering res = Reservaties[index];

            ORReserveringsnummerTextBox.Text = res.Reserveringsnummer.ToString();
            ORDatumTextBox.Text = res.Startdatum.ToString();
            ORklantNummerTextBox.Text = res.KlantNummer;
            ORklantenNaamTextBox.Text = res.KlantNaam;
            ORVoertuigTextBox.Text = res.Voertuig;
            ORArrangementTextBox.Text = res.Arrangement;
            ORBeginuurTextBox.Text = res.Beginuur.ToString();
            OREinduurTextBox.Text = res.Duur.ToString();
            ORStartPlaatsTExtBox.Text = res.Startplaats;
            ORAankomstPlaatsTextBox.Text = res.Aankomstplaats;

            ORPrijsEersteUurTextBox.Text = res.Eenheidsprijs.ToString();
            ORTotaalExclusiefTextBox.Text = res.TotaalExclusief.ToString();
            ORTotaalInclusiefTextBox.Text = res.TotaalInclusief.ToString();

            foreach (Klant klant in Customers)
            {
                if (klant.Klantennummer == res.KlantNummer)
                {
                    ORAdresTextBox.Text = $"{klant.Straat} {klant.Huisnummer} {klant.Busnummer}  ";
                    ORBTWnummerTextBox.Text = klant.BTWnummer;
                    break;
                }
            }
        }

        private void Vorige_Click(object sender, RoutedEventArgs e)
        {

            if (index > 0)
            {
                index--;
                VulReserveringenGegevens(index);
            }
        }
        private void Volgende_Click(object sender, RoutedEventArgs e)
        {
            if (index < Reservaties.Count - 1)
            {
                index++;
                VulReserveringenGegevens(index);
            }
        }
        #endregion
    }
}
