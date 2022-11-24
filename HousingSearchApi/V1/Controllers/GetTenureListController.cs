using Amazon.Lambda.Core;
using Hackney.Core.Logging;
using HousingSearchApi.V1.Boundary.Requests;
using HousingSearchApi.V1.Boundary.Responses;
using HousingSearchApi.V1.Boundary.Responses.Metadata;
using HousingSearchApi.V1.UseCase.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Threading.Tasks;

namespace HousingSearchApi.V1.Controllers
{
    [ApiVersion("1")]
    [Produces("application/json")]
    [Route("api/v1/search/tenures")]
    [ApiController]
    public class GetTenureListController : BaseController
    {
        private readonly IGetTenureListUseCase _getTenureListUseCase;
        private readonly IGetTenureListByPrnListUseCase _getTenureListByPrnListUseCase;

        public GetTenureListController(IGetTenureListUseCase getTenureListUseCase, IGetTenureListByPrnListUseCase getTenureListByPrnListUseCase)
        {
            _getTenureListUseCase = getTenureListUseCase;
            _getTenureListByPrnListUseCase = getTenureListByPrnListUseCase;
        }

        [ProducesResponseType(typeof(APIResponse<GetTenureListResponse>), 200)]
        [ProducesResponseType(typeof(APIResponse<NotFoundException>), 404)]
        [ProducesResponseType(typeof(APIResponse<BadRequestException>), 400)]
        [HttpGet, MapToApiVersion("1")]
        [LogCall(LogLevel.Information)]
        public async Task<IActionResult> GetTenureList([FromQuery] GetTenureListRequest request)
        {
            try
            {
                var tenuresSearchResult = await _getTenureListUseCase.ExecuteAsync(request).ConfigureAwait(false);
                var apiResponse = new APIResponse<GetTenureListResponse>(tenuresSearchResult);
                apiResponse.Total = tenuresSearchResult.Total();

                return new OkObjectResult(apiResponse);
            }
            catch (Exception e)
            {
                LambdaLogger.Log(e.Message + e.StackTrace);
                return new BadRequestObjectResult(e.Message);
            }
        }

        [ProducesResponseType(typeof(APIResponse<GetTenureListResponse>), 200)]
        [ProducesResponseType(typeof(APIResponse<NotFoundException>), 404)]
        [ProducesResponseType(typeof(APIResponse<BadRequestException>), 400)]
        [HttpGet("byPrnList"), MapToApiVersion("1")]
        [LogCall(LogLevel.Information)]
        public async Task<IActionResult> GetTenureList([FromQuery] GetTenureListByPrnListRequest request)
        {
            try
            {
                var tenuresSearchResult = await _getTenureListByPrnListUseCase.ExecuteAsync(request).ConfigureAwait(false);
                var apiResponse = new APIResponse<GetTenureListResponse>(tenuresSearchResult);
                apiResponse.Total = tenuresSearchResult.Total();

                return new OkObjectResult(apiResponse);
            }
            catch (Exception e)
            {
                LambdaLogger.Log(e.Message + e.StackTrace);
                return StatusCode((int) HttpStatusCode.InternalServerError, e.Message);
            }
        }
    }
}
