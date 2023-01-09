using ListOfCodesCheck;

var filePath = args[0];
var checkLine = args[1];

var file = File.ReadAllLines(filePath);
using StreamWriter checkedFile = new("checkedCodes.txt", append: true);
var parser = new CheckLineParser();
var validationRules = parser.Parse(checkLine);
var validator = new Validator(validationRules);

foreach (var groupsList in file)
{
    var errorMessage = validator.Validate(groupsList);

    if (errorMessage == null)
    {
        checkedFile.WriteLine(groupsList + "\t OK");
    }
    else
    {
        checkedFile.WriteLine(groupsList + $"\t {errorMessage}");
    }
}
