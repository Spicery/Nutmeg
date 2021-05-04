.PHONEY: all
all: help

.PHONEY: help
help:
	# Valid targets are:
	#	make clean - cleans out build files and artefacts
	#	make build - builds the compiler
	#	make unittests - runs unit tests

# Cleans out the build files and other artefacts. 
.PHONEY: clean
clean: 
	rm -rf _build
	rm -rf _working

# This actually builds a lot of files but we use the executable to indicate a successful build
.PHONEY: build
build: _build/nutmeg/nutmeg

_build/nutmeg/nutmeg:
	#cxfreeze launcher.py -O --silent --target-dir=_build/compiler --target-name=nutmeg
	pip3 install -r packaging_requirements.txt
	pyinstaller --noconfirm --workpath=_working --distpath=_build --name=nutmeg launcher.py

.PHONEY: unittests
unittests: 
	pip3 install -r test/test_requirements.txt
	pytest test/
