namespace RegistrationCoreService.Exceptions;

public sealed class MicroServiceUnavailableException(string message, Exception? innerException = null)
    : Exception(message, innerException);