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
