# Instruções para Publicação da Aplicação Web no Azure

Este documento contém instruções detalhadas para publicar a aplicação de competição no Azure Web App utilizando o modelo CODE com runtime stack .NET 8 (LTS), incluindo configuração de CI/CD com GitHub Actions.

## Pré-requisitos

- Uma conta do Microsoft Azure ativa
- Uma conta no GitHub
- Repositório GitHub configurado em github.com/corsec/1911sc
- .NET 8 SDK instalado em sua máquina local (para desenvolvimento)

## Opções de Publicação

Existem duas formas principais de publicar a aplicação:
1. **Publicação Manual** - Para implantações pontuais ou iniciais
2. **Publicação Automatizada via GitHub Actions** - Para integração e entrega contínuas (CI/CD)

## 1. Configuração Inicial do Azure Web App

### Criação do Azure Web App

1. Acesse o [Portal do Azure](https://portal.azure.com)
2. Clique em "Criar um recurso"
3. Pesquise por "Web App" e selecione esta opção
4. Clique em "Criar"
5. Preencha as informações básicas:
   - **Assinatura**: Selecione sua assinatura do Azure
   - **Grupo de Recursos**: Crie um novo ou selecione um existente
   - **Nome**: Escolha um nome único para sua aplicação (será parte da URL)
   - **Publicar**: Selecione **Código**
   - **Runtime stack**: Selecione **.NET 8 (LTS)**
   - **Sistema Operacional**: Windows
   - **Região**: Escolha a região mais próxima de seus usuários
   - **Plano do Serviço de Aplicativo**: Crie um novo ou selecione um existente
   - **Plano de preços**: Escolha o plano adequado às suas necessidades

6. Clique em "Revisar + criar" e, em seguida, em "Criar"
7. Aguarde a conclusão da implantação

## 2. Configuração do CI/CD com GitHub Actions

### Passo 1: Obter o Perfil de Publicação do Azure

1. No Portal do Azure, acesse o Web App criado anteriormente
2. No menu lateral esquerdo, selecione "Centro de Implantação"
3. Clique em "Credenciais de Implantação" e depois em "Perfil de Publicação"
4. Clique no botão "Baixar Perfil de Publicação"
5. Salve o arquivo XML baixado (contém credenciais sensíveis)

### Passo 2: Configurar o Segredo no GitHub

1. Acesse o repositório GitHub em [github.com/corsec/1911sc](https://github.com/corsec/1911sc)
2. Clique na aba "Settings" (Configurações)
3. No menu lateral esquerdo, clique em "Secrets and variables" e depois em "Actions"
4. Clique no botão "New repository secret"
5. Configure o segredo:
   - **Nome**: `AZURE_WEBAPP_PUBLISH_PROFILE`
   - **Valor**: Cole todo o conteúdo do arquivo XML do perfil de publicação baixado anteriormente
6. Clique em "Add secret"

### Passo 3: Configurar o Workflow do GitHub Actions

1. No repositório, crie a pasta `.github/workflows/` se ainda não existir
2. Crie um arquivo chamado `azure-deploy.yml` nesta pasta
3. Adicione o seguinte conteúdo ao arquivo (já incluído no repositório):

```yaml
name: Deploy to Azure Web App

on:
  push:
    branches: [ main ]
  workflow_dispatch:

jobs:
  build-and-deploy:
    runs-on: ubuntu-latest

    steps:
    - uses: actions/checkout@v3

    - name: Set up .NET
      uses: actions/setup-dotnet@v3
      with:
        dotnet-version: '8.0.x'
        
    - name: List directory contents
      run: ls -la
      
    - name: Find csproj files
      run: find . -name "*.csproj"

    - name: Build with dotnet
      run: |
        # Especifica o caminho completo para o arquivo .csproj
        dotnet build ./CompetitionApp.csproj --configuration Release
      
    - name: dotnet publish
      run: |
        # Especifica o caminho completo para o arquivo .csproj
        dotnet publish ./CompetitionApp.csproj -c Release -o ${{env.DOTNET_ROOT}}/myapp

    - name: Deploy to Azure Web App
      uses: azure/webapps-deploy@v2
      with:
        app-name: 'your-app-name'  # Substitua pelo nome do seu Web App no Azure
        slot-name: 'production'
        publish-profile: ${{ secrets.AZURE_WEBAPP_PUBLISH_PROFILE }}
        package: ${{env.DOTNET_ROOT}}/myapp
```

**Nota importante**: 
1. Os comandos `dotnet build` e `dotnet publish` agora especificam explicitamente o caminho completo para o arquivo `./CompetitionApp.csproj`.
2. Adicionamos etapas de diagnóstico (`List directory contents` e `Find csproj files`) para ajudar a identificar a estrutura do repositório e localizar os arquivos .csproj.
3. Estas alterações são necessárias para evitar o erro "MSB1011: Specify which project or solution file to use" que ocorre quando há múltiplos arquivos de projeto no repositório ou quando o arquivo não é encontrado no diretório atual.

4. **Importante**: Substitua `'your-app-name'` pelo nome real do seu Web App no Azure

### Passo 4: Ativar o Workflow

1. Faça commit do arquivo `azure-deploy.yml` no repositório
2. Faça push para a branch `main`
3. Acesse a aba "Actions" no GitHub para verificar se o workflow foi iniciado
4. Aguarde a conclusão da execução do workflow

## 3. Como Funciona o CI/CD

Após a configuração, o processo de CI/CD funcionará da seguinte forma:

1. **Trigger**: Sempre que houver um push na branch `main` do repositório, o workflow será acionado automaticamente
2. **Build**: O GitHub Actions irá:
   - Configurar o ambiente .NET 8
   - Compilar a aplicação
   - Publicar a aplicação para implantação
3. **Deploy**: O GitHub Actions implantará automaticamente a aplicação no Azure Web App configurado
4. **Verificação**: Você pode acessar a URL do seu Web App para verificar a implantação: `https://<nome-do-seu-webapp>.azurewebsites.net`

## 4. Atualizações Futuras

Para atualizar a aplicação:

1. Faça as alterações necessárias no código
2. Faça commit das alterações
3. Faça push para a branch `main` do repositório
4. O GitHub Actions será acionado automaticamente e implantará a nova versão

## 5. Execução Manual do Workflow

Se precisar acionar o workflow manualmente:

1. Acesse o repositório no GitHub
2. Clique na aba "Actions"
3. Selecione o workflow "Deploy to Azure Web App"
4. Clique no botão "Run workflow"
5. Selecione a branch `main` e clique em "Run workflow"

## 6. Solução de Problemas Comuns

### Falha no Workflow do GitHub Actions

1. Verifique os logs de execução na aba "Actions" do GitHub
2. Certifique-se de que o segredo `AZURE_WEBAPP_PUBLISH_PROFILE` está configurado corretamente
3. Verifique se o nome do Web App no arquivo `azure-deploy.yml` está correto

### A aplicação não carrega ou exibe erro 500

1. Verifique os logs da aplicação no Portal do Azure:
   - Acesse seu Web App
   - No menu lateral esquerdo, selecione "Logs de aplicativo"
   - Analise os erros reportados

2. Verifique se todas as dependências estão corretamente referenciadas no arquivo `.csproj`

3. Certifique-se de que o runtime stack selecionado é .NET 8 (LTS)

## Conclusão

Seguindo estas instruções, sua aplicação de competição estará disponível online através do Azure Web App, utilizando o modelo CODE com runtime stack .NET 8 (LTS), e com um pipeline de CI/CD configurado para atualizações automáticas sempre que o código for atualizado no GitHub. Os usuários poderão acessar a aplicação de qualquer dispositivo com acesso à internet, cadastrar participantes, registrar resultados e exportar relatórios em PDF.
