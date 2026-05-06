using MedicalData.Infrastructure.DTOs;

namespace DataGenerator.Generators
{
    public class DoctorSpecializationGenerator
    {
        public HashSet<(int, int)> existingRelations = new();
       
        public List<DoctorSpecializationSnapshotDTO> GenerateDoctorSpecializationRelation(List<int> doctorIds,List<int> specializationsId, HashSet<(int,int)> existingRelations) 
        {
            var result = new List<DoctorSpecializationSnapshotDTO>();
            var random = new Random();
            foreach (var doctorId in doctorIds)
            {
                int numberOfSpecializations = random.Next(1, 4);
                var assignedSpecializations = new HashSet<int>();
                for (int i = 0; i < numberOfSpecializations; i++)
                {
                    int specializationId;
                    do
                    {
                        specializationId = specializationsId[random.Next(specializationsId.Count)];
                    } while (assignedSpecializations.Contains(specializationId) || existingRelations.Contains((doctorId, specializationId)));
                    assignedSpecializations.Add(specializationId);
                    result.Add(new DoctorSpecializationSnapshotDTO
                    {
                        SpecializationId = specializationId,
                        DoctorId = doctorId,
                    });
                    existingRelations.Add((doctorId, specializationId));
                }
            }
            return result;
        }
    }
}
