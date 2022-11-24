using Hackney.Core.ElasticSearch.Interfaces;
using Hackney.Shared.HousingSearch.Gateways.Models.Tenures;
using HousingSearchApi.V1.Boundary.Requests;
using HousingSearchApi.V1.Interfaces.Factories;
using Nest;
using System;
using System.Collections.Generic;

namespace HousingSearchApi.V1.Infrastructure.Factories
{
    public class TenureQueryGenerator : IQueryGenerator<QueryableTenure>
    {
        private readonly IQueryBuilder<QueryableTenure> _queryBuilder;

        public TenureQueryGenerator(IQueryBuilder<QueryableTenure> queryBuilder)
        {
            _queryBuilder = queryBuilder;
        }

        public QueryContainer Create<TRequest>(TRequest request, QueryContainerDescriptor<QueryableTenure> q)
        {
            switch (request)
            {
                case GetTenureListRequest tenureListRequest:
                    if (string.IsNullOrWhiteSpace(tenureListRequest.SearchText))
                    {
                        return null;
                    }

                    return _queryBuilder
                   .WithWildstarQuery(tenureListRequest.SearchText, new List<string>
                   {
                            "paymentReference",
                            "tenuredAsset.fullAddress^3",
                            "householdMembers",
                            "householdMembers.fullName^3"
                   }).Build(q);

                case GetTenureListByPrnListRequest tenureListRequestByPrnList:
                    return q.Terms(c => c
                        .Name("named_query")
                        .Field("paymentReference")
                        .Boost(1.1)
                        .Terms(tenureListRequestByPrnList.PrnList));

                default:
                    throw new ArgumentNullException(nameof(request));
            }
        }
    }
}
