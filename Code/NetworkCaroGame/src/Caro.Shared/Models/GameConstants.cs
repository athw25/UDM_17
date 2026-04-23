namespace Caro.Shared.Models
{
    public static class GameConstants
    {
        public const int BOARD_SIZE = 15;
        
        // Username validation
        public const int MAX_USERNAME_LENGTH = 20;
        public const int MIN_USERNAME_LENGTH = 3;
        
        // Regex pattern: cho phép chữ (tiếng Anh, tiếng Việt), số, underscore, dấu cách
        public const string USERNAME_PATTERN = @"^[\p{L}\p{N}_\s]{3,20}$";
    }
}
