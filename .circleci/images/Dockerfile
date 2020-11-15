FROM cimg/python:3.8
ARG DEBIAN_FRONTEND=noninteractive
# Dotnet deb packages in dedicated repo
# Install utilities first
# Add repo
# Update to ensure fresh package data.
USER root
RUN apt-get install -y ca-certificates software-properties-common
RUN curl -sSL https://packages.microsoft.com/keys/microsoft.asc | apt-key add -
RUN apt-add-repository https://packages.microsoft.com/ubuntu/20.04/prod
RUN apt-get update --fix-missing
RUN apt-get install -y --no-install-recommends dotnet-sdk-3.1 
USER circleci
