.PHONEY: all
all: help

.PHONEY: help
help:
	# Valid targets are:
	#	make clean - cleans out build files and artefacts (uses dotnet clean, so doesn't fully work)
	#	make build - builds the compiler and runner
	#	make mkinstaller - creates a *.zip file suitable for installation on Linux/MacOS, use after `make build`
	#	make install - installs locally, use after `make build`
	#	make jumpstart - install all the necessary tools for devs to get started
	#	make run ENTRYPOINT=$(ENTRYPOINT) - start the interpreter on one of the supplied examples

PREFIX=/opt/nutmeg
INSTALL_DIR=$(PREFIX)/libexec/nutmeg
EXEC_DIR=/usr/local/bin

# osx-x64 linux-x64 win-x64
RID?=osx-x64

# Linux-oriented jumpstart. Run with sudo. Will install a bunch of packages and .NET 3.1.
.PHONEY: jumpstart
jumpstart:
	#	Add the Python SDK.
	apt-get update
	apt-get install -y build-essential python3 wget zip python-pip 
	pip3 install cx-freeze
	# 	Now add the .NET SDK.
	wget https://packages.microsoft.com/config/ubuntu/20.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
	dpkg -i packages-microsoft-prod.deb
	apt-get install -y apt-transport-https
	apt-get update
	# Use the noninteractive flag to stop being prompted for timezone.
	apt-get install -y dotnet-sdk-3.1

# This is purely a dev convenience, capturing the complicated dotnet command needed.
ENTRYPOINT?=program
.PHONEY: run
run:
	dotnet NutmegRunner/NutmegRunner/bin/Debug/netcoreapp3.1/NutmegRunner.dll --debug --entry-point=$(ENTRYPOINT) examples/bundler/simple.bundle

# Cleans out the build files and other artefacts. It uses `dotnet clean` to sort out the C#
# area but that does seem to leave a lot of caches and other build files behind. Will need to
# properly sort that out as it interferes with the use of `docker build .`.
.PHONEY: clean
clean:
	make clean-compiler
	make clean-runner

.PHONEY: clean-compiler
clean-compiler: 
	rm -rf _build

.PHONEY: clean-runner
clean-runner:
	make -C NutmegRunner clean

# Builds the compiler in the _build/compiler folder and the runner in the appropriate dotnet publish folder.
.PHONEY: build
build:
	make build-compiler
	make build-runner

# This actually builds a lot of files but we use the executable to indicate a successful build
.PHONEY: build-compiler
build-compiler: _build/compiler/nutmeg

_build/compiler/nutmeg:
	cxfreeze launcher.py -O --silent --target-dir=_build/compiler --target-name=nutmeg

# Builds into NutmegRunner/NutmegRunner/bin/Debug/netcoreapp3.1/$(RID)/publish
# Where $(RID) is the target architecture to build for. For a list of archhitectures
# see https://docs.microsoft.com/en-us/dotnet/core/rid-catalog
.PHONEY: build-runner
build-runner:
	make -C NutmegRunner

# Builds the installer into _build/installer.zip folder. This requires the system to be built
# using `make build` first.
.PHONEY: mkinstaller
mkinstaller:
	# Add the nutmeg & nutmegc scripts into _build/installer/bin.
	mkdir -p _build/installer/bin
	printf '#!/bin/bash\nexec $(INSTALL_DIR)/compiler/nutmeg $$*\n' > _build/installer/bin/nutmeg
	printf '#!/bin/bash\nexec $(INSTALL_DIR)/compiler/nutmeg compile $$*\n' > _build/installer/bin/nutmegc
	# Add the compiler and runner into _build/installer/libexec/nutmeg/.
	mkdir -p _build/installer/libexec/nutmeg
	( cd _build; tar cf - compiler ) | ( cd _build/installer/libexec/nutmeg; tar xf - )
	mkdir -p _build/installer/libexec/nutmeg/runner
	( cd NutmegRunner/NutmegRunner/bin/Debug/netcoreapp3.1/$(RID)/publish; tar cf - . ) | ( cd _build/installer/libexec/nutmeg/runner; tar xf - )
	# Add the installer.bsh script.
	python3 scripts/mkinstaller.py --install_dir=$(INSTALL_DIR) --exec_dir=$(EXEC_DIR) > _build/installer/install.bsh
	# And zip it all up.
	( cd _build/installer; zip -r ../installer.zip . )

# Do a local installation, building first if needed.
.PHONEY: install
install: build
	mkdir -p $(EXEC_DIR)
	printf '#!/bin/bash\nexec $(INSTALL_DIR)/compiler/nutmeg $$*\n' > $(EXEC_DIR)/nutmeg
	printf '#!/bin/bash\nexec $(INSTALL_DIR)/compiler/nutmeg compile $$*\n' > $(EXEC_DIR)/nutmegc
	chmod a+rx,a-w $(EXEC_DIR)/nutmeg
	chmod a+rx,a-w $(EXEC_DIR)/nutmegc
	make install-compiler
	make install-runner

.PHONEY: install-compiler
install-compiler:
	make uninstall-compiler
	mkdir -p $(INSTALL_DIR)
	( cd _build; tar cf - compiler ) | ( cd $(INSTALL_DIR); tar xf - )

.PHONEY: install-runner
install-runner:
	mkdir -p $(INSTALL_DIR)/runner
	( cd NutmegRunner/NutmegRunner/bin/Debug/netcoreapp3.1/$(RID)/publish; tar cf - . ) | ( cd $(INSTALL_DIR)/runner; tar xf - )

# Uninstall the application locally.
.PHONEY: uninstall
uninstall:
	make uninstall-compiler
	rm -f $(EXEC_DIR)/nutmeg
	rm -f $(EXEC_DIR)/nutmegc

.PHONEY: uninstall-compiler
uninstall-compiler:
	rm -rf $(INSTALL_DIR)/compiler

.PHONEY: uninstall-runner
uninstall-runner:
	rm -rf $(INSTALL_DIR)/runner
