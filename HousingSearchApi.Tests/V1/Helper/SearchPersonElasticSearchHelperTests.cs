using System;
using System.Threading.Tasks;
using FluentAssertions;
using HousingSearchApi.V1.Boundary.Requests;
using HousingSearchApi.V1.Interfaces;
using HousingSearchApi.V1.Interfaces.Sorting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Nest;
using Xunit;

namespace HousingSearchApi.Tests.V1.Helper
{
    public class SearchPersonElasticSearchHelperTests
    {
        private readonly SearchPersonElasticSearchHelper _classUnderTest;
        private readonly ServiceCollection _services;

        public SearchPersonElasticSearchHelperTests()
        {
            _services = new ServiceCollection();

            IConfigurationRoot configuration = new ConfigurationBuilder()
                .AddEnvironmentVariables()
                .AddJsonFile("appsettings.json")
                .Build();
            var startup = new Startup(configuration);
            startup.ConfigureServices(_services);
            var serviceProvider = _services.BuildServiceProvider();
            _classUnderTest = new SearchPersonElasticSearchHelper(serviceProvider.GetService<IElasticClient>(),
                serviceProvider.GetService<ISearchPersonsQueryContainerOrchestrator>(),
                serviceProvider.GetService<IPagingHelper>(),
                serviceProvider.GetService<IPersonListSortFactory>(),
                serviceProvider.GetService<ILogger<SearchPersonElasticSearchHelper>>());
        }

        [Fact(Skip = "Actually an intgration test as requires a real Elastic search instance running.")]
        // In our case, the query should be a SHOULD (the ElasticSearch option for OR), followed by wildcards for :
        // firstname, surname, middlename,prefferedfirstname, preferredsurname, dateofbirth
        public async Task WhenCallingElasticSearchHelperShouldGenerateTheRightQuery()
        {
            // arrange
            var searchText = "abc";
            // correctQuery is the query NEST generates behind the scenes to, in turn, send to the ES server.
            // In our case, it wildcards the 6 fields by which we are searching.
            var correctQuery =
                "{\"should\":[{\"wildcard\":{\"firstname\":{\"value\":\"*{0}*\"}}},{\"wildcard\":{\"surname\":{\"value\":\"*{0}*\"}}}]}";
            correctQuery = correctQuery.Replace("{0}", searchText, StringComparison.CurrentCulture);

            // act
            var response = await _classUnderTest.Search(new GetPersonListRequest { SearchText = searchText }).ConfigureAwait(false);

            // assert
            response.DebugInformation.IndexOf(correctQuery, StringComparison.CurrentCulture).Should().BeGreaterOrEqualTo(0);
        }
    }
}