using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;
using HousingSearchApi.Tests.V2.E2ETests.Fixtures;
using Xunit.Abstractions;


namespace HousingSearchApi.Tests.V2.E2ETests;

[Collection("V2.E2ETests Collection")]
public class PersonSearchTests : BaseSearchTests
{
    private readonly HttpClient _httpClient;

    public PersonSearchTests(CombinedFixture combinedFixture, ITestOutputHelper testOutputHelper) : base(combinedFixture, indexName: "persons")
    {
        _httpClient = combinedFixture.Factory.CreateClient();
    }


    #region General

    [Fact]
    public async Task SearchNoMatch()
    {
        // Arrange
        var request = CreateSearchRequest("XXXXXXXX");

        // Act
        var response = await _httpClient.SendAsync(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var root = GetResponseRootElement(response);
        root.GetProperty("total").GetInt32().Should().Be(0);
        root.GetProperty("results").GetProperty("persons").GetArrayLength().Should().Be(0);
    }

    # endregion

    # region Address

    [Fact]
    public async Task SearchAddress_Full()
    {
        const int attempts = 10;
        const int minSuccessCount = 9;

        var successCount = await RunWithScore(attempts, async () =>
        {
            // Arrange
            var randomPerson = RandomItem();
            var expectedReturnedId = randomPerson.GetProperty("id").GetString();
            var query = randomPerson.GetProperty("tenures")[0].GetProperty("assetFullAddress").GetString();
            var request = CreateSearchRequest(query);

            // Act
            var response = await _httpClient.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var root = GetResponseRootElement(response);

            root.GetProperty("total").GetInt32().Should().BeGreaterThan(0);
            var firstResult = root.GetProperty("results").GetProperty("persons")[0];
            firstResult.GetProperty("tenures")[0].GetProperty("assetFullAddress").GetString().Should().Be(query);
        });

        successCount.Should().BeGreaterThanOrEqualTo(minSuccessCount);
    }

    # endregion

    # region Tenure

    [Fact]
    public async Task SearchTenure_PaymentRef()
    {
        // Arrange
        var person = RandomItem();
        var searchText = person.GetProperty("tenures")[0].GetProperty("paymentReference").GetString();
        var request = CreateSearchRequest(searchText);

        // Act
        var response = await _httpClient.SendAsync(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var root = GetResponseRootElement(response);
        root.GetProperty("total").GetInt32().Should().BeGreaterThan(0);
        var firstResult = root.GetProperty("results").GetProperty("persons")[0];
        firstResult.GetProperty("tenures")[0].GetProperty("paymentReference").GetString().Should().Be(searchText);
    }

    # endregion

    # region Person

    [Fact]
    public async Task SearchPerson_Id()
    {
        // Arrange
        var person = RandomItem();
        var expectedReturnedId = person.GetProperty("id").GetString();
        var searchText = person.GetProperty("id").GetString();
        var request = CreateSearchRequest(searchText);

        // Act
        var response = await _httpClient.SendAsync(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var root = GetResponseRootElement(response);
        root.GetProperty("total").GetInt32().Should().BeGreaterThan(0);
        var firstResult = root.GetProperty("results").GetProperty("persons")[0];
        firstResult.GetProperty("id").GetString().Should().Be(expectedReturnedId);
    }

    [Fact]
    public async Task SearchPerson_Name()
    {
        const int attempts = 10;
        const int minSuccessCount = 9;

        var successCount = await RunWithScore(attempts, async () =>
        {
            // Arrange
            var person = RandomItem();
            var expectedReturnedId = person.GetProperty("id").GetString();
            var searchText = person.GetProperty("firstname").GetString() + " " + person.GetProperty("surname").GetString();
            var request = CreateSearchRequest(searchText);

            // Act
            var response = await _httpClient.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var root = GetResponseRootElement(response);
            root.GetProperty("total").GetInt32().Should().BeGreaterThan(0);
            var firstResult = root.GetProperty("results").GetProperty("persons")[0];
            firstResult.GetProperty("id").GetString().Should().Be(expectedReturnedId);
        });

        successCount.Should().BeGreaterThanOrEqualTo(minSuccessCount);
    }

    [Fact]
    public async Task SearchPerson_NamePartRemoved()
    {
        const int attempts = 10;
        const int minSuccessCount = 9;

        var successCount = await RunWithScore(attempts, async () =>
        {
            // Arrange
            var person = RandomItem();
            var expectedReturnedId = person.GetProperty("id").GetString();
            var memberName = person.GetProperty("firstname").GetString() + " " + person.GetProperty("surname").GetString();
            var nameParts = memberName.Split(' ');
            // Remove a random name part (i.e. firstname, "middle name", or surname)
            var randomIndexInNameParts = _random.Next(nameParts.Length);
            nameParts = nameParts.Where((_, index) => index != randomIndexInNameParts).ToArray();
            var searchText = string.Join(" ", nameParts);

            var request = CreateSearchRequest(searchText);

            // Act
            var response = await _httpClient.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var root = GetResponseRootElement(response);
            root.GetProperty("total").GetInt32().Should().BeGreaterThan(0);
            var firstResult = root.GetProperty("results").GetProperty("persons")[0];
            // all name parts should be present in the result
            nameParts.All(part => firstResult.GetProperty("firstname").GetString().Contains(part) || firstResult.GetProperty("surname").GetString().Contains(part)).Should().BeTrue();
        });

        successCount.Should().BeGreaterThanOrEqualTo(minSuccessCount);
    }

    # endregion
}
