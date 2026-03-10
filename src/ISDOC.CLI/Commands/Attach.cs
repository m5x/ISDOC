using System.CommandLine;
using ISDOC.Services;

namespace ISDOC.CLI.Commands;

public sealed class Attach : BaseCommand
{
  readonly PdfAttachmentService _pdfAttachmentService;

  public Attach( PdfAttachmentService pdfAttachmentService )
  {
    _pdfAttachmentService = pdfAttachmentService;
  }

  protected override Command CommandLineCommand
  {
    get
    {
      var inputIsdocArg = new Argument<string>( "in.isdoc" ) { Description = "Input ISDOC file path." };
      var inputPdfArg = new Argument<string>( "in.pdf" ) { Description = "Input PDF file path." };
      var outputPdfArg = new Argument<string>( "out.pdf" ) { Description = "Output PDF file path." };

      var cmd = new Command( "attach", "Attach an ISDOC file to a PDF file." )
      {
        inputIsdocArg,
        inputPdfArg,
        outputPdfArg
      };

      cmd.TreatUnmatchedTokensAsErrors = true;

      cmd.SetAction( parseResult => (int)Execute( parseResult.GetValue( inputIsdocArg )!,
                                                  parseResult.GetValue( inputPdfArg )!,
                                                  parseResult.GetValue( outputPdfArg )! ) );

      return cmd;
    }
  }

  ExitCode Execute( string isdocPath, string inputPdfPath, string outputPdfPath )
  {
    if( !File.Exists( inputPdfPath ) )
    {
      Console.Error.WriteLine( $"Input PDF not found: {inputPdfPath}" );
      return ExitCode.InputPdfNotFound;
    }

    if( !File.Exists( isdocPath ) )
    {
      Console.Error.WriteLine( $"ISDOC file not found: {isdocPath}" );
      return ExitCode.IsdocNotFound;
    }

    if( File.Exists( outputPdfPath ) )
    {
      Console.Error.WriteLine( $"Output PDF already exists: {outputPdfPath}" );
      return ExitCode.OutputAlreadyExists;
    }

    if( Path.GetFullPath( inputPdfPath ) == Path.GetFullPath( outputPdfPath ) )
    {
      Console.Error.WriteLine( "Output PDF must be different from input PDF." );
      return ExitCode.InvalidOutputPath;
    }

    try
    {
      _pdfAttachmentService.AttachIsdocToPdf( inputPdfPath, isdocPath, outputPdfPath );
      Console.WriteLine( $"Created ISDOC.PDF: {outputPdfPath}" );
      return ExitCode.Success;
    }
    catch( Exception ex )
    {
      Console.Error.WriteLine( $"Error: {ex.Message}" );
      return ExitCode.UnexpectedError;
    }
  }
}
