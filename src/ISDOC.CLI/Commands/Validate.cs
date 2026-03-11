using System.CommandLine;
using ISDOC.Models;
using ISDOC.Services;

namespace ISDOC.CLI.Commands;

public sealed class Validate : BaseCommand
{
  readonly IsdocValidationService _validationService;

  public Validate( IsdocValidationService validationService )
  {
    _validationService = validationService;
  }

  protected override Command CommandLineCommand
  {
    get
    {
      var inputIsdocArg = new Argument<string>( "in.isdoc" ) { Description = "Input ISDOC file path." };
      var strictOption = new Option<bool>( "--strict" ) { Description = "Perform additional checks beyond schema validation." };

      var cmd = new Command( "validate", "Verify whether input ISDOC file is valid according to specification." )
      {
        inputIsdocArg,
        strictOption
      };

      cmd.TreatUnmatchedTokensAsErrors = true;

      cmd.SetAction( parseResult => (int)Execute( parseResult.GetValue( inputIsdocArg )!,
                                                  parseResult.GetValue( strictOption ) ) );

      return cmd;
    }
  }

  ExitCode Execute( string isdocPath, bool strict )
  {
    if( !File.Exists( isdocPath ) )
    {
      Console.Error.WriteLine( $"ISDOC file not found: {isdocPath}" );
      return ExitCode.IsdocNotFound;
    }

    try
    {
      var result = _validationService.Validate( isdocPath );

      if( result.IsValid )
      {
        Console.WriteLine( "ISDOC file is valid." );
        return ExitCode.Success;
      }

      Console.Error.WriteLine( "ISDOC file is invalid." );
      foreach( var error in result.Errors )
      {
        var prefix = error.Severity == ValidationSeverity.Error ? "Error" : "Warning";
        Console.Error.WriteLine( $"[{prefix}] Line {error.LineNumber}, Position {error.LinePosition}: {error.Message}" );
      }

      return ExitCode.SomeFailed;
    }
    catch( Exception ex )
    {
      Console.Error.WriteLine( $"Unexpected Error: {ex.Message}" );
      return ExitCode.UnexpectedError;
    }
  }
}
