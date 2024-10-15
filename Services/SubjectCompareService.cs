using Schedule.Models;
using System.Diagnostics.CodeAnalysis;

namespace Schedule.Services
{
    public class SubjectCompareService : IEqualityComparer<Subject>
    {
        public bool Equals(Subject? x, Subject? y)
        {
            if (x is null || y is null)
            {
                return false;
            }
            else 
            {
                return x.Id == y.Id && x.Title == y.Title;
            }
        }

        public int GetHashCode([DisallowNull] Subject obj)
        {
            return obj.Id.GetHashCode() ^ obj.Title.GetHashCode();
        }
    }
}
