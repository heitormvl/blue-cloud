    # stage 1: build
    # Utiliza a imagem oficial do .NET SDK como imagem base para construir o aplicativo
    FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
    # Define o diretório de trabalho dentro do container
    WORKDIR /src
    # Copia o arquivo de projeto para o diretório de trabalho (aproveite o cache de camadas do Docker)
    COPY BlueCloudApi/BlueCloudApi.csproj BlueCloudApi/
    # Restaura as dependências do projeto
    RUN dotnet restore
    # Copia todo o código-fonte para o diretório de trabalho
    COPY . .
    WORKDIR /src/BlueCloudApi
    # Publica o aplicativo em modo Release para o diretório /app/publish
    RUN dotnet publish -c Release -o /app/publish

    # stage 2: runtime
    # Utiliza a imagem oficial do .NET Runtime como imagem base para executar o aplicativo
    FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
    # Define o diretório de trabalho dentro do container
    WORKDIR /app
    # Cria a pasta data e concede permissão de escrita
    RUN mkdir -p data && chmod -R 777 data
    # Copia os arquivos publicados do estágio de build para o diretório de trabalho
    COPY --from=build /app/publish .
    # Expõe a porta 80 para acesso ao aplicativo e configura a variável de ambiente ASPNETCORE_URLS
    ENV ASPNETCORE_URLS=http://+:80
    EXPOSE 80

    # Define o ponto de entrada do container para executar o aplicativo
    ENTRYPOINT ["dotnet", "BlueCloudApi.dll"]