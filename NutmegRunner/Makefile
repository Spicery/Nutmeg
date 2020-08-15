# This makefile is mainly a reminder that the dotnet command is the
# one-stop shop for managing .NET projects/solutions.

# Configuration -c can be Debug or Release
# -r is the target system. 
.PHONEY: publish
publish:
	dotnet publish -c Debug -r osx.10.10-x64 

.PHONEY: build
build:
	dotnet build

.PHONEY: clean
clean:
	dotnet clean
