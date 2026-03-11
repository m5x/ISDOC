using System.Xml.Linq;
using iText.IO.Font.Constants;
using iText.Kernel.Font;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Borders;
using iText.Layout.Element;
using iText.Layout.Properties;

namespace ISDOC.Services;

public sealed class IsdocRenderingService
{
  public void RenderToPdf( string isdocPath, string pdfPath )
  {
    var xDoc = XDocument.Load( isdocPath );
    var xRoot = xDoc.Root ?? throw new( "Input ISDOC file is empty." );
    var ns = (XNamespace)"http://isdoc.cz/namespace/2013";

    using var writer = new PdfWriter( pdfPath );
    using var pdf = new PdfDocument( writer );
    using var doc = new Document( pdf );

    var fontBold = PdfFontFactory.CreateFont( StandardFonts.HELVETICA_BOLD );

    var invoiceId = xRoot.Element( ns + "ID" )?.Value ?? "N/A";
    var issueDate = xRoot.Element( ns + "IssueDate" )?.Value ?? "N/A";

    doc.Add( new Paragraph( $"Invoice {invoiceId}" ).SetFont( fontBold ).SetFontSize( 20 ) );
    doc.Add( new Paragraph( $"Issue Date: {issueDate}" ) );

    // parties
    var supplier = xRoot.Element( ns + "AccountingSupplierParty" )?.Element( ns + "Party" );
    var customer = xRoot.Element( ns + "AccountingCustomerParty" )?.Element( ns + "Party" );

    var partyTable = new Table( UnitValue.CreatePercentArray( 2 ) ).SetWidth( UnitValue.CreatePercentValue( 100 ) );
    partyTable.AddCell( CreatePartyCell( "Supplier", supplier, ns, fontBold ) );
    partyTable.AddCell( CreatePartyCell( "Customer", customer, ns, fontBold ) );
    doc.Add( partyTable );

    doc.Add( new Paragraph( "\n" ) );

    // lines
    var lines = xRoot.Element( ns + "InvoiceLines" )?.Elements( ns + "InvoiceLine" );
    if( lines != null )
    {
      var lineTable = new Table( UnitValue.CreatePercentArray( new float[] { 50, 10, 20, 20 } ) ).SetWidth( UnitValue.CreatePercentValue( 100 ) );
      lineTable.AddHeaderCell( "Description" );
      lineTable.AddHeaderCell( "Qty" );
      lineTable.AddHeaderCell( "Unit Price" );
      lineTable.AddHeaderCell( "Total" );

      foreach( var line in lines )
      {
        var description = line.Element( ns + "Item" )?.Element( ns + "Description" )?.Value ?? "";
        var qty = line.Element( ns + "InvoicedQuantity" )?.Value ?? "0";
        var unitPrice = line.Element( ns + "UnitPrice" )?.Value ?? "0";
        var total = line.Element( ns + "LineExtensionAmount" )?.Value ?? "0";

        lineTable.AddCell( new Cell().Add( new Paragraph( description ) ) );
        lineTable.AddCell( new Cell().Add( new Paragraph( qty ) ) );
        lineTable.AddCell( new Cell().Add( new Paragraph( unitPrice ) ) );
        lineTable.AddCell( new Cell().Add( new Paragraph( total ) ) );
      }

      doc.Add( lineTable );
    }

    // totals
    var totalAmount = xRoot.Element( ns + "LegalMonetaryTotal" )?.Element( ns + "PayableAmount" )?.Value ?? "0";
    var currency = xRoot.Element( ns + "LocalCurrencyCode" )?.Value ?? "";
    doc.Add( new Paragraph( $"\nTotal Payable: {totalAmount} {currency}" ).SetTextAlignment( TextAlignment.RIGHT ).SetFont( fontBold ).SetFontSize( 14 ) );
  }

  static Cell CreatePartyCell( string title, XElement? party, XNamespace ns, PdfFont fontBold )
  {
    var cell = new Cell().SetBorder( Border.NO_BORDER );
    cell.Add( new Paragraph( title ).SetFont( fontBold ) );
    if( party != null )
    {
      var name = party.Element( ns + "PartyName" )?.Element( ns + "Name" )?.Value ?? "";
      var id = party.Element( ns + "PartyIdentification" )?.Element( ns + "ID" )?.Value ?? "";
      var addr = party.Element( ns + "PostalAddress" );
      var street = addr?.Element( ns + "StreetName" )?.Value ?? "";
      var bld = addr?.Element( ns + "BuildingNumber" )?.Value ?? "";
      var city = addr?.Element( ns + "CityName" )?.Value ?? "";
      var zip = addr?.Element( ns + "PostalZone" )?.Value ?? "";

      cell.Add( new Paragraph( name ) );
      if( !string.IsNullOrEmpty( id ) )
        cell.Add( new Paragraph( $"ID: {id}" ) );
      cell.Add( new Paragraph( $"{street} {bld}" ) );
      cell.Add( new Paragraph( $"{zip} {city}" ) );
    }

    return cell;
  }
}
