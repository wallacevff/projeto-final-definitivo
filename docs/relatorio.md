# Relatorio Geral do Projeto EAD

## Data
2026-01-06

## Fontes analisadas
- docs/especificacao.md
- docs/propont.md
- docs/memoria_projeto.md
- docs/context.md
- Estrutura do repositorio (src/ e docs/)

## Visao geral do projeto
Plataforma EAD com duas modalidades de curso: turmas interativas e distribuicao de material didatico. O sistema contempla cadastro de usuarios (aluno/professor), autenticacao JWT, criacao e publicacao de cursos, turmas, conteudos, atividades, forum e chat, com upload/download de midias via MinIO. O frontend Angular oferece dashboards para aluno e professor, gerenciamento de cursos/turmas e fluxo completo de inscricoes e submissao/correcao de atividades.

## Status por modulo (resumo)
### Autenticacao e usuarios
- Login JWT com guardas, interceptor e roles.
- Cadastro publico de usuario (aluno/professor) via /auth/register com hash BCrypt.

### Cursos e turmas
- Criacao/edicao/publicacao de cursos e turmas no frontend, com pre-selecao de curso ao criar turma pela gerencia.
- Cursos interativos com turmas, capacidade, aprovacao e codigo de inscricao.
- Cursos nao interativos (distribuicao) com acesso aberto a materiais e forum.
- API restringe acesso de instrutores apenas aos proprios cursos e turmas.

### Inscricoes e matrículas
- Inscricao de aluno em distribuicao (course-subscriptions) e em turmas interativas (class enrollments).
- Dashboard do aluno mescla inscricoes de distribuicao e matrículas em turmas interativas, com status.

### Conteudos e midias
- Upload de midias via API com deduplicacao por hash (MinIO).
- Conteudos com anexos e viewer com download autenticado.
- Anotacoes em videos de conteudo persistidas no backend.

### Atividades e correcoes
- Criacao de atividades por turma ou para todas as turmas.
- Envio de atividades pelo aluno com rich-text e anexos.
- Gerencia e correcao pelo professor em turma e em tela dedicada por atividade.
- Visualizacao de status/nota/feedback pelo aluno.
- Listagem e gestao de atividades restritas ao instrutor proprietario do curso.

### Forum e chat
- Forum com topicos por turma, posts autenticados, replies e pagina dedicada de discussao.
- Criacao de topicos restrita a professores; alunos podem responder.
- Forum filtrado por instrutor para impedir acesso a topicos de outros cursos.
- UI do topico renderiza respostas em niveis recursivos.

### Dashboard professor
- Indicadores de alunos matriculados e total de cursos interativos/nao interativos do instrutor.
- Secao de atividade recente em foruns mostrando os 3 topicos com ultima interacao e data.

### Documentacao
- Memoria/contexto em Markdown com historico de evolucao.
- Especificacao/proposta/transcricao migradas para .md.

## Conformidade com requisitos principais
- Duas modalidades de curso (interativo/distribuicao): implementado.
- Turmas com limite, inscricao com aprovacao/codigo: implementado.
- Forum e chat: forum implementado; chat previsto no modelo e habilitado em cursos/turmas.
- Upload de midias e anexos: implementado (MinIO).
- Comentarios em video: implementado para conteudos e anotacoes em submissao.
- Inscricao de alunos em qualquer curso: implementado (distribuicao + interativo).
- Pesquisa de cursos por nome/categoria: parcialmente (filtro por titulo/categoria via API).

## Cronograma resumido (implementado)
- 2025-09/10: base da API, contratos, estrutura Angular e shell.
- 2025-10: login JWT, criacao de cursos, turmas e publicacao; ajustes de rotas /api/v1.
- 2025-11: MinIO, uploads/downloads, viewer de conteudos/atividades, rich-text editor, inscricoes e dashboard aluno.
- 2025-11 fim: correcao de atividades, restricoes de exibicao, ajustes de forum.
- 2026-01: anotacoes de video persistentes, melhorias no UI/UX, forum com discussao, correcoes por atividade, cadastro publico de usuario.
- 2026-01-06: dashboard professor com contagens reais; docs migradas para Markdown.

## Pendencias e proximos passos sugeridos
- Revisar avisos de bundle/CSS do Angular antes do deploy final.
- Validar e finalizar fluxos de chat em tempo real (definir implementacao e UI).
- Reforcar validacoes/limpeza do HTML rich-text no backend (seguranca).
- Completar pesquisa de cursos no frontend por nome/categoria com UI dedicada.
- Revisar politicas de permissao (professor/aluno) em todos os endpoints sensiveis.
- Adicionar testes automatizados (unitarios/e2e) para inscricao, envio de atividades e forum.

## Observacoes
- Builds recentes executados com sucesso; warnings de budget/seletores ja conhecidos no Angular.
- Documentacao deve seguir append-only em memoria/contexto e transcricao.

## Cronograma de melhorias de usabilidade (iniciado)
### Itens planejados
- Consistencia de linguagem e acentos em rotulos e mensagens do frontend.
- Acessibilidade: foco visivel para links/botoes/inputs.
- Responsividade de tabelas densas (scroll horizontal em telas menores).
- Feedback visual de envio de anexos (indicadores durante upload).
- Performance percebida: reduzir warnings de budget (acao continua).

### Alteracoes ja aplicadas
- Acessibilidade: foco visivel global em `styles.css`.
- Responsividade: tabelas de inscricoes/submissoes com wrapper de scroll.
- Feedback de anexos: indicador visual animado durante envio.

### Proximos passos
- Padronizar textos com acentos/idioma em todos os modulos.
- Revisar componentes mais densos para simplificar a hierarquia visual.
- Definir estrategia para reduzir tamanhos de CSS/bundle.

### 2026-02-02
- Empacotamento para homologacao: Dockerfile multi-stage e docker-compose com SQL Server e MinIO. Configuracao Docker em `docker/appsettings.Docker.json` e uso de configs externos no swarm.
- Ajuste de appsettings.json para usar hosts do stack (projeto-final_sqlserver/projeto-final_minio).

### 2026-03-12
- Correcao de resiliencia no upload de midias: a API so reaproveita `MediaResources` por hash quando o objeto existe no MinIO; caso contrario, reenvia o arquivo e reidrata o registro.
- Evolucao da camada de storage com `ExistsAsync` no contrato e implementacao MinIO por `StatObject`.
- Entregue script operacional `docs/saneamento_media_resources_orfaos.sql` para limpeza controlada de registros orfaos sem vinculos em outras entidades.

### 2026-03-13
- Resolvido erro de build Docker `NETSDK1152` no publish da API removendo inclusao duplicada dos artefatos SPA no `ProjetoFinal.Api.csproj`.
- Validacao de regressao: `dotnet build`, `npm run build` e `docker build` executados com sucesso (warnings Angular conhecidos mantidos).

### 2026-03-13 (tema e nomenclatura)
- Aplicado tema azul na sidebar e componentes de acao relacionados no shell do frontend.
- Removida nomenclatura "SES" dos titulos/branding do frontend, padronizando para "Plataforma EAD".
- Validacao de build concluida com `dotnet build` e `npm run build` (apenas warnings Angular ja conhecidos).

### 2026-03-20 (feedback estruturado de correcoes)
- Implementado feedback estruturado nas submissoes de atividades com novos campos persistidos: `MasteryScore`, `ApplicationScore`, `CommunicationScore` (rubrica 1-5), `FeedbackTags` (tags diagnosticas) e `RecommendedAction` (acao recomendada).
- Backend atualizado em entidade/DTOs/servico com validacao de rubrica (faixa 1-5) e normalizacao de tags para armazenamento consistente.
- Migration EF criada: `20260320204713_AddStructuredActivityFeedback` com inclusao das novas colunas em `ActivitySubmissions`.
- Frontend de correcao atualizado nas telas de professor (`activity-corrections` e `class-group-manage`) com novos campos no formulario.
- Visualizacao do aluno atualizada em `course-activity-viewer` para exibir rubrica, tags e acao recomendada quando presentes.
- Validacao tecnica concluida com `dotnet build ProjetoFinal.sln` e `npm run build` (warnings Angular de budget/seletores permanecem conhecidos).
- Ajuste de usabilidade no campo de Tags diagnosticas: substituido `select multiple` por dropdown com checklist nas telas de correcao, permitindo abrir menu e selecionar multiplos itens sem perder contexto.

### 2026-03-20 (documentacao academica)
- Criado `docs/documento_projeto_final_uerj.md` com estrutura completa de documento final academico (modelo UERJ), incluindo capa, folha de rosto, resumo/abstract, objetivos, especificacao funcional e nao funcional, arquitetura, validacao, cronograma, riscos, conclusao e apendices.
- `ProjetoFinal.sln` atualizado na secao de Solution Items para incluir o novo arquivo `.md` em `docs/`.
- Documento academico UERJ (`docs/documento_projeto_final_uerj.md`) atualizado com tabela comparativa entre Projeto, Google Classroom, Moodle e Udemy, destacando diferenciais do sistema em funcionalidades nativas.
- Criado `.dockerignore` na raiz com regras globais para ignorar `**/wwwroot/**` e `**/node_modules/**` no contexto de build Docker.
- Dockerfile refatorado para multi-stage com estagio dedicado de build do frontend e copia explicita dos artefatos gerados para `wwwroot` da imagem final da API.
- Validacao operacional: `docker build -t projeto-final:test-multistage .` executado com sucesso.

### 2026-03-22 (forum assíncrono e rascunhos)
- Forum de cursos ajustado para permitir topicos em turmas assincronas, removendo restricao anterior a turmas interativas.
- Criada acao de publicacao no gerenciamento de cursos em rascunho (`Publicar curso`).
- Listagem de cursos do instrutor evoluida com status real do card e acoes de rascunho: editar e excluir com confirmacao.
- Gerenciamento de rascunho recebeu formulario de edicao rapida para `Nome do curso` e `Categoria` com persistencia via API.
- Validacoes executadas: `dotnet build ProjetoFinal.sln` e `npm run build` (apenas warnings conhecidos).

### 2026-03-22 (ajustes finais de UX e dados)
- Anexos de video em atividades agora sao reproduzidos inline no `course-activity-viewer` (sem acao de baixar para video), com preload autenticado do blob.
- Refinado estilo do player de atividade para o mesmo padrao do viewer de conteudo, reduzindo impacto visual e mantendo responsividade.
- Corrigido backend de atividades para incluir `MediaResource` no include de anexos, viabilizando deteccao de tipo de arquivo no frontend.
- Corrigido backend de turmas para retornar nome de aluno nas inscricoes via include de `Student` + mapeamento explicito em `ClassEnrollmentDto`.
- Validacao tecnica executada com `dotnet build ProjetoFinal.sln` e `npm run build`.

### 2026-03-22 (restricao de uma turma por curso)
- Implementada regra de negocio para impedir que um aluno tenha solicitacao ou matricula ativa (`Pending`/`Approved`) em mais de uma turma do mesmo curso.
- A validacao foi aplicada no pedido de inscricao e tambem na aprovacao pelo professor, cobrindo inclusive registros legados inconsistentes.
- Frontend da tela de cursos passou a mostrar a mensagem detalhada retornada pelo backend quando a inscricao e bloqueada.
- Validacao tecnica pendente de execucao com `dotnet build ProjetoFinal.sln` e `npm run build`.

### 2026-03-22 (validacao da restricao por curso)
- `dotnet build ProjetoFinal.sln` concluido com sucesso apos a implementacao.
- `npm.cmd run build` concluido com sucesso; o uso de `npm.cmd` contornou a restricao local de execucao do `npm.ps1`.
- Permanecem apenas warnings conhecidos: `NU1903` do `AutoMapper` no restore .NET e warnings de budget/seletores no Angular.

### 2026-03-22 (aluno sair do curso)
- Implementada a acao `Sair do curso` nos cards de `Meus cursos` da area do aluno.
- Para cursos assincronos, a saida remove a `CourseSubscription` do proprio aluno; para cursos interativos, remove a `ClassEnrollment` correspondente do proprio aluno.
- Backend protegido para impedir que um aluno remova inscricoes de terceiros, preservando o fluxo existente de remocao por instrutor no gerenciamento de turma.
- Frontend atualiza o estado local apos a exclusao, exibe confirmacao antes da acao e mostra mensagens detalhadas de erro/sucesso.
- Validacao concluida com `npm.cmd run build` e `dotnet build src/ProjetoFinal.Api/ProjetoFinal.Api.csproj -p:OutDir=D:\\Dev\\Wallacevff\\projeto-final-definitivo\\artifacts\\api-build\\`.
- Restricao encontrada no ambiente: `dotnet build ProjetoFinal.sln` padrao falhou por lock do processo `ProjetoFinal.Api` (PID 3356) sobre `src/ProjetoFinal.Api/bin/Debug/net8.0`.

### 2026-03-22 (cards de cursos com tamanho mais estavel)
- Ajustado o grid de `Meus cursos` para usar colunas `auto-fill` com largura minima maior, reduzindo o encolhimento visual quando muitos cards aparecem na mesma tela.
- Cards configurados para ocupar altura consistente dentro do grid, preservando alinhamento entre linhas.
- Validacao concluida com `npm.cmd run build` e build alternativo da API em `artifacts/api-build`.

### 2026-03-22 (estatisticas do card sem overflow)
- Ajustados os quadros internos de estatisticas em `Meus cursos` para impedir overflow de texto nos campos `Vagas ocupadas` e `Matriculado em`.
- Adicionados `min-width: 0`, `overflow-wrap` e `word-break` nos elementos do bloco de stats do card.
- Validacao concluida com `npm.cmd run build` (warnings Angular conhecidos mantidos).

### 2026-03-22 (cards mais largos para datas e ocupacao)
- Ampliada a largura minima e maxima dos cards do grid de `Meus cursos` para reduzir quebras de linha nos valores dos campos de data e ocupacao.
- Validacao concluida com `npm.cmd run build` (warnings Angular conhecidos mantidos).

### 2026-03-22 (SignalR no forum)
- Implementado hub autenticado para o forum com grupos por topico, permitindo broadcast de novos posts/respostas sem recarregar a pagina.
- Ajustado backend para aceitar JWT em conexoes SignalR (`access_token` em `/hubs`) e para retornar/broadcastar o `ForumPostDto` ja hidratado apos o POST.
- Criado servico de tempo real no Angular e integrada a tela `forum-thread`, que agora recebe novas mensagens em tempo real com deduplicacao local.
- Adicionada dependencia `@microsoft/signalr` no frontend.
- Validacao concluida com `npm.cmd run build` e `dotnet build src/ProjetoFinal.Api/ProjetoFinal.Api.csproj -p:OutDir=D:\\Dev\\Wallacevff\\projeto-final-definitivo\\artifacts\\api-build\\`.

### 2026-03-22 (correcao de compatibilidade do SignalR)
- Corrigido o hub do forum para serializar eventos em `PascalCase`, compatibilizando os eventos em tempo real com o mesmo contrato usado pela API REST e pelo frontend atual.
- Simplificada a assinatura dos metodos `JoinThread` e `LeaveThread` no hub para reduzir risco de falha na invocacao pelo cliente SignalR.
- Revalidado com `npm.cmd run build` e `dotnet build src/ProjetoFinal.Api/ProjetoFinal.Api.csproj -p:OutDir=D:\\Dev\\Wallacevff\\projeto-final-definitivo\\artifacts\\api-build\\`.

### 2026-03-22 (correcao de CORS do SignalR)
- Corrigida a politica CORS da API para permitir negociacao do hub com credenciais entre frontend e backend em ambientes com origens distintas.
- Substituido o uso de `AllowAnyOrigin` por politica com `SetIsOriginAllowed(_ => true)` + `AllowCredentials()`, eliminando o erro de preflight com `Access-Control-Allow-Origin: *`.
- Revalidado com `dotnet build src/ProjetoFinal.Api/ProjetoFinal.Api.csproj -p:OutDir=D:\\Dev\\Wallacevff\\projeto-final-definitivo\\artifacts\\api-build\\`.

### 2026-03-22 (inicio do chat flutuante)
- Criada a branch `feat/chat-flutuante-online` para isolar a implementacao do chat global.
- Backend recebeu `ChatHub` autenticado, rastreamento simples de presenca online/offline por turma e broadcasts de `MessageReceived`, `MessageUpdated`, `MessageDeleted` e `PresenceSnapshot`.
- Controller de chat foi endurecido para usar o usuario autenticado e validar acesso apenas para professor dono da turma, administrador ou aluno com matricula aprovada.
- DTO/repositorio de mensagens agora retornam nome do remetente e dados hidratados para historico e tempo real.
- Frontend ganhou widget flutuante global no shell principal, lista de turmas com chat habilitado, historico via REST e atualizacao em tempo real via SignalR.
- Validacao concluida com `npm.cmd run build` e `dotnet build src/ProjetoFinal.Api/ProjetoFinal.Api.csproj -p:OutDir=D:\\Dev\\Wallacevff\\projeto-final-definitivo\\artifacts\\api-build\\`.
- Restricao de ambiente: `dotnet build ProjetoFinal.sln` permaneceu bloqueado por lock do processo `ProjetoFinal.Api` (PID 29908) sobre `src/ProjetoFinal.Api/bin/Debug/net8.0`.
