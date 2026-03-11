namespace ISDOC.Models;

public class ValidationResult
{
  public bool IsValid => Errors.All( e => e.Severity != ValidationSeverity.Error );
  public List<ValidationError> Errors { get; } = new();
}
