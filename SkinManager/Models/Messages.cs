namespace SkinManager.Models;

public record OperationErrorMessage(string ErrorType, string ErrorText);
public record ErrorMessage(string ErrorType, string ErrorText);
public record DirectoryNotEmptyMessage(string DirectoryPath);
public record MessageBoxMessage(string Message);
public record FatalErrorMessage(string ErrorType, string ErrorText);