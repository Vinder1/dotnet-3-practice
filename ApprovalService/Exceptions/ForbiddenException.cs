namespace ApprovalService.Exceptions;

public sealed class ForbiddenException(string message) : Exception(message);
