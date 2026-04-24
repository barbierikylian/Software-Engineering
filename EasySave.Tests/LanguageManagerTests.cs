using EasySave.Model;

namespace EasySave.Tests
{
    public class LanguageManagerTests
    {
        [Fact]
        public void GetString_WhenFrenchLoaded_ReturnsFrenchWelcome()
        {
            // Arrange
            LanguageManager manager = new LanguageManager();
            manager.LoadLanguage("fr");

            // Act
            string result = manager.GetString("welcome");

            // Assert
            Assert.Equal("Bienvenue dans EasySave", result);
        }
    }
}