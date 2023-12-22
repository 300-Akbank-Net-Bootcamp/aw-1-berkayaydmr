using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace VbApi.Controllers;

public class Employee : IValidatableObject
{
    [Required]
    [StringLength(maximumLength: 250, MinimumLength = 10, ErrorMessage = "Invalid Name")]
    public string Name { get; set; }

    [Required] 
    public DateTime DateOfBirth { get; set; }

    [EmailAddress(ErrorMessage = "Email address is not valid.")]
    public string Email { get; set; }

    [Phone(ErrorMessage = "Phone is not valid.")]
    public string Phone { get; set; }

    [Range(minimum: 50, maximum: 400, ErrorMessage = "Hourly salary does not fall within allowed range.")]
    [MinLegalSalaryRequired(minJuniorSalary: 50, minSeniorSalary: 200)]
    public double HourlySalary { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        var minAllowedBirthDate = DateTime.Today.AddYears(-65);
        if (minAllowedBirthDate > DateOfBirth)
        {
            yield return new ValidationResult("Birthdate is not valid.");
        }
    }
}

public class EmployeeValidator : AbstractValidator<Employee>
{
    public EmployeeValidator()
    {
        RuleFor(e => e.Name).NotEmpty().Length(10, 250).WithMessage("Invalid Name");
        RuleFor(e => e.DateOfBirth).NotEmpty().WithMessage("Invalid Date of Birth");
        RuleFor(e => e.Email).EmailAddress().WithMessage("Email address is not valid.");
        RuleFor(e => e.Phone).NotEmpty().Must(new Regex(@"^\d{10}$").IsMatch).WithMessage("Phone is not valid.");
        RuleFor(e => e.HourlySalary).InclusiveBetween(50,400)
            .Must((e, hourlySalary) => MinLegalSalaryRequired(e, 50, 200))
            .WithMessage("Hourly salary does not fall within allowed range.");
    }

    private bool MinLegalSalaryRequired(Employee employee, double minSeniorSalary, double minJuniorSalary)
    {

        var dateBeforeThirtyYears = DateTime.Today.AddYears(-30);
        var isOlderThanThirdyYears = employee.DateOfBirth <= dateBeforeThirtyYears;

        var isValidSalary = isOlderThanThirdyYears ? employee.HourlySalary >= minSeniorSalary : employee.HourlySalary >= minJuniorSalary;

        return isValidSalary;
    }
}

public class MinLegalSalaryRequiredAttribute : ValidationAttribute
{
    public MinLegalSalaryRequiredAttribute(double minJuniorSalary, double minSeniorSalary)
    {
        MinJuniorSalary = minJuniorSalary;
        MinSeniorSalary = minSeniorSalary;
    }

    public double MinJuniorSalary { get; }
    public double MinSeniorSalary { get; }
    public string GetErrorMessage() => $"Minimum hourly salary is not valid.";

    protected override ValidationResult? IsValid(object value, ValidationContext validationContext)
    {
        var employee = (Employee)validationContext.ObjectInstance;
        var dateBeforeThirtyYears = DateTime.Today.AddYears(-30);
        var isOlderThanThirdyYears = employee.DateOfBirth <= dateBeforeThirtyYears;
        var hourlySalary = (double)value;

        var isValidSalary = isOlderThanThirdyYears ? hourlySalary >= MinSeniorSalary : hourlySalary >= MinJuniorSalary;

        return isValidSalary ? ValidationResult.Success : new ValidationResult(GetErrorMessage());
    }
}



[Route("api/[controller]")]
[ApiController]
public class EmployeeController : ControllerBase
{
    public EmployeeController()
    {
    }

    [HttpPost]
    public Employee Post([FromBody] Employee value)
    {
        var validator = new EmployeeValidator();
        var result = validator.Validate(value);
        if (!result.IsValid)
        {
            throw new Exception(result.Errors.ToString());
        }
        
        return value;
    }
}