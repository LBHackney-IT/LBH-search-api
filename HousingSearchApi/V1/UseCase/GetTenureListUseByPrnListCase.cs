using Hackney.Core.Logging;
using HousingSearchApi.V1.Boundary.Requests;
using HousingSearchApi.V1.Boundary.Responses;
using HousingSearchApi.V1.Gateways.Interfaces;
using HousingSearchApi.V1.UseCase.Interfaces;
using System.Threading.Tasks;

namespace HousingSearchApi.V1.UseCase
{
    public class GetTenureListUseByPrnListCase : IGetTenureListByPrnListUseCase
    {
        private readonly ISearchGateway _searchGateway;

        public GetTenureListUseByPrnListCase(ISearchGateway searchGateway)
        {
            _searchGateway = searchGateway;
        }

        [LogCall]
        public async Task<GetTenureListResponse> ExecuteAsync(GetTenureListByPrnListRequest housingSearchRequest)
        {
            return await _searchGateway.GetListOfTenures(housingSearchRequest).ConfigureAwait(false);
        }

    }
}
