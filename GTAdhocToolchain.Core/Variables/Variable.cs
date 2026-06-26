// Copyright (c) 2026 Nenkai
// SPDX-License-Identifier: MIT

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Esprima;

namespace GTAdhocToolchain.Core.Variables;

public class Variable
{
    public int StackIndex { get; set; } = -1;
    public AdhocSymbol Symbol { get; set; }
    public AdhocVariableType Type { get; set; } = AdhocVariableType.Unknown;
    public Location? IdLocation { get; set; }
    public Location? BodyLocation { get; set; }

    public string? DeclarationSourceFileName { get; set; }

    public List<Variable> Children { get; set; } = [];

    public Variable(AdhocSymbol symbol, AdhocVariableType variableType, int stackIndex, Location? idLocation = null, Location? bodyLocation = null)
    {
        Symbol = symbol;
        Type = variableType;
        StackIndex = stackIndex;

        if (idLocation is not null)
        {
            IdLocation = idLocation;
        }

        if (bodyLocation is not null)
        {
            BodyLocation = bodyLocation;
        }
    }

    public Variable(AdhocSymbol symbol, AdhocVariableType variableType, int stackIndex, Location? sourceLineNumber, string? sourceFileName)
    {
        Symbol = symbol;
        Type = variableType;
        StackIndex = stackIndex;
        IdLocation = sourceLineNumber;
        DeclarationSourceFileName = sourceFileName;
    }

    public override string ToString()
    {
        return $"Variable: {Symbol}";
    }
}
