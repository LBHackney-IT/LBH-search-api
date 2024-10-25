using Hackney.Core.Logging;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using HousingSearchApi.V2.Domain.DTOs;
using HousingSearchApi.V2.UseCase;
using System;

namespace HousingSearchApi.V2.Controllers;

[ApiVersion("1")]
[Produces("application/json")]
[Route("api/v2/search/")]
[ApiController]
public class SearchController : Controller
{
    private readonly SearchUseCase _searchUseCase;

    public SearchController(SearchUseCase searchUseCase)
    {
        _searchUseCase = searchUseCase;
    }

    [LogCall(Microsoft.Extensions.Logging.LogLevel.Information)]
    [HttpGet("{indexName}")]
    public async Task<IActionResult> Search(string indexName, [FromQuery] SearchParametersDto searchParametersDto)
    {
        var useSearchV1 = Environment.GetEnvironmentVariable("USE_SEARCH_V1") ?? "false";
        if (useSearchV1 == "true")
        {
            var redirectUrl = $"/api/v1/search/{indexName}{Request.QueryString}";
            return new RedirectResult(redirectUrl, permanent: false);
        }

        var searchResults = await _searchUseCase.ExecuteAsync(indexName, searchParametersDto).ConfigureAwait(false);

        var response = new
        {
            Results = new Dictionary<string, IReadOnlyCollection<object>>
            {
                [indexName] = searchResults.Documents
            },
            searchResults.Total
        };

        return new OkObjectResult(response);
    }
}

