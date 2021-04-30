using System.Threading.Tasks;
using HousingSearchApi.V1.Boundary.Requests;
using HousingSearchApi.V1.Gateways;
using Nest;

namespace HousingSearchApi.V1.Interfaces
{
    public interface ISearchPersonElasticSearchHelper
    {
        Task<ISearchResponse<QueryablePerson>> Search(GetPersonListRequest request);
    }
}
