FROM ubuntu:20.04 AS nutmeg-base
ENV TZ=Europe/London
ARG DEBIAN_FRONTEND=noninteractive
RUN apt-get update 
RUN apt-get install -y apt-transport-https
RUN apt-get install -y wget curl ca-certificates build-essential python3 python3-pip

RUN curl -sSL https://packages.microsoft.com/keys/microsoft.asc | apt-key add -
RUN apt-get install -y software-properties-common
RUN apt-add-repository https://packages.microsoft.com/ubuntu/20.04/prod
RUN apt-get update
RUN DEBIAN_FRONTEND=noninteractive apt-get install -y --no-install-recommends dotnet-sdk-3.1 

FROM nutmeg-base AS nutmeg-vscode
ENV TZ=Europe/London
ARG DEBIAN_FRONTEND=noninteractive
RUN bash -c "$(curl -fsSL "https://raw.githubusercontent.com/microsoft/vscode-dev-containers/v0.140.1/script-library/common-debian.sh")"
    # && apt-get clean -y \
    # && rm -rf /var/lib/apt/lists/*

# or FROM vscode ???
FROM nutmeg-base AS circleci
RUN DEBIAN_FRONTEND=noninteractive apt-get install -y openssh-server tar gzip git-all
RUN systemctl enable ssh
# RUN export DEBIAN_FRONTEND=noninteractive \
#     && systemctl enable ufw \
#     && systemctl start ufw \
#     && ufw default allow outgoing \
#     && ufw default deny incoming \
#     && ufw allow ssh

FROM nutmeg-base AS live-install
RUN apt-get install -y zip
RUN mkdir -p /tmp/nutmeg
WORKDIR /tmp/nutmeg RUN wget   https://github.com/Spicery/Nutmeg/archive/integration.zip
RUN unzip integration.zip
WORKDIR /tmp/nutmeg/Nutmeg-integration
RUN pip3 install str2bool pyinstaller
RUN make build RID=linux-x64
RUN make install RID=linux-x64
RUN ls /usr/local/bin 
RUN ls /opt/nutmeg
