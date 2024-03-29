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
	#	make unittests
	#   make inttests

LOCAL=SYSTEM
PREFIX=/opt/nutmeg
INSTALL_DIR=$(PREFIX)/libexec/nutmeg
EXEC_DIR=/usr/local/bin

# osx-x64 linux-x64 win-x64
RID?=osx-x64

# Linux-oriented jumpstart. Run with sudo. Will install a bunch of packages and .NET 3.1.
.PHONEY: jumpstart
jumpstart:
	#	Add UNIX tools and the Python SDK.
	apt-get update
	apt-get install -y build-essential python3 wget zip python3-pip 
	# 	Now add the .NET SDK.
	wget https://packages.microsoft.com/config/ubuntu/20.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
	dpkg -i packages-microsoft-prod.deb
	apt-get install -y apt-transport-https
	apt-get update
	# Use the noninteractive flag to stop being prompted for timezone.
	DEBIAN_FRONTEND=noninteractive apt-get install -y dotnet-sdk-3.1

# This is purely a dev convenience, capturing the complicated dotnet command needed.
ENTRYPOINT?=program
.PHONEY: run
run:
	dotnet runner/NutmegRunner/bin/Debug/netcoreapp3.1/NutmegRunner.dll --debug --entry-point=$(ENTRYPOINT) examples/bundler/simple.bundle

# Cleans out the build files and other artefacts. It uses `dotnet clean` to sort out the C#
# area but that does seem to leave a lot of caches and other build files behind. Will need to
# properly sort that out as it interferes with the use of `docker build .`.
.PHONEY: clean
clean:
	make clean-compiler
	make clean-runner
	rm -rf _build
	rm -rf _local

.PHONEY: clean-compiler
clean-compiler: 
	make -C compiler clean

.PHONEY: clean-runner
clean-runner:
	make -C runner clean

# Builds the compiler in the _build/compiler folder and the runner in the appropriate dotnet publish folder.
.PHONEY: build
build:
	make build-compiler
	make build-runner RID=$(RID)

# This actually builds a lot of files but we use the executable to indicate a successful build
.PHONEY: build-compiler
build-compiler:
	make -C compiler build
	rm -rf _build/compiler/
	mkdir -p _build/
	mv compiler/_build/nutmeg _build/compiler

# Redundant because compiler/Makefile?
_build/compiler/nutmeg/nutmeg:
	#cxfreeze launcher.py -O --silent --target-dir=_build/compiler --target-name=nutmeg
	pip3 install -r compiler/packaging_requirements.txt
	( cd compiler; pyinstaller --noconfirm --workpath=_working --distpath=../_build/compiler --name=nutmeg launcher.py )

# Builds into runner/NutmegRunner/bin/Debug/netcoreapp3.1/$(RID)/publish
# Where $(RID) is the target architecture to build for. For a list of archhitectures
# see https://docs.microsoft.com/en-us/dotnet/core/rid-catalog
.PHONEY: build-runner
build-runner:
	make -C runner

# Builds the installer into _build/nutmeg-installer.zip folder. This requires the system to be built
# using `make build` first.
.PHONEY: mkinstaller
mkinstaller: build
	# Add the nutmeg & nutmegc scripts into _build/nutmeg-installer/bin.
	mkdir -p _build/nutmeg-installer/bin
	python3 scripts/mkbinnutmeg.py --install_dir=$(INSTALL_DIR) > _build/nutmeg-installer/bin/nutmeg
	python3 scripts/mkbinnutmegc.py --install_dir=$(INSTALL_DIR) > _build/nutmeg-installer/bin/nutmegc
	# Add the compiler and runner into _build/nutmeg-installer/libexec/nutmeg/.
	mkdir -p _build/nutmeg-installer/libexec/nutmeg/compiler
	( cd _build/compiler/nutmeg; tar cf - . ) | ( cd _build/nutmeg-installer/libexec/nutmeg/compiler; tar xf - )
	mkdir -p _build/nutmeg-installer/libexec/nutmeg/runner
	( cd runner/NutmegRunner/bin/Debug/netcoreapp3.1/$(RID)/publish; tar cf - . ) | ( cd _build/nutmeg-installer/libexec/nutmeg/runner; tar xf - )
	# Add the installer.bsh script.
	python3 scripts/mkinstaller.py --install_dir=$(INSTALL_DIR) --exec_dir=$(EXEC_DIR) > _build/nutmeg-installer/install.bsh
	chmod a+x _build/nutmeg-installer/install.bsh
	# And zip it all up.
	( cd _build; zip -qr nutmeg-installer.zip nutmeg-installer )
	( cd _build; tar cf - nutmeg-installer ) | gzip > _build/nutmeg-installer.tgz


.PHONEY: local-install
local-install:
	mkdir -p _local/bin
	mkdir -p _local/libexec
	rm -rf _local/bin _local/libexec
	make install PREFIX=`realpath _local` EXEC_DIR=`realpath _local/bin` RID=$(RID)

# Do a local installation. Will need to be run as sudo.
.PHONEY: install
install:
	mkdir -p $(EXEC_DIR)
	python3 scripts/mkbinnutmeg.py --install_dir=$(INSTALL_DIR) > $(EXEC_DIR)/nutmeg
	python3 scripts/mkbinnutmegc.py --install_dir=$(INSTALL_DIR) > $(EXEC_DIR)/nutmegc
	chmod a+rx,a-w $(EXEC_DIR)/nutmeg
	chmod a+rx,a-w $(EXEC_DIR)/nutmegc
	make install-compiler
	make install-runner RID=$(RID)

.PHONEY: install-compiler
install-compiler:
	make uninstall-compiler
	mkdir -p $(INSTALL_DIR)
	( cd _build; tar cf - compiler ) | ( cd $(INSTALL_DIR); tar xf - )

.PHONEY: install-runner
install-runner:
	mkdir -p $(INSTALL_DIR)/runner
	( cd runner/NutmegRunner/bin/Debug/netcoreapp3.1/$(RID)/publish; tar cf - . ) | ( cd $(INSTALL_DIR)/runner; tar xf - )

# Uninstall the application locally.
.PHONEY: uninstall
uninstall:
	make uninstall-compiler
	make uninstall-runner
	rm -f $(EXEC_DIR)/nutmeg
	rm -f $(EXEC_DIR)/nutmegc

.PHONEY: uninstall-compiler
uninstall-compiler:
	rm -rf $(INSTALL_DIR)/compiler

.PHONEY: uninstall-runner
uninstall-runner:
	rm -rf $(INSTALL_DIR)/runner

.PHONEY: unittests
unittests:
	make -C compiler unittests
	make -C runner unittests

.PHONEY: inttests
inttests: 
	python3 scripts/unittest_here.py examples/kata/*/ integration_tests/*/	
