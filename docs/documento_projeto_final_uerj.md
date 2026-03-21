# Documento de Projeto Final (Modelo UERJ)

> **Instrucao de uso**: este arquivo foi estruturado para servir como base de especificacao do sistema e relatorio final academico. Substitua os campos entre colchetes conforme as exigencias da disciplina/orientador.

## Capa (Modelo Institucional)

**UNIVERSIDADE DO ESTADO DO RIO DE JANEIRO (UERJ)**  
**[Centro/Faculdade]**  
**[Curso de Graduacao]**  

**[NOME COMPLETO DO AUTOR]**

**PLATAFORMA EAD MULTIMODAL COM TURMAS INTERATIVAS, DISTRIBUICAO DE CONTEUDO E APOIO ANALITICO AO PROCESSO DE APRENDIZAGEM**

**Rio de Janeiro**  
**2026**

---

## Folha de Rosto

**[NOME COMPLETO DO AUTOR]**

**PLATAFORMA EAD MULTIMODAL COM TURMAS INTERATIVAS, DISTRIBUICAO DE CONTEUDO E APOIO ANALITICO AO PROCESSO DE APRENDIZAGEM**

Projeto Final apresentado ao curso de [NOME DO CURSO] da Universidade do Estado do Rio de Janeiro (UERJ), como requisito parcial para obtencao do grau de [Bacharel/Licenciado/Tecnologo] em [AREA].

Orientador(a): Prof(a). [NOME DO ORIENTADOR(A)]  
Coorientador(a) (se houver): Prof(a). [NOME]

**Rio de Janeiro**  
**2026**

---

## Resumo

Este trabalho apresenta o desenvolvimento de uma plataforma de Ensino a Distancia (EAD) multimodal, concebida para atender dois modelos pedagogicos complementares: (i) cursos com turmas interativas e (ii) cursos focados na distribuicao de conteudo didatico. A solucao integra funcionalidades de gestao academica, comunicacao assíncrona e sincrona, distribuicao de midias e avaliacao de atividades com suporte a anexos multimidia.

No modelo de turmas interativas, o sistema contempla controle de vagas, inscricoes com aprovacao, atividades avaliativas por turma e mecanismos de interacao por forum e chat. No modelo de distribuicao, prioriza-se acesso escalavel aos materiais e discussoes. A arquitetura foi implementada com backend em .NET (API REST), frontend Angular e persistencia em SQL Server, com armazenamento de arquivos em objeto (MinIO/S3).

Como evolucao funcional, foi incorporado feedback estruturado em correcoes de atividades, incluindo rubrica por criterio, tags diagnosticas e acao recomendada, possibilitando extracao de indicadores educacionais e suporte a dashboards docentes.

**Palavras-chave**: EAD, plataforma educacional, rubrica, feedback estruturado, analise de aprendizagem, arquitetura web.

---

## Abstract

This final project presents the development of a multimodal distance learning platform designed to support two complementary educational models: (i) interactive class-based courses and (ii) content-distribution courses. The solution integrates academic management, asynchronous and synchronous communication, media delivery, and assignment evaluation workflows with multimedia attachments.

In the interactive model, the system provides seat control, approval-based enrollment, class-oriented assessments, and collaboration through forum and chat modules. In the distribution model, the focus is scalable content delivery and discussion support. The platform architecture uses a .NET REST API, Angular frontend, SQL Server for transactional data, and object storage (MinIO/S3) for media assets.

As a functional evolution, structured grading feedback was introduced, including criterion-based rubrics, diagnostic tags, and recommended actions, enabling educational indicators and dashboard-oriented analysis.

**Keywords**: e-learning, educational platform, rubric, structured feedback, learning analytics, web architecture.

---

## Sumario (Modelo)

1. Introducao  
2. Contexto e Problema  
3. Objetivos  
4. Escopo do Sistema  
5. Fundamentacao e Referencial Tecnico  
6. Especificacao do Sistema  
7. Arquitetura e Implementacao  
8. Qualidade, Seguranca e Observabilidade  
9. Validacao e Resultados  
10. Cronograma e Gestao do Projeto  
11. Riscos e Mitigacoes  
12. Conclusao e Trabalhos Futuros  
Referencias  
Apendices e Anexos

---

## 1. Introducao

A digitalizacao do ensino ampliou a demanda por ambientes virtuais de aprendizagem capazes de combinar flexibilidade pedagogica, boa experiencia de uso e instrumentos de acompanhamento do desempenho discente. Este projeto propoe uma plataforma EAD que une caracteristicas de diferentes sistemas consolidados e as adapta para um contexto academico com foco em usabilidade, rastreabilidade e apoio a decisao do professor.

O trabalho organiza-se em duas frentes: desenvolvimento tecnico da plataforma e formalizacao academica do produto, com requisitos, arquitetura, validacao e discussoes sobre sustentabilidade da solucao.

---

## 2. Contexto e Problema

### 2.1 Cenário
- Sistemas EAD tradicionais tendem a privilegiar apenas um modelo de curso (turma fechada ou distribuicao massiva).
- Professores necessitam de instrumentos mais objetivos para acompanhar dificuldades recorrentes.
- Feedback exclusivamente textual limita consolidacao de indicadores e comparacoes entre turmas.

### 2.2 Problema
Como construir uma plataforma EAD que suporte diferentes modelos de oferta de curso e, ao mesmo tempo, gere dados estruturados para apoiar o acompanhamento pedagogico?

### 2.3 Justificativa
- Relevancia social e educacional: facilita acesso e monitoramento de aprendizagem.
- Relevancia tecnica: integra API web moderna, frontend SPA e storage de objetos.
- Relevancia academica: permite estudo de indicadores pedagogicos a partir de dados operacionais.

### 2.4 Analise comparativa com plataformas de mercado

Tabela comparativa de diferenciais do projeto em relacao a Google Classroom, Moodle e Udemy (considerando funcionalidades nativas em fluxo padrao):

| Funcionalidade | Projeto | Google Classroom | Moodle | Udemy | Diferencial do Projeto |
|---|---|---|---|---|---|
| Duas modalidades no mesmo produto (turma interativa + distribuicao de conteudo) | Sim (nativo) | Parcial | Parcial | Nao | Unificacao de dois modelos pedagogicos no mesmo fluxo operacional |
| Comentario temporal em video enviado pelo aluno (correcao em ponto exato do video) | Sim (nativo) | Nao nativo no fluxo padrao | Nao nativo no fluxo padrao | Nao nativo no fluxo padrao | Revisao audiovisual orientada por tempo no contexto de atividade |
| Feedback estruturado na correcao (rubrica por criterio + tags + acao recomendada) | Sim (nativo) | Nao nativo no fluxo padrao | Parcial (dependente de configuracao/plugin) | Nao nativo no fluxo padrao | Base pronta para indicadores e dashboards docentes |
| Controle de inscricao por senha e/ou aprovacao em turmas com limite | Sim (nativo) | Parcial | Parcial | Nao (modelo aberto de marketplace) | Governanca de acesso mais aderente a cursos fechados e academicos |
| Persistencia unificada de metadados academicos e objetos multimidia em arquitetura propria | Sim | Nao (plataforma fechada) | Parcial (geralmente exige customizacao de infraestrutura) | Nao (plataforma fechada) | Maior liberdade para evoluir analytics, IA e regras institucionais |

> Observacao: a comparacao considera recursos nativos e comportamento padrao de uso. Moodle, por ser altamente extensivel, pode aproximar alguns cenarios mediante plugins e customizacoes.

---

## 3. Objetivos

### 3.1 Objetivo geral
Desenvolver e validar uma plataforma EAD multimodal com recursos de interacao, avaliacao e feedback estruturado para suporte a analise educacional.

### 3.2 Objetivos especificos
- Implementar cadastro e autenticacao de usuarios com perfis de professor e aluno.
- Permitir criacao e gestao de cursos em duas modalidades: turmas interativas e distribuicao de conteudo.
- Viabilizar forum, chat, atividades e submissao de anexos multimidia.
- Implementar comentarios temporais em videos para revisao de atividades.
- Estruturar feedback docente com rubricas, tags diagnosticas e acoes recomendadas.
- Disponibilizar base de dados para futuros dashboards e indicadores de aprendizagem.

---

## 4. Escopo do Sistema

### 4.1 Escopo funcional
- Cadastro de usuarios e autenticacao JWT.
- Criacao/edicao/publicacao de cursos.
- Gerenciamento de turmas, capacidade e inscricao por aprovacao/senha.
- Publicacao de conteudos e anexos.
- Forum por turma e por curso.
- Chat em contexto de turmas interativas.
- Atividades com envio textual e anexos.
- Correcao com nota, status e feedback estruturado.

### 4.2 Fora de escopo (versao atual)
- Proctoring (fiscalizacao automatica de provas).
- Motor de recomendacao adaptativa em tempo real.
- Chatbot tutor totalmente autonomo.
- Integracao nativa com ERPs academicos externos.

---

## 5. Fundamentacao e Referencial Tecnico

### 5.1 Conceitos-chave
- Ambientes Virtuais de Aprendizagem (AVA).
- Learning Analytics.
- Rubricas avaliativas e feedback formativo.
- Arquitetura em camadas para aplicacoes web.

### 5.2 Base tecnologica adotada
- **Backend**: ASP.NET Core (API REST).
- **Frontend**: Angular.
- **Banco de dados**: SQL Server.
- **Storage de arquivos**: MinIO (S3 compatível).
- **Containerizacao**: Docker/Swarm.

---

## 6. Especificacao do Sistema

### 6.1 Perfis de usuario
- **Professor/Instrutor**: cria cursos, publica materiais, define atividades, corrige e acompanha indicadores.
- **Aluno**: inscreve-se, consome conteudo, participa de forum/chat e envia atividades.

### 6.2 Requisitos funcionais (RF)
- **RF01**: permitir cadastro e autenticacao de usuarios.
- **RF02**: permitir criacao de cursos por professores.
- **RF03**: suportar modalidades "turmas interativas" e "distribuicao".
- **RF04**: permitir gerenciamento de turmas, vagas e politicas de inscricao.
- **RF05**: permitir upload/download de anexos e midias.
- **RF06**: permitir publicacao e entrega de atividades.
- **RF07**: permitir correcao com nota e status.
- **RF08**: permitir feedback estruturado (rubrica + tags + acao recomendada).
- **RF09**: permitir anotacoes temporais em videos de submissao.
- **RF10**: disponibilizar dados para consolidacao de indicadores pedagogicos.

### 6.3 Requisitos nao funcionais (RNF)
- **RNF01 - Seguranca**: autenticacao via token, validacao de acesso por perfil, cuidado com segredos fora do codigo.
- **RNF02 - Desempenho**: resposta adequada para consultas e operacoes de upload.
- **RNF03 - Disponibilidade**: suporte a deploy containerizado e resiliencia de servicos.
- **RNF04 - Usabilidade**: interface responsiva e fluxo claro para docentes/discentes.
- **RNF05 - Escalabilidade**: separacao de responsabilidades entre API, banco e storage.
- **RNF06 - Auditabilidade**: persistencia de eventos de submissao/correcao para rastreabilidade.

### 6.4 Regras de negocio
- Um aluno nao pode enviar duas submissões distintas para a mesma atividade (na configuracao atual).
- Em turmas com capacidade lotada, novas inscricoes devem ser bloqueadas ou pendentes de aprovacao, conforme configuracao.
- Correcao pode registrar nota numerica e feedback textual.
- Feedback estruturado aceita rubrica por criterio na escala de 1 a 5.
- Tags diagnosticas devem ser armazenadas de forma normalizada para analise posterior.

### 6.5 Casos de uso principais
- UC01: professor cria curso.
- UC02: professor cria turma e define regras de inscricao.
- UC03: aluno solicita inscricao.
- UC04: professor publica conteudo e atividade.
- UC05: aluno envia atividade (texto e anexos).
- UC06: professor corrige submissao com feedback estruturado.
- UC07: aluno visualiza retorno da correcao.

---

## 7. Arquitetura e Implementacao

### 7.1 Visao arquitetural
- **Cliente (Angular)**: interface e orquestracao de chamadas HTTP.
- **API (.NET)**: regras de negocio, autorizacao e contratos.
- **Banco SQL**: dados transacionais (usuarios, cursos, turmas, atividades, correcoes).
- **Object Storage (MinIO)**: arquivos e midias.

### 7.2 Camadas da solucao
- `ProjetoFinal.Api`: endpoints e configuracao HTTP.
- `ProjetoFinal.Application.Contracts`: DTOs e interfaces.
- `ProjetoFinal.Aplication.Services`: servicos de negocio.
- `ProjetoFinal.Domain`: entidades e regras de dominio.
- `ProjetoFinal.Infra.Data`: EF Core, mapeamentos e repositorios.

### 7.3 Modelo de dados (resumo)
Entidades centrais:
- Usuario
- Curso
- Turma (`ClassGroup`)
- Atividade
- Submissao de atividade (`ActivitySubmission`)
- Anexos de submissao

Campos relevantes de feedback estruturado em submissao:
- `MasteryScore`
- `ApplicationScore`
- `CommunicationScore`
- `FeedbackTags`
- `RecommendedAction`

### 7.4 Integracao e deploy
- API e frontend empacotados em imagem Docker.
- SQL Server e MinIO em servicos dedicados.
- Nginx como proxy reverso HTTPS.

---

## 8. Qualidade, Seguranca e Observabilidade

### 8.1 Qualidade
- Build de backend com `dotnet build`.
- Build de frontend com `npm run build`.
- Tratamento de erros e padronizacao de respostas no backend.

### 8.2 Seguranca
- Controle de acesso por papel e por contexto (professor/aluno).
- Recomendacao de segredo JWT fora do repositorio.
- Validacao de upload e restricoes de tamanho conforme ambiente.

### 8.3 Observabilidade
- Logs de aplicacao.
- Diagnostico de inconsistencias entre banco e storage (scripts operacionais).

---

## 9. Validacao e Resultados

### 9.1 Cenarios validados
- Criacao e publicacao de cursos.
- Fluxo de inscricao em turmas.
- Submissao de atividades com anexos.
- Correcao com feedback textual e estruturado.
- Consulta dos dados de correcao pelo aluno.

### 9.2 Resultados observados
- Plataforma atende aos fluxos principais previstos no escopo.
- Introducao de feedback estruturado habilita analises quantitativas por criterio e tag.

### 9.3 Limitacoes atuais
- Warnings de budget CSS no frontend ainda presentes.
- Evolucoes de analytics e IA ainda em etapa de planejamento.

---

## 10. Cronograma e Gestao do Projeto

### 10.1 Macroetapas executadas
- Levantamento de requisitos e proposta.
- Implementacao de nucleo funcional (cadastro, cursos, turmas, atividades).
- Integracao de midias e storage.
- Correcoes, anotacoes em video e refinamento de UX.
- Evolucao para feedback estruturado.

### 10.2 Planejamento de continuidade
- Painel analitico docente (dashboards por turma/curso).
- Pipeline de IA para agrupamento de duvidas.
- Cobertura de testes automatizados e2e.

---

## 11. Riscos e Mitigacoes

- **Risco**: inconsistencias entre metadata no banco e objetos no storage.  
  **Mitigacao**: verificacao de existencia de objeto no upload e scripts de saneamento.

- **Risco**: crescimento de bundle frontend.  
  **Mitigacao**: revisao de estilos/componentes e estrategia de splitting.

- **Risco**: dependencia de configuracao manual em deploy.  
  **Mitigacao**: padronizacao por compose, configs e documentacao operacional.

---

## 12. Conclusao e Trabalhos Futuros

O projeto alcancou o objetivo de disponibilizar uma plataforma EAD com suporte a diferentes modelos de ensino e mecanismos de avaliacao mais ricos. A evolucao para feedback estruturado representa um passo importante para aproximar o sistema de praticas orientadas a dados no contexto educacional.

Como continuidade, recomenda-se priorizar a camada de analytics/IA para consolidacao automatica de duvidas recorrentes, deteccao de lacunas de aprendizagem e geracao de insights para acao pedagogica.

---

## Referencias (Modelo)

> Substitua pelos itens bibliograficos efetivamente utilizados, no estilo exigido pelo curso (ABNT/UERJ).

1. MICROSOFT. ASP.NET Core Documentation. Disponivel em: <https://learn.microsoft.com/aspnet/core/>. Acesso em: [data].
2. ANGULAR. Angular Documentation. Disponivel em: <https://angular.dev/>. Acesso em: [data].
3. MINIO. MinIO Documentation. Disponivel em: <https://min.io/docs/>. Acesso em: [data].
4. SILVA, [Nome]. [Titulo da obra de referencia pedagogica/AVA]. [Local]: [Editora], [ano].

---

## Apendice A - Checklist de formatacao academica (UERJ)

- [ ] Capa com identificacao institucional completa.
- [ ] Folha de rosto com natureza do trabalho e orientador.
- [ ] Resumo e abstract com palavras-chave.
- [ ] Sumario coerente com titulos do corpo do texto.
- [ ] Citacoes e referencias no padrao exigido.
- [ ] Numeracao de secoes e paginas revisada.
- [ ] Revisao ortografica e padronizacao terminologica.

## Apendice B - Estrutura sugerida para banca

- Motivacao e problema.
- Solucao proposta.
- Demo dos fluxos principais.
- Diferenciais (feedback estruturado e base para analytics).
- Limitacoes e roadmap de evolucao.
