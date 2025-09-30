# Gerar JWT Key
    node -p "crypto.randomBytes(256).toString('base64');"

# Migrations
## Terminal Gerenciador de Pacotes
- Listar Migrations

      Get-Migration -StartUp ProjetoFinal.Api -Project ProjetoFinal.Infra.Data

- Criar Migration

      Add-Migration -StartUp ProjetoFinal.Api -Project ProjetoFinal.Infra.Data <Nome da Migration>
- Remover Migration

      Remove-Migration -StartUp ProjetoFinal.Api -Project ProjetoFinal.Infra.Data
- Atualizar o Banco de Dados

      Update-Database -StartUp ProjetoFinal.Api -Project ProjetoFinal.Infra.Data <Nome da Migration>
- Listar Migrations

      Get-Migration -s ProjetoFinal.Api -p ProjetoFinal.Infra.Data

## Terminal Comum
- Criar Migration

      dotnet ef migrations add -s ProjetoFinal.Api -p ProjetoFinal.Infra.Data <Nome da Migration>
- Remover Migration

      dotnet ef migrations remove -s ProjetoFinal.Api -p ProjetoFinal.Infra.Data
- Atualizar o Banco de Dados

      dotnet ef database update -s ProjetoFinal.Api -p ProjetoFinal.Infra.Data
- Listar Migrations

      dotnet ef migrations list -s ProjetoFinal.Api -p ProjetoFinal.Infra.Data