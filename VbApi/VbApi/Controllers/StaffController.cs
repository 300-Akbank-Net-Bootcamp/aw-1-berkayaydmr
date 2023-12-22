using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Text.RegularExpressions;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace VbApi.Controllers;

public class Staff
{
    [Required]
    [StringLength(maximumLength: 250, MinimumLength = 10)]
    public string? Name { get; set; }

    [EmailAddress(ErrorMessage = "Email address is not valid.")]
    public string? Email { get; set; }

    [Phone(ErrorMessage = "Phone is not valid.")]
    public string? Phone { get; set; }

    [Range(minimum: 30, maximum: 400, ErrorMessage = "Hourly salary does not fall within allowed range.")]
    public decimal? HourlySalary { get; set; }
}

class StaffValidator : AbstractValidator<Staff>
{
    public StaffValidator()
    {
        RuleFor(s => s.Name).NotEmpty().Length(10,250).WithMessage("Invalid Name");
        RuleFor(s => s.Email).EmailAddress().WithMessage("Email address is not valid.");
        RuleFor(s => s.Phone).NotEmpty().Must(new Regex(@"^\d{10}$").IsMatch).WithMessage("Phone is not valid.");
        RuleFor(s => s.HourlySalary).InclusiveBetween(30, 400)
            .WithMessage("Hourly salary does not fall within allowed range.");
    }
} 

[Route("api/[controller]")]
[ApiController]
public class StaffController : ControllerBase
{
    public StaffController()
    {
    }

    [HttpPost]
    public Staff Post([FromBody] Staff value)
    {
        var validator = new StaffValidator();
        var result = validator.Validate(value);
        if (!result.IsValid)
        {
            throw new Exception(result.Errors.ToString());
        }
        
        return value;
    }
}