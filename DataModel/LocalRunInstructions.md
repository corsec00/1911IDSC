# Instruções para Execução Local da Aplicação de Competição

Este documento contém instruções detalhadas para executar e testar a aplicação de competição localmente usando o Visual Studio Code.

## Pré-requisitos

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) instalado em sua máquina
- [Visual Studio Code](https://code.visualstudio.com/) instalado
- Extensão [C#](https://marketplace.visualstudio.com/items?itemName=ms-dotnettools.csharp) instalada no VS Code
- Extensão [.NET Core Tools](https://marketplace.visualstudio.com/items?itemName=formulahendry.dotnet) (opcional, mas recomendada)

## Configuração Inicial

### 1. Abrir o Projeto no VS Code

1. Extraia o arquivo ZIP do projeto para uma pasta em seu computador
2. Abra o Visual Studio Code
3. Selecione **File > Open Folder** (ou **Arquivo > Abrir Pasta**)
4. Navegue até a pasta onde você extraiu o projeto e selecione-a
5. Aguarde o VS Code carregar o projeto e restaurar as dependências

### 2. Restaurar Pacotes NuGet

1. Abra o terminal integrado do VS Code (**Terminal > New Terminal** ou **Ctrl+`**)
2. Execute o comando:
   ```
   dotnet restore
   ```
3. Aguarde a conclusão da restauração dos pacotes

## Executando a Aplicação

### Método 1: Usando o Terminal

1. No terminal integrado do VS Code, execute:
   ```
   dotnet run
   ```
2. Aguarde a compilação e inicialização da aplicação
3. O terminal mostrará uma URL local (geralmente `https://localhost:5001` ou `http://localhost:5000`)
4. Abra esta URL em seu navegador para acessar a aplicação

### Método 2: Usando a Extensão .NET Core Tools

1. Clique no ícone de execução na barra lateral do VS Code (ou pressione **F5**)
2. Selecione **.NET Core** como ambiente
3. Selecione **CompetitionApp** como projeto
4. A aplicação será compilada e iniciada automaticamente
5. Um navegador será aberto com a aplicação em execução

### Método 3: Usando o Comando dotnet watch (Desenvolvimento com Hot Reload)

1. No terminal integrado, execute:
   ```
   dotnet watch run
   ```
2. Este comando monitora alterações nos arquivos e reinicia automaticamente a aplicação quando necessário
3. Um navegador será aberto automaticamente com a aplicação em execução

## Depurando a Aplicação

1. Defina pontos de interrupção (breakpoints) clicando à esquerda do número da linha no editor
2. Pressione **F5** para iniciar a depuração
3. A execução será pausada nos pontos de interrupção, permitindo inspecionar variáveis e o estado da aplicação
4. Use os controles de depuração na parte superior do VS Code para continuar, avançar, etc.

## Testando a Aplicação

1. Acesse a aplicação no navegador (geralmente em `https://localhost:5001` ou `http://localhost:5000`)
2. Teste o fluxo completo:
   - Cadastre participantes na página "Participantes"
   - Registre resultados na "Primeira Rodada"
   - Registre resultados na "Segunda Rodada"
   - Visualize a classificação final em "Resultados"
   - Teste a exportação para PDF

## Solução de Problemas Comuns

### Erros de Sintaxe Razor

Se você encontrar erros relacionados à sintaxe Razor (como RZ1031):

1. Evite usar expressões C# diretamente em atributos HTML. Em vez de:
   ```
   <option value="@name" @(completed ? "disabled" : "")>
   ```
   Use estruturas condicionais:
   ```
   @if (completed) {
       <option value="@name" disabled>@name</option>
   } else {
       <option value="@name">@name</option>
   }
   ```

2. Certifique-se de que o arquivo `_Layout.cshtml` não tenha a diretiva `@page` no topo

### Erros de Namespace ou Modelo

Se você encontrar erros relacionados a namespaces ou modelos não encontrados:

1. Verifique se os arquivos `_ViewImports.cshtml` e `_ViewStart.cshtml` estão presentes na pasta `Pages`
2. Confirme que todas as páginas Razor (.cshtml) têm a diretiva `@model` correta com o namespace completo
3. Certifique-se de que os arquivos code-behind (.cshtml.cs) declaram o namespace correto

### Erro de certificado HTTPS

Se você encontrar erros relacionados ao certificado de desenvolvimento:

1. Execute no terminal:
   ```
   dotnet dev-certs https --trust
   ```
2. Confirme a operação quando solicitado
3. Reinicie o navegador e a aplicação

### Erro de porta em uso

Se a porta padrão estiver em uso:

1. Modifique o arquivo `Properties/launchSettings.json` para usar portas diferentes
2. Ou encerre o processo que está usando a porta com:
   ```
   # No Windows
   netstat -ano | findstr :5000
   taskkill /PID [número_do_processo] /F
   
   # No Linux/Mac
   lsof -i :5000
   kill -9 [número_do_processo]
   ```

### Erro de dependências

Se houver problemas com dependências:

1. Limpe a solução:
   ```
   dotnet clean
   ```
2. Restaure os pacotes novamente:
   ```
   dotnet restore
   ```
3. Tente compilar e executar novamente

## Conclusão

Seguindo estas instruções, você poderá executar e testar a aplicação de competição localmente em sua máquina. As correções aplicadas nesta versão resolvem os problemas de sintaxe Razor, namespaces e modelos que impediam a compilação do projeto. Agora a aplicação deve compilar e executar sem erros, permitindo testar todas as funcionalidades antes de considerar a implantação em um ambiente de produção.
