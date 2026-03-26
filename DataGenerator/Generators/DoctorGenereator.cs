using DataGenerator.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataGenerator.Generators
{
    public class DoctorGenereator
    {
        private readonly List<string> firstNames = new List<string>
        { "John", "Jane", "Michael", "Emily", "David", "Sarah", "Robert", "Jessica", "William", "Olivia" };
        private readonly List<string> lastNames = new List<string>
        { "Smith", "Johnson", "Brown", "Taylor", "Anderson", "Thomas", "Jackson", "White", "Harris", "Martin" };

        public Doctor GenerateDoctor(List<int> branchId,int index)
        {
            string firstName = firstNames[Random.Shared.Next(firstNames.Count)];
            string lastName = lastNames[Random.Shared.Next(lastNames.Count)];
            string pesel = GeneratorMethods.PeselGenerator();
            while (!GeneratorMethods.GeneratedPesels.Add(pesel))
                {
                pesel = GeneratorMethods.PeselGenerator();
            }
            return new Doctor
            {
                FirstName = firstName,
                LastName = lastName,
                Pesel = pesel,
                PhoneNumber = GeneratorMethods.PhoneNumberGenerator(),
                Email = GeneratorMethods.EmailGenerator(firstName,lastName,index),
                BranchId = branchId[Random.Shared.Next(branchId.Count)]
            };
        }
    
    
    }
}
