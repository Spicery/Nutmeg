# This makefile is mainly a reminder that the dotnet command is the
# one-stop shop for managing .NET projects/solutions.

# Configuration -c can be Debug or Release
# -r is the target system. 
RID?=osx-x64
.PHONEY: publish
publish:
	dotnet publish -c Debug -r $(RID)

.PHONEY: build
build:
	dotnet build

.PHONEY: clean
clean:
	dotnet clean
