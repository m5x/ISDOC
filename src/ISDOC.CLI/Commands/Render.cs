using System.CommandLine;
using ISDOC.Services;

namespace ISDOC.CLI.Commands;

public sealed class Render( IsdocRenderingService renderingService ) : BaseCommand
{
  protected override Command CommandLineCommand
  {
    get
    {
      var inputIsdocArg = new Argument<string>( "in.isdoc" ) { Description = "Input ISDOC file path." };
      var outOption = new Option<string?>( "--out" ) { Description = "Output PDF file path." };

      var cmd = new Command( "render", "Render ISDOC file to human-readable PDF." )
      {
        inputIsdocArg,
        outOption
      };

      cmd.TreatUnmatchedTokensAsErrors = true;

      cmd.SetAction( parseResult => (int)Execute( parseResult.GetValue( inputIsdocArg )!,
                                                  parseResult.GetValue( outOption ) ) );

      return cmd;
    }
  }

  ExitCode Execute( string isdocPath, string? outPath )
  {
    if( !File.Exists( isdocPath ) )
    {
      Console.Error.WriteLine( $"ISDOC file not found: {isdocPath}" );
      return ExitCode.IsdocNotFound;
    }

    if( string.IsNullOrEmpty( outPath ) )
      outPath = Path.ChangeExtension( isdocPath, ".pdf" );

    if( File.Exists( outPath ) )
    {
      Console.Error.WriteLine( $"Output file already exists: {outPath}" );
      return ExitCode.OutputAlreadyExists;
    }

    try
    {
      renderingService.RenderToPdf( isdocPath, outPath );
      Console.WriteLine( $"ISDOC rendered to {outPath}" );
      return ExitCode.Success;
    }
    catch( Exception ex )
    {
      Console.Error.WriteLine( $"Rendering failed: {ex.Message}" );
      return ExitCode.RenderingFailed;
    }
  }
}
