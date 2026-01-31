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

### Forum e chat
- Forum com topicos por turma, posts autenticados, replies e pagina dedicada de discussao.
- Criacao de topicos restrita a professores; alunos podem responder.

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
