using HousingSearchApi.V1.Boundary.Requests;
using HousingSearchApi.V1.Gateways;
using Nest;

namespace HousingSearchApi.V1.Interfaces
{
    public interface ISearchPersonsQueryContainerOrchestrator
    {
        QueryContainer Create(GetPersonListRequest request,
            QueryContainerDescriptor<QueryablePerson> q);
    }
}
