namespace DataGenerator.DTO
{
    public class BenchmarkResultDTO
    {
        public double SqlWithIndexesAvgMs { get; set; }
        public double SqlWithoutIndexesAvgMs { get; set; }
        public double NoSqlWithIndexesAvgMs { get; set; }
        public double NoSqlWithoutIndexesAvgMs { get; set; }

        public int TotalRecords { get; set; }
    }
}
