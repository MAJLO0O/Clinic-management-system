using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MedicalData.Infrastructure.Helpers
{
    public static class PathHelper
    {
        public static string GetDataPath()
        {
            var rootPath = Directory
            .GetParent(AppContext.BaseDirectory)!
            .Parent!.Parent!.Parent!.Parent!.FullName;

            var dataPath = Path.Combine(rootPath, "Data");

            Directory.CreateDirectory(dataPath);

            return dataPath;
        }

        
    }
}
