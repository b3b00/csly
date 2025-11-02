# Version Comparison Benchmark

Ce projet benchmark compare les performances de la branche actuelle avec la version NuGet 3.7.6 de sly.

## Qu'est-ce qui est testé ?

Le benchmark teste le parsing d'expressions arithmétiques avec trois niveaux de complexité :

1. **Expressions simples** : `1 + 2`, `3 * 4`, etc.
2. **Expressions moyennes** : `1 + 2 * 3 - 4`, etc.
3. **Expressions complexes** : `((1 + 2) * (3 + 4)) - ((5 - 6) + (7 * 8))`, etc.

## Architecture

Le projet utilise des **aliases d'assemblage** pour référencer simultanément :
- La version locale (branche actuelle) via `ProjectReference` avec l'alias `LocalSly`
- La version NuGet 3.7.6 via `PackageReference` avec l'alias `NuGetSly`

Cela permet de comparer directement les deux versions dans le même processus.

## Comment exécuter

### Windows
```batch
run-benchmark.bat
```

### Linux/Mac
```bash
chmod +x run-benchmark.sh
./run-benchmark.sh
```

### Manuellement
```bash
dotnet build -c Release
dotnet run -c Release --no-build
```

## Résultats

Les résultats seront générés dans le dossier `BenchmarkDotNet.Artifacts/results/` avec :
- Des rapports HTML et Markdown
- Des statistiques détaillées de performance
- Des métriques de mémoire (allocations, GC, etc.)

## Interprétation

Le benchmark affichera :
- **Mean** : Temps moyen d'exécution
- **Error** : Marge d'erreur
- **StdDev** : Écart-type
- **Gen0/Gen1/Gen2** : Collections de garbage (nombre par 1000 opérations)
- **Allocated** : Mémoire allouée

Comparez les résultats entre `NuGet 3.7.6` et `Current Branch` pour voir les améliorations ou régressions de performance.

## Notes

- Le benchmark s'exécute en mode Release pour des résultats optimaux
- Plusieurs itérations sont effectuées pour garantir la stabilité des résultats
- Fermez les applications gourmandes en ressources pendant l'exécution pour des résultats précis

