# To build this file:
#   > docker build -t tmarkovski/sawtooth-sdk-net .
#
# To run interactively after build
#   > docker run -it --entrypoint "/bin/bash" tmarkovski/sawtooth-sdk-net
#
# In order to run the examples, change to their working directory and run the projects
#   > cd /sawtooth/examples
#   > dotnet run Processor tcp://127.0.0.1:4004
#   > dotnet run Client mykey set 42

FROM microsoft/dotnet

COPY . /sawtooth

EXPOSE 4004 8008

WORKDIR /sawtooth/src

RUN dotnet restore \
 && dotnet publish -c Release -f netstandard2.0 Sdk

CMD [ "dotnet", "test", "Test" ]
