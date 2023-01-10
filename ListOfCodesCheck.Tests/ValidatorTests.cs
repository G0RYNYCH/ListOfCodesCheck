using Xunit;

namespace ListOfCodesCheck.Tests;

public class ValidatorTests
{
    [Theory]
    [InlineData("1234 - 7", "12341234567", null)]
    [InlineData("1234 - 7, 123 - 5+; 12 - 3", "1234123456712312345\\u001d12123", null)]
    [InlineData("1234 - 7, 123 - 5+; 12 - 3, 234 - 3+", "1234123456712312345\\u001d12123234123123", null)]
    [InlineData("1234 - 7, 123 - 5+; 12 - 3, 234 - 3+", "1234123456712312345\\u001d121232341231235468", null)]
    public void Validate_GetValidGroupsList_Null(string checkLine, string groupsList, string expected)
    {
        var parser = new CheckLineParser();
        var validationRules = parser.Parse(checkLine);
        var validator = new Validator(validationRules);

        var result = validator.Validate(groupsList);

        Assert.Equal(expected, result);
    }

    [Theory]   
    [InlineData("1234 - 7, 123 - 5+", "12341234567", "Ожидалась группа применения 123, найдена <конец кода>. Позиция 12.")]
    [InlineData("1234 - 7, 123 - 5+", "1234123456789", "Ожидалась группа применения 123, найдена 89. Позиция 12.")]
    [InlineData("1234 - 7, 123 - 5+", "12341234567123123", "Длина группы применения 123 меньше указанной, ожидалось 5+, найдена 3. Позиция 12.")]
    [InlineData("1234 - 7", "123412345", "Группа применения 1234 переменной длины, ожидалось фиксированной 7. Позиция 1.")]
    [InlineData("1234 - 7", "123412345678", "Код продолжается после завершающей группы применения, найдено 8. Позиция 12.")]
    [InlineData("1234 - 7, 123 - 5+; 12 - 3", "1234123456712312345\\u001d12123456", "Код продолжается после завершающей группы применения, найдено 456. Позиция 31.")]
    [InlineData("1234 - 7, 123 - 5+", "1234123456712312345\\u001d12123456", "Код продолжается после завершающей группы применения, найдено 12123456. Позиция 26.")]
    public void Validate_GetInvalidGroupsList_ErrorMessage(string checkLine, string groupsList, string expected)
    {
        var parser = new CheckLineParser();
        var validationRules = parser.Parse(checkLine);
        var validator = new Validator(validationRules);

        var result = validator.Validate(groupsList);

        Assert.Equal(expected, result);
    }
}
