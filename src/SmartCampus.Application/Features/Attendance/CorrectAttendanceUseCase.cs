using SmartCampus.Application.Common.Interfaces;

namespace SmartCampus.Application.Features.Attendance;

public sealed class CorrectAttendanceUseCase(IAttendanceRecordRepository attendanceRecordRepository, IUnitOfWork unitOfWork)
{
    public async Task<AttendanceRecordDto?> ExecuteAsync(Guid id, CorrectAttendanceRequest request, CancellationToken cancellationToken = default)
    {
        var record = await attendanceRecordRepository.GetByIdAsync(id, cancellationToken);
        if (record is null)
        {
            return null;
        }

        record.Correct(request.Status, request.Notes);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return RecordAttendanceUseCase.ToDto(record);
    }
}
