using System;
using System.Collections.Generic;
using Xunit;
using RentACarDomein;

namespace RentACarTest
{
    public class ReserveringSysteemTests
    {

        private readonly ReserveringSysteem reserveringSysteem;

        public ReserveringSysteemTests()
        {
            reserveringSysteem = new();
        }

        [Fact]
        public void BerekenPrijs_GeefJuistePrijsTerug()
        {
            // Arrange
            string arrangement = "Airport";
            TimeSpan beginuur = new(21,31, 0);
            TimeSpan einduur = new(23, 31, 0);
            string voertuig = "BMW i8 Spyder";
            DateTime startdatum = new(2023, 06, 30);

            decimal expectedPrice = 660.00m;

            // Act
            decimal actualPrice = reserveringSysteem.BerekenPrijs(arrangement, beginuur, einduur, voertuig, startdatum);

            // Assert
            Assert.Equal(expectedPrice, actualPrice);
        }


        [Fact]
        public void BerekenEersteUur_GeefPrijsEersteUurTerug()
        {
            // Arrange
            string arrangement = "Business";
            string voertuig = "Mercedes G63 AMG";

            // Act
            decimal result = reserveringSysteem.BerekenEersteUur(arrangement, voertuig);

            // Assert
            decimal expectedPrice = 500.00m;
            Assert.Equal(expectedPrice, result);
        }

        [Fact]
        public void BeperkHuurDuur_LimiteertDagOp24Uur()
        {
            // Arrange
            TimeSpan duur = TimeSpan.FromHours(30);

            // Act
            TimeSpan result = reserveringSysteem.BeperkDagUren(duur);

            // Assert
            TimeSpan expectedDuration = TimeSpan.FromHours(6); // 30 - 24 = 6
            Assert.Equal(expectedDuration, result);
        }
    }
}