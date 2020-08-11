.PHONEY: all
all:
	# Valid targets are:
	#	make run ENTRYPOINT=$(ENTRYPOINT)

ENTRYPOINT?=program
.PHONEY: run
run:
	dotnet NutmegRunner/NutmegRunner/bin/Debug/netcoreapp3.1/NutmegRunner.dll --debug --entry-point=$(ENTRYPOINT) examples/bundler/simple.bundle

