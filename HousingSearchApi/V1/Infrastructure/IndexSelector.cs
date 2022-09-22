using System;
using System.Collections.Generic;
using Hackney.Shared.HousingSearch.Gateways.Models.Accounts;
using Hackney.Shared.HousingSearch.Gateways.Models.Assets;
using Hackney.Shared.HousingSearch.Gateways.Models.Tenures;
using Hackney.Shared.HousingSearch.Gateways.Models.Transactions;
using Hackney.Shared.HousingSearch.Gateways.Models.Persons;
using HousingSearchApi.V1.Interfaces;
using Nest;
using QueryableTenure = Hackney.Shared.HousingSearch.Gateways.Models.Tenures.QueryableTenure;
using QueryablePerson = Hackney.Shared.HousingSearch.Gateways.Models.Persons.QueryablePerson;
using Hackney.Shared.HousingSearch.Gateways.Models.Staffs;
using Hackney.Shared.HousingSearch.Gateways.Models.Processes;

namespace HousingSearchApi.V1.Infrastructure
{
    public class IndexSelector : IIndexSelector
    {
        public Indices.ManyIndices Create<T>()
        {
            var type = typeof(T);

            if (type == typeof(QueryablePerson))
                return Indices.Index(new List<IndexName> { "persons" });

            if (type == typeof(QueryableTenure))
                return Indices.Index(new List<IndexName> { "tenures" });

            if (type == typeof(QueryableAsset))
                return Indices.Index(new List<IndexName> { "assets" });

            if (type == typeof(QueryableAccount))
                return Indices.Index(new List<IndexName> { "accounts" });

            if (type == typeof(QueryableTransaction))
                return Indices.Index(new List<IndexName> { "transactions" });

            if (type == typeof(QueryableStaff))
                return Indices.Index(new List<IndexName> { "staff" });

            if (type == typeof(QueryableProcess))
                return Indices.Index(new List<IndexName> { "processes" });

            throw new NotImplementedException($"No index for type {typeof(T)}");
        }
    }
}
