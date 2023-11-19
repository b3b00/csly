using System;
using BravoLights.Common.Ast;

namespace BravoLights.Common
{
    public interface IVariable : IAstNode, IEquatable<IVariable>
    {
        /// <summary>
        /// Gets the identifier for this variable.  For an lvar this would be the simple name; for a sim var, it would be the name plus units.
        /// </summary>
        string Identifier { get; }
    }
}
