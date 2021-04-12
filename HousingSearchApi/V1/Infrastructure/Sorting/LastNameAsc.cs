using Nest;

namespace HousingSearchApi.V1.Infrastructure.Sorting
{
    public class LastNameAsc : IPersonListSort
    {
        public SortDescriptor<QueryablePerson> Get(SortDescriptor<QueryablePerson> descriptor)
        {
            return descriptor
                .Ascending(f => f.Surname)
                .Ascending(f => f.Firstname);
        }
    }
}
