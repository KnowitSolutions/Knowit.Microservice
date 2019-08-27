using FluentValidation;
using ProjectName;

namespace Service
{
    public class EchoRequestValidator : AbstractValidator<EchoRequest>
    {
        public EchoRequestValidator()
        {
            RuleFor(message => message.Message).NotEmpty();
        }
    }
}