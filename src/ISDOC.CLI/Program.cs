using System.CommandLine;
using ISDOC.CLI.Commands;
using ISDOC.Services;

namespace ISDOC.CLI;

static class Program
{
  static async Task<int> Main( string[] args )
  {
    var rootCommand = new RootCommand( "Tool for working with ISDOC files." )
    {
      TreatUnmatchedTokensAsErrors = true
    };

    var pdfAttachmentService = new PdfAttachmentService();
    var isdocValidationService = new IsdocValidationService();
    rootCommand.Subcommands.Add( new Attach( pdfAttachmentService ) );
    rootCommand.Subcommands.Add( new AutoPair( pdfAttachmentService ) );
    rootCommand.Subcommands.Add( new Extract( pdfAttachmentService ) );
    rootCommand.Subcommands.Add( new Validate( isdocValidationService ) );

    try
    {
      return await rootCommand.Parse( args ).InvokeAsync();
    }
    catch( Exception ex )
    {
      await Console.Error.WriteLineAsync( $"Error: {ex.Message}" );
      return (int)ExitCode.UnexpectedError;
    }
  }
}
