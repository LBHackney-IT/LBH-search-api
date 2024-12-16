using System;
using System.Collections.Generic;
using Hackney.Core.ElasticSearch.Interfaces;
using HousingSearchApi.V1.Interfaces;
using Hackney.Shared.HousingSearch.Gateways.Models.Assets;
using HousingSearchApi.V1.Boundary.Requests;
using HousingSearchApi.V1.Interfaces.Factories;
using Nest;

namespace HousingSearchApi.V1.Infrastructure.Factories
{
    public class AssetQueryGenerator : IQueryGenerator<QueryableAsset>
    {
        private readonly IQueryBuilder<QueryableAsset> _queryBuilder;
        private readonly IFilterQueryBuilder<QueryableAsset> _queryFilterBuilder;
        public AssetQueryGenerator(IQueryBuilder<QueryableAsset> queryBuilder,
            IFilterQueryBuilder<QueryableAsset> queryFilterBuilder)
        {
            _queryBuilder = queryBuilder;
            _queryFilterBuilder = queryFilterBuilder;
        }

        public QueryContainer Create<TRequest>(TRequest request, QueryContainerDescriptor<QueryableAsset> q)
        {
            //Pre-default check for relationships query
            if (request.GetType() == typeof(GetAssetRelationshipsRequest))
            {
                var relationshipRequest = request as GetAssetRelationshipsRequest;
                return _queryFilterBuilder
                    .WithExactQuery(relationshipRequest.SearchText, new List<string> { "parentAssetIds" })
                    .Build(q);
            }

            //Default to search endpoint
            GetAssetListRequest assetListRequest = request as GetAssetListRequest;

            if (assetListRequest == null)
                throw new ArgumentNullException($"{nameof(request).ToString()} shouldn't be null.");

            if (assetListRequest.IsSimpleQuery)
            {
                return _queryBuilder.BuildSimpleQuery(q, assetListRequest.SearchText, new List<string> { "assetAddress.addressLine1.textAddress", "assetAddress.postCode" });
            }

            if (request.GetType() == typeof(GetAssetListRequest))
            {
                //This is so assets search endpoint works as before
                return _queryFilterBuilder
                    .WithMultipleFilterQuery(assetListRequest.IsActive, new List<string> { "isActive" })
                    .WithWildstarQuery(assetListRequest.SearchText,
                        new List<string> { "assetAddress.addressLine1", "assetAddress.postCode", "assetAddress.uprn" })
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
            else
            {
                GetAllAssetListRequest assetListAllRequest = request as GetAllAssetListRequest;

                if (assetListRequest.SearchText != null && assetListRequest.SearchText.Length > 0 && assetListAllRequest.IsTemporaryAccomodation == "true")
                {
                    // temp accom search 
                    return _queryFilterBuilder
                        .WithMultipleFilterQuery(assetListAllRequest.NumberOfBedrooms, new List<string>
                            {
                            "assetCharacteristics.numberOfBedrooms"
                            })
                        .WithMultipleFilterQuery(assetListAllRequest.NumberOfBedSpaces, new List<string> { "assetCharacteristics.numberOfBedSpaces" })
                        .WithMultipleFilterQuery(assetListAllRequest.NumberOfCots, new List<string> { "assetCharacteristics.numberOfCots" })
                        .WithMultipleFilterQuery(assetListAllRequest.FloorNo, new List<string> { "assetLocation.floorNo" })
                        .WithMultipleFilterQuery(assetListAllRequest.PrivateBathroom, new List<string> { "assetCharacteristics.hasPrivateBathroom" })
                        .WithMultipleFilterQuery(assetListAllRequest.PrivateKitchen, new List<string> { "assetCharacteristics.hasPrivateKitchen" })
                        .WithMultipleFilterQuery(assetListAllRequest.StepFree, new List<string> { "assetCharacteristics.isStepFree" })
                        .WithMultipleFilterQuery(assetListAllRequest.IsTemporaryAccomodation, new List<string> { "assetManagement.isTemporaryAccomodation" })
                        .WithMultipleFilterQuery(assetListAllRequest.ParentAssetId, new List<string> { "rootAsset" })
                        .WithMultipleFilterQuery(assetListAllRequest.IsActive, new List<string> { "isActive" })
                        .WithWildstarBoolQuery(assetListAllRequest.SearchText,
                            new List<string> { "assetAddress.addressLine1", "assetAddress.postCode", "assetAddress.uprn" })
                        .WithExactQuery(assetListAllRequest.SearchText,
                            new List<string>
                            {
                            "assetAddress.addressLine1",
                            "assetAddress.uprn",
                            "assetAddress.postCode"
                            })
                        .WithFilterQuery(assetListAllRequest.AssetTypes, new List<string> { "assetType" })
                        .WithFilterQuery(assetListAllRequest.AssetStatus, new List<string> { "assetManagement.propertyOccupiedStatus" })
                        .WithFilterQuery(assetListAllRequest.TenureType, new List<string> { "tenure.type.keyword" })

                        .Build(q);
                }
                if (assetListRequest.SearchText != null && assetListRequest.SearchText.Length > 0)
                {
                    //For when we need to use searchText and filters together                    
                    return _queryFilterBuilder
                        .WithMultipleFilterQuery(assetListAllRequest.NumberOfBedrooms, new List<string>
                            {
                            "assetCharacteristics.numberOfBedrooms"
                            })
                        .WithMultipleFilterQuery(assetListAllRequest.NumberOfBedSpaces, new List<string> { "assetCharacteristics.numberOfBedSpaces" })
                        .WithMultipleFilterQuery(assetListAllRequest.NumberOfCots, new List<string> { "assetCharacteristics.numberOfCots" })
                        .WithMultipleFilterQuery(assetListAllRequest.FloorNo, new List<string> { "assetLocation.floorNo" })
                        .WithMultipleFilterQuery(assetListAllRequest.PrivateBathroom, new List<string> { "assetCharacteristics.hasPrivateBathroom" })
                        .WithMultipleFilterQuery(assetListAllRequest.PrivateKitchen, new List<string> { "assetCharacteristics.hasPrivateKitchen" })
                        .WithMultipleFilterQuery(assetListAllRequest.StepFree, new List<string> { "assetCharacteristics.isStepFree" })
                        .WithMultipleFilterQuery(assetListAllRequest.IsTemporaryAccomodation, new List<string> { "assetManagement.isTemporaryAccomodation" })
                        .WithMultipleFilterQuery(assetListAllRequest.ParentAssetId, new List<string> { "rootAsset" })
                        .WithMultipleFilterQuery(assetListAllRequest.IsActive, new List<string> { "isActive" })
                        .WithMultipleFilterQuery(assetListAllRequest.ContractApprovalStatus, new List<string> { "assetContracts.approvalStatus" })
                        .WithMultipleFilterQuery(assetListAllRequest.ContractIsActive, new List<string> { "assetContracts.isActive" })
                        .WithMultipleFilterQuery(assetListAllRequest.ContractEndReason, new List<string> { "assetContracts.endReason" })
                        .WithMultipleFilterQuery(assetListAllRequest.ContractApprovalStatusReason, new List<string> { "assetContracts.approvalStatusReason" })
                        .WithMultipleFilterQuery(assetListAllRequest.ChargesSubType, new List<string> { "assetContracts.charges.subType" })
                        .WithWildstarQuery(assetListAllRequest.SearchText,
                            new List<string> { "assetAddress.addressLine1", "assetAddress.postCode", "assetAddress.uprn" })
                        .WithExactQuery(assetListAllRequest.SearchText,
                            new List<string>
                            {
                            "assetAddress.addressLine1",
                            "assetAddress.uprn",
                            "assetAddress.postCode"
                            })
                        .WithFilterQuery(assetListAllRequest.AssetTypes, new List<string> { "assetType" })
                        .WithFilterQuery(assetListAllRequest.AssetStatus, new List<string> { "assetManagement.propertyOccupiedStatus" })
                        .WithFilterQuery(assetListAllRequest.TenureType, new List<string> { "tenure.type.keyword" })
                        .Build(q);
                }
                else
                {
                    //Only the all enpoint should exclude need for SearchText                    
                    return _queryFilterBuilder
                        .WithMultipleFilterQuery(assetListAllRequest.NumberOfBedrooms, new List<string>
                            {
                            "assetCharacteristics.numberOfBedrooms"
                            })
                        .WithMultipleFilterQuery(assetListAllRequest.NumberOfBedSpaces, new List<string> { "assetCharacteristics.numberOfBedSpaces" })
                        .WithMultipleFilterQuery(assetListAllRequest.NumberOfCots, new List<string> { "assetCharacteristics.numberOfCots" })
                        .WithMultipleFilterQuery(assetListAllRequest.FloorNo, new List<string> { "assetLocation.floorNo" })
                        .WithMultipleFilterQuery(assetListAllRequest.PrivateBathroom, new List<string> { "assetCharacteristics.hasPrivateBathroom" })
                        .WithMultipleFilterQuery(assetListAllRequest.PrivateKitchen, new List<string> { "assetCharacteristics.hasPrivateKitchen" })
                        .WithMultipleFilterQuery(assetListAllRequest.StepFree, new List<string> { "assetCharacteristics.isStepFree" })
                        .WithMultipleFilterQuery(assetListAllRequest.IsTemporaryAccomodation, new List<string> { "assetManagement.isTemporaryAccomodation" })
                        .WithMultipleFilterQuery(assetListAllRequest.ParentAssetId, new List<string> { "rootAsset" })
                        .WithMultipleFilterQuery(assetListAllRequest.IsActive, new List<string> { "isActive" })
                        .WithMultipleFilterQuery(assetListAllRequest.ContractApprovalStatus, new List<string> { "assetContracts.approvalStatus" })
                        .WithMultipleFilterQuery(assetListAllRequest.ContractApprovalStatusReason, new List<string> { "assetContracts.approvalStatusReason" })
                        .WithMultipleFilterQuery(assetListAllRequest.ContractIsActive, new List<string> { "assetContracts.isActive" })
                        .WithMultipleFilterQuery(assetListAllRequest.ContractEndReason, new List<string> { "assetContracts.endReason" })
                        .WithMultipleFilterQuery(assetListAllRequest.ChargesSubType, new List<string> { "assetContracts.charges.subType" })
                        .WithFilterQuery(assetListAllRequest.AssetTypes, new List<string> { "assetType" })
                        .WithFilterQuery(assetListAllRequest.AssetStatus, new List<string> { "assetManagement.propertyOccupiedStatus" })
                        .WithFilterQuery(assetListAllRequest.TenureType, new List<string> { "tenure.type.keyword" })
                        .Build(q);
                }
            }
        }
    }
}
