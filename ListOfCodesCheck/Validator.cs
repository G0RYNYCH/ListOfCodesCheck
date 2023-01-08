namespace ListOfCodesCheck;

/// <summary>
/// Returns string error
/// </summary>
public class Validator
{
    const string VariableLengthCodeSeparator = "\\u001d";
    private readonly GroupModel[] validationRules;

    public Validator(GroupModel[] validationRules)
    {
        this.validationRules = validationRules;
    }

    public string Validate(string str)
    {
        var span = str.AsSpan();

        for (var i = 0; i < validationRules.Length; i++)
        {
            var rule = validationRules[i];

            ReadOnlySpan<char> groupCode;

            if (span.Length < rule.GroupCode.Length)
            {
                groupCode = span.Slice(0);
            }
            else
            {
                groupCode = span.Slice(0, rule.GroupCode.Length);
            }

            //group check
            if (!MemoryExtensions.Equals(rule.GroupCode, groupCode, StringComparison.Ordinal))
            {
                var errorPosition = str.IndexOf(span.ToString()) + 1;
                return $"Ожидалась группа применения {rule.GroupCode}, найдена {groupCode}. Позиция {errorPosition}.";
            }

            //variable length group check
            if (rule.IsLengthVariable)
            {
                var indexOfSeparator = span.IndexOf(VariableLengthCodeSeparator);

                //separator check
                if (indexOfSeparator == -1 && i != validationRules.Length - 1)
                {
                    return $"Не указан символ-разделитель {VariableLengthCodeSeparator}.";
                }

                ReadOnlySpan<char> code;

                if (indexOfSeparator == -1)
                {
                    code = span.Slice(rule.GroupCode.Length);
                }
                else
                {
                    code = span.Slice(rule.GroupCode.Length, indexOfSeparator - rule.GroupCode.Length);
                }

                //length check of a variable length group
                if (code.Length < rule.MinGroupLength)
                {
                    var errorPosition = str.IndexOf(span.ToString()) + 1;
                    return $"Длина группы применения {rule.GroupCode} меньше указанной, ожидалось {rule.MinGroupLength}+, найдена {code.Length}. Позиция {errorPosition}.";
                }

                if (code.Length == 0)
                {
                    span = span.Slice(rule.GroupCode.Length);
                }
                else
                {
                    span = span.Slice(indexOfSeparator + VariableLengthCodeSeparator.Length);
                }
            }
            else
            {
                ReadOnlySpan<char> code;

                if (span.Length < rule.GroupCode.Length + rule.MinGroupLength)
                {
                    code = span.Slice(rule.GroupCode.Length);
                }
                else
                {
                    code = span.Slice(rule.GroupCode.Length, rule.MinGroupLength);
                }

                //fixed length group check
                if (code.Length != rule.MinGroupLength)
                {
                    var errorPosition = str.IndexOf(span.ToString()) + 1;
                    return $"Группа применения {rule.GroupCode} переменной длины, ожидалось фиксированной {rule.MinGroupLength}. Позиция {errorPosition}.";
                }

                span = span.Slice(rule.GroupCode.Length + rule.MinGroupLength);
            }
        }

        return null;
    }
}
