using Audionix.Models;
using Microsoft.EntityFrameworkCore;
using Serilog;
using System.Collections.Generic;
using System.Linq;

namespace Audionix.Services
{
    public class StationService
    {
        public List<AudioMetadata> GetFolderFileList(string selectedStation, List<Station>? stations, AudionixDbContext dbContext)
        {
            Log.Information("--- StationService - GetFolderFileList() -- Start");
            var filesInDirectory = new List<AudioMetadata>();

            if (!string.IsNullOrEmpty(selectedStation) && stations != null)
            {
                var station = stations.FirstOrDefault(s => s.CallLetters == selectedStation);
                if (station != null)
                {
                    filesInDirectory = dbContext.AudioMetadatas
                        .AsNoTracking()
                        .Where(am => am.StationId == station.Id)
                        .ToList();
                }
            }

            Log.Information("--- StationService - GetFolderFileList() -- End - filesInDirectory: {Count}", filesInDirectory.Count);
            return filesInDirectory;
        }
    }
}
