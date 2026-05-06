using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace MedicalData.Export.Manifest
{
    public class ExportManifestBuilder
    {
        public ExportManifestModel Build()
        {
            return new ExportManifestModel()
            {
                Version = 1,
                Entities = new List<ExportEntity>
                {
                    new(){EntityName="appointment_status",FileName="exported_appointment_status.json", Order=1},
                    new(){EntityName="branch",FileName="exported_branches.json", Order=2},
                    new(){EntityName="doctor",FileName="exported_doctors.json", Order=3},
                    new(){EntityName="specialization",FileName="exported_specializations.json", Order=4},
                    new(){EntityName="doctor_specialization",FileName="exported_doctors_specializations.json", Order=5},
                    new(){EntityName="payment_method",FileName="exported_payment_methods.json", Order =6},
                    new(){EntityName="payment_status",FileName="exported_payment_statuses.json",Order=7},
                    new(){EntityName="patient",FileName="exported_patients.json", Order=8},
                    new(){EntityName="appointment",FileName="exported_appointments.json", Order=9},
                    new(){EntityName="payment",FileName="exported_payments.json", Order=10},
                    new(){EntityName="medical_record",FileName="exported_medical_records.json", Order=11}
                }
            };
        }
        public async Task AddToZipAsync(ZipArchive zip, CancellationToken ct)
        {
            var entry = zip.CreateEntry("manifest.json");
            await using var stream = entry.Open();
            var manifest = Build();
            await JsonSerializer.SerializeAsync(stream,manifest,cancellationToken: ct);

            
        }
    }
}
