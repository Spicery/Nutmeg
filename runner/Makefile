# This makefile is mainly a reminder that the dotnet command is the
# one-stop shop for managing .NET projects/solutions.

# Configuration -c can be Debug or Release
# -r is the target system. 
RID?=osx-x64
.PHONEY: publish
publish: NutmegRunner/bin/Debug/netcoreapp3.1/$(RID)/publish/NutmegRunner.dll

NutmegRunner/bin/Debug/netcoreapp3.1/$(RID)/publish/NutmegRunner.dll: NutmegRunner/*.cs
	dotnet publish -c Debug -r $(RID)

.PHONEY: build
build: NutmegRunner/bin/Debug/netcoreapp3.1/NutmegRunner.dll

NutmegRunner/bin/Debug/netcoreapp3.1/NutmegRunner.dll: NutmegRunner/*.cs
	dotnet build

.PHONEY: clean
clean:
	dotnet clean

.PHONEY: unittests
unittests:
	echo 'Not implemented yet'
