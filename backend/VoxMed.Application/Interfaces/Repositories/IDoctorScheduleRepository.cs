using VoxMed.Domain.Entities;

namespace VoxMed.Application.Interfaces.Repositories;

public interface IDoctorScheduleRepository
{
    Task<DoctorSchedule?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<List<DoctorSchedule>> GetByDoctorIdAsync(Guid doctorId, CancellationToken cancellationToken = default);
    Task<List<DoctorSchedule>> GetByDoctorIdAndDateRangeAsync(
        Guid doctorId, 
        DateOnly startDate, 
        DateOnly endDate, 
        CancellationToken cancellationToken = default);
    Task<List<DoctorSchedule>> GetActiveDoctorSchedulesAsync(Guid doctorId, CancellationToken cancellationToken = default);
    Task<DoctorSchedule?> GetDoctorScheduleForDayAsync(
        Guid doctorId, 
        DayOfWeek dayOfWeek, 
        DateOnly date, 
        CancellationToken cancellationToken = default);
    Task<bool> HasConflictingScheduleAsync(
        Guid doctorId, 
        DayOfWeek dayOfWeek, 
        TimeOnly startTime, 
        TimeOnly endTime, 
        Guid? excludeScheduleId = null, 
        CancellationToken cancellationToken = default);
    Task AddAsync(DoctorSchedule doctorSchedule, CancellationToken cancellationToken = default);
    Task UpdateAsync(DoctorSchedule doctorSchedule, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<List<DoctorSchedule>> GetPaginatedAsync(
        int page, 
        int pageSize, 
        Guid? doctorId = null, 
        bool? isActive = null, 
        CancellationToken cancellationToken = default);
    Task<int> GetCountAsync(Guid? doctorId = null, bool? isActive = null, CancellationToken cancellationToken = default);
}
