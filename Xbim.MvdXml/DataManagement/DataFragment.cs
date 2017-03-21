using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using Microsoft.SqlServer.Server;

namespace Xbim.MvdXml.DataManagement
{
    public class DataFragment
    {
        internal List<string> FieldNames;
        internal List<List<object>> Values;

        public DataFragment()
        {
        }

        public DataFragment(List<string> fieldNames, List<List<object>> values)
        {
            FieldNames = fieldNames;
            Values = values;
        }

        public DataFragment(string storageName, object value)
        {
            FieldNames = new List<string>() { storageName };
            Values = new List<List<object>>() { new List<object>() { value } };
        }

        public DataFragment(string storageName, IEnumerable<object> asEnum)
        {
            FieldNames = new List<string>() {storageName};
            Values = new List<List<object>>();
            foreach (var o in asEnum)
            {
                Values.Add(new List<object>() { o });
            }
        }

        public static DataFragment Combine(List<DataFragment> fragments)
        {
            var okFragments = fragments.Where(f => f != null && !f.IsEmpty).ToArray();
            if (okFragments.Length == 0)
                return null;
            if (okFragments.Length == 1)
                return okFragments[0];

            var oList = new List<List<object>>[okFragments.Length];

            var i=0;
            foreach (var dataFragment in okFragments)
            {
                oList[i++] = dataFragment.Values;
            }

            var titles = okFragments.SelectMany(x => x.FieldNames).ToList();
            

            var rowsT = Combinations.GetCombinations(oList);
            var LRet = new List<List<object>>();
            foreach (var objectse in rowsT)
            {
                var l = new List<object>();
                foreach (var list in objectse)
                {
                    l.AddRange(list);
                }
                LRet.Add(l);
            }
            return new DataFragment(titles, LRet);
        }

        public void Merge(DataFragment p0)
        {
            if (p0 == null || p0.IsEmpty)
                return;
            if (!IsEmpty)
            {
                // both not empty: merge
                //
                if (FieldNames.SequenceEqual(p0.FieldNames))
                {
                    // just add other values to the array
                    //
                    Values.AddRange(p0.Values);
                }
                else
                {
                    throw new NotImplementedException();
                }
            }
            else if (IsEmpty)
            {
                // get the other
                //
                FieldNames = new List<string>();
                FieldNames.AddRange(p0.FieldNames);

                Values = new List<List<object>>();
                Values.AddRange(p0.Values);
            }
        }

        public bool IsEmpty => FieldNames == null || !FieldNames.Any();

        public void Populate(DataTable dt)
        {
            if (IsEmpty)
                return;
            foreach (var valueList in Values)
            {
                var row = dt.NewRow();
                var i = 0;
                foreach (var key in FieldNames)
                {
                    try
                    {
                        row[key] = valueList[i++];
                    }
                    catch
                    {
                        throw new Exception("Unexpected field name populating datatable.");
                    }
                }
                dt.Rows.Add(row);
            }
        }
    }
}
