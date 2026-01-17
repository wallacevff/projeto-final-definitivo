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
