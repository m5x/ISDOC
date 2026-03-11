using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Filespec;

namespace ISDOC.Services;

public sealed class PdfAttachmentService
{
  const string IsdocFileName = "invoice.isdoc";

  public void AttachIsdocToPdf( string inputPdfPath, string isdocPath, string outputPdfPath )
  {
    var isdocBytes = File.ReadAllBytes( isdocPath );

    using var pdfReader = new PdfReader( inputPdfPath );
    using var pdfWriter = new PdfWriter( outputPdfPath );
    using var pdfDoc = new PdfDocument( pdfReader, pdfWriter );

    var fileSpec = PdfFileSpec.CreateEmbeddedFileSpec( pdfDoc,
                                                       isdocBytes,
                                                       IsdocFileName,
                                                       new( "Alternative" ) );

    pdfDoc.AddFileAttachment( IsdocFileName, fileSpec );
    RemovePdfFileSpecDates( fileSpec );

    var afArray = new PdfArray { fileSpec.GetPdfObject() };
    var catalog = pdfDoc.GetCatalog();
    catalog.Put( new( "AF" ), afArray );
  }

  public byte[] ExtractIsdocFromPdf( string inputPdfPath, out bool isAlternative )
  {
    using var pdfReader = new PdfReader( inputPdfPath );
    using var pdfDoc = new PdfDocument( pdfReader );

    var catalog = pdfDoc.GetCatalog();
    var names = catalog.GetPdfObject().GetAsDictionary( PdfName.Names );
    var embeddedFiles = names?.GetAsDictionary( PdfName.EmbeddedFiles );

    if( embeddedFiles is not null )
    {
      var namesArray = embeddedFiles.GetAsArray( PdfName.Names );
      if( namesArray is not null )
        for( var i = 0; i < namesArray.Size(); i += 2 )
        {
          var fileName = namesArray.GetAsString( i ).ToUnicodeString();
          if( fileName.EndsWith( ".isdoc", StringComparison.OrdinalIgnoreCase ) )
          {
            var fileSpecDict = namesArray.GetAsDictionary( i + 1 );
            var result = ExtractFromFileSpec( fileSpecDict, out isAlternative );
            if( result is not null )
              return result;
          }
        }
    }

    var afArray = catalog.GetPdfObject().GetAsArray( new( "AF" ) );
    if( afArray is not null )
      for( var i = 0; i < afArray.Size(); i++ )
      {
        var fileSpecDict = afArray.GetAsDictionary( i );
        var fileName = fileSpecDict.GetAsString( PdfName.F )?.ToUnicodeString()
                    ?? fileSpecDict.GetAsString( PdfName.UF )?.ToUnicodeString();

        if( fileName?.EndsWith( ".isdoc", StringComparison.OrdinalIgnoreCase ) == true )
        {
          var result = ExtractFromFileSpec( fileSpecDict, out isAlternative );
          if( result is not null )
            return result;
        }
      }

    throw new InvalidOperationException( "No ISDOC attachment found in the PDF file." );
  }

  static byte[]? ExtractFromFileSpec( PdfDictionary fileSpecDict, out bool isAlternative )
  {
    isAlternative = false;
    var efDict = fileSpecDict.GetAsDictionary( PdfName.EF );
    var stream = efDict?.GetAsStream( PdfName.F ) ?? efDict?.GetAsStream( PdfName.UF );

    if( stream is null )
      return null;

    var subtype = fileSpecDict.GetAsName( PdfName.AFRelationship );
    isAlternative = subtype?.Equals( new PdfName( "Alternative" ) ) == true;

    return stream.GetBytes();
  }

  static void RemovePdfFileSpecDates( PdfFileSpec fileSpec )
  {
    var paramsDict = ( fileSpec.GetPdfObject() as PdfDictionary )?
       .GetAsDictionary( PdfName.EF )?
       .GetAsStream( PdfName.F )?
       .GetAsDictionary( PdfName.Params );

    if( paramsDict is null )
      return;

    paramsDict.Remove( PdfName.ModDate );
    paramsDict.Remove( PdfName.CreationDate );
  }
}
