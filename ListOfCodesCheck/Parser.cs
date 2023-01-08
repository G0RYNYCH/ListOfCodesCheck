namespace ListOfCodesCheck;

public class Parser
{
    public GroupModel[] ParseCheckLine(string checkLine) => checkLine.Split(',', ';').Select(MapToModel).ToArray();

    private GroupModel MapToModel(string parsedStr)
    {
        var strings = parsedStr.Split("-");

        var result = new GroupModel
        {
            GroupCode = strings[0].Trim()
        };

        if (strings.Length == 1)
        {
            result.IsLengthVariable = true;
            return result;
        }

        var groupLength = strings[1].Trim();

        if (groupLength.Contains('+'))
        {
            result.IsLengthVariable = true;
            groupLength = strings[1].Replace("+", "");
        }

        result.MinGroupLength = int.Parse(groupLength);

        return result;
    }
}
