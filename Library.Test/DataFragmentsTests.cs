using System;
using System.Collections.Generic;
using Xbim.MvdXml.DataManagement;
using Xunit;

namespace Tests
{
    public class DataFragmentsTests
    {
        [Fact]
        public void FragmentCombine()
        {
            var f1 = new DataFragment("ValueA", "A");
            var f2 = new DataFragment("ValueB", "B");
            var lst1 = new List<DataFragment>() {f1, f2};

            var ret1 = DataFragment.Combine(lst1);

            var f3 = new DataFragment("ValueC", "C");
            var lst2 = new List<DataFragment>() {ret1, f3};
            var ret2 = DataFragment.Combine(lst2);

            var f4 = new DataFragment("ValueMulti", new List<object>() {"Multi1", "Multi2"});
            var lst3 = new List<DataFragment>() { ret2, f4 };
            _ = DataFragment.Combine(lst3);
        }
    }
}
