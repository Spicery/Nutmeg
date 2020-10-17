FROM ubuntu AS base
RUN apt-get update 
RUN apt-get install -y apt-transport-https
RUN apt-get install -y build-essential python3 wget zip curl
RUN curl -sSL https://packages.microsoft.com/keys/microsoft.asc | sudo apt-key add -
RUN apt-add-repository https://packages.microsoft.com/ubuntu/20.04/prod
RUN apt-get update


# Use the noninteractive flag to stop being prompted for timezone.
RUN DEBIAN_FRONTEND=noninteractive apt-get install -y dotnet-sdk-3.1


FROM base AS 

FROM base AS live-install
RUN mkdir -p /tmp/nutmeg
WORKDIR /tmp/nutmeg RUN wget   https://github.com/Spicery/Nutmeg/archive/integration.zip
RUN unzip integration.zip
WORKDIR /tmp/nutmeg/Nutmeg-integration
RUN apt-get install -y python3-pip
RUN pip3 install str2bool pyinstaller
RUN make build RID=linux-x64
RUN make install RID=linux-x64
RUN ls /usr/local/bin 
RUN ls /opt/nutmeg
