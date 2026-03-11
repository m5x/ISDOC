### Specification: ISDOC `render` Command

This specification defines the `render` command for the ISDOC CLI tool, which converts an ISDOC file into a human-readable PDF document.

#### 1. Command Interface

The command will be added to the `ISDOC.CLI` tool.

**Usage:**
`isdoc render <in.isdoc> [--out <out.pdf>]`

**Arguments:**

- `<in.isdoc>`: Path to the input ISDOC file to be rendered.

**Options:**

- `--out <out.pdf>`: (Optional) Path where the generated PDF should be saved. If not provided, the output file will be named the same as the input file, but with a `.pdf` extension (e.g., `invoice.isdoc` -> `invoice.pdf`).

**Exit Codes:**

- `0` (Success): PDF was successfully generated.
- `4` (IsdocNotFound): The input ISDOC file was not found.
- `5` (OutputAlreadyExists): The output PDF file already exists.
- `10` (RenderingFailed): The rendering process failed (e.g., malformed ISDOC, PDF generation error).
- `99` (UnexpectedError): An unexpected error occurred.

#### 2. Rendering Logic

The rendering process will involve:

1. **File Existence Check**: 
    - Verify that the input ISDOC file exists.
    - Verify that the output PDF file does not exist. If it does, end with error.
2. **Parsing**: Load and parse the ISDOC XML file.
3. **Data Extraction**: Extract essential information for a standard invoice:
    - **Header**: Document ID, Document Type (Invoice, Credit Note, etc.), Issue Date, Tax Date, Due Date.
    - **Parties**: Seller and Buyer information (Name, Street, City, Zip, Country, Registration ID, VAT ID, Bank Account).
    - **Lines**: List of items with Description, Quantity, Unit, Unit Price, VAT Rate, and Total Price.
    - **Totals**: Tax base, VAT amount, and total amount per VAT rate, and the final grand total.
4. **Layout Generation**: Create a structured PDF document using `iText7`.
    - The layout should be professional and easy to read.
    - Use tables for invoice lines and totals.
    - Include labels for all fields (initially in English or Czech, or both).

#### 3. Implementation Details

- **Library Service**: A new service `IsdocRenderingService` will be added to the `ISDOC` project.
    - Method: `void RenderToPdf(string isdocPath, string pdfPath)`
- **CLI Command**: A new command class `Render` will be added to `ISDOC.CLI.Commands`, inheriting from `BaseCommand`.
- **Dependencies**: Uses the existing `itext7` package.

#### 4. Supported Versions

Primarily ISDOC 6.0.2. Older versions might be supported if the XML structure is compatible or if simple mapping is possible.
