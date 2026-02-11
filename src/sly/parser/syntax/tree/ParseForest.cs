using System;
using System.Collections.Generic;
using System.Linq;
using sly.parser.syntax.grammar;

namespace sly.parser.syntax.tree
{
    /// <summary>
    /// Représente une forêt d'arbres syntaxiques issus d'une grammaire ambiguë
    /// </summary>
    public class ParseForest<IN, OUT> where IN : struct, Enum
    {
        /// <summary>
        /// Liste des arbres syntaxiques valides (alternatives)
        /// </summary>
        public List<ISyntaxNode<IN, OUT>> Trees { get; set; }
        
        /// <summary>
        /// Indique si la forêt contient des ambiguïtés
        /// </summary>
        public bool IsAmbiguous => Trees != null && Trees.Count > 1;
        
        /// <summary>
        /// Nombre d'arbres dans la forêt
        /// </summary>
        public int Count => Trees?.Count ?? 0;
        
        /// <summary>
        /// Arbre principal (premier de la liste pour compatibilité)
        /// </summary>
        public ISyntaxNode<IN, OUT> MainTree => Trees?.FirstOrDefault();
        
        /// <summary>
        /// Informations sur les points d'ambiguïté dans la forêt
        /// </summary>
        public List<AmbiguityInfo<IN, OUT>> Ambiguities { get; set; }
    }
    
    /// <summary>
    /// Informations sur un point d'ambiguïté spécifique
    /// </summary>
    public class AmbiguityInfo<IN, OUT> where IN : struct, Enum
    {
        public string NonTerminalName { get; set; }
        public int Position { get; set; }
        public List<Rule<IN, OUT>> AlternativeRules { get; set; }
        public int AlternativeCount { get; set; }
    }
}
