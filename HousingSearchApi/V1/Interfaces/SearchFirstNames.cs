using HousingSearchApi.V1.Boundary.Requests;
using HousingSearchApi.V1.Gateways;
using HousingSearchApi.V1.Gateways.Models;
using Nest;

namespace HousingSearchApi.V1.Interfaces
{
    public class SearchFirstNames : ISearchPersonQueryContainer
    {
        public QueryContainer Create(GetPersonListRequest request, QueryContainerDescriptor<QueryablePerson> q)
        {
            if (string.IsNullOrWhiteSpace(request.SearchText)) return null;
            var searchText = request.SearchText?.Replace(" ", "").ToLower();

            var searchFirstNames = q.Wildcard(m =>
                m.Field(f => f.Firstname).Value($"*{searchText}*"));

            return searchFirstNames;
        }
    }
}