FROM ubuntu
RUN apt-get update && apt-get install -y build-essential python3 wget zip
RUN  mkdir -p /tmp/nutmeg
COPY compiler/ /tmp/nutmeg/compiler/
COPY runner/ /tmp/nutmeg/runner/
COPY scripts/ /tmp/nutmeg/scripts
COPY Makefile /tmp/nutmeg/
WORKDIR /tmp/nutmeg
RUN wget https://packages.microsoft.com/config/ubuntu/20.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
RUN dpkg -i packages-microsoft-prod.deb
RUN apt-get install -y apt-transport-https
RUN apt-get update
# Use the noninteractive flag to stop being prompted for timezone.
RUN DEBIAN_FRONTEND=noninteractive apt-get install -y dotnet-sdk-3.1
RUN apt-get install -y python3-pip
RUN pip3 install str2bool pyinstaller
RUN make build RID=linux-x64
RUN make install RID=linux-x64
RUN useradd -ms /bin/bash coder
USER coder
