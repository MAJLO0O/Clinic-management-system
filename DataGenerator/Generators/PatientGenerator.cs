using DataGenerator.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataGenerator.Generators
{
    public class PatientGenerator
    {
        private readonly List<string> firstNames = new()
         {
            "John","Michael","David","James","Robert","William","Richard","Thomas","Daniel","Matthew",
            "Christopher","Anthony","Mark","Steven","Paul","Andrew","Joshua","Kenneth","Kevin","Brian",
            "George","Edward","Ronald","Timothy","Jason","Jeffrey","Ryan","Jacob","Gary","Nicholas",
            "Eric","Jonathan","Stephen","Larry","Justin","Scott","Brandon","Benjamin","Samuel","Gregory"
         };

        private readonly List<string> lastNames = new()
        {
            "Smith","Johnson","Williams","Brown","Jones","Garcia","Miller","Davis","Rodriguez","Martinez",
            "Hernandez","Lopez","Gonzalez","Wilson","Anderson","Thomas","Taylor","Moore","Jackson","Martin",
            "Lee","Perez","Thompson","White","Harris","Sanchez","Clark","Ramirez","Lewis","Robinson",
            "Walker","Young","Allen","King","Wright","Scott","Torres","Nguyen","Hill","Flores"
        };

        public Patient GeneratePatient()
        {
            string firstName = firstNames[Random.Shared.Next(firstNames.Count)];
            string lastName = lastNames[Random.Shared.Next(lastNames.Count)];
            string pesel = GeneratorMethods.PeselGenerator();
            DateTime dateOfBirth = GeneratorMethods.DateOfBirthGenerator();
            while (!GeneratorMethods.GeneratedPesels.Add(pesel))
            {
                pesel = GeneratorMethods.PeselGenerator();
            }
            return new Patient
            {
                FirstName = firstName,
                LastName = lastName,
                Pesel = pesel,
                DateOfBirth = dateOfBirth,
                PhoneNumber = GeneratorMethods.PhoneNumberGenerator(),
                Email = GeneratorMethods.EmailGenerator(firstName, lastName),
            };
        }

    }
}
