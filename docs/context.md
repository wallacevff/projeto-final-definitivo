## Historico Inicial
- Projeto EAD full-stack (.NET + Angular); instrucoes fixas: nunca sobrescrever docs/memoria_projeto.md (apenas append) e sempre ler este context.md antes de continuar.
- Uploads/downloads de midia devem passar exclusivamente pela API; backend conversa com MinIO, cuidando de deduplicacao por hash e rotas autenticadas.

### Data: 2025-11-09
### Resumo
- Backend: uploads/donwloads MinIO, deduplicacao por hash, parse robusto do storagePath no download.
- Frontend: gestao de atividades/conteudos com opcoes para "todas as turmas", viewer de atividades/ conteudos com download autenticado e video+anotacoes; campos de descricao usam editor rich-text compartilhado.
- Build .NET falha se arquivos em src/ProjetoFinal.Api/bin estiverem abertos; Angular ainda com avisos de budget/seletores.
- Novo arquivo docs/transcriptions.txt deve ser usado para registrar transcricoes sempre que solicitado.

### Data: 2025-11-16
### Resumo
- Backend agora injeta o aluno autenticado ao criar CourseSubscription e nas solicitacoes de turmas (ClassGroupsController), restringindo inscricoes a cursos publicados e a modalidade correta.
- Frontend traz fluxo completo de inscricoes: botoes para alunos (distribuicao e turmas), painel de selecao com codigos, bloqueio quando ja ha solicitacao e dashboard unificado exibindo status.
- Sidebar alterna o item inicial (Meus cursos vs. Painel) conforme o papel logado; instrutores possuem botao direto para "Gerenciar curso" na listagem.
- npm run build segue com sucesso, mantendo apenas os avisos conhecidos de orcamento/seletores do Angular.

### Data: 2025-11-16
### Resumo
- Dashboard do aluno agora leva a /student/courses/:id com secoes de atividades, conteudos e forum carregadas dos services existentes.
- CourseActivityViewer passou a permitir envios (rich-text, anexos, validacao) e consome ActivitySubmissionsService (inclui busca detalhada via GET por Id).
- ActivitySubmissionsController atende tambem em /api/v1, alinhando com o baseUrl do frontend.
- Quando testar, lembrar de verificar uploads/downloads de anexos e os estados do formulario do aluno.

### Data: 2025-11-23
### Resumo
- class-group-manage ganhou gerencia de atividades com listagem, submissoes e painel de correcao (download de anexos, feedback, status/nota).
- CourseActivityViewer exibe status/nota/feedback ao aluno quando a submissao e corrigida.
- ActivitySubmissionDto expoe StudentName; AutoMapper e UsersService foram ajustados para preencher o nome real.
- ActivitySubmissionsService agora possui getById/updateSubmission; lembrar dos warnings de budget no ng build.

### Data: 2026-01-03
### Resumo
- Backend: persistencia de anotacoes de video para conteudos do curso via ContentVideoAnnotation, com controller /api/content-annotations.
- Backend: migration 20260103170626_AddContentVideoAnnotations criada; dotnet ef database update executado (banco ja estava atualizado).
- Frontend: viewer de conteudo salva e carrega anotacoes por anexo usando ContentAttachmentId.
- Frontend: ajustes em cursos para disponibilidade e ocupacao (aprovados + pendentes para disponibilidade; barra alinhada com aprovados).
- Infra: minio-compose.yaml atualizado conforme appsettings.Development.

### Data: 2026-01-04
### Resumo
- Frontend: botao "Criar novo curso" adicionado na tela de cursos para instrutores, com icone e responsividade ajustada.
- Infra: copy-to-wwwroot.js valida pasta de destino antes de remover e cria wwwroot se necessario.
- Git: user.name e user.email configurados globalmente conforme solicitado.

### Data: 2026-01-04
### Resumo
- Documentacao: memoria/contexto agora em Markdown (docs/memoria_projeto.md e docs/context.md).
- Instrucoes: antes de qualquer commit, atualizar memoria e contexto.

### Data: 2026-01-04
### Resumo
- Sidebar: icone de colapso atualizado para hamburguer e estado colapsado exibe apenas icones, preservando o logo.

### Data: 2026-01-04
### Resumo
- Sidebar: estado colapsado persistido por usuario via localStorage.

### Data: 2026-01-04
### Resumo
- Forum: criacao de topicos restrita a instrutores no backend/frontend; posts vinculados ao autor autenticado.
- Forum: nova tela de discussao com respostas em /forum/threads/:threadId.

### Data: 2026-01-04
### Resumo
- Forum: topicos na gerencia da turma agora linkam para a pagina de discussao.

### Data: 2026-01-04
### Resumo
- Forum: links de topicos na gerencia de turma estilizados para manter visual do card.

### Data: 2026-01-04
### Resumo
- Forum: scroll ao responder aponta para a secao completa de resposta.

### Data: 2026-01-04
### Resumo
- Student course view: botao para abrir forum agora vai direto ao topico.

### Data: 2026-01-05
### Resumo
- Class-group-manage: botao "Baixar" dos anexos ajustado para paleta do sistema.

### Data: 2026-01-05
### Resumo
- Class-group-manage: anexos de submissao com video podem ser carregados e visualizados no painel.

### Data: 2026-01-05
### Resumo
- Course-manage: cards de atividades exibem label "Todas as turmas" quando aplicavel.

### Data: 2026-01-05
### Resumo
- Class-group-create: ao acessar via curso, o seletor fica bloqueado e exibe o curso fixo.

### Data: 2026-01-05
### Resumo
- Activities: tela de correcoes dedicada em /activities/:activityId/corrections com seletor de turma.
- Activities: labels de turma mostram "Todas as turmas" quando aplicavel.

### Data: 2026-01-05
### Resumo
- Course-manage: botao para criar turma na secao de turmas.

### Data: 2026-01-06
### Resumo
- Auth: cadastro publico de usuarios via /auth/register com hash de senha e validacao de papel (aluno/professor).
- Frontend: login agora oferece aba de criacao de conta com validacao de senhas e selecao de perfil.
- Frontend: correcoes de build (RouterLink nao usado e erro passwordMismatch).

### Data: 2026-01-06
### Resumo
- Dashboard do professor com novos indicadores (alunos matriculados, cursos interativos e nao interativos) calculados a partir dos cursos do instrutor.
- Build .NET e Angular concluido com warnings conhecidos de budget/seletores.

### Data: 2026-01-06
### Resumo
- Migrados docs/especificacao, propont e transcription para .md; referencias no .sln ajustadas.
- AGENTS.md reforca leitura e manutencao do transcription.md com formato padrao e linha em branco entre falas.

### Data: 2026-01-06
### Resumo
- Criado docs/relatorio.md com analise geral e cronograma de implementacoes.
- AGENTS.md atualizado para manter o relatorio sempre em dia.

### Data: 2026-01-06
### Resumo
- Dashboard professor agora lista atividade recente em foruns (3 topicos mais recentes com ultima interacao).
- AGENTS.md reforca executar dotnet build e npm run build apos alteracoes de codigo.

### Data: 2026-01-06
### Resumo
- Melhorias de usabilidade: foco visivel global, scroll horizontal em tabelas densas e indicador de envio de anexos.
- Relatorio atualizado com cronograma de melhorias.
- Builds .NET e Angular concluídos com warnings conhecidos.

### Data: 2026-01-06
### Resumo
- Backend reforca restricao de cursos/turmas por instrutor (somente proprietario).
- Frontend filtra cursos do professor pelo InstructorId.
- Builds .NET e Angular concluídos com warnings conhecidos.

### Data: 2026-01-06
### Resumo
- Atividades agora respeitam instrutor logado (listagem e CRUD filtrados no backend).
- Builds .NET e Angular concluídos com warnings conhecidos.

### Data: 2026-01-06
### Resumo
- Forum agora filtra e valida acesso por instrutor (threads e posts).
- Builds .NET e Angular concluídos com warnings conhecidos.

### Data: 2026-01-06
### Resumo
- Forum: respostas agora renderizam em arvore (multinivel).
- Builds .NET e Angular concluídos com warnings conhecidos.

## 2026-01-31
- Atividades para alunos na rota /activities agora sao filtradas por matricula/inscricao e possuem filtro de status (todas/pendentes/realizadas).
- Acoes de professor (criar/gerenciar correcoes) ficam ocultas para alunos nessa tela.

## 2026-02-02
- Infra de docker adicionada (Dockerfile, docker-compose.yml, docker/appsettings.Docker.json) com suporte a swarm config externo.
- appsettings.json atualizado para apontar para projeto-final_sqlserver e projeto-final_minio.

### Data: 2026-03-12
### Resumo
- MediaResources: deduplicacao por SHA-256 passou a reutilizar registro existente somente quando o objeto existe no MinIO.
- MediaResources: para hash existente com objeto ausente, upload agora recria o objeto e atualiza o registro existente no banco.
- Infra storage: contrato `IObjectStorageService` ganhou `ExistsAsync`, implementado no MinIO com `StatObject`.
- Operacao: adicionado script `docs/saneamento_media_resources_orfaos.sql` para diagnostico e exclusao segura de registros orfaos sem referencias.

### Data: 2026-03-13
### Resumo
- Corrigido conflito de publish no Docker (`NETSDK1152`) por duplicidade de arquivos SPA na API.
- `ProjetoFinal.Api.csproj` simplificado no target de publish para evitar dupla inclusao de assets do frontend.
- Build Docker validado com sucesso apos a correcao.

### Data: 2026-03-13
### Resumo
- Sidebar do shell principal ajustada para tema azul consistente.
- Removido "SES" dos nomes exibidos no frontend (titulo da app e titulos de paginas/rotas).
- Atualizada chave de persistencia de autenticacao no frontend para `plataforma-ead-auth-state`.

### Data: 2026-03-20
### Resumo
- Feedback de correcao de atividades evoluido para formato estruturado com rubrica por criterio (1-5), tags diagnosticas e acao recomendada.
- Backend atualizado (dominio, DTOs, service e EF) com migration `AddStructuredActivityFeedback` para persistencia em `ActivitySubmissions`.
- Frontend de correcao atualizado em `activity-corrections` e `class-group-manage`; tags agora usam dropdown com checklist para selecao multipla.
- Tela do aluno (`course-activity-viewer`) passou a exibir feedback estruturado quando disponivel.

### Data: 2026-03-21
### Resumo
- Documento academico UERJ adicionado com especificacao funcional e modelo de projeto final (`docs/documento_projeto_final_uerj.md`).
- Solucao atualizada para incluir novo `.md` em Solution Items no `ProjetoFinal.sln`.
- Pipeline Docker atualizado para multi-stage com estagio de build frontend e copia dos artefatos para `wwwroot` no runtime.
- `.dockerignore` adicionado para excluir `**/wwwroot/**` e `**/node_modules/**` do contexto de build.
- `ProjetoFinal.Api.csproj` ajustado para evitar dependencia de build do esproj do frontend durante publish da API.

### Data: 2026-03-22
### Resumo
- Forum passou a aceitar turmas assincronas no frontend (sem exclusao por `IsMaterialsDistribution`).
- Fluxo de publicacao de curso rascunho implementado na tela de gerenciamento com botao dedicado.
- Cursos rascunho na listagem do instrutor agora suportam edicao e exclusao.
- Edicao de rascunho no gerenciamento passou a permitir alterar titulo e categoria com salvamento imediato.

### Data: 2026-03-22 (incremental)
### Resumo
- Visualizacao de atividade passou a renderizar player para anexos de video, substituindo o fluxo de download nesses casos.
- Repositorio de atividades atualizado para retornar metadados completos de media nos anexos (`ThenInclude` de `MediaResource`).
- Gerenciamento de turma: nomes de alunos em inscricoes estabilizados com includes no backend e fallback no frontend.

### Data: 2026-03-22 (restricao por curso)
### Resumo
- Backend de turmas agora impede que um mesmo aluno mantenha solicitacao/matricula ativa em mais de uma turma do mesmo curso.
- Regra reforcada tanto na solicitacao da inscricao quanto na aprovacao pelo professor.
- Frontend da vitrine de cursos passou a aproveitar a mensagem retornada pela API ao rejeitar a inscricao.

### Data: 2026-03-22 (validacao)
### Resumo
- Build backend validado com `dotnet build ProjetoFinal.sln`.
- Build frontend validado com `npm.cmd run build`, mantendo apenas warnings conhecidos do projeto.

### Data: 2026-03-22 (saida do curso)
### Resumo
- Aluno agora pode sair do curso a partir do dashboard, tanto em inscricoes de distribuicao quanto em matriculas/solicitacoes de turmas interativas.
- Controllers de `course-subscriptions` e `class-groups` passaram a validar propriedade do registro antes de remover a inscricao do aluno.
- Frontend atualizado para confirmar a saida, chamar o endpoint correto e remover o card da lista sem recarregar a pagina.
- Validacao do backend realizada com build da API em pasta alternativa devido lock no binario da instancia local em execucao.

### Data: 2026-03-22 (ajuste visual dos cards)
### Resumo
- Grid dos cards do dashboard do aluno atualizado para preservar melhor a largura dos cards mesmo com muitos cursos listados.
- Cards mantidos com altura consistente e colunas alinhadas ao inicio, evitando encolhimento excessivo.
- Validacao executada com `npm.cmd run build` e build alternativo da API.

### Data: 2026-03-22 (estatisticas sem vazamento)
### Resumo
- Blocos internos de estatisticas dos cards do dashboard passaram a quebrar texto e valores longos sem vazar do container.
- Ajuste aplicado especificamente nos `dt` e `dd` de `Meus cursos`.
- Validacao executada com `npm.cmd run build`.

### Data: 2026-03-22 (cards mais largos)
### Resumo
- Grid de `Meus cursos` teve a largura dos cards ampliada para reduzir quebra de linha nos valores de `Vagas ocupadas` e `Matriculado em`.
- Validacao executada com `npm.cmd run build`.

### Data: 2026-03-22 (forum realtime)
### Resumo
- SignalR adicionado ao backend e frontend para atualizar posts do forum em tempo real por topico.
- `forum-thread` agora combina carga inicial via REST com eventos `PostCreated` vindos do hub.
- Validacao executada com `npm.cmd run build` e build alternativo da API.

### Data: 2026-03-22 (ajuste de compatibilidade do hub)
### Resumo
- Serializacao do SignalR alinhada ao padrao `PascalCase` do restante da API para o frontend consumir o mesmo formato de DTO.
- Metodos do hub simplificados para reduzir risco de falha no `invoke` do cliente.
- Validacao executada com `npm.cmd run build` e build alternativo da API.

### Data: 2026-03-22 (CORS do SignalR)
### Resumo
- Corrigida a politica CORS da API para suportar negociacao do SignalR com credenciais em ambiente local.
- Ajuste necessario para o fluxo `localhost:4200` -> `localhost:5179/hubs/forum/negotiate`.
- Validacao executada com build alternativo da API.

### Data: 2026-03-22 (branch do chat flutuante)
### Resumo
- Branch `feat/chat-flutuante-online` aberta para a funcionalidade de chat flutuante.
- Primeira implementacao conecta um widget global do Angular a um `ChatHub` autenticado com presenca simples online/offline por turma aberta.
- Mensagens do chat agora passam pelo controller autenticado, validam acesso a turma e sao retransmitidas em tempo real para o grupo do SignalR.
- Historico do chat passou a retornar nome do remetente, permitindo renderizacao adequada no widget.
- Validacao executada com `npm.cmd run build` e build alternativo da API; o build completo da solucao segue bloqueado por lock da API em execucao.

### Data: 2026-03-22 (chat coletivo + individual)
### Resumo
- O chat da branch `feat/chat-flutuante-online` agora suporta tanto o canal da turma inteira quanto conversas individuais entre participantes da mesma turma.
- A persistencia do backend passou a diferenciar mensagens coletivas e privadas via `RecipientId` opcional, com grupos SignalR separados para a turma e para cada participante.
- O widget flutuante ganhou seletor de conversa, exibindo `Turma inteira` e os participantes da turma, com historico filtrado e envio em tempo real para cada modo.
- Migration manual adicionada para refletir a nova coluna `RecipientId` em `ChatMessages`.
- Validacao executada com `dotnet build ProjetoFinal.sln` e `npm.cmd run build`.

### Data: 2026-03-22 (historico do chat corrigido)
### Resumo
- Corrigido o filtro backend do historico para nao reaproveitar `SenderId` como contexto da conversa; o historico agora usa `CurrentUserId` para resolver corretamente canal coletivo e DM.
- Alteracao de esquema aplicada diretamente no banco local porque o `dotnet ef` do ambiente falhou ao carregar `System.Runtime 10.0.0.0`.
- Validacao executada com `dotnet build ProjetoFinal.sln` e `npm.cmd run build`.

### Data: 2026-03-22 (refinos finais do chat flutuante)
### Resumo
- O chat da branch eat/chat-flutuante-online foi refinado para exibir nomes reais dos alunos nas conversas individuais, apos incluir o relacionamento Student no carregamento das matriculas por curso.
- O widget ganhou ajustes de layout para ampliar a area principal da conversa, reduzir a pressao visual das colunas laterais e melhorar a leitura geral.
- A lista de mensagens passou a usar um viewport ancorado no rodape, com ordenacao cronologica por envio e rolagem automatica consistente para a ultima mensagem, em comportamento semelhante ao ChatGPT.
- .gitignore atualizado para ignorar qualquer pasta rtifacts no repositorio.
- Validacao executada com dotnet build ProjetoFinal.sln e 
pm.cmd run build.


### Data: 2026-03-23 (ajustes finais de UX do chat)
### Resumo
- O chat flutuante passou a forcar a rolagem ate a ultima mensagem tambem em mensagens recebidas via SignalR, com sincronizacao apos o render do Angular.
- A janela do chat foi ampliada verticalmente para aumentar o espaco util da conversa sem perder a estrutura lateral de turmas e participantes.
- Validacao executada com dotnet build ProjetoFinal.sln e 
pm.cmd run build (mantidos apenas warnings conhecidos e locks quando a API local esta em execucao).

