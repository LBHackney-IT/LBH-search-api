using System.Collections.Generic;
using System.Threading.Tasks;
using Nest;
using SearchApi.V1.Domain;

namespace SearchApi.V1.Gateways
{
    public class GetPersonGateway : IGetPersonGateway
    {
        private readonly IElasticClient _esClient;

        public GetPersonGateway(IElasticClient esClient)
        {
            _esClient = esClient;
        }

        public Task<List<Person>> Search(SearchParameters parameters)
        {
            return Task.FromResult(new List<Person>());
        }
    }
}
