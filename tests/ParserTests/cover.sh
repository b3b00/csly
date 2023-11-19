#!/bin/sh -x

dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=lcov /p:CoverletOutput=./lcov /p:exclude=\"[while]*,[jsonparser]*,[expressionParser]*,[SimpleExpressionParser]*,[GenericLexerWithCallbacks]*,[indentedWhile]*,[indented]*,[SimpleTemplate]*,[[while]*,[jsonparser]*,[expressionParser]*,[SimpleExpressionParser]*,[GenericLexerWithCallbacks]*,[indentedWhile]*,[indented]*,[postProcessedLexerParser]*,[XML]*\" ParserTests.csproj
