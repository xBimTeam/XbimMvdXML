using System.Collections.Generic;

namespace Xbim.MvdXml.DataManagement
{
    internal static class Mathematics
    {
        // combination function from 
        // http://newscentral.exsees.com/item/5a032ab499762278ca45602a69135f19-4e33b3ef2b87f26076f715c0d25b11f6

        private static void GetCombinationsRec<T>(IList<IEnumerable<T>> sources, T[] chain, int index, ICollection<T[]> combinations)
        {
            foreach (var element in sources[index])
            {
                chain[index] = element;
                if (index == sources.Count - 1)
                {
                    var finalChain = new T[chain.Length];
                    chain.CopyTo(finalChain, 0);
                    combinations.Add(finalChain);
                }
                else
                {
                    GetCombinationsRec(sources: sources, chain: chain, index: index + 1, combinations: combinations);
                }
            }
        }

        internal static List<T[]> GetCombinations<T>(params IEnumerable<T>[] enumerables)
        {
            var combinations = new List<T[]>(enumerables.Length);
            if (enumerables.Length <= 0)
                return combinations;
            var chain = new T[enumerables.Length];
            GetCombinationsRec(sources: enumerables, chain: chain, index: 0, combinations: combinations);
            return combinations;
        }
    }
}
