using HousingSearchApi.V1.Boundary.Requests;
using HousingSearchApi.V1.Boundary.Responses;
using System.Threading.Tasks;
using HousingSearchApi.V1.Boundary.Responses.Transactions;

namespace HousingSearchApi.V1.Gateways.Interfaces
{
    public interface ISearchGateway
    {
        Task<GetPersonListResponse> GetListOfPersons(GetPersonListRequest query);
        Task<GetTenureListResponse> GetListOfTenures(GetTenureListRequest query);
        Task<GetTenureListResponse> GetListOfTenures(GetTenureListByPrnListRequest query);
        Task<GetAssetListResponse> GetListOfAssets(GetAssetListRequest query);
        Task<GetAllAssetListResponse> GetListOfAssetsSets(GetAllAssetListRequest query);
        Task<GetAccountListResponse> GetListOfAccounts(GetAccountListRequest query);
        Task<GetTransactionListResponse> GetListOfTransactions(GetTransactionListRequest request);
        Task<GetStaffListResponse> GetListOfStaffs(GetStaffListRequest query);
        Task<GetProcessListResponse> GetListOfProcesses(GetProcessListRequest query);
    }
}
