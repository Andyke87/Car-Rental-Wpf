using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RentACarDomein
{
    public class Locatie
    {
        public string Naam { get; set; }

        public Locatie(string naam)
        {
            Naam = naam;
        }
    }
}
