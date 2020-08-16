.PHONEY: all
all:
	# Valid targets are:
	#   make jumpstart - install all the necessary tools for devs to get started
	#	make run ENTRYPOINT=$(ENTRYPOINT) - start the interpreter on one of the supplied examples

PREFIX=/opt/nutmeg
INSTALL_DIR=$(PREFIX)/libexec/nutmeg
EXEC_DIR=/usr/local/bin

# osx-x64 linux-x64 win-x64
RID?=osx-x64

ENTRYPOINT?=program
.PHONEY: run
run:
	dotnet NutmegRunner/NutmegRunner/bin/Debug/netcoreapp3.1/NutmegRunner.dll --debug --entry-point=$(ENTRYPOINT) examples/bundler/simple.bundle

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

.PHONEY: build
build:
	make build-compiler
	make build-runner

.PHONEY: build-compiler
build-compiler: 
	make clean-compiler
	cxfreeze launcher.py -O --silent --target-dir=_build/compiler --target-name=nutmeg

.PHONEY: build-runner
build-runner:
	make -C NutmegRunner

.PHONEY: install
install:
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
	( cd NutmegRunner/bin/Debug/netcoreapp3.1/$(RID)/publish; tar cf - . ) | ( cd $(INSTALL_DIR)/runner; tar xf - )

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

