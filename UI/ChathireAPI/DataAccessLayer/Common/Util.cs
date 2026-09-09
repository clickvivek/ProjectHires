using Microsoft.Data.SqlClient.Server;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccessLayer.Common
{
    internal class Util
    {


        public static DataTable PopulateDataTable(List<object> searchObj, Type type)
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("Value", type);

            //populate your Datatable
            foreach (object searchString in searchObj)
            {
                DataRow dr = null;
                dr = dt.NewRow(); // have new row on each iteration
                dr["Value"] = searchString;
                dt.Rows.Add(dr);
            }

            return dt;
        }
        public static IEnumerable<SqlDataRecord> CreateDataTable(List<string> searchs, SqlMetaData[] _meta)
        {

            var record = new SqlDataRecord(_meta);
            var enumerable = searchs.Select(a =>
            {
                for (int i = 0; i < a.Length; i++)
                    record.SetValue(i, a[i]);
                return record;
            });
            return enumerable;
        }
    }
}
