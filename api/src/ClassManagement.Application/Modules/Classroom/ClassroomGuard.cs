using ClassManagement.Application.Exceptions;

// Ownership/existence checks shared by teacher class command handlers. A teacher may only manage
// classes they own (BR-2-04) — any other class (or a missing one) is rejected.
internal static class ClassroomGuard
{
    public static Class EnsureOwned(Class? cls, long? currentUserId)
    {
        if (cls is null)
            throw new NotFoundException("Class.NotFound");

        if (currentUserId is null || cls.OwnerId != currentUserId.Value)
            throw new ForbiddenException("Class.NotOwner");

        return cls;
    }

    public static ClassMembership EnsureInClass(ClassMembership? membership, long classId)
    {
        if (membership is null || membership.ClassId != classId)
            throw new NotFoundException("Membership.NotFound");

        return membership;
    }
}
