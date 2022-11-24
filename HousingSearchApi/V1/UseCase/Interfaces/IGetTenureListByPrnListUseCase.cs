using HousingSearchApi.V1.Boundary.Requests;
using HousingSearchApi.V1.Boundary.Responses;
using System.Threading.Tasks;

namespace HousingSearchApi.V1.UseCase.Interfaces
{
    public interface IGetTenureListByPrnListUseCase
    {
        Task<GetTenureListResponse> ExecuteAsync(GetTenureListByPrnListRequest getPersonListRequest);
    }
}
