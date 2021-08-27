using System.Collections.Generic;

namespace HousingSearchApi.V1.Infrastructure
{
    public class WildCardAppenderAndPrepender : IWildCardAppenderAndPrepender
    {
        public List<string> Process(string phrase)
        {
            if (string.IsNullOrEmpty(phrase))
            {
                return new List<string>();
            }
            
            var listOfWildcardWords = new List<string>();

            foreach (var word in phrase.Split(' '))
            {
                listOfWildcardWords.Add($"*{word}*");
            }

            return listOfWildcardWords;
        }
    }
}
