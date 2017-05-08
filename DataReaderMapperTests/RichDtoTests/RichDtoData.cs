using System;
using System.Data;

namespace DataReaderMapperTests.RichDtoTests
{
    public static class RichDtoData
    {
        public const string StrText = "StrText";
        public const int IntCount = int.MaxValue;
        public const string StrErrorCode = "strErrorCode";
        public const string StrErrorCategory = "strErrorCategory";
        public const string StrName = "strName";
        public const int IntAge = 20;
        public const string ListOrders = "order1,order2,order3,order4";
        public const string StrId = "strId";
        public const string StrCity = "strCity";
        public const string StrZip = "strZIP";
        public static DateTime DatLastLogin = DateTime.MaxValue;
        public const bool BoolConsent = true;

        public static DataTableReader BuildReader(int numberOfRows = 1, bool generateNulls = false)
        {
            var dataTable = new DataTable();
            dataTable.Columns.Add(new DataColumn("strText", typeof(string)));
            dataTable.Columns.Add(new DataColumn("intCount", typeof(int)));
            dataTable.Columns.Add(new DataColumn("strErrorCode", typeof(string)));
            dataTable.Columns.Add(new DataColumn("strErrorCategory", typeof(string)));
            dataTable.Columns.Add(new DataColumn("strName", typeof(string)));
            dataTable.Columns.Add(new DataColumn("intAge", typeof(int)));
            dataTable.Columns.Add(new DataColumn("listOrders", typeof(string)));
            dataTable.Columns.Add(new DataColumn("strId", typeof(string)));
            dataTable.Columns.Add(new DataColumn("strCity", typeof(string)));
            dataTable.Columns.Add(new DataColumn("strZIP", typeof(string)));
            dataTable.Columns.Add(new DataColumn("datLastLogin", typeof(DateTime)));
            dataTable.Columns.Add(new DataColumn("boolConsent", typeof(string)));

            for (int i = 0; i < numberOfRows; i++)
            {
                var dataRow = dataTable.NewRow();
                dataRow["strText"] = generateNulls ? null : StrText;
                dataRow["intCount"] = IntCount;
                dataRow["strErrorCode"] = generateNulls ? null : StrErrorCode;
                dataRow["strErrorCategory"] = generateNulls ? null : StrErrorCategory;
                dataRow["strName"] = StrName;
                dataRow["intAge"] = IntAge;
                dataRow["listOrders"] = ListOrders;
                dataRow["strId"] = StrId;
                dataRow["strCity"] = StrCity;
                dataRow["strZIP"] = StrZip;
                dataRow["datLastLogin"] = DatLastLogin;
                dataRow["boolConsent"] = "1";

                dataTable.Rows.Add(dataRow);
            }

            return dataTable.CreateDataReader();
        }
    }
}