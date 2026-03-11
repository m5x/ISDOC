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

    var cmds = rootCommand.Subcommands;
    cmds.Add( new Attach( pdfAttachmentService ) );
    cmds.Add( new AutoPair( pdfAttachmentService ) );
    cmds.Add( new Extract( pdfAttachmentService ) );
    cmds.Add( new Validate( new IsdocValidationService() ) );
    cmds.Add( new Render( new IsdocRenderingService() ) );

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
