using ListOfCodesCheck;

var filePath = args[0];
var checkLine = args[1];

var file = File.ReadAllLines(filePath);
using StreamWriter checkedFile = new("checkedCodes.txt", append: true);
var parser = new Parser();

var validationRules = parser.ParseCheckLine(checkLine);
var validator = new Validator(validationRules);

foreach (var str in file)
{
    var validationError = validator.Validate(str);

    if (validationError == null)
    {
        checkedFile.WriteLine(str + "\t OK");
    }
    else
    {
        checkedFile.WriteLine(str + $"\t {validationError}");
    }
}
