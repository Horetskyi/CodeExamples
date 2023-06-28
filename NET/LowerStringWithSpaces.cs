using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using Abstract.Helpful.Lib;
using Abstract.Helpful.Lib.Utils;

namespace Domain.Types.DomainProcesses._PageCrawling.Types;

public readonly struct LowerStringWithSpaces : IEquatable<LowerStringWithSpaces>
{
    private const char SPACE = ' ';
    private static readonly HashSet<char> punctuationsHashSet = "\n\t~`!@:;\"\\$%^&*()-=+_|/?#№[]{}.,——"
        .ToCharArray()
        .ToHashSet();
        
    public string Value { get; }

    public LowerStringWithSpaces(string value, bool isAllowMultipleSpacesInRow = false)
    {
        if (string.IsNullOrEmpty(value))
        {
            Value = string.Empty;
            return;
        }

        Value = value
            .ToLowerInvariant()
            .ReplaceChars(punctuationsHashSet, SPACE)
            .ReplaceMultiSpacesIntoSingleOne(isAllowMultipleSpacesInRow)
            .TrimEnd();
    }

    [Pure]
    [Safe]
    public int WordsCount()
    {
        if (Value.IsNullOrEmpty())
            return default;
            
        return Value.Split(SPACE).Length;
    }

    [Pure]
    [Safe]
    public List<LowerStringWithSpaces> GetWords()
    {
        return GetWords()
            .Select(word => word.ToLowerStringWithSpaces())
            .Where(word => !word.IsNullOrWhiteSpace())
            .ToList();
    }
    
    [Pure]
    [Safe]
    public List<string> GetWords(HashSet<string> wordsToExclude = default)
    {
        if (Value.IsNullOrWhiteSpace())
            return new List<string>();
        
        var inProgress =  Value
            .Split(SPACE, StringSplitOptions.RemoveEmptyEntries);
            
        if (wordsToExclude == null || wordsToExclude.Count == 0)
            return inProgress.ToList();

        return inProgress
            .Where(word => !wordsToExclude.Contains(word))
            .ToList();
    }

    [Pure]
    [Safe]
    public bool IsNumber()
    {
        return Value.IsNullOrEmpty() || Value.All(char.IsNumber);
    }

    [Pure]
    [Safe]
    public override string ToString()
    {
        return Value.ToStringSafe();
    }

    #region Equals

    public bool Equals(LowerStringWithSpaces other)
    {
        return string.Equals(Value, other.Value, StringComparison.InvariantCultureIgnoreCase);
    }

    public override bool Equals(object obj)
    {
        return obj is LowerStringWithSpaces other && Equals(other);
    }

    public override int GetHashCode()
    {
        return (Value != null ? StringComparer.InvariantCultureIgnoreCase.GetHashCode(Value) : 0);
    }

    public static bool operator ==(LowerStringWithSpaces left, LowerStringWithSpaces right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(LowerStringWithSpaces left, LowerStringWithSpaces right)
    {
        return !left.Equals(right);
    }

    #endregion
}