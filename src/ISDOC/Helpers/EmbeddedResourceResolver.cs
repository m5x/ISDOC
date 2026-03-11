using System.Reflection;
using System.Xml;

namespace ISDOC.Helpers;

sealed class EmbeddedResourceResolver( Assembly assembly, string resourcePrefix ) : XmlResolver
{
  public override object? GetEntity( Uri absoluteUri, string? role, Type? ofObjectToReturn )
  {
    var fileName = Path.GetFileName( absoluteUri.LocalPath );
    var resourceName = resourcePrefix + fileName;
    var stream = assembly.GetManifestResourceStream( resourceName );
    return stream;
  }

  public override Uri ResolveUri( Uri? baseUri, string? relativeUri )
  {
    if( baseUri != null && !string.IsNullOrEmpty( relativeUri ) )
      return new( baseUri, relativeUri );
    if( !string.IsNullOrEmpty( relativeUri ) && !relativeUri.Contains( "://" ) )
      return new( "http://isdoc-res/" + relativeUri );
    return base.ResolveUri( baseUri, relativeUri );
  }
}
