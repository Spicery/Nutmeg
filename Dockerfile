FROM ubuntu:20.04 AS nutmeg-base
ARG DEBIAN_FRONTEND=noninteractive
RUN apt-get update 
RUN apt-get install -y apt-transport-https
RUN apt-get install -y tar gzip zip wget curl 
RUN apt-get install -y ca-certificates build-essential software-properties-common python3 python3-pip git-all
# Dotnet deb packages in dedicated repo; update to get fresh data.
RUN curl -sSL https://packages.microsoft.com/keys/microsoft.asc | apt-key add -
RUN apt-add-repository https://packages.microsoft.com/ubuntu/20.04/prod
RUN apt-get update 
RUN apt-get install -y --no-install-recommends dotnet-sdk-3.1 

########################################
# For vscode development
# 
FROM nutmeg-base AS nutmeg-vscode
ARG DEBIAN_FRONTEND=noninteractive
ARG INSTALL_ZSH="true"
ARG UPGRADE_PACKAGES="true"
ARG USERNAME=vscode
ARG USER_UID=1000
ARG USER_GID=$USER_UID
RUN bash -c "$(curl -fsSL "https://raw.githubusercontent.com/microsoft/vscode-dev-containers/v0.140.1/script-library/common-debian.sh")"
    # && apt-get clean -y \
    # && rm -rf /var/lib/apt/lists/*
RUN pip3 install pytest pylint black
# This may not be ideal, but global install latest seems OK for these tools
# Cannot install requirements.txt because cannot start 
# Could we do better if we assume pipenv?

########################################
# For running from command line: docker run -it --rm <image_id>:live-install
# 
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

########################################
# For use in Circle CI pipeline.
# Circle CI expects to pull container image from a repo.
# Currently publishing to philallen117 account in Docker Hub.
# Make a Spicery account in Docker Hub? Or in GitHub Container Repos?
# 
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
