using BenchmarkDotNet.Attributes;
using DataReaderMapper;
using System.Collections.Generic;
using System.Data;

namespace Benchmarks.Benchmarks
{
    [Config(typeof(MappingConfiguration))]
    public class Mapper_IEnumerableMapping
    {
        private DataReaderMapper<DataTableReader> _drm = new DataReaderMapper<DataTableReader>();

        [Params(10, 100, 500)]
        public int NumberOfRecords;
        public DataTable _table;

        [Setup]
        public void Setup()
        {
            _drm.Configure<Dto10Prop>();
            _table = BuildDataTable(NumberOfRecords);
        }

        [Benchmark(Baseline = true)]
        public void ManualMappingLoop()
        {
            List<Dto10Prop> list = new List<Dto10Prop>();
            using (var reader = _table.CreateDataReader())
            {
                while (reader.Read())
                {
                    list.Add(ReadManually(reader));
                }
            }
        }


        [Benchmark]
        public void MapperMapAll()
        {
            using (var reader = _table.CreateDataReader())
            {
                var list = _drm.MapAll<Dto10Prop>(reader);
            }
        }

        private static DataTable BuildDataTable(int rows)
        {
            var dt = new DataTable("TestTable");
            dt.Columns.Add(new DataColumn("1", typeof(string)));
            dt.Columns.Add(new DataColumn("2", typeof(string)));
            dt.Columns.Add(new DataColumn("3", typeof(string)));
            dt.Columns.Add(new DataColumn("4", typeof(string)));
            dt.Columns.Add(new DataColumn("5", typeof(string)));
            dt.Columns.Add(new DataColumn("6", typeof(string)));
            dt.Columns.Add(new DataColumn("7", typeof(string)));
            dt.Columns.Add(new DataColumn("8", typeof(string)));
            dt.Columns.Add(new DataColumn("9", typeof(string)));
            dt.Columns.Add(new DataColumn("10", typeof(string)));

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
            nr["2"] = "Whoa";
            nr["3"] = "19/04/2014";
            nr["4"] = "Another kind of text";
            nr["5"] = "Name";
            nr["6"] = 1;
            nr["7"] = 1;
            nr["8"] = 1;
            nr["9"] = 1;
            nr["10"] = 1;

            dt.Rows.Add(nr);
        }

        private static Dto10Prop ReadManually(IDataReader reader)
        {
            return new Dto10Prop
            {
                MyProperty = reader["1"].ToString(),
                MyProperty2 = reader["2"].ToString(),
                MyProperty3 = reader["3"].ToString(),
                MyProperty4 = reader["4"].ToString(),
                MyProperty5 = reader["5"].ToString(),
                MyProperty6 = reader["6"].ToString(),
                MyProperty7 = reader["7"].ToString(),
                MyProperty8 = reader["8"].ToString(),
                MyProperty9 = reader["9"].ToString(),
                MyProperty10 = reader["10"].ToString()
            };
        }
    }
}
