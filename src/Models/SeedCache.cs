using System;
using System.Collections;

namespace PigeonAPI.Models;

/// <summary>
/// An cache to store seed prompts in memory without going to the DB unnecessarily
/// </summary>
public class SeedCache
{
    // custom comparer to compare by first appearance in list
    private class CustomOrderingByGivenOrdering : IComparer<DatabaseImage>
    {
        private readonly long[] idOrderings;

        public CustomOrderingByGivenOrdering(long[] idOrderings)
        {
            this.idOrderings = idOrderings;
        }

        int IComparer<DatabaseImage>.Compare(DatabaseImage? x, DatabaseImage? y)
        {
            if (x == null || y == null)
            {
                throw new NotImplementedException();
            }
            
            int xIndex = Array.IndexOf<long>(idOrderings, x.Id);
            int yIndex = Array.IndexOf<long>(idOrderings, y.Id);
            return xIndex.CompareTo(yIndex);
        }
    }

    public SeedCache(ILogger logger, params long[] seedsInOrder)
    {

        foreach (long seed in seedsInOrder)
        {
            if (seed <= 0)
            {
                throw new Exception("Given nonexistant negative seed");
            }
        }

        List<DatabaseImage> prompts;
        using (var db = new DatabaseAccess(logger))
        {
            var speeder = new HashSet<long>(seedsInOrder);


            if (speeder.Count() != seedsInOrder.Count())
            {
                throw new Exception("Given duplicate seeds.");
            }

            prompts = 
                db.Images.Where(item => seedsInOrder.Contains(item.Id)).ToList();
            
            // sort prompts by seed orderings
            prompts.Sort(new CustomOrderingByGivenOrdering(seedsInOrder));
        }

        this.Prompts = prompts;
    }

    public IReadOnlyList<DatabaseImage> Prompts { get; }
}


