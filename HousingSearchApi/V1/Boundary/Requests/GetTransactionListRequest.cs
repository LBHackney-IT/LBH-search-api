using Microsoft.AspNetCore.Mvc;

namespace HousingSearchApi.V1.Boundary.Requests
{
    public class GetTransactionSearchRequest
    {
        private const int DefaultPageSize = 12;

        /// <summary>
        /// Some search phrase. Can be empty to return all transactions
        /// </summary>
        /// <example>HSGSUN</example>
        [FromQuery(Name = "searchText")]
        public string SearchText { get; set; }

        /// <summary>
        /// Page size. Default value is 12
        /// </summary>
        /// <example>10</example>
        [FromQuery(Name = "pageSize")]
        public int PageSize { get; set; } = DefaultPageSize;

        /// <summary>
        /// Page number for pagination
        /// </summary>
        /// <example>1</example>
        [FromQuery(Name = "page")]
        public int Page { get; set; }
    }
}