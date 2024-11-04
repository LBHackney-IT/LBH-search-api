using Nest;
using System;
using System.Collections.Generic;
using System.Linq;


namespace HousingSearchApi.V2.Gateways;

static class SearchOperations
{
    public static Func<QueryContainerDescriptor<object>, QueryContainer>
    NestedMultiMatch(string searchText, string path, Fields fields, TextQueryType matchType = TextQueryType.BestFields, Fuzziness fuzziness = null, int boost = 1)
    {
        return should => should
            .Nested(n => n
                .Path(path)
                .Query(qq => qq
                    .MultiMatch(mm =>
                        {
                            mm.Type(matchType)
                            .Query(searchText)
                            .Fields(fields)
                            .Boost(boost);
                            if (fuzziness != null)
                                mm.Fuzziness(fuzziness);
                            return mm;
                        }
                    )
                )
            );
    }

    // Score for matching a single (best) field
    public static Func<QueryContainerDescriptor<object>, QueryContainer>
        MultiMatchBestFields(string searchText, Fields fields = null, int boost = 1) =>
        should => should
            .MultiMatch(mm => mm
                .Fields(fields ?? new[] { "*" })
                .Query(searchText)
                .Type(TextQueryType.BestFields)
                .Operator(Operator.And)
                .Fuzziness(Fuzziness.Auto)
                .Boost(boost)
            );

    // Score for matching the combination of many fields
    public static Func<QueryContainerDescriptor<object>, QueryContainer>
        MultiMatchCrossFields(string searchText, Fields fields = null, int boost = 1) =>
        should => should
            .MultiMatch(mm => mm
                .Fields(fields ?? new[] { "*" })
                .Query(searchText)
                .Type(TextQueryType.CrossFields)
                .Operator(Operator.Or)
                .Boost(boost)
            );

    // Score for matching a high number (quantity) of fields
    public static Func<QueryContainerDescriptor<object>, QueryContainer>
        MultiMatchMostFields(string searchText, int boost, Fields fields = null) =>
        should => should
            .MultiMatch(mm => mm
                .Fields(fields ?? new[] { "*" })
                .Query(searchText)
                .Type(TextQueryType.MostFields)
                .Operator(Operator.Or)
                .Fuzziness(Fuzziness.Auto)
                .Boost(boost)
            );


    // Score for matching a value which contains the search text
    public static Func<QueryContainerDescriptor<object>, QueryContainer>
        WildcardMatch(string searchText, Fields fieldNames, int boost)
    {
        List<string> ProcessWildcards(string phrase)
        {
            if (string.IsNullOrEmpty(phrase))
                return new List<string>();
            return phrase.Split(' ').Select(word => $"*{word}*").ToList();
        }

        var listOfWildcardedWords = ProcessWildcards(searchText);
        var wildcardQueries = fieldNames.SelectMany(fieldName => 
            listOfWildcardedWords.Select(term =>
                new WildcardQuery
                    {
                    Field = fieldName,
                    Value = term,
                    Boost = boost
                    }
                )
        ).ToList();

        return q => q.Bool(b => b
            .Should(wildcardQueries.Select(wq =>
                new QueryContainerDescriptor<object>()
                    .Wildcard(w => w
                        .Field(wq.Field)
                        .Value(wq.Value)
                        .Boost(wq.Boost)
                    )
                ).ToArray()
            )
        );
    }
}
