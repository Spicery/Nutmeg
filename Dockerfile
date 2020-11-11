FROM ubuntu:20.04 AS nutmeg-base
# ENV TZ=Europe/London
ARG DEBIAN_FRONTEND=noninteractive
# RUN apt-get install -y systemd && timedatectl set-timezone Europe/London
RUN apt-get update 
RUN apt-get install -y apt-transport-https
RUN apt-get install -y tar gzip zip wget curl 
RUN apt-get install -y ca-certificates build-essential python3 python3-pip git-all

RUN curl -sSL https://packages.microsoft.com/keys/microsoft.asc | apt-key add -
RUN apt-get install -y software-properties-common
RUN apt-add-repository https://packages.microsoft.com/ubuntu/20.04/prod
RUN apt-get update
RUN apt-get install -y --no-install-recommends dotnet-sdk-3.1 

FROM nutmeg-base AS nutmeg-vscode
ENV TZ=Europe/London
ARG DEBIAN_FRONTEND=noninteractive
RUN bash -c "$(curl -fsSL "https://raw.githubusercontent.com/microsoft/vscode-dev-containers/v0.140.1/script-library/common-debian.sh")"
    # && apt-get clean -y \
    # && rm -rf /var/lib/apt/lists/*

FROM nutmeg-base AS live-install
ARG DEBIAN_FRONTEND=noninteractive
RUN pip3 install str2bool pyinstaller
RUN mkdir -p /tmp/nutmeg
WORKDIR /tmp/nutmeg
RUN wget https://github.com/Spicery/Nutmeg/archive/integration.zip
RUN unzip integration.zip
WORKDIR /tmp/nutmeg/Nutmeg-integration
RUN make build RID=linux-x64
RUN make install RID=linux-x64
WORKDIR /tmp

# or FROM vscode ???
FROM nutmeg-base AS circleci
ARG DEBIAN_FRONTEND=noninteractive
RUN apt-get install -y --no-install-recommends openssh-client

# install dockerize
RUN DOCKERIZE_URL="https://circle-downloads.s3.amazonaws.com/circleci-images/cache/linux-amd64/dockerize-latest.tar.gz" \
  && curl --silent --show-error --location --fail --retry 3 --output /tmp/dockerize-linux-amd64.tar.gz $DOCKERIZE_URL \
  && tar -C /usr/local/bin -xzvf /tmp/dockerize-linux-amd64.tar.gz \
  && rm -rf /tmp/dockerize-linux-amd64.tar.gz \
  && dockerize --version

RUN groupadd --gid 3434 circleci \
  && useradd --uid 3434 --gid circleci --shell /bin/bash --create-home circleci \
  && echo 'circleci ALL=NOPASSWD: ALL' >> /etc/sudoers.d/50-circleci \
  && echo 'Defaults    env_keep += "DEBIAN_FRONTEND"' >> /etc/sudoers.d/env_keep

USER circleci
ENV PATH /home/circleci/.local/bin:/home/circleci/bin:${PATH}

CMD ["/bin/sh"]

