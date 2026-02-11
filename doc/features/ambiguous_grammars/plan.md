# Plan d'implémentation : Support des grammaires ambiguës dans CSLY

## Contexte

Actuellement, CSLY ne gère que des grammaires non ambiguës. Lorsqu'une grammaire ambiguë est parsée, le parseur résout l'ambiguïté en retournant la première dérivation (celle qui consomme le plus de tokens). L'objectif est de permettre au parseur de retourner une **forêt d'analyse** (parse forest) au lieu d'un arbre unique, laissant à l'utilisateur la responsabilité de résoudre les ambiguïtés.

## Architecture actuelle

### Structures de données principales

1. **`ParseResult<IN, OUT>`** : Résultat final du parsing
   - Contient un seul `ISyntaxNode<IN, OUT> SyntaxTree`
   - Contient un résultat `OUT Result` (après visite du tree)
   - Gère les erreurs via `List<ParseError> Errors`

2. **`SyntaxParseResult<IN, OUT>`** : Résultat intermédiaire du parsing syntaxique
   - Contient un seul `ISyntaxNode<IN, OUT> Root`
   - Position de fin, erreurs, etc.

3. **`ISyntaxNode<IN, OUT>`** : Interface pour les nœuds d'arbre syntaxique
   - Implémenté par `SyntaxLeaf<IN, OUT>` (terminaux)
   - Implémenté par `SyntaxNode<IN, OUT>` (non-terminaux)

4. **`RecursiveDescentSyntaxParser<IN, OUT>`** : Parseur récursif descendant
   - Méthode `ParseNonTerminal()` : **POINT CLÉ** - collecte plusieurs résultats de règles alternatives dans `rulesResults`, mais ne retourne que le meilleur (`maxOk` ou `maxKo`)

### Point d'ambiguïté critique

Dans `RecursiveDescentSyntaxParser.NonTerminal.cs`, ligne ~37-120 :
```csharp
var rulesResults = new List<SyntaxParseResult<IN, OUT>>();
// ... collecte tous les résultats de règles alternatives ...
// PUIS sélectionne seulement le meilleur :
if (hasOk) { max = maxOk; } else { max = maxKo; }
result.Root = max.Root; // ← UN SEUL arbre retourné
```

**C'est ici que l'ambiguïté est résolue arbitrairement.**

---

## Plan d'implémentation

### Phase 1 : Modification des structures de données (Fondations)

#### 1.1 Créer une nouvelle structure `ParseForest<IN, OUT>`

**Objectif** : Représenter une forêt d'arbres syntaxiques avec méta-informations sur les ambiguïtés.

**Fichier** : `src/sly/parser/syntax/tree/ParseForest.cs` (nouveau)

```csharp
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
```

#### 1.2 Étendre `SyntaxParseResult<IN, OUT>`

**Objectif** : Permettre de stocker plusieurs arbres alternatifs au niveau du parsing syntaxique.

**Fichier** : `src/sly/parser/parser/SyntaxParseResult.cs`

```csharp
public class SyntaxParseResult<IN, OUT> where IN : struct, Enum
{
    // ... propriétés existantes ...
    
    /// <summary>
    /// Liste des racines alternatives (pour grammaires ambiguës)
    /// Si null ou 1 élément, pas d'ambiguïté
    /// </summary>
    public List<ISyntaxNode<IN, OUT>> AlternativeRoots { get; set; }
    
    /// <summary>
    /// Indique si ce résultat contient des alternatives ambiguës
    /// </summary>
    public bool HasAmbiguity => AlternativeRoots != null && AlternativeRoots.Count > 1;
    
    /// <summary>
    /// Compatibilité : Root pointe vers la première alternative
    /// </summary>
    public ISyntaxNode<IN, OUT> Root 
    { 
        get => AlternativeRoots?.FirstOrDefault(); 
        set 
        {
            if (AlternativeRoots == null)
                AlternativeRoots = new List<ISyntaxNode<IN, OUT>>();
            if (AlternativeRoots.Count == 0)
                AlternativeRoots.Add(value);
            else
                AlternativeRoots[0] = value;
        }
    }
}
```

#### 1.3 Étendre `ParseResult<IN, OUT>`

**Objectif** : Exposer la forêt d'analyse dans le résultat final.

**Fichier** : `src/sly/parser/parser/ParseResult.cs`

```csharp
public class ParseResult<IN, OUT> where IN : struct, Enum
{
    // ... propriétés existantes ...
    
    /// <summary>
    /// Forêt d'arbres syntaxiques (pour grammaires ambiguës)
    /// </summary>
    public ParseForest<IN, OUT> Forest { get; set; }
    
    /// <summary>
    /// Compatibilité : SyntaxTree pointe vers l'arbre principal de la forêt
    /// </summary>
    public ISyntaxNode<IN, OUT> SyntaxTree 
    { 
        get => Forest?.MainTree; 
        set 
        {
            if (Forest == null)
                Forest = new ParseForest<IN, OUT> { Trees = new List<ISyntaxNode<IN, OUT>>() };
            if (Forest.Trees.Count == 0)
                Forest.Trees.Add(value);
            else
                Forest.Trees[0] = value;
        }
    }
    
    /// <summary>
    /// Indique si le parsing a produit des ambiguïtés
    /// </summary>
    public bool IsAmbiguous => Forest?.IsAmbiguous ?? false;
}
```

---

### Phase 2 : Modification du parseur (Capture des alternatives)

#### 2.1 Ajouter une option de configuration

**Objectif** : Permettre d'activer/désactiver la détection d'ambiguïtés.

**Fichier** : `src/sly/parser/ParserConfiguration.cs`

```csharp
public class ParserConfiguration<IN, OUT> where IN : struct, Enum
{
    // ... propriétés existantes ...
    
    /// <summary>
    /// Si true, le parseur retourne toutes les dérivations possibles en cas d'ambiguïté
    /// Si false, retourne la première dérivation (comportement actuel)
    /// </summary>
    public bool CaptureAmbiguities { get; set; } = false;
    
    /// <summary>
    /// Stratégie de résolution d'ambiguïté par défaut
    /// </summary>
    public AmbiguityResolutionStrategy AmbiguityStrategy { get; set; } 
        = AmbiguityResolutionStrategy.First;
}

public enum AmbiguityResolutionStrategy
{
    /// <summary>Retourne la première dérivation (comportement actuel)</summary>
    First,
    /// <summary>Retourne toutes les dérivations</summary>
    All,
    /// <summary>Lance une exception si ambiguïté détectée</summary>
    ThrowException,
    /// <summary>Retourne la dérivation la plus longue</summary>
    Longest
}
```

#### 2.2 Modifier `ParseNonTerminal()` pour capturer les ambiguïtés (Parseur BNF)

**Objectif** : Au lieu de sélectionner un seul arbre, conserver tous les arbres valides.

**Fichier** : `src/sly/parser/parser/llparser/bnf/RecursiveDescentSyntaxParser.NonTerminal.cs`

```csharp
public SyntaxParseResult<IN, OUT> ParseNonTerminal(Token<IN>[] tokens, string nonTerminalName,
    int currentPosition, SyntaxParsingContext<IN, OUT> parsingContext)
{
    // ... code existant jusqu'à la collecte de rulesResults ...
    
    var rulesResults = new List<SyntaxParseResult<IN, OUT>>();
    // ... collecte des résultats ...
    
    // NOUVEAU CODE : Gestion des ambiguïtés
    var result = new SyntaxParseResult<IN, OUT>();
    
    if (Configuration.CaptureAmbiguities)
    {
        // Filtrer les résultats OK avec la même longueur maximale
        var okResults = rulesResults.Where(r => r.IsOk).ToList();
        
        if (okResults.Any())
        {
            var maxLength = okResults.Max(r => r.EndingPosition);
            var maxResults = okResults.Where(r => r.EndingPosition == maxLength).ToList();
            
            if (maxResults.Count > 1)
            {
                // AMBIGUÏTÉ DÉTECTÉE
                result.AlternativeRoots = maxResults.Select(r => r.Root).ToList();
                result.EndingPosition = maxLength;
                result.IsError = false;
                
                // Enregistrer l'ambiguïté
                if (result.Ambiguities == null)
                    result.Ambiguities = new List<AmbiguityInfo<IN, OUT>>();
                result.Ambiguities.Add(new AmbiguityInfo<IN, OUT>
                {
                    NonTerminalName = nonTerminalName,
                    Position = currentPosition,
                    AlternativeCount = maxResults.Count
                });
            }
            else
            {
                // Pas d'ambiguïté, un seul résultat optimal
                result.Root = maxResults[0].Root;
                result.EndingPosition = maxResults[0].EndingPosition;
                result.IsError = false;
            }
        }
        else
        {
            // Tous les résultats sont en erreur
            max = maxKo;
            result.Root = max.Root;
            result.EndingPosition = max.EndingPosition;
            result.IsError = true;
        }
    }
    else
    {
        // COMPORTEMENT ACTUEL (rétrocompatibilité)
        if (hasOk) { max = maxOk; } else { max = maxKo; }
        result.Root = max.Root;
        result.EndingPosition = max.EndingPosition;
        result.IsError = max.IsError;
    }
    
    // ... reste du code existant ...
}
```

#### 2.2bis Modifier `ParseChoice()` pour capturer les ambiguïtés (Parseur EBNF)

**⚠️ POINT CRITIQUE IDENTIFIÉ** : Le parseur EBNF a sa propre gestion des choix via `ParseChoice()` qui doit également être modifiée.

**Objectif** : Dans les clauses EBNF avec alternatives (`A | B | C`), capturer toutes les alternatives valides au lieu de retourner la première qui réussit.

**Fichier** : `src/sly/parser/parser/llparser/ebnf/EBNFRecursiveDescentSyntaxParser.Choice.cs`

**Code actuel** (ligne 37-60) :
```csharp
foreach (var alternate in clause.Choices)
{
    // ... parse chaque alternative ...
    result = alternateResults.Last();
    if (result.IsOk)
    {
        // STOP dès la première alternative qui réussit
        parsingContext.Memoize(clause, position, result);
        return result;  // ← UN SEUL résultat retourné
    }
}
```

**Nouveau code** :
```csharp
public SyntaxParseResult<IN, OUT> ParseChoice(Token<IN>[] tokens, ChoiceClause<IN, OUT> clause,
    int position, SyntaxParsingContext<IN, OUT> parsingContext)
{
    // ... code memoization existant ...
    
    List<SyntaxParseResult<IN, OUT>> alternateResults = new List<SyntaxParseResult<IN, OUT>>();
    
    // Collecter TOUS les résultats (pas de early return)
    foreach (var alternate in clause.Choices)
    {
        switch (alternate)
        {
            case TerminalClause<IN, OUT> terminalAlternate:
                var rterm = ParseTerminal(tokens, terminalAlternate, currentPosition, parsingContext);
                alternateResults.Add(rterm);
                break;
            case NonTerminalClause<IN, OUT> nonTerminalAlternate:
                var rnonterm = ParseNonTerminal(tokens, nonTerminalAlternate, currentPosition, parsingContext);
                alternateResults.Add(rnonterm);
                break;
        }
        
        // NOUVEAU : Ne pas s'arrêter à la première alternative qui réussit si CaptureAmbiguities
        if (!Configuration.CaptureAmbiguities && alternateResults.Last().IsOk)
        {
            // Comportement actuel : retourner la première alternative OK
            result = alternateResults.Last();
            if (clause.IsTerminalChoice && clause.IsDiscarded && result.Root is SyntaxLeaf<IN, OUT> leaf)
            {
                var discardedToken = new SyntaxLeaf<IN, OUT>(leaf.Token, true);
                result.Root = discardedToken;
            }
            parsingContext.Memoize(clause, position, result);
            return result;
        }
    }
    
    // NOUVEAU : Gestion des ambiguïtés
    var result = new SyntaxParseResult<IN, OUT>();
    
    if (Configuration.CaptureAmbiguities)
    {
        var okResults = alternateResults.Where(r => r.IsOk).ToList();
        
        if (okResults.Any())
        {
            // Toutes les alternatives réussies à la même position finale
            var maxLength = okResults.Max(r => r.EndingPosition);
            var maxResults = okResults.Where(r => r.EndingPosition == maxLength).ToList();
            
            if (maxResults.Count > 1)
            {
                // AMBIGUÏTÉ dans le choix EBNF
                result.AlternativeRoots = maxResults.Select(r => r.Root).ToList();
                result.EndingPosition = maxLength;
                result.IsError = false;
                
                // Enregistrer l'ambiguïté
                if (result.Ambiguities == null)
                    result.Ambiguities = new List<AmbiguityInfo<IN, OUT>>();
                result.Ambiguities.Add(new AmbiguityInfo<IN, OUT>
                {
                    NonTerminalName = $"Choice[{string.Join("|", clause.Choices.Select(c => c.ToString()))}]",
                    Position = position,
                    AlternativeCount = maxResults.Count
                });
            }
            else
            {
                result = maxResults[0];
            }
            
            // Appliquer les transformations (discard, etc.)
            if (clause.IsTerminalChoice && clause.IsDiscarded && result.Root is SyntaxLeaf<IN, OUT> leaf)
            {
                var discardedToken = new SyntaxLeaf<IN, OUT>(leaf.Token, true);
                result.Root = discardedToken;
            }
        }
        else
        {
            // Toutes les alternatives ont échoué
            result = HandleAllChoicesFailed(clause, tokens, currentPosition, alternateResults);
        }
    }
    else
    {
        // Si on arrive ici, toutes les alternatives ont échoué (comportement actuel)
        result = HandleAllChoicesFailed(clause, tokens, currentPosition, alternateResults);
    }
    
    parsingContext.Memoize(clause, position, result);
    return result;
}

private SyntaxParseResult<IN, OUT> HandleAllChoicesFailed(
    ChoiceClause<IN, OUT> clause, 
    Token<IN>[] tokens, 
    int currentPosition, 
    List<SyntaxParseResult<IN, OUT>> alternateResults)
{
    var result = new SyntaxParseResult<IN, OUT>
    {
        IsError = true,
        EndingPosition = currentPosition
    };
    
    if (clause.IsTerminalChoice)
    {
        var terminalAlternates = clause.Choices.Cast<TerminalClause<IN, OUT>>();
        var expected = terminalAlternates.Select(x => x.ExpectedToken).ToList();
        result.AddError(new UnexpectedTokenSyntaxError<IN>(tokens[currentPosition], LexemeLabels, I18n,
            expected.ToArray()));
    }
    else
    {
        var greaterPosition = alternateResults.Select(x => x.EndingPosition).Max();
        var errors = alternateResults.Where(x => x.EndingPosition == greaterPosition)
            .SelectMany(x => x.GetErrors()).ToList();
        result.AddErrors(errors);
    }
    
    return result;
}
```

#### 2.3 Propager les ambiguïtés dans la chaîne de parsing

**Objectif** : S'assurer que les ambiguïtés remontent jusqu'au `ParseResult` final.

**Fichier** : `src/sly/parser/parser/Parser.cs`

Dans la méthode `ParseWithContext()` :

```csharp
public ParseResult<IN, OUT> ParseWithContext(IList<Token<IN>> tokens, object parsingContext = null, 
    string startingNonTerminal = null)
{
    var result = new ParseResult<IN, OUT>();
    var syntaxResult = SyntaxParser.Parse(tokens.ToArray(), startingNonTerminal);
    
    if (!syntaxResult.IsError && syntaxResult.Root != null)
    {
        // NOUVEAU : Gérer la forêt d'arbres
        if (syntaxResult.HasAmbiguity)
        {
            result.Forest = new ParseForest<IN, OUT>
            {
                Trees = syntaxResult.AlternativeRoots,
                Ambiguities = syntaxResult.Ambiguities
            };
            
            // Visiter tous les arbres ou appliquer la stratégie
            switch (Configuration.AmbiguityStrategy)
            {
                case AmbiguityResolutionStrategy.First:
                    result.Result = Visitor.VisitSyntaxTree(result.Forest.MainTree, parsingContext);
                    break;
                    
                case AmbiguityResolutionStrategy.ThrowException:
                    throw new AmbiguousGrammarException<IN, OUT>(result.Forest);
                    
                case AmbiguityResolutionStrategy.All:
                    // Ne pas visiter automatiquement, laisser l'utilisateur le faire
                    result.Result = default(OUT);
                    break;
            }
        }
        else
        {
            // Pas d'ambiguïté, comportement normal
            result.SyntaxTree = syntaxResult.Root;
            result.Result = Visitor.VisitSyntaxTree(syntaxResult.Root, parsingContext);
        }
        
        result.IsError = false;
    }
    else
    {
        // ... gestion des erreurs existante ...
    }
    
    return result;
}
```

#### 2.4 Propager les ambiguïtés dans les structures EBNF imbriquées

**Objectif** : S'assurer que les ambiguïtés détectées dans `ParseChoice()` remontent correctement à travers les structures EBNF (options, répétitions, etc.).

**Fichiers concernés** :
- `src/sly/parser/parser/llparser/ebnf/EBNFRecursiveDescentSyntaxParser.Option.cs`
- `src/sly/parser/parser/llparser/ebnf/EBNFRecursiveDescentSyntaxParser.Many.cs`
- `src/sly/parser/parser/llparser/ebnf/EBNFRecursiveDescentSyntaxParser.cs` (méthode `Parse()` principale)

**Modifications nécessaires** :

1. **Dans `ParseOption()`** : Si l'option contient un choix ambigu, propager les alternatives
```csharp
case ChoiceClause<IN, OUT> choice:
    innerResult = ParseChoice(tokens, choice, currentPosition, parsingContext);
    
    // NOUVEAU : Propager les ambiguïtés
    if (innerResult.HasAmbiguity)
    {
        // L'option contient plusieurs alternatives ambiguës
        // Créer des OptionSyntaxNode pour chaque alternative
        result.AlternativeRoots = innerResult.AlternativeRoots
            .Select(altRoot => new OptionSyntaxNode<IN, OUT>(
                rule.NonTerminalName, 
                new List<ISyntaxNode<IN, OUT>> { altRoot },
                rule.GetVisitorMethod()) as ISyntaxNode<IN, OUT>)
            .ToList();
        result.EndingPosition = innerResult.EndingPosition;
        result.Ambiguities = innerResult.Ambiguities;
    }
    break;
```

2. **Dans `Parse()` principale (EBNF)** : Propager les ambiguïtés des clauses vers le nœud parent
```csharp
case ChoiceClause<IN, OUT> choice:
{
    var choiceResult = ParseChoice(tokens, choice, currentPosition, parsingContext);
    currentPosition = choiceResult.EndingPosition;
    
    // NOUVEAU : Si le choix est ambigu, propager à ce niveau
    if (choiceResult.HasAmbiguity)
    {
        // On doit créer plusieurs arbres alternatifs pour cette règle
        // en variant seulement l'enfant correspondant au choix ambigu
        // (Plus complexe - voir implémentation détaillée ci-dessous)
    }
    
    if (choiceResult.IsError && choiceResult.GetErrors() != null && choiceResult.GetErrors().Any())
    {
        errors.AddRange(choiceResult.GetErrors());
    }
    isError = choiceResult.IsError;
    children.Add(choiceResult.Root);
    break;
}
```

**Note importante** : La propagation des ambiguïtés à travers les clauses EBNF imbriquées est complexe car une règle peut contenir plusieurs clauses, et si l'une d'elles est ambiguë, il faut créer plusieurs versions du nœud parent avec les mêmes enfants sauf celui qui varie.

**Solution simplifiée (Phase 1)** : 
- Détecter et capturer les ambiguïtés au niveau des choix (`ParseChoice`) et des non-terminaux (`ParseNonTerminal`)
- Les propager directement au résultat final
- Laisser la combinaison d'ambiguïtés imbriquées pour une phase ultérieure (explosion combinatoire potentielle)

**Solution complète (Phase future)** :
- Implémenter une logique de combinaison cartésienne pour gérer les ambiguïtés imbriquées
- Exemple : si une règle `A -> B C D` où B et D sont ambigus (2 alternatives chacun), créer 4 arbres (2×2)

---

### Phase 3 : API utilisateur pour la résolution d'ambiguïtés

#### 3.1 Créer une exception `AmbiguousGrammarException`

**Fichier** : `src/sly/parser/exceptions/AmbiguousGrammarException.cs` (nouveau)

```csharp
namespace sly.parser.exceptions
{
    public class AmbiguousGrammarException<IN, OUT> : Exception where IN : struct, Enum
    {
        public ParseForest<IN, OUT> Forest { get; }
        
        public AmbiguousGrammarException(ParseForest<IN, OUT> forest)
            : base($"Ambiguous grammar detected: {forest.Count} alternative parse trees found")
        {
            Forest = forest;
        }
        
        public override string Message
        {
            get
            {
                var sb = new StringBuilder(base.Message);
                sb.AppendLine();
                sb.AppendLine("Ambiguity points:");
                foreach (var amb in Forest.Ambiguities)
                {
                    sb.AppendLine($"  - NonTerminal '{amb.NonTerminalName}' at position {amb.Position}: {amb.AlternativeCount} alternatives");
                }
                return sb.ToString();
            }
        }
    }
}
```

#### 3.2 Créer des méthodes utilitaires pour la résolution manuelle

**Fichier** : `src/sly/parser/ParseResult.cs` (extension)

```csharp
public class ParseResult<IN, OUT> where IN : struct, Enum
{
    // ... propriétés existantes ...
    
    /// <summary>
    /// Visite tous les arbres de la forêt et retourne les résultats
    /// </summary>
    public List<OUT> VisitAllTrees(SyntaxTreeVisitor<IN, OUT> visitor, object context = null)
    {
        if (!IsAmbiguous)
            return new List<OUT> { Result };
            
        return Forest.Trees.Select(tree => visitor.VisitSyntaxTree(tree, context)).ToList();
    }
    
    /// <summary>
    /// Permet à l'utilisateur de sélectionner un arbre spécifique
    /// </summary>
    public OUT SelectTree(int index, SyntaxTreeVisitor<IN, OUT> visitor, object context = null)
    {
        if (Forest == null || index >= Forest.Count)
            throw new ArgumentOutOfRangeException(nameof(index));
            
        return visitor.VisitSyntaxTree(Forest.Trees[index], context);
    }
    
    /// <summary>
    /// Résoudre l'ambiguïté via un sélecteur personnalisé
    /// </summary>
    public OUT ResolveAmbiguity(Func<List<ISyntaxNode<IN, OUT>>, ISyntaxNode<IN, OUT>> selector,
        SyntaxTreeVisitor<IN, OUT> visitor, object context = null)
    {
        if (!IsAmbiguous)
            return Result;
            
        var selectedTree = selector(Forest.Trees);
        return visitor.VisitSyntaxTree(selectedTree, context);
    }
}
```

#### 3.3 Créer une interface pour les visiteurs désambiguïsants

**Fichier** : `src/sly/parser/generator/visitor/IAmbiguityResolver.cs` (nouveau)

```csharp
namespace sly.parser.generator.visitor
{
    /// <summary>
    /// Interface pour implémenter une logique de résolution d'ambiguïté personnalisée
    /// </summary>
    public interface IAmbiguityResolver<IN, OUT> where IN : struct, Enum
    {
        /// <summary>
        /// Sélectionne l'arbre syntaxique à utiliser parmi plusieurs alternatives
        /// </summary>
        /// <param name="alternatives">Liste des arbres alternatifs</param>
        /// <param name="ambiguityInfo">Informations sur le point d'ambiguïté</param>
        /// <returns>L'arbre sélectionné</returns>
        ISyntaxNode<IN, OUT> Resolve(List<ISyntaxNode<IN, OUT>> alternatives, 
            AmbiguityInfo<IN, OUT> ambiguityInfo);
    }
}
```

---

### Phase 4 : Optimisation (optionnelle) - Shared Packed Parse Forest (SPPF)

**Objectif** : Réduire la mémoire en partageant les sous-arbres communs entre les dérivations alternatives.

**Note** : Cette phase est optionnelle pour la première version. Elle peut être implémentée plus tard si les performances deviennent un problème.

#### 4.1 Créer une structure SPPF

**Fichier** : `src/sly/parser/syntax/tree/SPPF.cs` (nouveau)

```csharp
namespace sly.parser.syntax.tree
{
    /// <summary>
    /// Nœud d'une Shared Packed Parse Forest
    /// </summary>
    public interface ISPPFNode<IN, OUT> where IN : struct, Enum
    {
        int StartPosition { get; }
        int EndPosition { get; }
    }
    
    /// <summary>
    /// Nœud symbole : représente un non-terminal ou terminal
    /// </summary>
    public class SPPFSymbolNode<IN, OUT> : ISPPFNode<IN, OUT> where IN : struct, Enum
    {
        public string Symbol { get; set; }
        public int StartPosition { get; set; }
        public int EndPosition { get; set; }
        public List<SPPFPackedNode<IN, OUT>> Alternatives { get; set; }
    }
    
    /// <summary>
    /// Nœud packed : représente une alternative de dérivation
    /// </summary>
    public class SPPFPackedNode<IN, OUT> where IN : struct, Enum
    {
        public Rule<IN, OUT> Rule { get; set; }
        public List<ISPPFNode<IN, OUT>> Children { get; set; }
    }
}
```

**Cette phase nécessite une refonte plus profonde et peut être reportée à une version ultérieure.**

---

### Phase 5 : Tests et documentation

#### 5.1 Tests unitaires

**Fichier** : `tests/ParserTests/AmbiguousGrammarTests.cs` (nouveau)

Créer des tests pour :

**Tests BNF** :
- Grammaire arithmétique ambiguë classique : `E -> E + E | E * E | INT`
- Grammaire if-then-else ambiguë (dangling else)
- Multiple règles alternatives pour un même non-terminal

**Tests EBNF** :
- Choix EBNF ambigu : `A -> (B | C | D)` où B, C et D peuvent tous matcher la même entrée
- Options ambiguës : `A -> B [C | D]` où C et D sont ambigus
- Répétitions avec alternatives ambiguës : `A -> (B | C)*` où B et C peuvent matcher les mêmes tokens

**Tests de stratégie** :
- Vérifier que `CaptureAmbiguities = false` conserve le comportement actuel
- Vérifier que `CaptureAmbiguities = true` retourne tous les arbres
- Tester les stratégies de résolution (`First`, `All`, `ThrowException`, `Longest`)

**Tests de propagation** :
- Ambiguïtés imbriquées : choix dans une option dans une répétition
- Combinaisons multiples d'ambiguïtés dans la même règle

#### 5.2 Exemples d'utilisation

**Fichier** : `src/samples/AmbiguousGrammarExample/` (nouveau dossier)

Créer un exemple complet montrant :
1. Définition d'une grammaire ambiguë
2. Activation de `CaptureAmbiguities`
3. Parcours de la forêt d'arbres
4. Résolution manuelle via un `IAmbiguityResolver`

#### 5.3 Documentation

**Fichier** : `doc/features/ambiguous_grammars/usage.md` (nouveau)

Documenter :
- Comment activer la détection d'ambiguïtés
- Les différentes stratégies disponibles
- Comment implémenter un résolveur personnalisé
- Exemples de grammaires ambiguës courantes
- Considérations de performance

---

## Plan de migration et rétrocompatibilité

### Compatibilité ascendante garantie

1. **Par défaut, aucun changement** : `CaptureAmbiguities = false` par défaut
   - Le comportement actuel est préservé
   - Les utilisateurs existants ne sont pas impactés

2. **API compatible** :
   - `ParseResult.SyntaxTree` continue de fonctionner (pointe vers le premier arbre)
   - `ParseResult.Result` continue de fonctionner (visite le premier arbre)

3. **Activation opt-in** :
   ```csharp
   var builder = new ParserBuilder<MyToken, MyResult>();
   var buildResult = builder.BuildParser(parserInstance, ParserType.EBNF_LL_RECURSIVE_DESCENT, "root");
   var parser = buildResult.Result;
   
   // NOUVEAU : Activer la détection d'ambiguïtés
   parser.Configuration.CaptureAmbiguities = true;
   parser.Configuration.AmbiguityStrategy = AmbiguityResolutionStrategy.ThrowException;
   
   var result = parser.Parse(source);
   if (result.IsAmbiguous)
   {
       // Traiter l'ambiguïté
   }
   ```

---

## Ordre d'implémentation recommandé

1. **Semaine 1** : Phase 1 (structures de données)
   - Créer `ParseForest`, `AmbiguityInfo`
   - Modifier `SyntaxParseResult` et `ParseResult`
   - Tests de base sur les nouvelles structures

2. **Semaine 2** : Phase 2.1 et 2.2 (configuration et capture - BNF)
   - Ajouter `CaptureAmbiguities` et `AmbiguityResolutionStrategy`
   - Modifier `ParseNonTerminal()` (BNF) pour capturer les alternatives
   - Tests avec grammaire BNF simple ambiguë

3. **Semaine 3** : Phase 2.2bis et 2.4 (capture et propagation - EBNF)
   - Modifier `ParseChoice()` (EBNF) pour capturer les alternatives EBNF
   - Implémenter la propagation des ambiguïtés dans `ParseOption()` et `Parse()` principal
   - Tests avec grammaire EBNF ambiguë (choix multiples)

4. **Semaine 4** : Phase 2.3 et 3.1-3.2 (propagation finale et API)
   - Propager les ambiguïtés jusqu'au `ParseResult`
   - Créer `AmbiguousGrammarException`
   - Ajouter méthodes utilitaires (`VisitAllTrees`, etc.)

5. **Semaine 5** : Phase 3.3 et 5 (résolveurs et tests)
   - Interface `IAmbiguityResolver`
   - Tests exhaustifs avec grammaires classiques
   - Exemples et documentation

5. **Version future** : Phase 4 (SPPF)
   - Optimisation mémoire si nécessaire

---

## Points d'attention

### Performance
- **Impact mémoire** : Stocker plusieurs arbres peut consommer beaucoup de mémoire pour des grammaires très ambiguës
- **Solution** : Implémenter SPPF dans une phase ultérieure si nécessaire
- **Compromis** : Limiter le nombre d'alternatives capturées (configurable)

### Parseurs concernés
- **LL Recursive Descent (BNF)** : ✅ Couvert par ce plan (Phase 2.2 - `ParseNonTerminal()`)
- **EBNF Recursive Descent** : ✅ Couvert par ce plan (Phase 2.2bis - `ParseChoice()` + Phase 2.4 - propagation)
  - **Point critique** : `ParseChoice()` gère les alternatives EBNF (`A | B | C`)
  - **Complexité additionnelle** : Propagation des ambiguïtés à travers les structures imbriquées (options, répétitions)
- **LR/LALR** : ⚠️ Non couvert, architecture différente (à évaluer séparément)

### Edge cases
- Grammaire infiniment ambiguë : ajouter un timeout ou limite d'alternatives
- Ambiguïtés imbriquées : tester la propagation correcte
- Performances sur grammaires très ambiguës : benchmarker

---

## Validation du plan

### Critères de succès

1. ✅ Rétrocompatibilité totale (comportement par défaut inchangé)
2. ✅ Capacité de détecter et capturer les ambiguïtés
3. ✅ API claire et flexible pour la résolution
4. ✅ Documentation complète avec exemples
5. ✅ Tests exhaustifs couvrant les cas courants

### Points de validation

- [ ] Tests unitaires passent (anciens + nouveaux)
- [ ] Exemple de grammaire ambiguë fonctionne
- [ ] Pas de régression de performance sur grammaires non ambiguës
- [ ] Documentation à jour
- [ ] Review de code complète

---

## Conclusion

Ce plan permet d'introduire progressivement le support des grammaires ambiguës dans CSLY tout en préservant la rétrocompatibilité. L'implémentation est structurée en phases incrémentales, permettant de valider chaque étape avant de passer à la suivante.

**Prochaine étape** : Validation du plan avec les mainteneurs et début de l'implémentation de la Phase 1.
