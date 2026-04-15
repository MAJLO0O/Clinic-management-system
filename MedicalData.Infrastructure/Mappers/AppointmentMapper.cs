using MedicalData.Infrastructure.MongoModels;
using MedicalData.Infrastructure.NoSqlDTOs;
using MongoDB.Driver.Core.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MedicalData.Infrastructure.Mappers
{
    public class AppointmentMapper
    {
        public List<AppointmentDocument> MapToAppointmentDocuments(List<AppointmentExport> dto)
        {
            var appointmentDocuments = dto.Select(appointment => new AppointmentDocument
            {
                Id = appointment.AppointmentId,
                StartingDateTime = appointment.AppointmentStartingDateTime,
                Status = appointment.AppointmentStatus,
                MedicalRecordSnapshot = appointment.MedicalRecordNote != null ? new MedicalRecordSnapshot
                {
                    Note = appointment.MedicalRecordNote,
                    CreatedAt = appointment.MedicalRecordCreatedAt
                } : null,
                DoctorSnapshot = new DoctorSnapshot
                {
                    Id = appointment.DoctorId,
                    FirstName = appointment.DoctorFirstName,
                    LastName = appointment.DoctorLastName,
                    Specializations = appointment.DoctorSpecializations.ToList(),
                    Pesel = appointment.DoctorPesel,

                    BranchSnapshot = new BranchSnapshot
                    {
                        Id = appointment.DoctorBranchId,
                        City = appointment.DoctorBranchCity,
                        Address = appointment.DoctorBranchAddress
                    }
                },
                PatientSnapshot = new PatientSnapshot
                {
                    Id = appointment.PatientId,
                    FirstName = appointment.PatientFirstName,
                    LastName = appointment.PatientLastName,
                    Pesel = appointment.PatientPesel
                },
                PaymentSnapshot = appointment.PaymentId >0 ? new PaymentSnapshot
                {
                    Id = appointment.PaymentId,
                    Number = appointment.PaymentNumber,
                    Amount = appointment.PaymentAmount,
                    Method = appointment.PaymentMethod,
                    Status = appointment.PaymentStatus
                } : null
            });

            return appointmentDocuments.ToList();
        }
    }
}
