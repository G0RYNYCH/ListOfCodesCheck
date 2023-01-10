namespace ListOfCodesCheck;

public class Validator
{
    const string VariableLengthCodeSeparator = "\\u001d";

    private readonly ValidationRule[] _validationRules;

    public Validator(ValidationRule[] validationRules)
    {
        _validationRules = validationRules;
    }

    /// <summary>
    /// Validates a specified list of groups.
    /// </summary>
    /// <param name="groupsList">Line of groups from argument file.</param>
    /// <returns>Null if validation is OK otherwise error message</returns>
    public string Validate(string groupsList)
    {
        var groupsListAsSpan = groupsList.AsSpan();
        
        for (var currentRuleNumber = 0; currentRuleNumber < _validationRules.Length; currentRuleNumber++)
        {
            var rule = _validationRules[currentRuleNumber];

            var errorMessage = ValidateGroup(groupsList, groupsListAsSpan, rule);
            if (errorMessage != null)
            {
                return errorMessage;
            }

            ReadOnlySpan<char> code;

            if (rule.IsLengthVariable)
            {
                var indexOfSeparator = groupsListAsSpan.IndexOf(VariableLengthCodeSeparator);

                code = indexOfSeparator == -1
                    ? groupsListAsSpan.Slice(rule.GroupCode.Length)
                    : groupsListAsSpan.Slice(rule.GroupCode.Length, indexOfSeparator - rule.GroupCode.Length);

                errorMessage = ValidateVariableLength(groupsList, code, groupsListAsSpan, rule);
                if (errorMessage != null)
                {
                    return errorMessage;
                }

                groupsListAsSpan = code.Length == 0 || indexOfSeparator == -1 && currentRuleNumber == _validationRules.Length - 1
                    ? groupsListAsSpan.Slice(rule.GroupCode.Length)
                    : groupsListAsSpan.Slice(indexOfSeparator + VariableLengthCodeSeparator.Length);

                errorMessage = ValidateVariableCodeContinuation(currentRuleNumber, indexOfSeparator, groupsList, groupsListAsSpan);
                if (errorMessage != null)
                {
                    return errorMessage;
                }

                continue;
            }

            code = groupsListAsSpan.Length < rule.GroupCode.Length + rule.MinGroupLength
                ? groupsListAsSpan.Slice(rule.GroupCode.Length)
                : groupsListAsSpan.Slice(rule.GroupCode.Length, rule.MinGroupLength);

            errorMessage = ValidateFixedLength(groupsList, code, groupsListAsSpan, rule);
            if (errorMessage != null)
            {
                return errorMessage;
            }

            groupsListAsSpan = groupsListAsSpan.Slice(rule.GroupCode.Length + rule.MinGroupLength);

            errorMessage = ValidateFixedCodeContinuation(currentRuleNumber, groupsList, groupsListAsSpan);
            if (errorMessage != null)
            {
                return errorMessage;
            }
        }

        return null;
    }

    /// <summary>
    /// Checks whether the validation group consists in the line of groups.
    /// </summary>
    /// <param name="groupsList">Line of groups from argument file.</param>
    /// <param name="groupsListAsSpan">Line of groups converted to span.</param>
    /// <param name="rule">Current rule for validating.</param>
    /// <returns>Null if validation is OK otherwise error message with expecting group.</returns>
    private string ValidateGroup(string groupsList, ReadOnlySpan<char> groupsListAsSpan, ValidationRule rule)
    {
        var groupCode = groupsListAsSpan.Length < rule.GroupCode.Length
            ? groupsListAsSpan.Slice(0)
            : groupsListAsSpan.Slice(0, rule.GroupCode.Length);

        if (!MemoryExtensions.Equals(rule.GroupCode, groupCode, StringComparison.Ordinal))
        {
            if (groupCode.IsEmpty)
            {
                return $"Ожидалась группа применения {rule.GroupCode}, найдена <конец кода>. " +
                       $"Позиция {groupsList.Length + 1}.";
            }
            var errorPosition = groupsList.Length - groupsListAsSpan.Length + 1;

            return $"Ожидалась группа применения {rule.GroupCode}, найдена {groupCode}. " +
                   $"Позиция {errorPosition}.";
        }

        return null;
    }

    /// <summary>
    /// Validates the length of variable length group.
    /// </summary>
    /// <param name="groupsList">Line of groups from argument file.</param>
    /// <param name="code">Code that immediately follows the group code.</param>
    /// <param name="groupsListAsSpan">Line of groups converted to span.</param>
    /// <param name="rule">Current rule for validating.</param>
    /// <returns>Null if validation is OK otherwise error message with expecting length.</returns>
    private string ValidateVariableLength(string groupsList, ReadOnlySpan<char> code, ReadOnlySpan<char> groupsListAsSpan, ValidationRule rule)
    {
        if (code.Length < rule.MinGroupLength)
        {
            var errorPosition = groupsList.Length - groupsListAsSpan.Length + 1;

            return $"Длина группы применения {rule.GroupCode} меньше указанной, ожидалось {rule.MinGroupLength}+, найдена {code.Length}. " +
                   $"Позиция {errorPosition}.";
        }

        return null;
    }

    /// <summary>
    /// Validates the length of fixed length group.
    /// </summary>
    /// <param name="groupsList">Line of groups from argument file.</param>
    /// <param name="code">Code that immediately follows the group code.</param>
    /// <param name="groupsListAsSpan">Line of groups converted to span.</param>
    /// <param name="rule">Current rule for validating.</param>
    /// <returns>Null if validation is OK otherwise error message with expecting length.</returns>
    private string ValidateFixedLength(string groupsList, ReadOnlySpan<char> code, ReadOnlySpan<char> groupsListAsSpan, ValidationRule rule)
    {
        if (code.Length != rule.MinGroupLength)
        {
            var errorPosition = groupsList.Length - groupsListAsSpan.Length + 1;

            return $"Группа применения {rule.GroupCode} переменной длины, ожидалось фиксированной {rule.MinGroupLength}. " +
                   $"Позиция {errorPosition}.";
        }

        return null;
    }

    /// <summary>
    /// Checks whether the variable code continues after its expected completion.
    /// </summary>
    /// <param name="currentRuleNumber">Number of current validation rule in a loop.</param>
    /// <param name="indexOfSeparator">Index of separator character in a line of groups that was converted to span preliminarily.</param>
    /// <param name="groupsList">Line of groups from argument file.</param>
    /// <param name="groupsListAsSpan">Line of groups converted to span.</param>
    /// <returns>Null if validation is OK otherwise error message with unexpected part of the code.</returns>
    private string ValidateVariableCodeContinuation(int currentRuleNumber, int indexOfSeparator, string groupsList, ReadOnlySpan<char> groupsListAsSpan)
    {
        if (currentRuleNumber == _validationRules.Length - 1 && indexOfSeparator != -1 && !groupsListAsSpan.IsEmpty)
        {
            var errorPosition = groupsList.Length - groupsListAsSpan.Length + 1;

            return $"Код продолжается после завершающей группы применения, найдено {groupsListAsSpan}. " +
                   $"Позиция {errorPosition}.";
        }

        return null;
    }

    /// <summary>
    /// Checks whether the fixed code continues after its expected completion.
    /// </summary>
    /// <param name="currentRuleNumber">Number of current validation rule in a loop.</param>
    /// <param name="groupsList">Line of groups from argument file.</param>
    /// <param name="groupsListAsSpan">Line of groups converted to span.</param>
    /// <param name="rule">Current rule for validating.</param>
    /// <returns>Null if validation is OK otherwise error message with unexpected part of the code.</returns>
    private string ValidateFixedCodeContinuation(int currentRuleNumber, string groupsList, ReadOnlySpan<char> groupsListAsSpan)
    {
        if (currentRuleNumber == _validationRules.Length - 1 && !groupsListAsSpan.IsEmpty)
        {
            var errorPosition = groupsList.Length - groupsListAsSpan.Length + 1;

            return $"Код продолжается после завершающей группы применения, найдено {groupsListAsSpan}. " +
                   $"Позиция {errorPosition}.";
        }

        return null;
    }
}
