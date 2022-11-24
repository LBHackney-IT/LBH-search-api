using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace HousingSearchApi.V1.Boundary.Requests
{
    public class GetTenureListByPrnListRequest : HousingSearchRequest
    {
        [FromQuery(Name = "prnList")]
        [Required, MinLength(1)]
        public List<string> PrnList { get; set; }
    }
}
