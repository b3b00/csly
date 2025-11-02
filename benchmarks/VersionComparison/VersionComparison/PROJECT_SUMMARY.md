# Version Comparison Benchmark - Project Summary

## 📁 Structure du Projet

```
benchmarks/VersionComparison/
│
├── VersionComparison.csproj          # Projet principal avec BenchmarkDotNet
├── Program.cs                         # Point d'entrée
├── VersionComparisonBenchmarks.cs    # Définition des benchmarks
│
├── LocalVersion/                      # Wrapper pour la branche actuelle
│   ├── LocalVersion.csproj           # Référence src/sly (ProjectReference)
│   └── ParserWrapper.cs              # Parser utilisant votre code local
│
├── NuGetVersion/                      # Wrapper pour NuGet 3.7.6
│   ├── NuGetVersion.csproj           # Référence sly 3.7.6 (PackageReference)
│   └── ParserWrapper.cs              # Parser utilisant le package NuGet
│
├── run-benchmark.bat                  # Script Windows
├── run-benchmark.sh                   # Script Linux/Mac
├── README.md                          # Documentation principale
├── QUICKSTART.md                      # Guide de démarrage rapide
└── .gitignore                         # Fichiers à ignorer
```

## 🎯 Objectif

Comparer les performances de **votre branche de développement actuelle** avec la **version officielle NuGet 3.7.6** de sly.

## 🔧 Implémentation Technique

### Séparation des Versions

Pour éviter les conflits de versions d'assemblages, nous utilisons **deux projets séparés** :

1. **LocalVersion** : 
   - Référence directe au projet `../../src/sly/sly.csproj`
   - Compile votre code local actuel
   - Expose un `ParserWrapper` utilisant votre version

2. **NuGetVersion** :
   - Référence au package NuGet `sly` version `3.7.6`
   - Utilise la version stable publiée
   - Expose un `ParserWrapper` utilisant cette version

3. **VersionComparison** :
   - Référence les deux wrappers
   - Exécute les benchmarks BenchmarkDotNet
   - Compare les performances côte à côte

### Parser de Test

Un parser simple d'expressions arithmétiques qui supporte :
- Nombres entiers
- Opérateurs : `+`, `-`, `*`, `/`
- Parenthèses pour la priorité
- Grammaire LL avec récursion descendante

## 📊 Benchmarks Inclus

| Benchmark | Description | Nombre d'expressions |
|-----------|-------------|---------------------|
| **SimpleExpressions** | `1 + 2` | 20 |
| **MediumExpressions** | `1 + 2 * 3 - 4` | 20 |
| **ComplexExpressions** | `((1 + 2) * (3 + 4)) - ((5 - 6) + (7 * 8))` | 20 |

Chaque benchmark est exécuté avec les **deux versions** pour comparaison directe.

## 🚀 Utilisation

### Exécution Rapide
```batch
# Windows
run-benchmark.bat

# Linux/Mac
chmod +x run-benchmark.sh
./run-benchmark.sh
```

### Commandes Manuelles
```bash
cd benchmarks/VersionComparison
dotnet build -c Release
dotnet run -c Release --no-build
```

## 📈 Interprétation des Résultats

### Métriques Clés

- **Mean** : Temps moyen d'exécution
  - Plus bas = meilleur
  - Différence > 10% = significatif

- **Allocated** : Mémoire allouée
  - Plus bas = meilleur
  - Impact sur les GC

- **Gen0/1/2** : Collections du GC
  - Plus bas = meilleur
  - Gen0 : petits objets temporaires
  - Gen1/2 : objets plus persistants

### Exemple de Résultat

```
| Method                           | Mean      | Error    | StdDev   | Allocated |
|--------------------------------- |----------:|---------:|---------:|----------:|
| SimpleExpressions_NuGet          | 156.2 us  | 2.1 us   | 1.9 us   | 26.4 KB   |
| SimpleExpressions_Local          | 128.5 us  | 1.8 us   | 1.6 us   | 21.2 KB   |
| MediumExpressions_NuGet           | 245.8 us  | 3.2 us   | 2.8 us   | 42.1 KB   |
| MediumExpressions_Local           | 198.3 us  | 2.5 us   | 2.2 us   | 34.5 KB   |
| ComplexExpressions_NuGet          | 428.6 us  | 5.1 us   | 4.7 us   | 71.3 KB   |
| ComplexExpressions_Local          | 342.1 us  | 4.2 us   | 3.9 us   | 58.2 KB   |
```

**Interprétation** : La branche locale est ~20% plus rapide et alloue ~20% moins de mémoire.

## 🔍 Que Mesure-t-on ?

### Inclus dans la Mesure
- Temps de parsing complet
- Allocations mémoire
- Collections du GC
- Construction de l'arbre syntaxique

### Exclus de la Mesure
- Construction du parser (GlobalSetup)
- Initialisation des données de test
- Overhead de BenchmarkDotNet lui-même

## ⚙️ Configuration BenchmarkDotNet

```csharp
[MemoryDiagnoser]                    // Active le diagnostic mémoire
[Orderer(SummaryOrderPolicy.FastestToSlowest)]  // Trie par vitesse
[RankColumn]                         // Ajoute un classement
```

## 🎓 Bonnes Pratiques

1. **Environnement Stable**
   - Fermez les applications inutiles
   - Branchez sur secteur (laptops)
   - Désactivez les économies d'énergie

2. **Plusieurs Exécutions**
   - Lancez 2-3 fois pour confirmer
   - Vérifiez la cohérence des résultats
   - Attention aux outliers

3. **Mode Release**
   - Toujours en Release (optimisations activées)
   - Jamais en Debug pour les benchmarks

4. **Interprétation**
   - Différence < 5% : probablement du bruit
   - Différence 5-10% : notable, à vérifier
   - Différence > 10% : significatif

## 📝 Modifications Possibles

### Ajouter un Benchmark

```csharp
[Benchmark(Description = "Mon nouveau test - NuGet 3.7.6")]
public void MonTest_NuGet()
{
    // Votre code de test avec _nugetParser
}

[Benchmark(Description = "Mon nouveau test - Current Branch")]
public void MonTest_Local()
{
    // Votre code de test avec _localParser
}
```

### Changer les Expressions de Test

Modifiez la méthode `Setup()` dans `VersionComparisonBenchmarks.cs` :

```csharp
[GlobalSetup]
public void Setup()
{
    // Initialisez vos parsers...
    
    // Ajoutez vos propres expressions
    _simpleExpressions.Add("votre expression");
}
```

### Comparer Avec une Autre Version

Modifiez `NuGetVersion/NuGetVersion.csproj` :

```xml
<PackageReference Include="sly" Version="3.7.5" />  <!-- ou autre -->
```

## 🐛 Dépannage

### Erreur : "Could not find project"
- Vérifiez que `src/sly/sly.csproj` existe
- Utilisez des chemins relatifs corrects

### Erreur : "Package not found"
- Exécutez `dotnet restore`
- Vérifiez votre connexion internet

### Résultats Incohérents
- Relancez plusieurs fois
- Vérifiez les processus en arrière-plan
- Redémarrez votre machine

## 📦 Dépendances

- .NET 8.0 SDK
- BenchmarkDotNet 0.13.10
- sly (local) - votre branche
- sly 3.7.6 (NuGet)

## 🎉 Prochaines Étapes

Après avoir exécuté le benchmark :

1. ✅ Consultez les résultats dans `BenchmarkDotNet.Artifacts/results/`
2. 📊 Ouvrez le fichier HTML pour une vue graphique
3. 📈 Identifiez les améliorations ou régressions
4. 🔧 Ajustez votre code si nécessaire
5. 🔄 Re-benchmark pour valider les changements

## 📚 Ressources

- [BenchmarkDotNet Documentation](https://benchmarkdotnet.org/)
- [SLY GitHub Repository](https://github.com/b3b00/sly)
- [Benchmark Best Practices](https://benchmarkdotnet.org/articles/guides/good-practices.html)

