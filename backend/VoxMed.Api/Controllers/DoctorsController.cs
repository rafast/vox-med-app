using Microsoft.AspNetCore.Mvc;
using VoxMed.Auth.Models;
using Microsoft.AspNetCore.Identity;
using System.Linq;

namespace VoxMed.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DoctorsController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;

    public DoctorsController(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    [HttpGet]
    public ActionResult GetAvailableDoctors()
    {
        // Get all users with UserType.Doctor and IsActive
        var doctors = _userManager.Users
            .Where(u => u.UserType == UserType.Doctor && u.IsActive)
            .Select(u => new {
                Id = u.Id,
                Name = u.FullName,
                Specialization = u.Specialization,
                Department = u.Department,
                MedicalLicenseNumber = u.MedicalLicenseNumber
            })
            .ToList();

        return Ok(doctors);
    }
}
