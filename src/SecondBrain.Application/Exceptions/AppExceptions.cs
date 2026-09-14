namespace SecondBrain.Application.Exceptions;

// Exceções de aplicação, traduzidas para códigos HTTP pelo middleware global da API
// (ver SecondBrain.API/Middleware/ExceptionHandlingMiddleware.cs).
public class NotFoundException(string message) : Exception(message);

public class ConflictException(string message) : Exception(message);
