FROM ubuntu:20.04 AS base
ARG DEBIAN_FRONTEND=noninteractive
RUN apt-get update --fix-missing
RUN apt-get install -y apt-transport-https
RUN apt-get install -y tar gzip zip wget curl 
RUN apt-get install -y build-essential software-properties-common
RUN apt-get install -y python3 python3-pip git-all
# Dotnet deb packages in dedicated repo; update to get fresh data.
# Install utilities first
RUN apt-get install -y ca-certificates software-properties-common
RUN curl -sSL https://packages.microsoft.com/keys/microsoft.asc | apt-key add -
RUN apt-add-repository https://packages.microsoft.com/ubuntu/20.04/prod
RUN apt-get update 
RUN apt-get install -y --no-install-recommends dotnet-sdk-3.1 
# TODO 67 - factor out common base image that devcontainer can share

########################################
# For running from command line: 
#   docker run -it --rm <image_id>:packaged
# 
FROM base AS packaged
ARG DEBIAN_FRONTEND=noninteractive
RUN pip3 install str2bool pyinstaller
# TODO: #66 change to install from requirements files - with pyinstaller in separate build_requirements
RUN mkdir -p /tmp/nutmeg
COPY compiler/ /tmp/nutmeg/compiler/
COPY runner/ /tmp/nutmeg/runner/
COPY scripts/ /tmp/nutmeg/scripts
COPY Makefile /tmp/nutmeg/
WORKDIR /tmp/nutmeg
RUN make build RID=linux-x64
RUN make install RID=linux-x64
WORKDIR /tmp
# Set up basic dev environment.
RUN apt-get install -y sudo nano vim less file
RUN useradd -ms /bin/bash coder
RUN usermod -aG sudo coder
RUN echo 'coder:nutmeg<3' | chpasswd
# Switch to the unprivileged user 'coder'
USER coder
WORKDIR /home/coder
RUN touch /home/coder/.sudo_as_admin_successful
