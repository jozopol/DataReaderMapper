using BenchmarkDotNet.Attributes;
using DataReaderMapper;
using System.Data;
using Benchmarks.Benchmarks.DTOs;

namespace Benchmarks.Benchmarks
{
    [Config(typeof(MappingConfiguration))]
    public class MapperMapping
    {
        private readonly DataReaderMapper<DataTableReader> _drm = new DataReaderMapper<DataTableReader>();
        private DataTable _mapperInputTable;

        [Params(1, 100, 10000)]
        public int RowCount;

        [Setup]
        public void Setup()
        {
            _drm.Configure<Dto10Prop>();
            _drm.Configure<Dto10NullableProp>();
            _mapperInputTable = BuildDataTable(RowCount);
        }

        [Benchmark(Baseline = true)]
        public void ManualMapping()
        {
            using (var reader = _mapperInputTable.CreateDataReader())
            {
                while (reader.Read())
                {
                    var dto = ReadManually(reader);
                }
            }
        }
        
        [Benchmark]
        public void Mapper()
        {
            using (var reader = _mapperInputTable.CreateDataReader())
            {
                while (reader.Read())
                {
                    var dto = _drm.Map<Dto10Prop>(reader);
                }
            }
        }

        [Benchmark]
        public void ManualMappingNullables()
        {
            using (var reader = _mapperInputTable.CreateDataReader())
            {
                while (reader.Read())
                {
                    var dto = ReadManuallyNullables(reader);
                }
            }
        }

        [Benchmark]
        public void MapperNullables()
        {
            using (var reader = _mapperInputTable.CreateDataReader())
            {
                while (reader.Read())
                {
                    var dto = _drm.Map<Dto10NullableProp>(reader);
                }
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

        private static Dto10Prop ReadManuallyNullables(IDataReader reader)
        {
            return new Dto10Prop
            {
                MyProperty = reader.IsDBNull(reader.GetOrdinal("1")) ? null : reader["1"].ToString(),
                MyProperty2 = reader.IsDBNull(reader.GetOrdinal("2")) ? null: reader["2"].ToString(),
                MyProperty3 = reader.IsDBNull(reader.GetOrdinal("3")) ? null: reader["3"].ToString(),
                MyProperty4 = reader.IsDBNull(reader.GetOrdinal("4")) ? null: reader["4"].ToString(),
                MyProperty5 = reader.IsDBNull(reader.GetOrdinal("5")) ? null: reader["5"].ToString(),
                MyProperty6 = reader.IsDBNull(reader.GetOrdinal("6")) ? null: reader["6"].ToString(),
                MyProperty7 = reader.IsDBNull(reader.GetOrdinal("7")) ? null: reader["7"].ToString(),
                MyProperty8 = reader.IsDBNull(reader.GetOrdinal("8")) ? null: reader["8"].ToString(),
                MyProperty9 = reader.IsDBNull(reader.GetOrdinal("9")) ? null : reader["9"].ToString(),
                MyProperty10 = reader.IsDBNull(reader.GetOrdinal("10")) ? null : reader["10"].ToString()  
            };
        }
    }    
}
