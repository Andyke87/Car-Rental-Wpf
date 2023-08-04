using System.Data.SqlClient;
using RentACarDomein;

namespace RentACarDatabank
{
    public class DataManager
    {
        private List<Klant> Klanten { get; set; }

        public DataManager(string excelFilePath, string databaseConnectionString)
        {
            Klanten = LeesKlantenInExcelFile(excelFilePath);
            InsertCustomersIntoDatabase(Klanten, databaseConnectionString);
        }

        public static List<Klant> LeesKlantenInExcelFile(string filePath)
        {
            List<Klant> klanten = new();

            // Lees het CSV-bestand en verwerk elke regel als een klant
            string[] lines = System.IO.File.ReadAllLines(filePath);
            foreach (string line in lines)
            {
                string[] values = line.Split(';');

                string klantennummer = values[0];
                string voornaam = values[1];
                string achternaam = values[2];
                string straat = values[3];
                int huisnummer = int.Parse(values[4]);
                string busnummer = string.IsNullOrEmpty(values[5]) ? null : values[5];
                string stad = values[6];
                int postcode = int.Parse(values[7]);
                string btwnummer = string.IsNullOrEmpty(values[8]) ? null : values[8];

                Klant klant = new(klantennummer, voornaam, achternaam, straat, huisnummer, busnummer, stad, postcode, btwnummer);
                klanten.Add(klant);
            }

            return klanten;

        }

        public static void InsertCustomersIntoDatabase(List<Klant> klanten, string databaseConnectionString)
        {
            using (SqlConnection connection = new(databaseConnectionString))
            {
                connection.Open(); // Openen van de connectie met de database

                // Voor elke klant in de lijst toevoegen aan de database als Klant object
                foreach (Klant customer in klanten)
                {
                    // Controleren of de klant al bestaat in de database op basis van klantennummer
                    SqlCommand checkCommand = new("SELECT COUNT(*) FROM Klanten WHERE Klantnummer = @CustomerNumber", connection);
                    checkCommand.Parameters.AddWithValue("@CustomerNumber", customer.Klantennummer);

                    int existingCount = (int)checkCommand.ExecuteScalar();

                    if (existingCount > 0)
                    {
                        Console.WriteLine($"Klant {customer.Klantennummer} bestaat al, wordt overgeslagen.");
                        continue; // Klant overslaan als deze al bestaat
                    }

                    // Klant toevoegen aan de database
                    SqlCommand insertCommand = new("INSERT INTO Klanten (Klantnummer, Voornaam, Naam, Straat, Straatnummer, Busnummer, Plaats, Postcode, BTWnummer) VALUES (@CustomerNumber, @FirstName, @LastName, @Street, @HouseNumber, @BusNumber, @City, @PostalCode, @VATNumber)", connection);

                    insertCommand.Parameters.AddWithValue("@CustomerNumber", customer.Klantennummer);
                    insertCommand.Parameters.AddWithValue("@FirstName", customer.Voornaam);
                    insertCommand.Parameters.AddWithValue("@LastName", customer.Achternaam);
                    insertCommand.Parameters.AddWithValue("@Street", customer.Straat);
                    insertCommand.Parameters.AddWithValue("@HouseNumber", customer.Huisnummer);
                    insertCommand.Parameters.AddWithValue("@BusNumber", (object)customer.Busnummer ?? DBNull.Value); // DBNull.Value voor null waarden
                    insertCommand.Parameters.AddWithValue("@City", customer.Stad);
                    insertCommand.Parameters.AddWithValue("@PostalCode", customer.Postcode);
                    insertCommand.Parameters.AddWithValue("@VATNumber", (object)customer.BTWnummer ?? DBNull.Value); // DBNull.Value voor null waarden

                    insertCommand.ExecuteNonQuery(); // Uitvoeren van de query
                }

                Console.WriteLine("Klanten succesvol geïmporteerd in de database.");
            }
        }

        public List<Klant> GeefKlantenTerug()
        {
            return Klanten;
        }
    }
}

