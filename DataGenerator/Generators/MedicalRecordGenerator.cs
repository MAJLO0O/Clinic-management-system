using MedicalData.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataGenerator.Generators
{
    public class MedicalRecordGenerator
    {
        public List<string> Notes = new()
        {
            "Patient is recovering well.",
            "Prescribed medication for pain relief.",
            "Recommended physical therapy sessions.",
            "Follow-up appointment scheduled in two weeks.",
            "Patient reported mild side effects from medication.",
            "Blood pressure is within normal range.",
            "Advised patient to maintain a healthy diet.",
            "Patient is showing signs of improvement.",
            "Recommended regular exercise for better health.",
            "Patient is responding positively to treatment." };
        
        public List<MedicalRecord> GenerateMedicalRecords(List<(int Id,DateTime CreatedAt)> appointmentIdsAndTheirCreatedAt)
        {
            var medicalRecords = new List<MedicalRecord>();
            for (int i = 0; i < appointmentIdsAndTheirCreatedAt.Count; i++)
            {
                var item = appointmentIdsAndTheirCreatedAt[i];
                medicalRecords.Add(new MedicalRecord
                {
                    AppointmentId = item.Id,
                    Note = $"Appointment note: {Notes[Random.Shared.Next(Notes.Count)]}",
                    CreatedAt = item.CreatedAt.AddMinutes(Random.Shared.Next(10,60))
                });
            }
            return medicalRecords;
        }
    }
}
