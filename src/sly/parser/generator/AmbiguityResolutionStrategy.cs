using System;

namespace sly.parser.generator
{
    /// <summary>
    /// Stratégie de résolution d'ambiguïté
    /// </summary>
    public enum AmbiguityResolutionStrategy
    {
        /// <summary>Returns first derivation default)</summary>
        First,
        
        /// <summary>Returns all derivations (in a forest)</summary>
        All,
        
        /// <summary>THrows an exception if ambiguity is detected</summary>
        ThrowException,
        
        /// <summary>Returns longest derivation</summary>
        Longest
    }
}
