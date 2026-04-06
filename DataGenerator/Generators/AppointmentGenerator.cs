using MedicalData.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataGenerator.Generators
{
    public class AppointmentGenerator
    {
      
         public DateTime GenerateRandomDateAppointment()
          {
            List<int> validMinutes = new List<int> { 0, 30 };
            List<DateTime> validYears = new() { DateTime.Now.AddYears(-1), DateTime.Now, DateTime.Now.AddYears(1) };
            int hour = Random.Shared.Next(8, 17);
            int minute = validMinutes[Random.Shared.Next(validMinutes.Count)];
            int year = validYears[Random.Shared.Next(validYears.Count)].Year;
            int month = Random.Shared.Next(1, 13);
            int day = Random.Shared.Next(1, DateTime.DaysInMonth(year, month) + 1);
            return new DateTime(year, month, day, hour, minute, 0);
        } 
        public Appointment GenerateAppointment(List<int> doctorIds, List<int> patientIds,List<int> appointmentStatusIds,HashSet<(int DoctorId, DateTime StartingDateTime)> existingAppointments)
        {
            int doctorId;
            DateTime startingDateTime;
            do
            {
                doctorId = doctorIds[Random.Shared.Next(doctorIds.Count)];
                startingDateTime = GenerateRandomDateAppointment();
            } while (existingAppointments.Contains((doctorId, startingDateTime)));
            existingAppointments.Add((doctorId, startingDateTime));
            int patientId = patientIds[Random.Shared.Next(patientIds.Count)];
            int apoointmentStatusId = appointmentStatusIds[Random.Shared.Next(appointmentStatusIds.Count)]; 
            return new Appointment
            {
                DoctorId = doctorId,
                PatientId = patientId,
                StartingDateTime = startingDateTime,
                AppointmentStatusId = apoointmentStatusId
            };
        } 
    }
}
