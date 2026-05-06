using MedicalData.Export.Manifest;
using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace MedicalData.Import.ZipReader
{
    public class ManifestReader
    {
        private const string ManifestFileName = "manifest.json";


        public async Task<ExportManifestModel> ReadManifestAsync(ZipArchive zip, CancellationToken ct)
        {
            var entry = zip.GetEntry(ManifestFileName);
            if (entry == null)
            {
                throw new Exception("manifest file not found in archive");
            }
            await using var stream = entry.Open();
            var manifest = await JsonSerializer.DeserializeAsync<ExportManifestModel>(stream, cancellationToken: ct);

            if (!manifest.Entities.Any() || manifest.Entities == null)
                throw new InvalidOperationException("manifest file is empty");

            return manifest;
        }
    }
}
