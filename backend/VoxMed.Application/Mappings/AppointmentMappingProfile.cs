using AutoMapper;
using VoxMed.Application.DTOs.Appointments;
using VoxMed.Domain.Entities;

namespace VoxMed.Application.Mappings;

public class AppointmentMappingProfile : Profile
{
    public AppointmentMappingProfile()
    {
        CreateMap<Appointment, AppointmentResponse>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.PatientId, opt => opt.MapFrom(src => src.PatientId))
            .ForMember(dest => dest.DoctorId, opt => opt.MapFrom(src => src.DoctorId))
            .ForMember(dest => dest.ScheduleId, opt => opt.MapFrom(src => src.ScheduleId))
            .ForMember(dest => dest.AppointmentDateTime, opt => opt.MapFrom(src => src.AppointmentDateTime))
            .ForMember(dest => dest.DurationMinutes, opt => opt.MapFrom(src => src.DurationMinutes))
            .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status))
            .ForMember(dest => dest.Reason, opt => opt.MapFrom(src => src.Reason))
            .ForMember(dest => dest.Notes, opt => opt.MapFrom(src => src.Notes))
            .ForMember(dest => dest.Symptoms, opt => opt.MapFrom(src => src.Symptoms))
            .ForMember(dest => dest.Diagnosis, opt => opt.MapFrom(src => src.Diagnosis))
            .ForMember(dest => dest.Treatment, opt => opt.MapFrom(src => src.Treatment))
            .ForMember(dest => dest.IsOnline, opt => opt.MapFrom(src => src.IsOnline))
            .ForMember(dest => dest.Location, opt => opt.MapFrom(src => src.Location))
            .ForMember(dest => dest.ConfirmedAt, opt => opt.MapFrom(src => src.ConfirmedAt))
            .ForMember(dest => dest.CancelledAt, opt => opt.MapFrom(src => src.CancelledAt))
            .ForMember(dest => dest.CancellationReason, opt => opt.MapFrom(src => src.CancellationReason))
            .ForMember(dest => dest.CompletedAt, opt => opt.MapFrom(src => src.CompletedAt))
            .ForMember(dest => dest.DateAdded, opt => opt.MapFrom(src => src.DateAdded))
            .ForMember(dest => dest.DateUpdated, opt => opt.MapFrom(src => src.DateUpdated));

        CreateMap<CreateAppointmentRequest, Appointment>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.Status, opt => opt.Ignore()) // Will be set to default Scheduled
            .ForMember(dest => dest.ConfirmedAt, opt => opt.Ignore())
            .ForMember(dest => dest.CancelledAt, opt => opt.Ignore())
            .ForMember(dest => dest.CancellationReason, opt => opt.Ignore())
            .ForMember(dest => dest.CompletedAt, opt => opt.Ignore())
            .ForMember(dest => dest.DateAdded, opt => opt.Ignore())
            .ForMember(dest => dest.DateUpdated, opt => opt.Ignore())
            .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
            .ForMember(dest => dest.DateDeleted, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.Version, opt => opt.Ignore())
            .ForMember(dest => dest.DomainEvents, opt => opt.Ignore());
    }
}
