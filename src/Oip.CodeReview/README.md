# LLM code review

## Review all changes

````shell
dotnet run ./Oip.CodeReview \
--WorkDir=/Users/igortulakov/Projects/Lukoil-SKPP/ \
--SourceBranch=add-tank-reconciliation-module-tests \
--TargetBranch=main
````

## Review changes in a single file

````shell
dotnet run ./Oip.CodeReview \
--WorkDir=/Users/igortulakov/Projects/Lukoil-SKPP/ \
--SourceBranch=add-tank-reconciliation-module-tests \
--TargetBranch=main \
--FilePath=./src/Indusoft.Lukoil.Skpp.UiTest/TankReconciliationModuleTests.cs


````

## Generate a prompt for a public LLM

````shell
dotnet run ./Oip.CodeReview \
--WorkDir=/Users/igortulakov/Projects/Lukoil-SKPP/ \
--SourceBranch=handle-no-case-from-smb \
--TargetBranch=main \
--PromptOnly=true
````