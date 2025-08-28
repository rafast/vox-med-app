using Microsoft.EntityFrameworkCore;
using VoxMed.Application.Interfaces.Repositories;
using VoxMed.Domain.Entities;
using VoxMed.Infrastructure.Data;

namespace VoxMed.Infrastructure.Repositories;

public class DoctorScheduleRepository : IDoctorScheduleRepository
{
    private readonly ApplicationDbContext _context;

    public DoctorScheduleRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<DoctorSchedule?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.DoctorSchedules
            .Include(ds => ds.Exceptions)
            .Include(ds => ds.Appointments)
            .FirstOrDefaultAsync(ds => ds.Id == id && !ds.IsDeleted, cancellationToken);
    }

    public async Task<List<DoctorSchedule>> GetByDoctorIdAsync(Guid doctorId, CancellationToken cancellationToken = default)
    {
        return await _context.DoctorSchedules
            .Include(ds => ds.Exceptions)
            .Where(ds => ds.DoctorId == doctorId && !ds.IsDeleted)
            .OrderBy(ds => ds.DayOfWeek)
            .ThenBy(ds => ds.StartTime)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<DoctorSchedule>> GetByDoctorIdAndDateRangeAsync(
        Guid doctorId, 
        DateOnly startDate, 
        DateOnly endDate, 
        CancellationToken cancellationToken = default)
    {
        return await _context.DoctorSchedules
            .Include(ds => ds.Exceptions)
            .Where(ds => ds.DoctorId == doctorId && 
                        !ds.IsDeleted &&
                        (ds.EffectiveFrom == null || ds.EffectiveFrom <= endDate) &&
                        (ds.EffectiveTo == null || ds.EffectiveTo >= startDate))
            .OrderBy(ds => ds.DayOfWeek)
            .ThenBy(ds => ds.StartTime)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<DoctorSchedule>> GetActiveDoctorSchedulesAsync(Guid doctorId, CancellationToken cancellationToken = default)
    {
        return await _context.DoctorSchedules
            .Include(ds => ds.Exceptions)
            .Where(ds => ds.DoctorId == doctorId && ds.IsActive && !ds.IsDeleted)
            .OrderBy(ds => ds.DayOfWeek)
            .ThenBy(ds => ds.StartTime)
            .ToListAsync(cancellationToken);
    }

    public async Task<DoctorSchedule?> GetDoctorScheduleForDayAsync(
        Guid doctorId, 
        DayOfWeek dayOfWeek, 
        DateOnly date, 
        CancellationToken cancellationToken = default)
    {
        return await _context.DoctorSchedules
            .Include(ds => ds.Exceptions)
            .Where(ds => ds.DoctorId == doctorId && 
                        ds.DayOfWeek == dayOfWeek &&
                        ds.IsActive &&
                        !ds.IsDeleted &&
                        (ds.EffectiveFrom == null || ds.EffectiveFrom <= date) &&
                        (ds.EffectiveTo == null || ds.EffectiveTo >= date))
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<bool> HasConflictingScheduleAsync(
        Guid doctorId, 
        DayOfWeek dayOfWeek, 
        TimeOnly startTime, 
        TimeOnly endTime, 
        Guid? excludeScheduleId = null, 
        CancellationToken cancellationToken = default)
    {
        var query = _context.DoctorSchedules
            .Where(ds => ds.DoctorId == doctorId && 
                        ds.DayOfWeek == dayOfWeek &&
                        ds.IsActive &&
                        !ds.IsDeleted &&
                        ((startTime >= ds.StartTime && startTime < ds.EndTime) ||
                         (endTime > ds.StartTime && endTime <= ds.EndTime) ||
                         (startTime <= ds.StartTime && endTime >= ds.EndTime)));

        if (excludeScheduleId.HasValue)
        {
            query = query.Where(ds => ds.Id != excludeScheduleId.Value);
        }

        return await query.AnyAsync(cancellationToken);
    }

    public async Task AddAsync(DoctorSchedule doctorSchedule, CancellationToken cancellationToken = default)
    {
        await _context.DoctorSchedules.AddAsync(doctorSchedule, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(DoctorSchedule doctorSchedule, CancellationToken cancellationToken = default)
    {
        _context.DoctorSchedules.Update(doctorSchedule);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var schedule = await GetByIdAsync(id, cancellationToken);
        if (schedule != null)
        {
            schedule.SoftDelete();
            await UpdateAsync(schedule, cancellationToken);
        }
    }

    public async Task<List<DoctorSchedule>> GetPaginatedAsync(
        int page, 
        int pageSize, 
        Guid? doctorId = null, 
        bool? isActive = null, 
        CancellationToken cancellationToken = default)
    {
        var query = _context.DoctorSchedules
            .Include(ds => ds.Exceptions)
            .Where(ds => !ds.IsDeleted);

        if (doctorId.HasValue)
            query = query.Where(ds => ds.DoctorId == doctorId.Value);

        if (isActive.HasValue)
            query = query.Where(ds => ds.IsActive == isActive.Value);

        return await query
            .OrderBy(ds => ds.DayOfWeek)
            .ThenBy(ds => ds.StartTime)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> GetCountAsync(Guid? doctorId = null, bool? isActive = null, CancellationToken cancellationToken = default)
    {
        var query = _context.DoctorSchedules.Where(ds => !ds.IsDeleted);

        if (doctorId.HasValue)
            query = query.Where(ds => ds.DoctorId == doctorId.Value);

        if (isActive.HasValue)
            query = query.Where(ds => ds.IsActive == isActive.Value);

        return await query.CountAsync(cancellationToken);
    }
}
