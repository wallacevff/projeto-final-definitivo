## Estado Atual do Projeto EAD

### Data: 2025-09-29 22:02:37

1. API e Contratos
 - `BaseController` alinhado com o novo `IDefaultService` e endpoints REST criados para usuários, cursos, turmas, conteúdos, atividades, submissões, fórum, chat, mídia e inscrições (`src/ProjetoFinal.Api/Controllers`).
 - Contratos e serviços já expõem funcionalidades como busca por slug, gerenciamento de inscrições e submissões de atividades.

2. Infraestrutura de Dados
 - `AppDbContextFactory` garante execução dos comandos `dotnet ef` com base no `appsettings` (ambientes Development/Default usando `(localdb)\\mssqllocaldb` ? `ProjetoFinalDb`).
 - Mapeamentos usam `DeleteBehavior.Restrict` onde necessário para evitar múltiplos cascades; migração `20250930002230_InitialCreate` aplicada ao SQL Server local.

3. Frontend Angular (novo)
 - Shell reformulado com sidebar, topbar e avatar, preparado para navegação entre módulos (`app.component.*`).
 - Shell reformulado com sidebar, topbar e avatar, preparado para navegacao entre modulos (`app.component.*`).
 - Paginas reais para cursos, turmas, atividades e forum com dados mockados e layout responsivo (`pages/courses`, `pages/class-groups`, `pages/activities`, `pages/forum`).
 - Paginas agora consomem dados reais via services (cursos, turmas, atividades, forum) com estado de carregamento e tratamento de erros.
 - Estilos globais e do shell com tokens de cor, gradientes e responsividade definidos em `app.component.css` e `styles.css`.

4. Build e Execução
 - `dotnet build` e `npm run build` finalizam com sucesso; permaneceram os warnings conhecidos do Angular (budget >500 kB e regras de CSS herdadas).
 - Registro de `IUnityOfWork` garantido via `services.AddScoped<IUnityOfWork, UnityOfWork>()` no IoC.

Observação próximo encontro: retomar os itens abaixo na abertura da próxima sessão.

5. Próximos Passos Guardados
 - Integrar o dashboard a dados reais da API, substituindo os números mockados.
 - Implementar telas completas para cursos, turmas, atividades e fórum baseadas no novo layout.
 - Tratar os avisos de build Angular (budget e seletores) antes do pacote de produção.

## Estado Atual do Projeto EAD

### Data: 2025-10-01 22:33:01

1. API e Contratos
 - Sem novas alteracoes nesta sessao.

2. Infraestrutura de Dados
 - Sem novas alteracoes nesta sessao.

3. Frontend Angular (novo)
 - Sidebar agora utiliza os icones do Bootstrap Icons aplicados com ngClass nos itens de navegacao (app.component.html / app.component.ts).
 - O arquivo styles.css importa bootstrap-icons/font/bootstrap-icons.css para garantir o carregamento global dos icones.

4. Build e Execucao
 - Nao foram executados testes automatizados nesta sessao; recomenda-se validacao visual com npm start.

5. Observacao proximo encontro
 - Reforcar a consulta periodica aos documentos docs/propont.txt e docs/especificacao.txt como base de requisitos.
 - Manter as pendencias registradas nas entradas anteriores.
 - Commits devem ser criados por categoria, respeitando a organizacao definida pelo usuario.
## Estado Atual do Projeto EAD

### Data: 2025-10-01 22:50:46

1. API e Contratos
 - Sem novas alteracoes nesta sessao.

2. Infraestrutura de Dados
 - Sem novas alteracoes nesta sessao.

3. Frontend Angular (novo)
 - Criada pagina de criacao de curso com formulario reativo incluindo configuracao de modalidade e turmas dinamicas (src/ProjetoFinal.ClientApp/src/app/pages/course-create).
 - CoursesService agora possui contrato CreateCoursePayload e metodo createCourse para integrar com o endpoint de cadastro.

4. Build e Execucao
 - Nao executei build ou testes automatizados; sugiro validar com npm start e testes de API ao conectar no backend.

5. Observacao proximo encontro
 - Validar o envio do formulario contra a API real e ajustar mensagens conforme resposta.
 - Manter as pendencias anteriores, consulta aos arquivos docs/propont.txt e docs/especificacao.txt e a regra de commits por categoria.
## Estado Atual do Projeto EAD

### Data: 2025-10-12 08:45:42

1. API e Contratos
 - Seed automatico garante categorias padrao e instrutor demonstrativo ao subir o backend.

2. Infraestrutura de Dados
 - Sem novas alteracoes nesta sessao.

3. Frontend Angular (novo)
 - Tela de criacao de cursos agora carrega categorias e instrutores reais via API, exibe estados de carregamento/erro e impede envio sem dados obrigatorios.
 - Fluxo de gravacao envia as turmas criadas para a API apos o curso ser salvo e publica automaticamente quando solicitado.

4. Build e Execucao
 - npm run build executado com sucesso (mantidos os avisos conhecidos de budget e seletores do Bootstrap).

5. Observacao proximo encontro
 - Validar a disponibilidade de categorias/instrutores no backend antes de novas demos e revisar o feedback visual apos a gravacao das turmas/publicacao.


## Estado Atual do Projeto EAD

### Data: 2025-10-16 21:57:54

1. API e Contratos
 - Autenticacao JWT implementada com emissao via `AuthController`/`AuthAppService` e claims para identificacao do instrutor logado.
 - `CoursesController` passa a criar cursos vinculando automaticamente o `InstructorId` a partir do token e aceita rotas `/api` e `/api/v1`.

2. Infraestrutura de Dados
 - Migration `20251017001034_AddUserCredentials` adiciona `Username`/`PasswordHash` em `Users` e migra dados existentes.
 - Seeder cria usuarios padrao `wallace.vidal` (professor) e `robert.leite` (aluno) com hash BCrypt.

3. Frontend Angular (novo)
 - Fluxo de login JWT com guarda, interceptor e pagina dedicada; formularios do shell adaptados para exibir usuario logado.
 - Formulario de criacao de cursos agora usa categoria texto livre e dispensa a escolha manual de instrutor.

4. Build e Execucao
 - `dotnet build ProjetoFinal.sln` e `dotnet ef database update` executados; permanecem avisos de tamanho de bundle Angular e regras CSS.

5. Observacao proximo encontro
 - Revisar protecoes por perfil (professor/aluno) nas rotas e tratar avisos de build Angular.



## Estado Atual do Projeto EAD

### Data: 2025-10-17 15:40:00

1. API e Contratos
 - ClassGroupsController agora responde tambem em /api/v1/class-groups, alinhando com os endpoints usados pelo frontend.
 - Semente fixa GUIDs dos usuarios padrao, evitando conflitos ao recriar banco.
 - DTOs e servicos ajustados para que atividades e foruns dependam de turmas, com flag de distribuicao de materiais nas turmas.

2. Infraestrutura de Dados
 - Migration unica 20251017143934_InitialCreate cobre os novos relacionamentos (activities -> classGroups, forum threads por turma e flag de distribuicao).
 - Repositorios de cursos incluem Instructor e turmas com matriculas para consultas completas.

3. Frontend Angular
 - Tela de criacao de cursos envia turmas e trata publicacao; painel e listagem direcionam para o fluxo correto.
 - Pagina de gerenciamento de curso exibe cards de resumo e turmas; cards de cursos mostram instrutor.
 - Forum e atividades exibem turmas associadas; servicos atualizados para os novos campos.

4. Build e Execucao
 - dotnet build ProjetoFinal.sln -c Release concluido (mantidos avisos de budget/seletores do Angular).

5. Pendencias
 - Avaliar ajuste no warning de disabled nos formularios.
 - Popular dashboard e forum com dados reais quando API estiver pronta.

## Estado Atual do Projeto EAD

### Data: 2025-10-19 23:32:27

1. API e Contratos
 - Controllers de forum (threads/posts) anotados com ApiController e rota dupla `/api`/`/api/v1`, evitando 405 nas chamadas do frontend.
 - DTO `ForumThreadDto` passa a expor `CreatedByName`; AutoMapper e repositorio carregam o usuario associado.

2. Infraestrutura de Dados
 - `ForumThreadRepository` inclui `ClassGroup` e `CreatedBy` nos includes, garantindo dados completos para as listas.

3. Frontend Angular
 - Tela de forum recebe formulario reativo para criar topicos, com selecao de curso/turma interativa, validacoes e envio para a API.
 - Threads exibem o nome de quem criou e a turma correspondente; botoes superiores/inferiores usam as mesmas cores (variaveis globais definidas em `styles.css`).
 - Gerenciamento de cursos ganhou acao "Gerenciar turma" em cada card; ajustes nos botoes da pagina de turma para manter estilo/espacamento.

4. Build e Execucao
 - `npm run build` e `dotnet build ProjetoFinal.sln -c Release` rodados; permanecem os avisos conhecidos de budget do Angular.

5. Pendencias
 - Avaliar limitacao de criacao de topicos por perfil/permicao.
 - Revisar budgets de CSS e bundle do Angular antes do deploy.

## Estado Atual do Projeto EAD

### Data: 2025-11-09 20:36:00

1. API e Contratos
 - Configuracao do MinIO adicionada aos appsettings e exposta via `MinioConfiguration`, com registros no IoC e cliente dedicado.
 - `MediaResourcesController` passou a aceitar upload multipart via MinIO, calcular hash SHA-256 para evitar duplicidades e reutilizar arquivos ja existentes.
 - `ActivitiesController` e `CourseContentsController` agora respondem tambem por `/api/v1`, alinhando com o frontend.
 - `CourseContentRepository` inclui `Attachments`/`MediaResource`, garantindo que os DTOs retornem a quantidade correta de anexos.

2. Infraestrutura de Dados
 - Criado `MinioObjectStorageService` e interfaces auxiliares para upload de objetos.
 - Repositorio de midias ganhou consulta por hash (`GetByShaAsync`) para suportar deduplicacao de arquivos.

3. Frontend Angular
 - Novos modulos na pagina de gestao de curso: `CourseActivitiesComponent` e `CourseContentsComponent`, ambos com formularios reativos, upload de anexos e listagem dos itens do curso.
 - Serviços e contratos expandidos (`CourseContentsService`, `MediaService`, ajustes em `ActivitiesService`) para integrar criacao de conteudos/atividades aos endpoints REST.
 - Dropdowns e payloads de conteudo passaram a enviar os enums como numeros, eliminando os 400 de validacao.

4. Build e Execucao
 - `dotnet build ProjetoFinal.sln` executado com sucesso; permanecem os avisos conhecidos de budget do Angular e seletores Bootstrap.

5. Pendencias
 - Revisar budgets de bundle/estilos do Angular antes do deploy.
 - Avaliar mecanismos de reordenacao em massa para conteudos/atividades quando o volume crescer.

### Instrucoes Fixas
 - Nunca substituir conteudo existente em docs/memoria_projeto.md; apenas fazer append quando houver novas atualizacoes.
 - Antes de qualquer atividade relevante, ler o arquivo docs/context.md para garantir alinhamento com o estado atual.

## Estado Atual do Projeto EAD

### Data: 2025-11-09 22:15:00

1. API e Contratos
 - Suporte a download autenticado de mídias via MediaResourcesController, com leitura segura do storagePath (deduplicação por hash mantida).
 - ActivitiesService exposto para busca por Id, permitindo visualização detalhada no frontend.

2. Frontend Angular
 - Módulos de criação de atividades e conteúdos com opção "todas as turmas"; uploads e downloads via API com JWT.
 - Novo CourseActivityViewer exibe detalhes, prazos e anexos das atividades; cards de cursos e listagem geral navegam para o viewer.
 - Viewer de conteúdos mostra vídeos com anotações locais e anexos baixados via backend.
 - `docs/context.md` registra o estado corrente; instruções permanentes reforçam leitura desse contexto e proíbem sobrescrever a memória.

3. Build e Execução
 - `npm run build` concluído (avisos de budget/seletores do Angular permanecem conhecidos).
 - `dotnet build` depende de não haver processos segurando DLLs em src/ProjetoFinal.Api/bin.

4. Pendências / Observações
 - Avaliar os avisos de bundle/estilos antes de deploy final.
 - Manter o hábito de consultar `docs/context.md` e apenas anexar conteúdo à memória.

## Estado Atual do Projeto EAD

### Data: 2025-11-10 09:12:00

1. API e Contratos
 - Sem alterações desde a última sessão; endpoints existentes foram reutilizados para leitura de atividades e anexos.

2. Frontend Angular
 - Criado componente `RichTextEditorComponent` (contenteditable + toolbar) e aplicado nas descrições do fórum, nova atividade e conteúdo.
 - Botões “Ver detalhes/Visualizar” de atividades navegam para o novo viewer; cards da área de curso e listagem geral compartilham a mesma rota.

3. Build e Execução
 - `npm run build` concluído (mantidos avisos de budget/seletores).

4. Pendências
 - Persistência real das anotações de vídeo ainda não foi integrada; campo rich-text envia HTML, avaliar validações/limpeza futura.

## Estado Atual do Projeto EAD

### Data: 2025-11-10 11:05:00

1. Documentação
 - Criado `docs/transcriptions.txt` para armazenar futuras transcrições solicitadas.
 - Contexto atualizado reforçando o uso desse arquivo.

2. Instruções
 - Lembrar de registrar transcrições no arquivo acima sempre que o usuário pedir.
 - Commits devem ser escritos em PT-BR (títulos e descrições).

## Estado Atual do Projeto EAD

### Data: 2025-11-16 20:30:00

1. API e Contratos
 - `CourseSubscriptionsController` agora força o uso do aluno autenticado tanto em `/api` quanto em `/api/v1`.
 - `ClassGroupsController` injeta o aluno logado ao solicitar inscrição em turmas interativas.
 - `CourseSubscriptionAppService` valida modalidade (distribuição) e estado do curso antes de aceitar novas inscrições.

2. Frontend Angular
 - Botões da listagem de cursos ajustam rótulo/comportamento conforme perfil (aluno encontra “Inscrever-se” / “Selecionar turma”; instrutor vê “Gerenciar curso”).
 - Fluxo de inscrição para turmas: painel lateral permite escolher turmas, informar códigos e envia o POST para `/class-groups/{id}/enrollments`, bloqueando quando já houver solicitação.
 - Dashboard do aluno mescla cursos de distribuição e matrículas em turmas interativas, exibindo nome da turma e status (pendente/aprovado).
 - Sidebar alterna automaticamente entre “Meus cursos” (aluno) e “Painel” (instrutor).

3. Build e Execução
 - `npm run build` continua concluindo com os avisos conhecidos de orçamento (bundle inicial, CSSs de dashboard/courses/app/class-group-manage) e seletores Bootstrap.

## Atualizacao 2025-11-16 21:20
- Frontend: dashboard do aluno exibe botao "Acessar curso" e nova rota /student/courses/:courseId mostra aba unica com atividades, conteudos e forum integrados aos services existentes.
- Frontend: CourseActivityViewer ganhou modulo de envio de atividades (editor rich-text, upload multiplo via MediaService, consultas/envio em ActivitySubmissionsService) com resumo de anexos e reconciliação com backend.
- Backend: ActivitySubmissionsController agora responde em /api e /api/v1, evitando 405 para o client Angular.
- Observacao: lembranca para testar fluxos de envio de anexos (upload + download) e validar exibicao imediata no painel do aluno.

## Atualizacao 2025-11-23 15:50
- Frontend: class-group-manage ganhou secao completa de gerencia de atividades (listagem, tabela de submissões, painel de correção com download de anexos e salvamento de status/notas).
- Frontend: CourseActivityViewer agora mostra a correção ao aluno (status, nota, feedback) após envio.
- API/Contracts: ActivitySubmissionDto passou a expor StudentName e o AutoMapper injeta o nome vindo do user associado.
- Services: activity-submissions.service oferece getById/updateSubmission; UsersService tem getById para complementar nomes.
- Observacao: builds ng reportam os avisos de budget conhecidos; lembrar de executar npm run build e dotnet build antes de PR.

## Atualizacao 2025-11-23 21:25
- Frontend: student-course-view agora identifica o aluno logado e restringe as atividades exibidas às turmas aprovadas/pendentes ou grupos de distribuição do curso, evitando mostrar tarefas de outras turmas.
- Documentacao: transcricao atualizada conforme solicitado, mantendo o historico das interacoes.

## Atualizacao 2026-01-03 14:14:14
- Backend: persistencia de anotacoes de video para conteudos do curso via ContentVideoAnnotation (entidade, repositorio, service e controller /api/content-annotations).
- Backend: migration 20260103170626_AddContentVideoAnnotations criada; dotnet ef database update executado (banco ja estava atualizado).
- Frontend: viewer de conteudo passa a salvar/carregar anotacoes de video por anexo usando ContentAttachmentId.
- Frontend: ajustes em cursos para disponibilidade e ocupacao (aprovados + pendentes para disponibilidade; barra alinhada com aprovados).
- Infra: minio-compose.yaml atualizado para credenciais/portas do appsettings.Development.

## Atualizacao 2026-01-04 21:06:07
- Frontend: botao "Criar novo curso" visivel na tela de cursos para instrutores, com icone e responsividade ajustada.
- Infra: script copy-to-wwwroot agora valida existencia/estado da pasta antes de remover e cria wwwroot caso nao exista.
- Git: user.name e user.email configurados globalmente conforme solicitado.

## Atualizacao 2026-01-04 21:09:38
- Documentacao: memoria e contexto migrados para Markdown (.md) e referencias atualizadas.
- Instrucoes: AGENTS.md reforca atualizar memoria/contexto antes de qualquer commit.

## Atualizacao 2026-01-04 21:17:13
- Frontend: botao de collapse da sidebar usa icone hamburguer e alinhamento corrigido; modo colapsado esconde textos mantendo apenas icones e preserva o logo SES.

## Atualizacao 2026-01-04 21:29:16
- Frontend: preferencia de sidebar colapsada agora persiste por usuario via localStorage (chave sidebar:collapsed:{userId}).

## Atualizacao 2026-01-04 21:40:09
- Forum: backend passou a exigir autenticacao, vincula autor pelo token e restringe criacao de topicos para instrutores.
- Forum: endpoint de posts agora retorna AuthorName e inclui relacionamentos necessarios.
- Forum: frontend ganhou tela de discussao com respostas (aluno e professor), rota /forum/threads/:threadId e servico de posts.
- Forum: listagem bloqueia criacao de topico para alunos.

## Atualizacao 2026-01-04 21:48:37
- Frontend: topicos do forum na gerencia de turma agora sao links para a discussao completa.

## Atualizacao 2026-01-04 21:50:05
- Frontend: estilizacao dos links de topicos na gerencia de turma para manter visual do card com foco acessivel.

## Atualizacao 2026-01-04 21:52:56
- Forum: acao "Responder" agora rola ate a secao de resposta incluindo o titulo.

## Atualizacao 2026-01-04 21:56:09
- Frontend: botao "Abrir no forum" do curso do aluno agora abre a discussao do topico correspondente.

## Atualizacao 2026-01-05 18:20:46
- Frontend: botao "Baixar" dos anexos na gerencia de turmas agora segue a paleta de cores do sistema.

## Atualizacao 2026-01-05 18:37:58
- Frontend: anexos de submissao na gerencia de turmas agora exibem videos carregados inline com player.

## Atualizacao 2026-01-05 18:48:08
- Frontend: cards de atividades na gerencia de curso agora exibem "Todas as turmas" ou lista de turmas conforme alcance da atividade.

## Atualizacao 2026-01-05 18:54:13
- Frontend: criacao de turma iniciada pela gerencia do curso agora bloqueia o seletor e mostra o curso fixo.

## Atualizacao 2026-01-05 19:05:57
- Frontend: nova tela de correcoes em /activities/:activityId/corrections com seletor de turma e painel de correcao.
- Frontend: botao "Gerenciar correcoes" em /activities aponta para a nova tela.
- Frontend: labels de turma em /activities agora exibem "Todas as turmas" quando aplicavel.

## Atualizacao 2026-01-05 19:26:41
- Frontend: botao "Criar turma" na gerencia de curso com pre-selecao do curso na tela de criacao.

## Atualizacao 2026-01-06 00:05:00
- Auth: endpoint /auth/register com validacoes, hash de senha e restricao de papel aluno/professor.
- Frontend: tela de login agora permite criar conta (aluno/professor) com validacao de senha e selecao de perfil.
- Frontend: ajustes de build removendo RouterLink nao usado e acesso correto ao erro passwordMismatch.

## Atualizacao 2026-01-06 00:24:00
- Dashboard professor: cards agora mostram ALUNOS MATRICULADOS, CURSOS INTERATIVOS e CURSOS NAO INTERATIVOS com contagem real por instrutor.
- Frontend: agregado de matriculas soma aprovados+pendentes nas turmas interativas e inscricoes em cursos nao interativos.
- Build: dotnet build e npm run build executados (warnings de budget/seletores mantidos).

## Atualizacao 2026-01-06 00:45:00
- Docs: especificacao, proposta e transcricao migradas para Markdown (.md) e referencias atualizadas no .sln.
- Docs: transcriptions removido; AGENTS agora exige leitura e atualizacao do transcription.md com formatacao e linhas em branco entre falas.

## Atualizacao 2026-01-06 00:55:00
- Docs: criado relatorio geral em docs/relatorio.md com analise do projeto e cronograma.
- Instrucoes: AGENTS.md agora exige manter o relatorio atualizado.

## Atualizacao 2026-01-06 01:20:00
- Dashboard professor: secao de atividade recente em foruns com 3 ultimas interacoes por topico.
- Instrucoes: AGENTS.md agora exige build backend/frontend apos mudancas de codigo.
- Build: dotnet build e npm run build executados (warnings de budget/seletores mantidos).

## Atualizacao 2026-01-06 01:50:00
- Usabilidade: foco visivel global para elementos interativos e tabelas com scroll horizontal em telas menores.
- Usabilidade: indicador animado de envio de anexos em atividades.
- Docs: cronograma de melhorias adicionado ao relatorio.
- Build: dotnet build e npm run build executados (warnings de budget/seletores mantidos).

## Atualizacao 2026-01-06 02:15:00
- Seguranca: listagem de cursos e turmas agora valida instrutor logado no backend (bloqueia acesso a cursos/turmas de outros professores).
- Frontend: listagem de cursos do professor filtra por InstructorId do usuario logado.
- Build: dotnet build e npm run build executados (warnings de budget/seletores mantidos).

## Atualizacao 2026-01-06 02:55:00
- Atividades: filtro por instrutor no backend (ActivityFilter.InstructorId) e protecao de CRUD para evitar acesso a atividades de outros professores.
- Infra: ActivityRepository inclui Course e aplica filtro por instrutor; ordenacao padrao de cursos mantida.
- Build: dotnet build e npm run build executados (warnings de budget/seletores mantidos).

## Atualizacao 2026-01-06 03:20:00
- Forum: filtro por instrutor em threads/posts e protecao de CRUD para acesso somente a cursos do professor logado.
- Build: dotnet build e npm run build executados (warnings de budget/seletores mantidos).

## Atualizacao 2026-01-06 03:55:00
- Forum: exibicao de respostas agora suporta niveis recursivos (respostas a respostas).
- Build: dotnet build e npm run build executados (warnings de budget/seletores mantidos).

## 2026-01-31
- Ajustei a tela /activities para alunos: agora carrega apenas atividades das turmas em que o aluno esta matriculado ou inscrito em cursos de distribuicao, e inclui filtro de status (todas/pendentes/realizadas).
- Mantive as acoes de professor ocultas para alunos na listagem de atividades.
- Builds: `dotnet build ProjetoFinal.sln` e `npm run build` (warnings de budget/seletores ja conhecidos).

## 2026-02-02
- Criei Dockerfile multi-stage para build da API + frontend e inclui docker-compose com app, SQL Server e MinIO.
- Adicionei config docker/appsettings.Docker.json e ajustei docker-compose para usar config externo no swarm.
- Atualizei appsettings.json (default) para apontar para serviços do stack (projeto-final_sqlserver e projeto-final_minio).

## 2026-03-12
- Upload de midias: deduplicacao por SHA-256 agora valida existencia fisica do objeto no MinIO antes de reaproveitar registro existente.
- Upload de midias: quando hash existe no banco mas objeto esta ausente no storage, a API reenvia o arquivo e reidrata o mesmo registro (`MediaResources`) para evitar falso positivo de upload.
- Storage: adicionada operacao `ExistsAsync` no contrato de object storage e implementacao MinIO via `StatObject`.
- Saneamento: criado `docs/saneamento_media_resources_orfaos.sql` para diagnosticar/remover registros orfaos de `MediaResources` (sem arquivo e sem referencias em tabelas relacionadas).
- Validacao: `dotnet build ProjetoFinal.sln` e `npm run build` executados com sucesso (warnings Angular de budget/seletores mantidos).

## 2026-03-13
- Docker build: corrigido erro `NETSDK1152` no publish da API causado por artefatos SPA duplicados (`wwwroot` e `dist/browser`).
- `ProjetoFinal.Api.csproj`: target `PublishRunWebpack` ajustado para nao adicionar novamente `DistFiles`, mantendo apenas o build do frontend e a copia realizada pelo `postbuild`.
- Validacao: `docker build` concluido com sucesso apos ajuste.

## 2026-03-13 (tema e branding frontend)
- Frontend: sidebar atualizada para paleta azul (gradiente e estados visuais dos controles da barra lateral).
- Frontend: removida nomenclatura "SES" dos textos de interface principais (titulo da aplicacao e titulos de rotas).
- Frontend: chave de armazenamento de autenticacao renomeada para `plataforma-ead-auth-state`.
- Frontend: titulo da pagina inicial ajustado para identidade sem "SES".
- Validacao: `dotnet build ProjetoFinal.sln` e `npm run build` executados com sucesso (warnings de budget/seletores mantidos).

## 2026-03-20 (feedback estruturado em correcoes)
- Correcao de atividades evoluida para feedback estruturado com rubrica (1-5) por criterio (`MasteryScore`, `ApplicationScore`, `CommunicationScore`), tags diagnosticas (`FeedbackTags`) e acao recomendada (`RecommendedAction`), mantendo campo textual livre.
- Backend atualizado em entidade, DTOs, mapeamento EF e service com validacoes de faixa (1-5) e normalizacao de tags.
- Migration criada: `AddStructuredActivityFeedback` adicionando novas colunas em `ActivitySubmissions`.
- Frontend de professor atualizado nas telas `activity-corrections` e `class-group-manage` com novos campos estruturados.
- UX: campo de tags migrado de `select multiple` para dropdown com checklist, permitindo selecao multipla sem fechar a lista.
- Visualizacao do aluno em `course-activity-viewer` passou a exibir rubrica, tags e acao recomendada quando informadas.
- Validacao executada com `dotnet build ProjetoFinal.sln` e `npm run build`.

## 2026-03-21 (documentacao UERJ e docker)
- Criado `docs/documento_projeto_final_uerj.md` com estrutura de especificacao e documento final academico no modelo UERJ.
- `ProjetoFinal.sln` atualizado para incluir o novo arquivo de documentacao em Solution Items.
- Criado `.dockerignore` com exclusao global de `wwwroot` e `node_modules` no contexto de build.
- Dockerfile refatorado para multi-stage com build dedicado do frontend e copia de `dist/browser` para `wwwroot` na imagem final da API.
- Ajustado `ProjetoFinal.Api.csproj` para remover dependencia de `ProjetoFinal.ClientApp.esproj` no publish da API.
- Validacao de imagem com `docker build -t projeto-final:test-multistage .` concluida com sucesso.

## 2026-03-22 (forum assíncrono e ciclo de rascunho)
- Forum do curso liberado para turmas assincronas (`IsMaterialsDistribution`), removendo o filtro que permitia apenas turmas interativas na criacao/seleção de topicos.
- Gerenciamento de curso ganhou acao `Publicar curso` para rascunhos, com feedback visual de carregamento e toast.
- Listagem de cursos para instrutor passou a exibir status real (Publicado/Rascunho) e acoes para rascunho: `Editar rascunho` e `Excluir rascunho` com confirmacao.
- Em curso rascunho, adicionado bloco de edicao rapida para alterar `Nome do curso` e `Categoria`, persistindo via update sem publicar.
- Validacao executada com `dotnet build ProjetoFinal.sln` e `npm run build`.

## 2026-03-22 (mídia de atividade e nomes de inscrição)
- `course-activity-viewer`: anexos de atividade em video passaram a exibir player inline no lugar do botao de download.
- Player de video da atividade ajustado para o mesmo padrao visual do viewer de conteudo (layout compacto, responsivo e com altura maxima).
- Backend de atividades corrigido para incluir `ActivityAttachment.MediaResource` no retorno de atividade, permitindo identificar tipo de anexo no frontend.
- Tabela de inscricoes da turma corrigida para resolver nomes de alunos com fallback confiavel.
- Backend de turmas ajustado para incluir `Enrollments.Student` e mapear `ClassEnrollmentDto.StudentName` por `Student.FullName`.

## 2026-03-22 (restricao de matricula por curso)
- Matricula em turmas interativas agora bloqueia solicitacao duplicada em turmas diferentes do mesmo curso quando ja existe inscricao `Pending` ou `Approved`.
- Aprovacao de inscricao pelo professor tambem valida a regra por curso, evitando consolidar dados antigos inconsistentes.
- Frontend da listagem de cursos passou a exibir a mensagem retornada pela API ao falhar a inscricao em turma interativa.
- Validacao prevista com `dotnet build ProjetoFinal.sln` e `npm run build`.

## 2026-03-22 (validacao da restricao de matricula)
- `dotnet build ProjetoFinal.sln` executado com sucesso apos a correcao.
- `npm.cmd run build` executado com sucesso; uso de `npm.cmd` foi necessario por politica do PowerShell local bloqueando `npm.ps1`.
- Warnings mantidos: `NU1903` do pacote `AutoMapper` e budgets/seletores conhecidos do Angular.

## 2026-03-22 (saida voluntaria do curso)
- Dashboard do aluno ganhou a acao `Sair do curso` nos cards de `Meus cursos`.
- Cursos assincronos removem a propria `CourseSubscription`; cursos interativos removem a propria `ClassEnrollment`.
- Backend reforcado para permitir exclusao apenas do proprio registro do aluno ao sair do curso, mantendo a remocao por instrutor para gerenciamento de turma.
- Validacao: `npm.cmd run build` executado com sucesso e API compilada com `dotnet build src/ProjetoFinal.Api/ProjetoFinal.Api.csproj -p:OutDir=...\\artifacts\\api-build\\`.
- Observacao: `dotnet build ProjetoFinal.sln` padrao estava bloqueado por uma instancia em execucao de `ProjetoFinal.Api` usando o binario em `src/ProjetoFinal.Api/bin/Debug/net8.0`.

## 2026-03-22 (layout estavel dos cards de cursos)
- Grid de `Meus cursos` ajustado para usar colunas com largura minima maior (`auto-fill` + `minmax`) e alinhamento estavel no inicio da linha.
- Cards passaram a manter altura consistente no grid, reduzindo a sensacao de layout comprimido quando ha muitos cursos.
- Validacao: `npm.cmd run build` executado com sucesso e API recompilada em saida alternativa (`artifacts/api-build`).

## 2026-03-22 (quebra de texto nos quadros do card)
- Quadros de estatisticas do card de curso receberam `min-width: 0` e quebra de linha para evitar vazamento visual em rotulos e valores.
- Ajuste focado nos blocos `Vagas ocupadas` e `Matriculado em` da area `Meus cursos`.
- Validacao: `npm.cmd run build` executado com sucesso.

## 2026-03-22 (largura ampliada dos cards)
- Cards de `Meus cursos` tiveram a largura minima e maxima ampliadas no grid para acomodar melhor os valores de data e ocupacao sem quebra desnecessaria.
- Validacao: `npm.cmd run build` executado com sucesso.

## 2026-03-22 (forum em tempo real com SignalR)
- Backend passou a expor hub autenticado `ForumHub` com grupos por topico (`threadId`) e suporte a token JWT via query string em rotas `/hubs`.
- Publicacao de posts no forum agora retorna o DTO hidratado e dispara broadcast `PostCreated` para os clientes conectados ao topico.
- Frontend ganhou servico de tempo real para o forum e a tela `forum-thread` passou a conectar/desconectar do hub conforme o topico aberto, inserindo mensagens e respostas em tempo real com deduplicacao local.
- Dependencia adicionada no frontend: `@microsoft/signalr`.
- Validacao: `npm.cmd run build` executado com sucesso e API compilada com `dotnet build src/ProjetoFinal.Api/ProjetoFinal.Api.csproj -p:OutDir=...\\artifacts\\api-build\\`.

## 2026-03-22 (correcao de compatibilidade do SignalR)
- Hub do forum ajustado para serializar payloads com o mesmo `PascalCase` da API REST, evitando divergencia de nomes de propriedades entre REST e eventos em tempo real.
- Metodos do hub simplificados para assinatura mais direta (`JoinThread` e `LeaveThread`), reduzindo risco de binding inconsistente na invocacao do cliente.
- Validacao: `npm.cmd run build` e `dotnet build src/ProjetoFinal.Api/ProjetoFinal.Api.csproj -p:OutDir=...\\artifacts\\api-build\\`.

## 2026-03-22 (correcao de CORS para SignalR local)
- Politica de CORS da API ajustada para suportar negociacao do SignalR com credenciais entre `localhost:4200` e `localhost:5179`.
- `AllowAnyOrigin` foi substituido por politica com `SetIsOriginAllowed(_ => true)` e `AllowCredentials()`, evitando resposta com `Access-Control-Allow-Origin: *` em chamadas com credenciais.
- Validacao: `dotnet build src/ProjetoFinal.Api/ProjetoFinal.Api.csproj -p:OutDir=...\\artifacts\\api-build\\`.

## 2026-03-22 (branch e inicio do chat flutuante)
- Branch dedicada criada para a funcionalidade: `feat/chat-flutuante-online`.
- Backend recebeu uma primeira base de chat em tempo real com `ChatHub`, rastreador de presenca em memoria por turma e broadcast de mensagens do controller para o grupo SignalR correspondente.
- DTO de chat passou a expor `SenderName` e `UpdatedAt`, e o repositório de mensagens agora inclui remetente/midia para hidratar historico e eventos em tempo real.
- Controller de `chat/messages` foi protegido com autenticacao, passou a resolver o usuario logado no backend e validar acesso a turma antes de listar/enviar mensagens.
- Frontend ganhou widget global flutuante no `app.component`, servicos REST/SignalR de chat e listagem de turmas com chat habilitado para aluno matriculado ou professor dono do curso.
- Validacao: `npm.cmd run build` com sucesso e `dotnet build src/ProjetoFinal.Api/ProjetoFinal.Api.csproj -p:OutDir=D:\\Dev\\Wallacevff\\projeto-final-definitivo\\artifacts\\api-build\\` com sucesso.
- Observacao: `dotnet build ProjetoFinal.sln` continuou falhando por lock da instancia em execucao de `ProjetoFinal.Api` sobre `src/ProjetoFinal.Api/bin/Debug/net8.0`.

## 2026-03-22 (chat coletivo e individual por turma)
- Chat evoluido para dois modos por turma: conversa com a turma inteira e conversa individual entre participantes daquela mesma turma.
- `ChatMessage` agora possui `RecipientId` opcional; quando nulo, a mensagem pertence ao canal coletivo, e quando preenchido, representa uma conversa privada bilateral.
- `ChatHub` passou a adicionar o usuario tambem a um grupo privado por turma/participante, permitindo broadcast isolado de DMs sem perder a presenca coletiva da turma.
- Widget flutuante do Angular agora exibe a opcao `Turma inteira` e a lista de participantes elegiveis da turma, alternando historico e envio conforme a conversa selecionada.
- Migration manual adicionada em `src/ProjetoFinal.Infra.Data/Migrations/20260322215000_AddChatDirectMessages.cs` para criar `RecipientId` em `ChatMessages`, indice e FK para `Users`.
- Validacao: `dotnet build ProjetoFinal.sln` e `npm.cmd run build` executados com sucesso.

## 2026-03-22 (correcao do historico do chat)
- Filtro do historico ajustado para usar `CurrentUserId` separado de `SenderId`, evitando distorcao na consulta do canal coletivo e das conversas individuais.
- Banco local atualizado manualmente com a coluna `RecipientId`, indice e FK em `ChatMessages`, contornando falha do `dotnet ef` no ambiente (`System.Runtime 10.0.0.0` ausente no tooling).
- Validacao: `dotnet build ProjetoFinal.sln` e `npm.cmd run build` executados com sucesso.

## 2026-03-22 (refinos visuais e de ordenacao do chat)
- .gitignore passou a ignorar qualquer pasta rtifacts via regra **/artifacts/.
- Repositorio de cursos passou a incluir Enrollments -> Student, garantindo nomes reais dos alunos nas conversas individuais do chat.
- Widget flutuante do chat foi refinado visualmente com painel maior, laterais mais compactas, conversa principal mais ampla e tipografia reduzida.
- Layout do chat foi reorganizado para listar participantes da turma com mais clareza e mostrar o nome real do participante no cabecalho, estado vazio e rotulo da conversa individual.
- Conversa passou a usar viewport ancorado no rodape, com ordenacao cronologica por SentAt e rolagem automatica ate a ultima mensagem, aproximando o comportamento do padrao do ChatGPT.
- Validacao: dotnet build ProjetoFinal.sln e 
pm.cmd run build executados com sucesso.


## 2026-03-23 (ajustes de scroll e altura do chat)
- Scroll automatico do chat reforcado para mensagens recebidas, com efeito reativo observando a lista ordenada e scroll disparado apos o render final.
- Area util da conversa aumentada com painel mais alto e compactacao leve do cabecalho, banner de presenca e composer.
- Validacao: dotnet build ProjetoFinal.sln e 
pm.cmd run build executados com sucesso; o build da solucao pode emitir warnings de lock quando ProjetoFinal.Api esta em execucao.


## 2026-03-23 (alerta sonoro e ordenacao fina do chat)
- Adicionado alerta sonoro discreto para mensagens recebidas de outros usuarios, sem tocar para mensagens proprias e silenciando quando a aba esta em foco na conversa aberta.
- O som passou a usar Web Audio no frontend e so fica disponivel apos a primeira interacao do usuario com a pagina, respeitando restricoes do navegador.
- Ordenacao do chat refinada para comparar SentAt explicitamente ate segundos e milissegundos, reduzindo empates e inversoes em mensagens muito proximas.
- Validacao: dotnet build ProjetoFinal.sln e 
pm.cmd run build executados com sucesso.


## 2026-03-24 (endurecimento do backend de chat)
- Chat recebeu normalizacao de GUIDs opcionais (`RecipientId`, `ReplyToMessageId`, `MediaResourceId`) antes da persistencia para evitar `Guid.Empty` quebrando FKs no banco.
- Historico REST do chat passou a enviar `PageNumber=1` explicitamente, enquanto a base generica de repositorios agora trata paginacao ausente ou invalida com fallback seguro.
- Validacao de acesso ao chat em controller, hub e app service foi otimizada com consulta enxuta de turma/instrutor (`GetChatAccessInfoAsync`), reduzindo carga de includes desnecessarios.
- Middleware global de excecao passou a registrar a causa raiz (`GetBaseException`) no log de erro interno, facilitando diagnostico em homologacao/producao.
- Observacao: o diretorio `src/ProjetoFinal.Infra.Data/Migrations` esta com um reset local do historico para um novo `Initial`, o que precisa de avaliacao cuidadosa antes de ser versionado.


## 2026-04-24 (integracao inicial com IA)
- Criado o modulo de insights com IA em `AiInsightsController` e `IAiInsightsAppService`, com suporte a provedor compativel com OpenAI/DeepSeek via `AiProviderConfiguration`.
- A API agora oferece resumo didatico de conteudo por `contentId` e consolidacao de duvidas frequentes do forum para professores autenticados.
- `ProjetoFinal.Api.csproj` recebeu `UserSecretsId` para guardar a chave localmente sem commitar segredo.
- `IoCManager` passou a registrar `AiProvider` por options + typed `HttpClient`.
- `ForumPostRepository` foi ampliado para incluir `ClassGroup`, permitindo contexto de turma nas analises de duvidas recorrentes.
- `course-content-viewer` ganhou o card `Resumo com IA`, com estados de carregamento, erro e exibicao de pontos principais/pontos de atencao.
- `dashboard` do professor ganhou a secao `Duvidas frequentes com IA`, exibindo tema, pergunta representativa, curso/turma e acao sugerida.
- Validacao: `dotnet build ProjetoFinal.sln` e `npm run build` executados com sucesso; permanecem warnings conhecidos (`NU1903` do AutoMapper e budgets/seletores do Angular).


## 2026-04-24 (migracao para .NET 10)
- Todos os projetos `.csproj` da solucao foram atualizados de `net8.0` para `net10.0`.
- Criado `global.json` na raiz fixando o SDK `10.0.104` com `rollForward` em `latestFeature`.
- `ProjetoFinal.Aplication.Services.csproj` foi ajustado adicionalmente apos um desalinhamento inicial de target framework identificado no restore.
- Validacao: `dotnet build ProjetoFinal.sln` executado com sucesso em `net10.0`; `npm run build` tambem concluiu com sucesso.
- Observacao: a migracao nao tratou upgrade de pacotes NuGet; continuam presentes warnings de vulnerabilidade em dependencias antigas que agora ficaram mais visiveis no restore/build com o SDK 10.


## 2026-04-24 (baseline de migration para banco legado)
- Identificada falha na inicializacao da API: `There is already an object named 'MediaResources' in the database` durante `context.Database.MigrateAsync()`.
- Causa: o banco ja possuia o schema criado, mas o diretorio de migrations local foi consolidado em uma unica migration `Initial`, deixando o historico do EF inconsistente com o estado real do banco.
- `DataSeeder` foi ajustado para detectar ausencia da tabela `__EFMigrationsHistory` em banco com tabelas principais existentes (`MediaResources`, `Users`, `Courses`), criar o historico e registrar a migration `20260324111045_Initial` como baseline antes do `MigrateAsync`.
- Validacao: `dotnet build ProjetoFinal.sln` e `npm run build` executados com sucesso.


## 2026-04-24 (correcao da connection string no startup)
- Identificada nova falha de startup: `The ConnectionString property has not been initialized` ao abrir conexao no `DataSeeder`.
- Causa: o `IoCManager` estava populando o `DbContext` por bind manual da secao `ConnectionStrings`, abordagem mais fragil que o uso direto de `GetConnectionString("DefaultConnection")`.
- Ajustado `AddApplicationDbContext` para usar `configuration.GetConnectionString("DefaultConnection")` e falhar cedo com mensagem clara se estiver ausente.
- `DataSeeder` tambem passou a validar `context.Database.GetConnectionString()` antes de tentar inspecionar tabelas.
- Validacao: `dotnet build ProjetoFinal.sln` e `npm run build` executados com sucesso.


## 2026-04-24 (correcao da conexao compartilhada no DataSeeder)
- O erro de `ConnectionString property has not been initialized` persistia mesmo apos o ajuste do `IoCManager` porque o helper `TableExistsAsync` descartava a conexao compartilhada do `DbContext` com `await using`.
- Isso quebrava a segunda chamada sequencial de verificacao de tabela no fluxo de baseline da migration.
- Ajustado o helper para nao descartar `context.Database.GetDbConnection()`, apenas abrir/fechar quando necessario e manter o descarte restrito ao `DbCommand`.
- Validacao: `dotnet build ProjetoFinal.sln` executado com sucesso.
