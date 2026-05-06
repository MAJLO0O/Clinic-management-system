using MedicalData.Aplication.Services;
using MedicalData.Export.Manifest;
using MedicalData.Export.Services;
using MedicalData.Import.Services;
using MedicalData.Import.ZipReader;
using MedicalData.Infrastructure.Repositories;
using DataGenerator.Services;
using DataGenerator.Generators;
using MongoDB.Driver;
using MedicalData.Aplication.Services.CRUD;
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle

builder.Services.AddSingleton<IMongoClient>(sp =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    return new MongoClient(config.GetConnectionString("MongoDb"));
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<SeedService>();
builder.Services.AddScoped<SyncService>();
builder.Services.AddScoped<ReadService>();
builder.Services.AddScoped<DoctorSeederService>();
builder.Services.AddScoped<AppointmentDataSeeder>();
builder.Services.AddScoped<PatientDataSeeder>();
builder.Services.AddScoped<BenchmarkService>();

builder.Services.AddScoped<PatientService>();
builder.Services.AddScoped<DoctorService>();
builder.Services.AddScoped<SpecializationService>();
builder.Services.AddScoped<BranchService>();

builder.Services.AddScoped<IndexBenchmarkService>();
builder.Services.AddScoped<AppointmentRepository>();
builder.Services.AddScoped<ImportDataRepository>();
builder.Services.AddScoped<AppointmentStatusRepository>();
builder.Services.AddScoped<BranchRepository>();
builder.Services.AddScoped<DoctorRepository>();
builder.Services.AddScoped<DoctorSpecializationRepository>();
builder.Services.AddScoped<ExportDataRepository>();
builder.Services.AddScoped<MedicalRecordRepository>();
builder.Services.AddScoped<PatientRepository>();
builder.Services.AddScoped<PaymentMethodRepository>();
builder.Services.AddScoped<PaymentRepository>();
builder.Services.AddScoped<PaymentStatusRepository>();
builder.Services.AddScoped<SpecializationRepository>();
builder.Services.AddScoped<ExportManifestBuilder>();
builder.Services.AddScoped<ExportManifestModel>();
builder.Services.AddScoped<ManifestReader>();
builder.Services.AddScoped<ExportDataService>();
builder.Services.AddScoped<MongoRepository>();
builder.Services.AddScoped<MedicalDataImportService>();

builder.Services.AddScoped<DoctorGenereator>();
builder.Services.AddScoped<PatientGenerator>();
builder.Services.AddScoped<DoctorSpecializationGenerator>();
builder.Services.AddScoped<AppointmentGenerator>();
builder.Services.AddScoped<MedicalRecordGenerator>();
builder.Services.AddScoped<PaymentGenerator>();
builder.Services.AddScoped<GeneratorMethods>();
builder.Services.AddCors(options =>
{
options.AddPolicy("AllowAll",
        policy => {
            policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
});
});

builder.WebHost.ConfigureKestrel(options =>
{
    options.AllowSynchronousIO = true;
});

var app = builder.Build();


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");

app.UseAuthorization();

app.MapControllers();

app.Run();
