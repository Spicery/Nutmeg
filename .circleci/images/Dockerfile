FROM cimg/base:stable-20.04
ARG DEBIAN_FRONTEND=noninteractive
USER root
RUN apt-get update

# Utilities already installed
# Install Python

RUN apt-get install -y python3.8
RUN apt-get install -y python3-pip
RUN python3 -m pip install --upgrade pip

# Dotnet deb packages in dedicated repo
# Add repo
# Update to ensure fresh package data.
RUN curl -sSL https://packages.microsoft.com/keys/microsoft.asc | apt-key add -
RUN apt-add-repository https://packages.microsoft.com/ubuntu/20.04/prod
RUN apt-get update --fix-missing
RUN apt-get install -y --no-install-recommends dotnet-sdk-3.1 

USER circleci
