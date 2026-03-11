namespace ISDOC.Models;

public record ValidationError( int LineNumber, int LinePosition, string Message, ValidationSeverity Severity );
