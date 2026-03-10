namespace ISDOC.CLI;

public enum ExitCode
{
  Success = 0,
  SomeFailed = 1,
  AllFailed = 2,
  InputPdfNotFound = 3,
  IsdocNotFound = 4,
  OutputAlreadyExists = 5,
  InvalidOutputPath = 6,
  UnexpectedError = 99
}
