using MedicalData.Domain.Models;
using MedicalData.Infrastructure.Repositories;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MedicalData.Aplication.Services.CRUD
{
    public class AppointmentService
    {
        private readonly string _connectionString;
        private readonly AppointmentRepository _appointmentRepository;
        public AppointmentService(IConfiguration configuration, AppointmentRepository appointmentRepository)
        {
            _connectionString = configuration.GetConnectionString("Postgres") ?? throw new ArgumentNullException(nameof(configuration));
            _appointmentRepository = appointmentRepository;        
        }
        public async Task<bool> UpdateAppointmentAsync(int id,Appointment appointment, CancellationToken ct)
        {
            await using var connection = DbConnectionFactory.CreateDbConnection(_connectionString);
            await connection.OpenAsync(ct);

            return await _appointmentRepository.UpdateAppointmentAsync(connection, id, appointment, ct);

        }
    }
}
