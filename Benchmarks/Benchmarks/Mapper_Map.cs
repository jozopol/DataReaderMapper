using BenchmarkDotNet.Attributes;
using DataReaderMapper;
using Mapping;
using System.Data;

namespace Benchmarks.Benchmarks
{
    [Config(typeof(MappingConfiguration))]
    public class Mapper_Mapping
    {
        private DataReaderMapper<DataTableReader> _drm = new DataReaderMapper<DataTableReader>();
        private static DataTable _table = BuildDataTable(1);

        [Setup]
        public void Setup()
        {
            _drm.Configure<Dto10Prop>();
        }

        [Benchmark(Baseline = true)]
        public void ManualMappingOnce()
        {
            using (var reader = _table.CreateDataReader())
            {
                reader.Read();
                ReadManually(reader);
            }
        }


        [Benchmark]
        public void MapperOnce()
        {
            using (var reader = _table.CreateDataReader())
            {
                reader.Read();
                _drm.Map<Dto10Prop>(reader);
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
                if (i % 9 == 0)
                {
                    AddRow(dt, true);
                }
                AddRow(dt);
            }

            return dt;
        }

        private static void AddRow(DataTable dt, bool causeException = false)
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
            if (causeException)
            {
                nr["10"] = null;
            }
            else
            {
                nr["10"] = 1;
            } 

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
