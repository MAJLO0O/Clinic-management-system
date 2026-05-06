using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MedicalData.Export.Manifest
{
    public class ExportManifestModel
    {
        public int Version { get; set; }
        public List<ExportEntity> Entities { get; set; }
    }
    public class ExportEntity
    {
        public string EntityName { get; set; }
        public string FileName { get; set; }
        public int Order { get; set; }
    }
}
