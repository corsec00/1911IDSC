# Guia de Instalação - Competition App PostgreSQL

## 📦 Conteúdo do Pacote

Este pacote contém uma versão **completamente limpa** do Competition App migrado para PostgreSQL, sem arquivos desnecessários ou conflitos.

### ✅ O que está incluído:

- **Código fonte limpo** sem dependências do Azure Table Storage
- **Modelos de dados** otimizados para PostgreSQL
- **Serviços** implementados com Entity Framework Core
- **Páginas Razor** funcionais e testadas
- **Scripts de configuração** do Azure
- **Pipeline CI/CD** configurado
- **Documentação completa**

### ❌ O que foi removido:

- Arquivos conflitantes do Azure Table Storage
- Dependências desnecessárias
- Código duplicado
- Arquivos de debug e temporários

## 🚀 Instalação Rápida

### 1. Extrair e configurar
```bash
# Extrair o pacote
unzip CompetitionApp_PostgreSQL_Clean_Final.zip
cd CompetitionApp_Clean

# Restaurar dependências
dotnet restore

# Verificar se compila
dotnet build
```

### 2. Configurar banco de dados

#### Opção A: PostgreSQL Local
```bash
# Instalar PostgreSQL
sudo apt-get install postgresql postgresql-contrib

# Criar banco
sudo -u postgres createdb competitiondb

# Configurar connection string
export DATABASE_CONNECTION_STRING="Host=localhost;Database=competitiondb;Username=postgres;Password=password;"
```

#### Opção B: Azure Database for PostgreSQL
```bash
# Executar script de configuração
./Scripts/setup-azure-postgresql.sh \
  -g "CompetitionApp-RG" \
  -s "competitionapp-db" \
  -u "dbadmin" \
  -p "SuaSenhaSegura123!"
```

### 3. Aplicar migrações
```bash
# Criar migração inicial
dotnet ef migrations add InitialCreate

# Aplicar ao banco
dotnet ef database update
```

### 4. Executar aplicação
```bash
dotnet run
```

## 🔧 Configuração Detalhada

### Connection String
Configure em `appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=seu-servidor;Database=competitiondb;Username=usuario;Password=senha;SSL Mode=Require;"
  }
}
```

### GitHub Actions
Configure os secrets:
- `DATABASE_CONNECTION_STRING`
- `AZURE_WEBAPP_PUBLISH_PROFILE`

## 📋 Checklist de Verificação

- [ ] ✅ Projeto compila sem erros
- [ ] ✅ Connection string configurada
- [ ] ✅ Migrações aplicadas
- [ ] ✅ Aplicação executa localmente
- [ ] ✅ Páginas carregam corretamente
- [ ] ✅ Banco de dados conecta
- [ ] ✅ GitHub Actions configurado (se aplicável)

## 🎯 Próximos Passos

1. **Testar funcionalidades básicas**
2. **Configurar ambiente de produção**
3. **Configurar backup automático**
4. **Personalizar conforme necessário**

## 📞 Suporte

Se encontrar problemas:
1. Verificar logs da aplicação
2. Confirmar connection string
3. Testar conectividade com banco
4. Consultar README.md para detalhes

## 🎉 Pronto!

Sua aplicação está configurada e pronta para uso com PostgreSQL!

