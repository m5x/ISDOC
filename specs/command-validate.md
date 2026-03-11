### Specification: ISDOC `validate` Command

This specification defines the `validate` command for the ISDOC CLI tool, which verifies if a given ISDOC file conforms
to the ISDOC 6.0.2 specification.

#### 1. Command Interface

The command will be added to the `ISDOC.CLI` tool.

**Usage:**
`isdoc validate <in.isdoc> [--strict]`

**Arguments:**

- `<in.isdoc>`: Path to the input ISDOC file to be validated.

**Options:**

- `--strict`: (Optional) If set, the validation will also perform additional checks beyond schema validation (e.g.,
  specific business rules, if applicable).

**Exit Codes:**

- `0` (Success): File is valid according to the specification.
- `1` (SomeFailed): File is invalid according to the specification.
- `4` (IsdocNotFound): The input ISDOC file was not found.
- `99` (UnexpectedError): An unexpected error occurred during validation.

#### 2. Validation Logic

Validation will consist of the following steps:

1. **File Existence Check**: Verify that the input file exists.
2. **XML Well-formedness**: Ensure the file is a valid XML.
3. **XSD Schema Validation**: Validate the XML against the ISDOC 6.0.2 XSD schemas.
    - The tool will include the necessary XSD files for version 6.0.2 (and possibly earlier versions if needed for
      auto-detection).
    - The schema validation will use `System.Xml.Schema.XmlSchemaSet`.
4. **Digital Signature Validation (Optional/Phase 2)**: If the ISDOC file contains a digital signature (`ds:Signature`),
   the tool should verify its validity.
5. **Output**:
    - On success: Print `ISDOC file is valid.` to stdout.
    - On failure: Print `ISDOC file is invalid.` to stderr, followed by a list of validation errors (line numbers and
      descriptions).

#### 3. Implementation Details

- **Library Service**: A new service `IsdocValidationService` will be added to the `ISDOC` project.
    - Method: `ValidationResult Validate(string isdocPath)`
    - `ValidationResult` will contain a boolean `IsValid` and a collection of `ValidationError` objects.
- **CLI Command**: A new command class `Validate` will be added to `ISDOC.CLI.Commands`, inheriting from `BaseCommand`.
- **Resources**: XSD schemas for ISDOC 6.0.2 will be added to the `ISDOC` project as `Embedded Resources`.
- **Dependencies**: No new external dependencies are expected for basic XSD validation.
  `System.Security.Cryptography.Xml` might be needed for digital signature validation.

#### 4. Supported Versions

While the focus is on version 6.0.2, the implementation should ideally be extensible to support other versions if the
namespace/version can be detected from the XML root element.
