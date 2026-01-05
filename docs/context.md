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
