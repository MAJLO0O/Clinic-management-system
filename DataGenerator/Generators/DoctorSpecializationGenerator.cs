using DataGenerator.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataGenerator.Generators
{
    public class DoctorSpecializationGenerator
    {
        public HashSet<(int, int)> existingRelations = new();
       
        public List<(int doctorId, int specializationId)> GenerateDoctorSpecializationRelation(List<int> doctorIds,List<int> specializationsId, HashSet<(int,int)> existingRelations) 
        {
            var result = new List<(int doctorId, int specializationId)>();
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
                    result.Add((doctorId, specializationId));
                    existingRelations.Add((doctorId, specializationId));
                }
            }
            return result;
        }
    }
}
