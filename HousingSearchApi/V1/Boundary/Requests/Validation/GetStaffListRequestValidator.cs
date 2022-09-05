using FluentValidation;
using Hackney.Core.Validation;

namespace HousingSearchApi.V1.Boundary.Requests.Validation
{
    public class GetStaffListRequestValidator : AbstractValidator<GetStaffListRequest>
    {
        public GetStaffListRequestValidator()
        {
            RuleFor(x => x.SearchText).NotNull()
                                      .NotEmpty()
                                      .MinimumLength(2)
                                      .NotXssString();
            RuleFor(x => x.PageSize).GreaterThan(0);
            RuleFor(x => x.SortBy).NotXssString();
        }
    }
}
