# ProjetoFinal

Template de solução .NET multiprojeto com arquitetura em camadas, ideal para aplicações corporativas organizadas em `src/`, `tests/`, e `docs/`.

Este repositório contém uma estrutura base com projetos separados por responsabilidade (Api, Application, Domain, Infra, etc.), prontos para serem clonados e reutilizados como template de desenvolvimento com `dotnet new`.

---

## 🚀 Como instalar o template

### 1. Clone este repositório

```bash
git clone https://github.com/wallacevff/ProjetoFinal.git
```

### 2. Instale o template localmente

Entre na pasta do repositório clonado:

```bash
cd ProjetoFinal
```

Instale o template com o comando:

```bash
dotnet new --install .
```

✅ Isso registra o template no seu ambiente local. Agora você pode usá-lo com o comando `dotnet new`.

---

## 🛠️ Como criar uma nova solução a partir do template

Use o nome do template (`shortName`):

```bash
dotnet new ses-template --name NomeDoSeuProjeto
```

Isso criará uma nova solução com todos os projetos e pastas organizadas:

```
NomeDoSeuProjeto/
├── src/
│   ├── NomeDoSeuProjeto.Api
│   ├── NomeDoSeuProjeto.Application
│   └── ...
├── tests/
├── docs/
├── NomeDoSeuProjeto.sln
```

---

## 🔁 Atualizar o template

Se você fizer alterações no repositório e quiser atualizar o template instalado:

```bash
dotnet new --install . --force
```

---

## ❌ Como remover o template

Se quiser desinstalar o template do seu ambiente local:

```bash
dotnet new --uninstall .
```

---

## 📁 Estrutura da Solução

| Projeto                | Responsabilidade                                        |
| ---------------------- | ------------------------------------------------------- |
| `src/Nome.Api`         | API ASP.NET Core                                        |
| `src/Nome.Application` | Casos de uso e orquestração de lógica de negócio        |
| `src/Nome.Domain`      | Entidades e regras de negócio                           |
| `src/Nome.Infra`       | Acesso a dados, integração com banco, serviços externos |
| `tests/Nome.Tests`     | Testes unitários e de integração                        |
| `docs/`                | Documentação técnica e funcional                        |

---

## 🧑‍💻 Autor

Wallace Vidal  
🔗 [https://github.com/wallacevff](https://github.com/wallacevff)
