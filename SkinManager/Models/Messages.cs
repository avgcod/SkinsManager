namespace SkinManager.Models
{
    public record NewGameMessage(string newGameName); 
    public record class OperationErrorMessage(string ErrorType, string ErrorMessage);
    public record class DirectoryNotEmptyMessage(string DirectoryPath);
}
