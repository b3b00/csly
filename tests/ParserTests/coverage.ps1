
dotnet test -v normal /p:CollectCoverage=true '/p:CoverletOutputFormat="json,lcov"' /p:CoverletOutput=./lcov '/p:exclude="[while]*,[jsonparser]*,[expressionParser]*,[SimpleExpressionParser]*,[GenericLexerWithCallbacks]*,[indentedWhile]*,[indented]*,[SimpleTemplate]*,[while]*,[jsonparser]*,[expressionParser]*,[SimpleExpressionParser]*,[GenericLexerWithCallbacks]*,[indentedWhile]*,[indented]*,[postProcessedLexerParser]*,[XML]*,[SlowEOS]*"' ParserTests.csproj
reportgenerator.exe -reports:lcov.info -targetdir:coverage-report -reporttypes:Html
