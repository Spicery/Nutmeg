FROM ubuntu
RUN apt-get update && apt-get install -y build-essential python3 wget zip
RUN mkdir -p /tmp/nutmeg
WORKDIR /tmp/nutmeg
RUN wget https://packages.microsoft.com/config/ubuntu/20.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
RUN dpkg -i packages-microsoft-prod.deb
RUN apt-get install -y apt-transport-https
RUN apt-get update
# Use the noninteractive flag to stop being prompted for timezone.
RUN DEBIAN_FRONTEND=noninteractive apt-get install -y dotnet-sdk-3.1
#RUN wget https://github.com/Spicery/Nutmeg/archive/integration.zip <-change to this.
RUN wget   https://github.com/Spicery/Nutmeg/archive/v0.1/11_build_and_install.zip
#RUN unzip integration.zip
RUN unzip 11_build_and_install.zip
#WORKDIR Nutmeg-integration
WORKDIR Nutmeg-0.1-11_build_and_install
RUN apt-get install -y python3-pip
RUN pip3 install str2bool pyinstaller
RUN make build RID=linux-x64
RUN make install RID=linux-x64
RUN ls /usr/local/bin 
RUN ls /opt/nutmeg
