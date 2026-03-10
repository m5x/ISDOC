using System.CommandLine;
using ISDOC.Services;

namespace ISDOC.CLI.Commands;

public sealed class AutoPair( PdfAttachmentService pdfAttachmentService ) : BaseCommand
{
  protected override Command CommandLineCommand
  {
    get
    {
      var prefixArg = new Argument<string>( "prefix" ) { Description = "Prefix to prepend to generated PDF file names." };
      var recursiveOpt = new Option<bool>( "recursive", "-r", "--recursive" ) { Description = "Process subdirectories recursively." };

      var cmd = new Command( "autopair", "Attach each file in current directory with .isdoc extension to PDF stored in the file with the same name but .pdf extension and save it to file with the same name but prefixed with the given prefix." )
      {
        prefixArg,
        recursiveOpt
      };

      cmd.TreatUnmatchedTokensAsErrors = true;

      cmd.SetAction( parseResult => (int)Execute( parseResult.GetValue( prefixArg )!,
                                                  parseResult.GetValue( recursiveOpt ) ) );

      return cmd;
    }
  }

  ExitCode Execute( string prefix, bool recursive )
  {
    if( string.IsNullOrWhiteSpace( prefix ) )
    {
      Console.Error.WriteLine( "Prefix must not be empty." );
      return ExitCode.SomeFailed;
    }

    var currentDirectory = Directory.GetCurrentDirectory();
    var searchOption = recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
    var isdocPaths = Directory.GetFiles( currentDirectory, "*.isdoc", searchOption );

    var succeededCount = 0;
    var failedCount = 0;

    foreach( var isdocPath in isdocPaths )
    {
      var inputPdfPath = Path.ChangeExtension( isdocPath, ".pdf" );
      var directory = Path.GetDirectoryName( inputPdfPath ) ?? string.Empty;
      var outputFileName = prefix + Path.GetFileName( inputPdfPath );
      var outputPdfPath = Path.Combine( directory, outputFileName );

      try
      {
        if( !File.Exists( inputPdfPath ) )
          throw new FileNotFoundException( $"Matching PDF not found: {inputPdfPath}" );

        if( Path.GetFullPath( inputPdfPath ) == Path.GetFullPath( outputPdfPath ) )
          throw new IOException( "Output PDF must be different from input PDF." );

        if( File.Exists( outputPdfPath ) )
          throw new IOException( $"Output PDF already exists: {outputPdfPath}" );

        pdfAttachmentService.AttachIsdocToPdf( inputPdfPath, isdocPath, outputPdfPath );
        succeededCount++;
        Console.WriteLine( $"Created ISDOC.PDF: {outputPdfPath}" );
      }
      catch( Exception ex )
      {
        failedCount++;
        Console.Error.WriteLine( $"Failed: {isdocPath}" );
        Console.Error.WriteLine( $"  {ex.Message}" );
      }
    }

    Console.WriteLine( $"Summary: processed {succeededCount}, failed {failedCount}." );

    return
        failedCount == 0 ? ExitCode.Success :
        succeededCount == 0 ? ExitCode.AllFailed : ExitCode.SomeFailed;
  }
}
