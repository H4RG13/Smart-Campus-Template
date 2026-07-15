namespace SmartCampus.Domain.Constants;

public static class RoleNames
{
    public const string Admin = "Admin";
    public const string Teacher = "Teacher";
    public const string Staff = "Staff";
    public const string Parent = "Parent";
    public const string Student = "Student";

    public static readonly IReadOnlyList<string> All =
    [
        Admin, Teacher, Staff, Parent, Student
    ];
}
