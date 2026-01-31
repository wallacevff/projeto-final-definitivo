# Repository Guidelines

## Project Structure & Module Organization
- `ProjetoFinal.sln` reúne nove projetos .NET dentro de `src/`, separados em camadas (`ProjetoFinal.Api`, `...Domain`, `...Infra`, `...IoC`) e contratos/serviços de aplicação.
- O frontend Angular mora em `src/ProjetoFinal.ClientApp`, com assets públicos em `public` e código em `src/app` (páginas em `pages/`, componentes compartilhados em `shared/`).
- Documentação viva está em `docs/` (`memoria_projeto.md`, `context.md`, `especificacao.md`, `propont.md`, `transcription.md`). Nunca sobrescreva entradas existentes nesses arquivos: apenas acrescente conteúdo.
- Antes de iniciar qualquer tarefa, leia `docs/transcription.md` e mantenha esse arquivo atualizado em cada interação, preservando a formatação (timestamp + autor + mensagem) e adicionando uma linha em branco entre falas do User e do Assistant.

## Build, Test, and Development Commands
- Backend: `dotnet build ProjetoFinal.sln` (compila toda a solução) e `dotnet test ProjetoFinal.sln` quando houver projetos de teste.
- Frontend: `npm install` (instala dependências), `npm start` (serve em `http://localhost:4200`), `npm run build` (gera artefatos de produção em `dist/` e copia para `wwwroot` via `postbuild`), `npm test` (roda Karma/Jasmine).
- Banco & migrations: use `dotnet ef migrations add -s ProjetoFinal.Api -p ProjetoFinal.Infra.Data <Nome>` e `dotnet ef database update ...` conforme exemplos em `docs/commands.md`.

## Coding Style & Naming Conventions
- C#: siga guidelines padrão da Microsoft — PascalCase para classes/métodos públicos, camelCase para campos privados, use `async`/`await` com sufixo `Async` e mantenha arquivos codificados em UTF-8 sem BOM.
- Angular/TypeScript: use `strict` typing, componentes em `kebab-case` (`courses.component.ts`), serviços terminados em `.service.ts`, módulos em `.module.ts`. Aplique lint/format via `ng lint` ou `eslint` (quando configurado) antes dos commits.
- CSS/SCSS: tokens de cor e utilitários globais residem em `src/styles.css`; prefira classes BEM para novos blocos.

## Testing Guidelines
- Back-end tests devem usar `xUnit`/`MSTest` (verifique o projeto correspondente) com convenção `MethodName_StateUnderTest_ExpectedBehavior`.
- Frontend testes unitários ficam ao lado do componente com sufixo `.spec.ts`. Use `ng test --watch=false --code-coverage` para gerar cobertura mínima aceitável (70% linha/branch quando aplicável).
- Para fluxos críticos (ex.: inscrições em cursos/turmas), complemente com testes e2e ou scripts manuais documentados em PRs.

## Commit & Pull Request Guidelines
- Commits **devem** estar em português do Brasil; siga padrão `tipo: breve descrição` (ex.: `feat: ajustar fluxo de inscrições interativas`). Descrições detalhadas também em pt-BR.
- PRs precisam: 1) descrever objetivo e impacto, 2) listar comandos/testes executados, 3) anexar evidências (logs, capturas de tela) quando UI for afetada, 4) referenciar issues/tarefas relevantes.
- Antes de abrir PR, atualize `docs/transcription*.txt` se a interação exigir, e confirme que builds (`dotnet build`, `npm run build`) passam sem novos avisos além dos já conhecidos (budgets Angular).
- Antes de qualquer commit, garanta que `docs/memoria_projeto.md` e `docs/context.md` estejam atualizados.
- Mantenha o relatorio geral sempre atualizado em `docs/relatorio.md` quando houver mudancas relevantes no projeto ou na documentacao.
- Sempre que novos arquivos `.md` forem adicionados ou removidos em `docs/`, atualize o `ProjetoFinal.sln` para incluir/remover esses arquivos na secao de Solution Items.

## Security & Configuration Tips
- JWT secrets e chaves de storage ficam fora do repositório; gere chaves com `node -p "crypto.randomBytes(256).toString('base64');"` conforme `docs/commands.md`.
- Uploads/downloads de mídia devem usar os endpoints do backend (com MinIO) — evite acessar o storage diretamente.
- Ao rodar `dotnet build`, feche processos que possam bloquear `src/ProjetoFinal.Api/bin` para evitar falhas de lock.
