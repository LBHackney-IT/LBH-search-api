using System;
using System.Collections.Generic;
using Hackney.Core.ElasticSearch.Interfaces;
using Hackney.Shared.HousingSearch.Gateways.Models.Assets;
using HousingSearchApi.V1.Boundary.Requests;
using HousingSearchApi.V1.Interfaces.Factories;
using Nest;

namespace HousingSearchApi.V1.Infrastructure.Factories
{
    public class AssetQueryGenerator : IQueryGenerator<QueryableAsset>
    {
        private readonly IQueryBuilder<QueryableAsset> _queryBuilder;

        public AssetQueryGenerator(IQueryBuilder<QueryableAsset> queryBuilder)
        {
            _queryBuilder = queryBuilder;
        }

        public QueryContainer Create<TRequest>(TRequest request, QueryContainerDescriptor<QueryableAsset> q)
        {
            GetAssetListRequest assetListRequest = request as GetAssetListRequest;
            if (assetListRequest == null)
                throw new ArgumentNullException($"{nameof(request).ToString()} shouldn't be null.");

            var wildcardFields = new List<string> { "assetAddress.postCode", "assetAddress.uprn" };

            if (!assetListRequest.ExactMatch) wildcardFields.Add("assetAddress.addressLine1");

            return _queryBuilder
                .WithWildstarQuery(assetListRequest.SearchText,
                    wildcardFields)
                .WithExactQuery(assetListRequest.SearchText,
                    new List<string>
                    {
                        "assetAddress.addressLine1",
                        "assetAddress.uprn",
                        "assetAddress.postCode"
                    })
                .WithFilterQuery(assetListRequest.AssetTypes, new List<string> { "assetType" })
                .Build(q);
        }
    }
}
