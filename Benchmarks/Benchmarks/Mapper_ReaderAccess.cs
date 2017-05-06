using BenchmarkDotNet.Attributes;
using DataReaderMapper;
using System.Data;

namespace Benchmarks.Benchmarks
{
    [Config(typeof(MappingConfiguration))]
    public class Mapper_ReaderAccess
    {
        private DataReaderMapper<DataTableReader> _drm = new DataReaderMapper<DataTableReader>();
        private static DataTable _table;

        [Params(10, 100, 500, 5000)]
        public int TableSize;

        [Setup]
        public void Setup()
        {
            _drm.Configure<ReaderAccessBenchmarkDto>();
            _table = BuildDataTable(TableSize);
        }

        [Benchmark]
        public void ReadersMethodAccess()
        {
            using (var reader = _table.CreateDataReader())
            {
                reader.Read();
                string value = reader.GetString(reader.GetOrdinal("1"));
            }
        }

        [Benchmark(Baseline = true)]
        public void AccessByColumnNameWithTypeCast()
        {
            using (var reader = _table.CreateDataReader())
            {
                reader.Read();
                string value = reader["1"].ToString();
            }
        }

        private static DataTable BuildDataTable(int rows)
        {
            var dt = new DataTable("TestTable");
            dt.Columns.Add(new DataColumn("1", typeof(string)));

            for (int i = 0; i < rows; i++)
            {
                AddRow(dt);
            }

            return dt;
        }

        private static void AddRow(DataTable dt)
        {
            var nr = dt.NewRow();
            nr["1"] = "This is a text";

            dt.Rows.Add(nr);
        }
    }

    public class ReaderAccessBenchmarkDto
    {
        public string StringProperty { get; set; }
    }

}
