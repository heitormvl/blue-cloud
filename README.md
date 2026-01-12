# Blue-Cloud
## Infraestrutura Automatizada para APIs .NET


### Roadmap:
#### 1. Aplicação e Dockerização

* **A Aplicação:** Uma Web API simples em .NET Core.
* **Docker:** `Dockerfile` otimizado (multi-stage build) para a aplicação.
* **Segurança:** Rodar sob HTTPS, expondo as portas corretas (80 e 443).

#### 2. Automação com Shell Script

* Script `setup-env.sh` que automatiza a configuração de um ambiente de desenvolvimento local em Linux:
    * Verifica se o Docker está instalado.
    * Cria redes Docker e volumes para persistência.
    * Faz o *build* da imagem localmente.

#### 3. Fluxo de Trabalho no GitHub

* **Branching:** **GitFlow** (branches `develop` e `main`).
* **GitHub Actions:** Workflow (`.github/workflows/main.yml`) que:
    1. Execute os testes unitários da aplicação .NET.
    2. Faça o scan de vulnerabilidades com o **Dependabot**.
    3. Gere a imagem Docker e faça o `push` para o Docker Hub ou Azure Container Registry.
    4. Faça o deploy automático para um **Azure App Service**.

#### 4. Infraestrutura e Redes

* **DNS & HTTPS:** Domínio da aplicação no Azure usa o certificado SSL/TLS (HTTPS).
* **Monitoramento:** Scripts ou as ferramentas do Azure para monitorar o consumo de CPU/Memória do container e agende uma tarefa (Cron/Azure Functions) para limpar logs antigos.