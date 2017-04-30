using BenchmarkDotNet.Attributes;
using DataReaderMapper;
using Mapping;
using System.Data;
using static Benchmarks.Mapper_Configure;

namespace Benchmarks
{
    [Config(typeof(BenchmarkConfiguration))]
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
            nr["6"] = "Field";
            nr["7"] = "AnotherOne";
            nr["8"] = "Beach";
            nr["9"] = "Pumpkin";
            nr["10"] = "What";

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

    [Config(typeof(BenchmarkConfiguration))]
    public class Mapper_Configure
    {
        private DataReaderMapper<DataTableReader> _mapper = new DataReaderMapper<DataTableReader>();

        [Benchmark(Baseline = true)]
        public void Configure1()
        {
            _mapper.Configure<Dto1Prop>();
        }

        private class Dto1Prop
        {
            [Mappable("1")]
            public string MyProperty { get; set; }
        }

        [Benchmark]
        public void Configure5()
        {
            _mapper.Configure<Dto5Prop>();
        }

        private class Dto5Prop
        {
            [Mappable("1")]
            public string MyProperty { get; set; }

            [Mappable("2")]
            public string MyProperty2 { get; set; }

            [Mappable("3")]
            public string MyProperty3 { get; set; }

            [Mappable("4")]
            public string MyProperty4 { get; set; }

            [Mappable("5")]
            public string MyProperty5 { get; set; }
        }

        [Benchmark]
        public void Configure10()
        {
            _mapper.Configure<Dto10Prop>();
        }

        public class Dto10Prop
        {
            [Mappable("1")]
            public string MyProperty { get; set; }

            [Mappable("2")]
            public string MyProperty2 { get; set; }

            [Mappable("3")]
            public string MyProperty3 { get; set; }

            [Mappable("4")]
            public string MyProperty4 { get; set; }

            [Mappable("5")]
            public string MyProperty5 { get; set; }

            [Mappable("6")]
            public string MyProperty6 { get; set; }

            [Mappable("7")]
            public string MyProperty7 { get; set; }

            [Mappable("8")]
            public string MyProperty8 { get; set; }

            [Mappable("9")]
            public string MyProperty9 { get; set; }

            [Mappable("10")]
            public string MyProperty10 { get; set; }
        }
    }
}
