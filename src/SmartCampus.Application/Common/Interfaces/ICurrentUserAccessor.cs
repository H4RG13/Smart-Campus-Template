namespace SmartCampus.Application.Common.Interfaces;

public interface ICurrentUserAccessor
{
    Guid? UserId { get; }
}
