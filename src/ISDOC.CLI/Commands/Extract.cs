using System.CommandLine;
using ISDOC.Services;

namespace ISDOC.CLI.Commands;

public sealed class Extract : BaseCommand
{
  readonly PdfAttachmentService _pdfAttachmentService;

  public Extract( PdfAttachmentService pdfAttachmentService )
  {
    _pdfAttachmentService = pdfAttachmentService;
  }

  protected override Command CommandLineCommand
  {
    get
    {
      var inputPdfArg = new Argument<string>( "in.pdf" ) { Description = "Input PDF file path." };
      var outputIsdocArg = new Argument<string>( "out.isdoc" ) { Description = "Output ISDOC file path." };

      var cmd = new Command( "extract", "Extract an ISDOC file from a PDF file." )
      {
        inputPdfArg,
        outputIsdocArg
      };

      cmd.TreatUnmatchedTokensAsErrors = true;

      cmd.SetAction( parseResult => (int)Execute( parseResult.GetValue( inputPdfArg )!,
                                                  parseResult.GetValue( outputIsdocArg )! ) );

      return cmd;
    }
  }

  ExitCode Execute( string inputPdfPath, string outputIsdocPath )
  {
    if( !File.Exists( inputPdfPath ) )
    {
      Console.Error.WriteLine( $"Input PDF not found: {inputPdfPath}" );
      return ExitCode.InputPdfNotFound;
    }

    if( File.Exists( outputIsdocPath ) )
    {
      Console.Error.WriteLine( $"Output ISDOC already exists: {outputIsdocPath}" );
      return ExitCode.OutputAlreadyExists;
    }

    try
    {
      var isdocBytes = _pdfAttachmentService.ExtractIsdocFromPdf( inputPdfPath, out var isAlternative );

      if( !isAlternative )
      {
        Console.WriteLine( "Warning: ISDOC attachment is not properly marked as alternative representation." );
      }

      File.WriteAllBytes( outputIsdocPath, isdocBytes );
      Console.WriteLine( $"Extracted ISDOC to: {outputIsdocPath}" );
      return ExitCode.Success;
    }
    catch( InvalidOperationException ex )
    {
      Console.Error.WriteLine( $"Error: {ex.Message}" );
      return ExitCode.SomeFailed;
    }
    catch( Exception ex )
    {
      Console.Error.WriteLine( $"Error: {ex.Message}" );
      return ExitCode.UnexpectedError;
    }
  }
}
