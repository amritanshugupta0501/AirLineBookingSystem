using FluentValidation;
using System;
using PassengerModel = Passenger.API.Models.Passenger;

namespace Passenger.API.Validators
{
    public class PassengerValidator : AbstractValidator<PassengerModel>
    {
        public PassengerValidator()
        {
            RuleFor(p => p.FirstName)
                .NotEmpty().WithMessage("First name is required.")
                .MaximumLength(100);

            RuleFor(p => p.LastName)
                .NotEmpty().WithMessage("Last name is required.")
                .MaximumLength(100);

            RuleFor(p => p.Email)
                .NotEmpty().WithMessage("Email is required.")
                .EmailAddress().WithMessage("A valid email address is required.");

            RuleFor(p => p.Phone)
                .NotEmpty().WithMessage("Phone number is required.")
                .Matches(@"^\+?[1-9]\d{6,14}$").WithMessage("Phone must be a valid international number (e.g. +911234567890).");

            RuleFor(p => p.PassportNumber)
                .NotEmpty().WithMessage("Passport number is required.")
                .MinimumLength(5).WithMessage("Passport number must be at least 5 characters.");

            RuleFor(p => p.Nationality)
                .NotEmpty().WithMessage("Nationality is required.");

            // Age rule: must be at least 2 years old
            RuleFor(p => p.DateOfBirth)
                .NotEmpty().WithMessage("Date of birth is required.")
                .Must(dob => dob <= DateTime.UtcNow.AddYears(-2))
                .WithMessage("Passenger must be at least 2 years old.");

            // Passport expiry: must not be expired
            RuleFor(p => p.PassportExpiryDate)
                .NotEmpty().WithMessage("Passport expiry date is required.")
                .Must(expiry => expiry > DateTime.UtcNow)
                .WithMessage("Passport has expired. Please provide a valid passport.");
        }
    }
}
