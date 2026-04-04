using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using MedicalData.Infrastructure.DTOs;
using MedicalData.Infrastructure.Repositories;

namespace MedicalData.Import.Services
{
    public class MedicalDataImportService
    {
        private readonly DoctorRepository _doctorRepository;
        private readonly PatientRepository _patientRepository;
        private readonly AppointmentRepository _appointmentRepository;

        public MedicalDataImportService(DoctorRepository doctorRepository, PatientRepository patientRepository, AppointmentRepository appointmentRepository)
        {
            _doctorRepository = doctorRepository;
            _patientRepository = patientRepository;
            _appointmentRepository = appointmentRepository;
        }

        public async Task<List<AppointmentExportDTO>> ImportDataFromJson()
        {
            var json = File.ReadAllText(@"C:\Users\czupr\source\repos\MAJLO0O\Clinic-management-system\MedicalData.Export\bin\Debug\net8.0\exported_appointments.json");
            var data = JsonSerializer.Deserialize<List<AppointmentExportDTO>>(json);

            return data;
        }
    }
}
