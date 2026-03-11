using System.Reflection;
using System.Xml;
using System.Xml.Schema;
using ISDOC.Helpers;
using ISDOC.Models;

namespace ISDOC.Services;

public sealed class IsdocValidationService
{
  static readonly XmlSchemaSet _Schemas = LoadSchemas();

  static XmlSchemaSet LoadSchemas()
  {
    var schemaSet = new XmlSchemaSet();
    var assembly = Assembly.GetExecutingAssembly();
    const string resourcePrefix = "ISDOC.Resources.Schemas._6._0._2.";

    // custom resolver to find embedded schemas by their filename
    var resolver = new EmbeddedResourceResolver( assembly, resourcePrefix );
    schemaSet.XmlResolver = resolver;

    // main schemas, the rest will be resolved from imports/redefines via the resolver
    string[] mainSchemas =
    {
      "isdoc-invoice-dsig-6.0.2.xsd",
      "isdoc-manifest-6.0.2.xsd"
    };

    foreach( var schemaName in mainSchemas )
    {
      var resourceName = resourcePrefix + schemaName;
      using var stream = assembly.GetManifestResourceStream( resourceName );
      if( stream == null )
        throw new InvalidOperationException( $"Resource {resourceName} not found." );

      using var reader = XmlReader.Create( stream, new() { XmlResolver = resolver }, "http://isdoc-res/" + schemaName );
      schemaSet.Add( null, reader );
    }

    schemaSet.Compile();
    return schemaSet;
  }

  public ValidationResult Validate( string isdocPath )
  {
    var result = new ValidationResult();
    var settings = new XmlReaderSettings
    {
      Schemas = _Schemas,
      ValidationType = ValidationType.Schema
    };

    settings.ValidationEventHandler += ( sender, e ) =>
    {
      var severity = e.Severity == XmlSeverityType.Error ? ValidationSeverity.Error : ValidationSeverity.Warning;
      result.Errors.Add( new( e.Exception.LineNumber, e.Exception.LinePosition, e.Message, severity ) );
    };

    try
    {
      using var reader = XmlReader.Create( isdocPath, settings );
      while( reader.Read() ) { }
    }
    catch( XmlException ex )
    {
      result.Errors.Add( new( ex.LineNumber, ex.LinePosition, ex.Message, ValidationSeverity.Error ) );
    }
    catch( Exception ex )
    {
      result.Errors.Add( new( 0, 0, ex.Message, ValidationSeverity.Error ) );
    }

    return result;
  }
}