FROM microsoft/dotnet

WORKDIR /sdk

COPY src/ /sdk

EXPOSE 4004

RUN dotnet restore
RUN dotnet publish -c Release -f netstandard2.0 Sdk

CMD [ "dotnet", "test", "Test" ]
