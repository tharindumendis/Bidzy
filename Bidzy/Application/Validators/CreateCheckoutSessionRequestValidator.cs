using FluentValidation;
using Bidzy.API.DTOs.payment;

namespace Bidzy.Application.Validators
{
    public class CreateCheckoutSessionRequestValidator : AbstractValidator<CreateCheckoutSessionRequest>
    {
        public CreateCheckoutSessionRequestValidator()
        {
            RuleFor(x => x.AuctionId)
                .NotEmpty().WithMessage("AuctionId cannot be empty.");

            RuleFor(x => x.SuccessUrl)
                .NotEmpty().WithMessage("SuccessUrl cannot be empty.")
                .Must(BeAValidUrl).WithMessage("SuccessUrl must be a valid HTTP or HTTPS URL.");

            RuleFor(x => x.CancelUrl)
                .NotEmpty().WithMessage("CancelUrl cannot be empty.")
                .Must(BeAValidUrl).WithMessage("CancelUrl must be a valid HTTP or HTTPS URL.");
        }

        private bool BeAValidUrl(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                return false;
            }

            return Uri.TryCreate(url, UriKind.Absolute, out Uri? uriResult) &&
                   (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
        }
    }
}