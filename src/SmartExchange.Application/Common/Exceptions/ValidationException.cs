namespace SmartExchange.Application.Common.Exceptions;

// Erro de validação de entrada da aplicação (ex.: montante não positivo, período inválido).
public class ValidationException : Exception
{
    public ValidationException(string message) : base(message)
    {
    }
}
