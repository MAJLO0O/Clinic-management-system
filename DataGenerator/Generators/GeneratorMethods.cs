using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataGenerator.Generators
{
    public class GeneratorMethods
    {
        public static HashSet<string> GeneratedPesels = new HashSet<string>();
        public static string PeselGenerator()
        {
            char [] pesel = new char[11];
            for (int i = 0; i < 11; i++)
            {
                pesel[i] = (char)('0'+Random.Shared.Next(0, 10));
            }
            return new string(pesel);
            
        }
        public static string PhoneNumberGenerator()
        {
            char[] digits = new char[12];

            digits[0] = '+';
            digits[1] = '4';
            digits[2] = '8';

            for (int i = 3; i < 12; i++)
            {
                digits[i] = (char)('0' + Random.Shared.Next(0, 10));
            }

            return new string(digits);
        }
        public static string EmailGenerator(string firstName, string lastName)
        {
            string email = $"{firstName.ToLower()}.{lastName.ToLower()}{Random.Shared.Next(100,999)}@example.com";
            return email;
        }
        public static DateTime DateOfBirthGenerator()
        {
            int year = Random.Shared.Next(1950, 2000);
            int month = Random.Shared.Next(1, 13);
            int day = Random.Shared.Next(1, DateTime.DaysInMonth(year, month) + 1);
            return new DateTime(year, month, day);
        }
    }
}

