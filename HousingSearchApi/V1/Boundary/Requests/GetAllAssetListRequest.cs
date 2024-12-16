using Microsoft.AspNetCore.Mvc;

namespace HousingSearchApi.V1.Boundary.Requests
{
    public class GetAllAssetListRequest : GetAssetListRequest
    {
        [FromQuery(Name = "lastHitId")]
        public string LastHitId { get; set; }

        [FromQuery(Name = "assetStatus")]
        public string AssetStatus { get; set; }

        [FromQuery(Name = "numberOfBedSpaces")]
        public string NumberOfBedSpaces { get; set; }

        [FromQuery(Name = "numberOfCots")]
        public string NumberOfCots { get; set; }

        [FromQuery(Name = "floorNo")]
        public string FloorNo { get; set; }

        [FromQuery(Name = "privateBathroom")]
        public string PrivateBathroom { get; set; }

        [FromQuery(Name = "privateKitchen")]
        public string PrivateKitchen { get; set; }

        [FromQuery(Name = "stepFree")]
        public string StepFree { get; set; }

        [FromQuery(Name = "contractApprovalStatus")]
        public string ContractApprovalStatus { get; set; }

        [FromQuery(Name = "contractApprovalStatusReason")]
        public string ContractApprovalStatusReason { get; set; }

        [FromQuery(Name = "contractIsActive")]
        public string ContractIsActive { get; set; }

        [FromQuery(Name = "contractEndReason")]
        public string ContractEndReason { get; set; }

        [FromQuery(Name = "chargesSubType")]
        public string ChargesSubType { get; set; }
        [FromQuery(Name = "isTemporaryAccomodation")]
        public string IsTemporaryAccomodation { get; set; }

        [FromQuery(Name = "parentAssetId")]
        public string ParentAssetId { get; set; }

        [FromQuery(Name = "tenureType")]
        public string TenureType { get; set; }
    }
}
