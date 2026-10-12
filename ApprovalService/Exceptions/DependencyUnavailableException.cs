namespace ApprovalService.Exceptions;

public sealed class DependencyUnavailableException(string message, Exception? innerException = null)
    : Exception(message, innerException);
