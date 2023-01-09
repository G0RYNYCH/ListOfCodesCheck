namespace ListOfCodesCheck;

public class Validator
{
    const string VariableLengthCodeSeparator = "\\u001d";
    private readonly ValidationRule[] validationRules;

    public Validator(ValidationRule[] validationRules)
    {
        this.validationRules = validationRules;
    }

    /// <summary>
    /// Returns string error
    /// </summary>
    public string Validate(string groupsList)
    {
        var groupsListAsSpan = groupsList.AsSpan();
        ReadOnlySpan<char> code;
        string validationErrorMessage;

        for (var currentRuleNumber = 0; currentRuleNumber < validationRules.Length; currentRuleNumber++)
        {
            var rule = validationRules[currentRuleNumber];

            validationErrorMessage = GroupCheck(groupsList, groupsListAsSpan, rule);
            if (validationErrorMessage != null)
            {
                return validationErrorMessage;
            }

            if (rule.IsLengthVariable)
            {
                var indexOfSeparator = groupsListAsSpan.IndexOf(VariableLengthCodeSeparator);

                

                if (indexOfSeparator == -1)
                {
                    code = groupsListAsSpan.Slice(rule.GroupCode.Length);
                }
                else
                {
                    code = groupsListAsSpan.Slice(rule.GroupCode.Length, indexOfSeparator - rule.GroupCode.Length);
                }

                validationErrorMessage = VariableLengthCheck(groupsList, code, groupsListAsSpan, rule);
                if (validationErrorMessage != null)
                {
                    return validationErrorMessage;
                }

                if (code.Length == 0 || indexOfSeparator == -1 && currentRuleNumber == validationRules.Length - 1)
                {
                    groupsListAsSpan = groupsListAsSpan.Slice(rule.GroupCode.Length);
                }
                else
                {
                    groupsListAsSpan = groupsListAsSpan.Slice(indexOfSeparator + VariableLengthCodeSeparator.Length);
                }

                validationErrorMessage = VariableCodeContinuationCheck(currentRuleNumber, indexOfSeparator, groupsList, groupsListAsSpan);
                if (validationErrorMessage != null)
                {
                    return validationErrorMessage;
                }

                continue;
            }

            if (groupsListAsSpan.Length < rule.GroupCode.Length + rule.MinGroupLength)
            {
                code = groupsListAsSpan.Slice(rule.GroupCode.Length);
            }
            else
            {
                code = groupsListAsSpan.Slice(rule.GroupCode.Length, rule.MinGroupLength);
            }

            validationErrorMessage = FixedLengthCheck(groupsList, code, groupsListAsSpan, rule);
            if (validationErrorMessage != null)
            {
                return validationErrorMessage;
            }

            groupsListAsSpan = groupsListAsSpan.Slice(rule.GroupCode.Length + rule.MinGroupLength);

            validationErrorMessage = FixedCodeContinuationCheck(currentRuleNumber, groupsList, groupsListAsSpan, rule);
            if (validationErrorMessage != null)
            {
                return validationErrorMessage;
            }
        }

        return null;
    }

    private string GroupCheck(string groupsList, ReadOnlySpan<char> groupsListAsSpan, ValidationRule rule)
    {
        ReadOnlySpan<char> groupCode;

        if (groupsListAsSpan.Length < rule.GroupCode.Length)
        {
            groupCode = groupsListAsSpan.Slice(0);
        }
        else
        {
            groupCode = groupsListAsSpan.Slice(0, rule.GroupCode.Length);
        }

        if (!MemoryExtensions.Equals(rule.GroupCode, groupCode, StringComparison.Ordinal))
        {
            if (groupCode.IsEmpty)
            {
                return $"Ожидалась группа применения {rule.GroupCode}, найдена <конец кода>." +
                       $"Позиция {groupsList.Length + 1}.";
            }
            var errorPosition = groupsList.Length - groupsListAsSpan.Length + 1;

            return $"Ожидалась группа применения {rule.GroupCode}, найдена {groupCode}. " +
                   $"Позиция {errorPosition}.";
        }

        return null;
    }

    private string VariableLengthCheck(string groupsList, ReadOnlySpan<char> code, ReadOnlySpan<char> groupsListAsSpan, ValidationRule rule)
    {
        if (code.Length < rule.MinGroupLength)
        {
            var errorPosition = groupsList.Length - groupsListAsSpan.Length + 1;

            return $"Длина группы применения {rule.GroupCode} меньше указанной, ожидалось {rule.MinGroupLength}+, найдена {code.Length}." +
                   $"Позиция {errorPosition}.";
        }

        return null;
    }

    private string FixedLengthCheck(string groupsList, ReadOnlySpan<char> code, ReadOnlySpan<char> groupsListAsSpan, ValidationRule rule)
    {
        if (code.Length != rule.MinGroupLength)
        {
            var errorPosition = groupsList.Length - groupsListAsSpan.Length + 1;

            return $"Группа применения {rule.GroupCode} переменной длины, ожидалось фиксированной {rule.MinGroupLength}." +
                   $"Позиция {errorPosition}.";
        }

        return null;
    }

    private string VariableCodeContinuationCheck(int currentRuleNumber, int indexOfSeparator, string groupsList, ReadOnlySpan<char> groupsListAsSpan)
    {
        if (currentRuleNumber == validationRules.Length - 1 && indexOfSeparator != -1 && !groupsListAsSpan.IsEmpty)
        {
            var errorPosition = groupsList.Length - groupsListAsSpan.Length + 1;

            return $"Код продолжается после завершающей группы применения, найдено {groupsListAsSpan}." +
                   $"Позиция {errorPosition}.";
        }

        return null;
    }

    private string FixedCodeContinuationCheck(int currentRuleNumber, string groupsList, ReadOnlySpan<char> groupsListAsSpan, ValidationRule rule)
    {
        if (currentRuleNumber == validationRules.Length - 1 && !groupsListAsSpan.IsEmpty && groupsListAsSpan.Length > rule.MinGroupLength)
        {
            var errorPosition = groupsList.Length - groupsListAsSpan.Length + VariableLengthCodeSeparator.Length + 1;

            return $"Код продолжается после завершающей группы применения, найдено {groupsListAsSpan.Slice(rule.MinGroupLength)}." +
                   $"Позиция {errorPosition}.";
        }

        return null;
    }
}
