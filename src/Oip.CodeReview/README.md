# LLM code review

## Review all diff
````shell
dotnet run ./Oip.CodeReview \
--WorkDir=/Users/igortulakov/Projects/Oip/ \
--SourceBranch=add-feature \
--TargetBranch=main
````

## Review single file diff
````shell
dotnet run ./Oip.CodeReview \
--WorkDir=/Users/igortulakov/Projects/Oip/ \
--SourceBranch=code-review-change \
--TargetBranch=main \
--FilePath=./src/Oip.CodeReview/README.md
````

## Create prompt for past in public LLM

````shell
dotnet run ./Oip.CodeReview \
--WorkDir=/Users/igortulakov/Projects/Oip/ \
--SourceBranch=code-review-change \
--TargetBranch=main \
--FilePath=./src/Oip.CodeReview/README.md \
--PromptOnly=true
````