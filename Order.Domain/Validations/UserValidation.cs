using FluentValidation;
using Order.Domain.Models;

namespace Order.Domain.Validations
{
    public class UserValidation : AbstractValidator<UserModel>
    {
        public UserValidation()
        {
            ValidatorOptions.Global.CascadeMode = CascadeMode.Stop;

            RuleFor(x => x.Email)
                .NotNull()
                .NotEmpty()
                .Length(3, 70);

            RuleFor(x => x.PasswordHash)
                .NotNull()
                .NotEmpty()
                .MinimumLength(6);
        }
    }
}
