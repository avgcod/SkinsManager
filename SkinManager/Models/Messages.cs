namespace SkinManager.Models
{
    public record NewGameMessage(string NewGameName); 
    public record class OperationErrorMessage(string ErrorType, string ErrorText);
    public record class ErrorMessage(string ErrorType, string ErrorText);
    public record class DirectoryNotEmptyMessage(string DirectoryPath);
    public record class MessageBoxMessage(string Message);
}
