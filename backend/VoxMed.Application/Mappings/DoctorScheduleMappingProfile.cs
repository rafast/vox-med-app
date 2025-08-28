using AutoMapper;
using VoxMed.Application.DTOs.DoctorSchedules;
using VoxMed.Domain.Entities;

namespace VoxMed.Application.Mappings;

public class DoctorScheduleMappingProfile : Profile
{
    public DoctorScheduleMappingProfile()
    {
        CreateMap<DoctorSchedule, DoctorScheduleResponse>()
            .ForMember(dest => dest.TotalSlotsPerDay, opt => opt.Ignore()); // Will be set manually

        CreateMap<CreateDoctorScheduleRequest, DoctorSchedule>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
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
