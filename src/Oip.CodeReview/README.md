# LLM code review

## Review all changes

````shell
dotnet run ./Oip.CodeReview \
--WorkDir=/path/to/project/ \
--SourceBranch=your-branch \
--TargetBranch=main
````

## Review changes in a single file

````shell
dotnet run ./Oip.CodeReview \
--WorkDir=/path/to/project/ \
--SourceBranch=your-branch \
--TargetBranch=main \
--FilePath=/path/to/project/src/Oip.CodeReview/Program.cs
````

## Generate a prompt for a public LLM

````shell
dotnet run ./Oip.CodeReview \
--WorkDir=/path/to/project/ \
--SourceBranch=code-review-change \
--TargetBranch=main \
--FilePath=/path/to/project/src/Oip.CodeReview/Program.cs \
--PromptOnly=true
````