using HousingSearchApi.V1.Boundary.Requests;
using HousingSearchApi.V1.Boundary.Responses;
using HousingSearchApi.V1.Controllers;
using HousingSearchApi.V1.UseCase.Interfaces;
using Moq;
using System.Threading.Tasks;
using Xunit;

namespace HousingSearchApi.Tests.V1.Controllers
{
    [Collection("LogCall collection")]
    public class GetTenureListControllerTests
    {
        private readonly Mock<IGetTenureListUseCase> _mockGetTenureListUseCase;
        private readonly Mock<IGetTenureListByPrnListUseCase> _mockGetTenureListByPrnListUseCase;
        private readonly GetTenureListController _classUnderTest;


        public GetTenureListControllerTests()
        {
            new LogCallAspectFixture().RunBeforeTests();

            _mockGetTenureListUseCase = new Mock<IGetTenureListUseCase>();
            _mockGetTenureListByPrnListUseCase = new Mock<IGetTenureListByPrnListUseCase>();
            _classUnderTest = new GetTenureListController(_mockGetTenureListUseCase.Object, _mockGetTenureListByPrnListUseCase.Object);
        }

        [Fact]
        public async Task GetTenureListShouldCallGetTenureListUseCase()
        {
            // given
            var request = new GetTenureListRequest();
            var response = new GetTenureListResponse();
            _mockGetTenureListUseCase.Setup(x => x.ExecuteAsync(request)).ReturnsAsync(response);

            // when
            await _classUnderTest.GetTenureList(request).ConfigureAwait(false);

            // then
            _mockGetTenureListUseCase.Verify(x => x.ExecuteAsync(request), Times.Once);
        }
    }
}
