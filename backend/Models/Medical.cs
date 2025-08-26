using System.ComponentModel.DataAnnotations;

namespace VoxMedApi.Models;

public class Patient
{
    public Guid Id { get; set; } = Guid.NewGuid();
    
    [Required]
    [MaxLength(50)]
    public string PatientNumber { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(100)]
    public string FirstName { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(100)]
    public string LastName { get; set; } = string.Empty;
    
    [Required]
    public DateTime DateOfBirth { get; set; }
    
    [MaxLength(10)]
    public string? Gender { get; set; }
    
    [Phone]
    [MaxLength(20)]
    public string? Phone { get; set; }
    
    [EmailAddress]
    [MaxLength(255)]
    public string? Email { get; set; }
    
    public string? Address { get; set; } // JSON string
    public string? EmergencyContact { get; set; } // JSON string
    public string? MedicalHistory { get; set; } // JSON string
    public string[]? Allergies { get; set; }
    public string? CurrentMedications { get; set; } // JSON string
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    public Guid? CreatedBy { get; set; }

    // Navigation properties
    public virtual User? CreatedByUser { get; set; }
    public virtual ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
    public virtual ICollection<MedicalRecord> MedicalRecords { get; set; } = new List<MedicalRecord>();
    
    // Computed properties
    public string FullName => $"{FirstName} {LastName}";
    public int Age => DateTime.Today.Year - DateOfBirth.Year - (DateTime.Today.DayOfYear < DateOfBirth.DayOfYear ? 1 : 0);
}

public class Appointment
{
    public Guid Id { get; set; } = Guid.NewGuid();
    
    [Required]
    public Guid PatientId { get; set; }
    
    [Required]
    public Guid DoctorId { get; set; }
    
    [Required]
    public DateTime AppointmentDate { get; set; }
    
    public int DurationMinutes { get; set; } = 30;
    
    [MaxLength(20)]
    public string Status { get; set; } = "scheduled";
    
    [MaxLength(50)]
    public string? Type { get; set; }
    
    public string? Notes { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public virtual Patient Patient { get; set; } = null!;
    public virtual User Doctor { get; set; } = null!;
}

public class MedicalRecord
{
    public Guid Id { get; set; } = Guid.NewGuid();
    
    [Required]
    public Guid PatientId { get; set; }
    
    public Guid? AppointmentId { get; set; }
    
    [Required]
    public Guid DoctorId { get; set; }
    
    [Required]
    [MaxLength(50)]
    public string RecordType { get; set; } = string.Empty;
    
    public string? Diagnosis { get; set; } // JSON string
    public string[]? Symptoms { get; set; }
    public string? TreatmentPlan { get; set; } // JSON string
    public string? Medications { get; set; } // JSON string
    public string? Notes { get; set; }
    public string? Attachments { get; set; } // JSON string
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public virtual Patient Patient { get; set; } = null!;
    public virtual User Doctor { get; set; } = null!;
    public virtual Appointment? Appointment { get; set; }
}
