# Quick Start Guide - Version Comparison Benchmark

## Vue d'ensemble

Ce benchmark compare les performances de **votre branche actuelle** avec la **version NuGet 3.7.6** de sly.

## Architecture

Le projet utilise une structure en trois parties :
- **VersionComparison** : Projet principal contenant les benchmarks BenchmarkDotNet
- **LocalVersion** : Wrapper utilisant votre code local (référence au projet `src/sly`)
- **NuGetVersion** : Wrapper utilisant le package NuGet 3.7.6

Cette séparation évite les conflits de versions et permet une comparaison directe.

## Exécution

### Option 1 : Script automatique (Windows)
```batch
cd C:\Users\olduh\dev\csly\benchmarks\VersionComparison
run-benchmark.bat
```

### Option 2 : Script automatique (Linux/Mac)
```bash
cd /path/to/csly/benchmarks/VersionComparison
chmod +x run-benchmark.sh
./run-benchmark.sh
```

### Option 3 : Commandes manuelles
```bash
cd C:\Users\olduh\dev\csly\benchmarks\VersionComparison
dotnet build -c Release
dotnet run -c Release --no-build
```

## Que teste-t-on ?

Le benchmark teste le parsing de 3 types d'expressions arithmétiques :

### 1. Expressions simples (20 itérations)
```
1 + 2
2 + 3
3 + 4
...
```

### 2. Expressions moyennes (20 itérations)
```
1 + 2 * 3 - 4
2 + 3 * 4 - 5
...
```

### 3. Expressions complexes (20 itérations)
```
((1 + 2) * (3 + 4)) - ((5 - 6) + (7 * 8))
((2 + 3) * (4 + 5)) - ((6 - 7) + (8 * 9))
...
```

## Résultats

Les résultats seront dans `BenchmarkDotNet.Artifacts/results/` :
- Fichiers HTML pour visualisation web
- Fichiers Markdown pour documentation
- Fichiers CSV pour analyse

### Métriques importantes

- **Mean** : Temps moyen d'exécution (plus bas = meilleur)
- **Error** : Marge d'erreur statistique
- **StdDev** : Écart-type (plus bas = plus stable)
- **Allocated** : Mémoire allouée (plus bas = meilleur)
- **Gen0/Gen1/Gen2** : Nombre de GC (plus bas = meilleur)

### Exemple de lecture

```
| Method                      | Mean      | Allocated |
|---------------------------- |----------:|----------:|
| SimpleExpressions_NuGet     | 150.0 us  | 25 KB     |
| SimpleExpressions_Local     | 120.0 us  | 20 KB     | ← 20% plus rapide!
```

## Conseils

1. **Fermez les applications gourmandes** pendant le benchmark
2. **Ne touchez pas à l'ordinateur** pendant l'exécution (5-10 minutes)
3. **Lancez en mode Release** (déjà fait par les scripts)
4. **Lancez plusieurs fois** pour confirmer les tendances

## Dépannage

### Erreur de build
```bash
# Nettoyez et reconstruisez
dotnet clean
dotnet build -c Release
```

### Package NuGet non trouvé
```bash
# Restaurez les packages
dotnet restore
```

### Résultats incohérents
- Vérifiez qu'aucune application lourde ne tourne
- Branchez votre ordinateur sur secteur
- Désactivez temporairement l'antivirus

## Prochaines étapes

Après avoir exécuté le benchmark :
1. Consultez les résultats dans `BenchmarkDotNet.Artifacts/results/`
2. Comparez `SimpleExpressions_Local` vs `SimpleExpressions_NuGet`
3. Identifiez les améliorations ou régressions
4. Utilisez les métriques pour guider vos optimisations

