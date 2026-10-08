# Tech4Hr: documentação técnica das melhorias (PIM IV)

Este documento descreve as melhorias feitas no Tech4Hr na etapa de integração entre a API, o site (Web), o banco de dados e o aplicativo móvel. Ele serve de base para o relatório do PIM IV do curso de Análise e Desenvolvimento de Sistemas e registra o que foi mudado, por que foi mudado, como foi testado e o que ainda depende de outras pessoas.

Data de referência: 06/10/2026.

## 1. Objetivo

O Tech4Hr é um sistema de recursos humanos e controle de ponto. Antes desta etapa, os três componentes funcionavam, mas havia problemas de integração e de regra de negócio:

1. O horário do ponto dependia do relógio do navegador e do servidor Web, e não de uma fonte única.
2. A desativação de usuários e funcionários não estava restrita aos administradores em todas as camadas, e uma conta desativada continuava com acesso até o token expirar.
3. O perfil Operacional existia como um tipo de usuário administrativo separado, com login próprio.
4. A interface web tinha pouca adaptação para celular e nenhum recurso de tema ou rolagem suave.
5. O aplicativo móvel estava ligado a uma API diferente, com banco próprio, e não à API e ao banco do restante do sistema.

Os requisitos tratados foram:

| Requisito | Solução resumida | Seção |
|---|---|---|
| API do horário de batida do ponto | Horário de Brasília calculado pela API, endpoint `GET /api/pontos/hoje`, erros de regra em português | 4 |
| Desativar usuário somente por administradores | Trava na rota do Web, na API e conferência da conta a cada requisição | 5 |
| Operacional dentro do login de funcionário | Coluna `Funcionario.NivelAcesso`, login administrativo somente para ADMIN | 6 |
| Modificar o front-end web | Novo visual responsivo, tema escuro, modo para daltonismo, rolagem suave | 7 |
| App baseado no web e conectado à API | App Expo religado à `Tech4Hr.API` por configuração | 8 |
| Conectar tudo ao banco existente | Mesmas tabelas, novo script `04`, ordem de implantação definida | 9 |

## 2. Arquitetura

```text
Navegador ──► Tech4Hr.Web (ASP.NET Core MVC) ──┐
                                               ├─► Tech4Hr.API (ASP.NET Core Web API) ──► Azure SQL (Tech4HrDB)
Aplicativo móvel (Expo, React Native) ─────────┘
```

| Componente | Tecnologia | Responsabilidade |
|---|---|---|
| `Tech4Hr.API` | .NET 10, Entity Framework Core, JWT | Regras de negócio, autenticação, acesso ao banco |
| `Tech4Hr.Web` | ASP.NET Core MVC, Razor, CSS e JavaScript próprios | Interface para administradores e funcionários |
| `Tech4Hr.Tests` | xUnit, EF Core InMemory | Testes automatizados da API e do Web |
| `database` | T-SQL | Tabelas, procedure de registro de ponto, trigger e migração |
| Aplicativo móvel (branch `feature/app`) | Expo, React Native, TypeScript | Registro e consulta de ponto no celular |

O Web e o aplicativo não acessam o banco: tudo passa pela API. Isso mantém as regras em um lugar só.

### 2.1 Perfis e permissões

O token JWT carrega o tipo de conta (`tipo_conta`) e o perfil (`role`).

| Perfil | Tipo de conta | Login | O que pode |
|---|---|---|---|
| ADMIN | USUARIO | Administrativo | Tudo: usuários, funcionários, ativação e desativação, consulta de pontos |
| OPERACIONAL | FUNCIONARIO | Funcionário | Bate o próprio ponto, gerencia funcionários e consulta pontos. Não ativa nem desativa |
| FUNCIONARIO | FUNCIONARIO | Funcionário | Bate e consulta o próprio ponto |

Rotas da API e quem as acessa:

| Rota | Quem acessa |
|---|---|
| `POST /api/auth/login` | Anônimo (somente contas ADMIN são aceitas) |
| `POST /api/auth/login-funcionario` | Anônimo (funcionários e operacionais) |
| `GET /api/pontos/hoje`, `POST /api/pontos/registrar`, `GET /api/pontos/meus-pontos` | Conta de funcionário, inclusive o operacional |
| `GET /api/pontos/consultar` | ADMIN e OPERACIONAL |
| `POST, GET, PUT /api/funcionarios` | ADMIN e OPERACIONAL |
| `PATCH /api/funcionarios/{id}/status` | Somente ADMIN |
| `/api/usuarios` (todas as rotas) | Somente ADMIN |

## 3. Controle de versão

O trabalho foi desenvolvido em branches separadas, com um commit por tema, para facilitar a revisão: `feature/ajustes-leandro` (a partir da `dev`) para a API, o Web e o banco, e `feature/app-leandro` (a partir da `feature/app`) para o aplicativo. Cada etapa virou um commit, na ordem das seções 4 a 8. As melhorias complementares (limite de tentativas de login, integração contínua e remoção do Bootstrap) e o visual novo do Web ficaram em commits próprios depois deles.

Em 07/10/2026 o trabalho do Web, da API, do banco, do CI e da documentação foi integrado à `dev` por merge, e o trabalho do aplicativo foi incorporado à `feature/app`, que é a branch dele. As branches temporárias foram apagadas depois da integração, e os commits continuam no histórico da `dev` e da `feature/app`. O aplicativo segue em linha própria: trazê-lo para a `dev` é uma decisão da equipe, por Pull Request.

## 4. Horário do ponto

### 4.1 Problema

A procedure `sp_RegistrarPonto` já gravava o horário no fuso de Brasília, mas o restante do sistema não acompanhava:

- A data de "hoje" no Web vinha do relógio do servidor Web (`DateTime.Today`), e o relógio exibido na tela vinha do navegador (`new Date()`). Um servidor em UTC, ou um celular com a hora errada, mostrava um dia e uma hora diferentes dos que o banco gravava. Entre 21h e 24h em Brasília, o servidor em UTC já está no dia seguinte.
- Quando a procedure recusava uma batida (primeira marcação diferente de entrada, ou fora de sequência), a API devolvia erro 500 genérico.

### 4.2 Solução

Passou a existir uma única fonte de data e hora: a API.

- `RelogioBrasil` (`Tech4Hr.API/Services`) calcula a hora e o dia de Brasília a partir de um `TimeProvider`, o que permite testar com relógio fixo. Ele procura o fuso `America/Sao_Paulo` e, se não existir (Windows), `E. South America Standard Time`.
- `JornadaPonto` (`Tech4Hr.API/Services`) concentra a ordem das batidas e traduz os erros da procedure em mensagens.
- O endpoint `GET /api/pontos/hoje` devolve o retrato do dia. Exemplo (valores ilustrativos):

```json
{
  "dataReferencia": "2026-10-05",
  "agora": "2026-10-05T22:12:40-03:00",
  "fusoHorario": "America/Sao_Paulo",
  "ponto": {
    "idPonto": 5,
    "dataPonto": "2026-10-05T00:00:00",
    "entrada": "2026-10-05T08:01:12",
    "saidaAlmoco": "2026-10-05T12:00:03",
    "entradaAlmoco": null,
    "saida": null
  },
  "proximoTipoRegistro": "ENTRADA_ALMOCO"
}
```

Sequência de batidas, validada pela API e pela procedure:

```text
ENTRADA → SAIDA_ALMOCO → ENTRADA_ALMOCO → SAIDA
```

### 4.3 Erros de regra

Os erros lançados pela procedure (`THROW`) passaram a virar resposta 409 com mensagem em português:

| Código na procedure | Resposta da API |
|---|---|
| 50001 | Funcionário inexistente ou inativo |
| 50002 | A primeira marcação do dia deve ser entrada |
| 50003 | Marcação fora de sequência |
| 2601 ou 2627 (índice único) | Esta marcação já foi registrada hoje |

### 4.4 Efeito no Web

A tela de registro do funcionário usa a resposta de `GET /api/pontos/hoje`. O relógio da tela é corrigido uma vez pela diferença entre a hora do servidor e a do navegador e depois segue sozinho, então um celular com a hora errada mostra e registra o mesmo horário de Brasília. A data é escrita em português independentemente do idioma do servidor.

## 5. Desativação somente por administradores

### 5.1 Problema

A API já exigia o perfil ADMIN para ativar ou desativar, mas a rota do Web aceitava também o OPERACIONAL: só o botão era escondido. Além disso, uma conta desativada continuava válida até o token expirar (60 minutos).

### 5.2 Solução

A regra passou a valer em quatro camadas:

1. Interface: o botão de ativar ou desativar só aparece para administradores.
2. Web: a rota `POST /Funcionarios/AlterarStatus` confere o perfil na sessão e recusa os demais.
3. API: `PATCH /api/funcionarios/{id}/status` e as rotas de usuários exigem o perfil ADMIN.
4. Conta válida a cada requisição: o `ValidadorDeConta`, executado no evento `OnTokenValidated` do JWT, consulta o banco e recusa o token se a conta foi desativada ou se o perfil mudou desde o login.

Quando a API responde 401 a um token que o Web enviou, o `SessaoExpiradaHandler` registra o fato e o `SessaoExpiradaFilter` limpa a sessão e leva a pessoa à tela de login com a mensagem "Sua sessão expirou ou a conta foi desativada. Entre novamente.". Antes, a tela mostrava um erro técnico.

A proteção já existente contra desativar o último administrador ativo (trigger `trg_Usuario_ManterAdminAtivo`) foi mantida.

## 6. Operacional dentro do login de funcionário

### 6.1 Decisão

O operacional passou a ser um funcionário com permissão extra, e não mais um tipo de usuário administrativo. Com isso ele entra pelo login de funcionário, bate ponto como os demais e ganha as telas de gestão. O login administrativo ficou só para o ADMIN.

### 6.2 Mudanças

- Banco: nova coluna `Funcionario.NivelAcesso` (`FUNCIONARIO` ou `OPERACIONAL`, padrão `FUNCIONARIO`), com restrição de valores. Script `database/04_operacional_funcionario.sql`.
- API: o login de funcionário emite o perfil que está no cadastro. Somente o ADMIN pode definir ou alterar o nível de acesso de um funcionário. A criação de usuários OPERACIONAL na tabela `Usuario` foi bloqueada.
- Web: a sessão do operacional guarda o tipo de conta de funcionário e o nível OPERACIONAL, e os menus e rotas de gestão aparecem para ele. O cadastro de funcionários por um operacional sempre cria o nível comum.
- Migração: os usuários OPERACIONAL antigos deixam de entrar pelo login administrativo e precisam ser recadastrados como funcionários com nível Operacional. O CPF e a data de admissão não existem na tabela `Usuario`, então não há migração automática. O próprio script `04` lista quem precisa ser recadastrado.

## 7. Interface web

O CSS foi reescrito com abordagem mobile first e variáveis de design (cores, espaçamentos, raios). O visual segue uma linha institucional: superfícies planas separadas por linhas finas de 1 px, sem sombras nem degradês, cantos de 4 a 6 px, uma única cor de destaque (azul `#1d4e89`) e texto no lugar de ícones decorativos. A tipografia usa a fonte do sistema (Segoe UI no Windows), então não há dependência de fontes externas.

| Recurso | Como funciona |
|---|---|
| Navegação | A partir de 768 px, menu em texto dentro do cabeçalho, com o item atual sublinhado. Abaixo disso, barra de abas fixa na parte de baixo, com ícone e texto |
| Tabelas | Abaixo de 768 px cada linha vira um cartão, com o rótulo de cada coluna (`data-label`). Entre 768 e 1099 px as colunas de CPF e admissão saem da tabela para ela caber sem rolagem |
| Rolagem | `scroll-behavior: smooth`, desligada quando o sistema pede movimento reduzido |
| Tema escuro | Segue a preferência do sistema automaticamente |
| Daltonismo | O botão com um olho, no cabeçalho, liga um modo que muda bem a aparência: fundo creme, azul e laranja da paleta Okabe e Ito no lugar do par verde e vermelho, links sempre sublinhados, faixa marcada sob o cabeçalho e listras nas séries dos gráficos. A escolha fica lembrada no navegador. A situação ativo ou inativo também se distingue pelo formato do marcador, e não só pela cor |
| Ícones | Ícones SVG em um único arquivo parcial, usados na barra de abas do celular, no olho do modo para daltônicos e em poucos controles. Botões e ações de tabela usam texto |
| Gráficos do painel | Três gráficos feitos só com HTML e CSS (sem biblioteca): situação de hoje, equipe e expedientes dos últimos 14 dias. Cada um tem legenda com os valores e uma tabela de dados como alternativa |
| Botão de editar | Amarelo em todas as telas e em todos os temas, com texto escuro (contraste de pelo menos 10:1) |
| Confirmações | Caixa de diálogo própria (`<dialog>`) no lugar do `confirm()` do navegador |
| Formulários | Botão de mostrar senha, proteção contra clique duplo, avisos que somem sozinhos |
| Impressão | Folha de estilo própria: a consulta de pontos imprime sem cabeçalho, filtros nem botões |
| Instalação | Manifesto web e ícones, para adicionar o site à tela inicial do celular |

### Gráficos do painel

O painel mostra, além do resumo em listas, três gráficos calculados a partir do que a API já devolve (lista de funcionários e consulta de pontos dos últimos 14 dias):

| Gráfico | O que mostra |
|---|---|
| Situação de hoje | Onde cada funcionário ativo está no dia: trabalhando, em almoço, jornada encerrada ou sem registro. Funcionário inativo não conta |
| Equipe | Funcionários ativos e inativos |
| Expedientes dos últimos 14 dias | Em cada dia, quantos funcionários iniciaram o expediente, separados em jornada completa (com saída) e em aberto (sem saída). Em dias passados, "em aberto" costuma indicar batida esquecida |

As contas ficam em código puro (`GraficosDoPainel`), coberto por testes, e o "hoje" é o dia de Brasília (`DiaDeBrasilia`), não o do servidor Web. Se a consulta de pontos falhar, o resumo continua na tela e só os gráficos de ponto dão lugar a um aviso.

As cores seguem uma paleta categórica validada por script (separação para os tipos comuns de daltonismo, faixa de luminosidade e contraste), em passos próprios para o tema claro e para o escuro. Como alguns tons ficam abaixo de 3:1 contra o fundo claro, cada série tem legenda com o valor em texto e há uma tabela de dados em cada gráfico. No modo para daltônicos e na impressão as séries ganham listras, para não dependerem só da cor.

Também foram corrigidos três problemas: a validação de formulário no navegador não rodava porque o jQuery não era carregado antes dos scripts de validação, a rota inicial mostrava a página padrão do projeto, e o login voltava sempre para a aba "Funcionário" depois de um erro. O Bootstrap, que o site não usava, foi removido de `wwwroot/lib` (ficaram o jQuery e as bibliotecas de validação de formulário).

## 8. Aplicativo móvel

O aplicativo estava ligado à `Tech4Hr.PublicApi`, uma API independente com banco SQLite próprio, e não à API e ao banco do restante do sistema. Foi adicionada a variável `EXPO_PUBLIC_API_TARGET`:

| Valor | API usada | Uso |
|---|---|---|
| `tech4hr` | `Tech4Hr.API`, a mesma do site | Login do funcionário e do operacional, ponto e histórico |
| `public` (padrão) | `Tech4Hr.PublicApi` | Telas que ainda não existem na API principal: correções, ausências, jornadas, anexos |

O padrão continua `public` para não quebrar o `.env` de quem já usa o aplicativo.

Com o alvo `tech4hr`, o app usa `POST /api/auth/login-funcionario`, `GET /api/pontos/hoje`, `GET /api/pontos/meus-pontos` e `POST /api/pontos/registrar`. O dia e a hora exibidos, e o dia em que cada batida cai, são os de Brasília e vêm da API (módulo `server-clock`). A hora ou o fuso do aparelho não alteram isso. As mensagens de erro da API (por exemplo, o 409 de marcação fora de sequência) aparecem na tela. A paleta de cores do aplicativo foi alinhada à do site.

Limites do alvo `tech4hr`: a API ainda não guarda a localização da batida, não tem recuperação de senha por e-mail e não tem correções, ausências, jornadas nem anexos.

## 9. Banco de dados

Tabelas: `Usuario`, `Funcionario`, `Ponto` (espelho diário) e `RegistroPonto` (eventos de auditoria).

| Script | Função |
|---|---|
| `01_criar_tabelas.sql` | Cria as tabelas, somente em banco novo |
| `02_registrar_ponto.sql` | Cria a procedure `sp_RegistrarPonto` |
| `03_triggers.sql` | Impede remover ou desativar o último administrador ativo |
| `04_operacional_funcionario.sql` | Cria `Funcionario.NivelAcesso` e a restrição de valores. Pode ser executado mais de uma vez |

### 9.1 Ordem de implantação

1. Executar o script `04` no banco. Ele é seguro com a API antiga no ar, porque a coluna nasce com o valor padrão `FUNCIONARIO`.
2. Publicar a API nova. Sem a coluna, a API nova não consegue consultar funcionários.
3. Publicar o Web.
4. Recadastrar os operacionais antigos como funcionários com nível Operacional.

### 9.2 Permissões do banco

A API precisa de permissão de leitura e escrita nas tabelas e de `EXECUTE` na procedure `sp_RegistrarPonto`. Quem executa o script `04` precisa de permissão para alterar a tabela `Funcionario`.

### 9.3 Situação em 07/10/2026

A verificação foi feita só com leitura e, depois, com um roteiro de ponta a ponta contra o Azure SQL:

- A coluna `NivelAcesso` e a regra de valores já existem: o script 04 foi aplicado pela equipe.
- A conta de desenvolvimento recebeu `EXECUTE` e `VIEW DEFINITION` na procedure. A procedure do banco é igual à do repositório (comparação sem comentários e espaços).
- Uma batida real feita às 23h de Brasília, quando o banco em UTC já estava no dia seguinte, caiu no espelho do dia correto.
- O roteiro de ponta a ponta teve 48 verificações na API real e 29 no Web, todas aprovadas. Ele criou contas de teste (um ADMIN, funcionários comuns e operacionais), exercitou login, permissões, as quatro batidas na ordem, os erros 409, a desativação com efeito imediato, o rebaixamento de perfil e o limite de tentativas de login (429), e depois apagou tudo o que criou. O banco voltou ao estado de antes, com os mesmos identificadores em todas as tabelas.
- Ainda falta recadastrar os operacionais antigos, reduzir ao mínimo a permissão da conta de desenvolvimento e trocar a senha dela.

## 10. Segurança

- Autenticação por JWT, com validação de emissor, público, assinatura e validade, sem tolerância de relógio.
- Senhas armazenadas com hash (`PasswordHasher` do ASP.NET Core).
- Segredos (connection string e chave JWT) ficam em User Secrets ou variáveis de ambiente, nunca no repositório. A chave JWT precisa estar em Base64 e ter ao menos 32 bytes.
- Sessão do Web com cookie `HttpOnly` e expiração por inatividade de 30 minutos.
- Conta desativada ou com perfil alterado perde o acesso na próxima requisição.
- Mensagens de login não revelam se o e-mail existe.
- Limite de tentativas de login por conta: cinco senhas erradas em dez minutos bloqueiam o e-mail por dez minutos. Durante o bloqueio a API responde 429 com o tempo de espera, mesmo que a senha certa chegue, e o Web e o aplicativo mostram essa mensagem. Os valores ficam em `Seguranca:Login` (`MaxFalhas`, `JanelaMinutos` e `BloqueioMinutos`).

O limite é por conta e não por IP porque o Web chama a API de servidor para servidor: a API enxergaria um único IP para todos os usuários, que dividiriam o mesmo limite. E-mails que não existem também contam, para o bloqueio não revelar quais contas existem. Os contadores ficam na memória da API, então reiniciar a API zera tudo e, com mais de uma instância, cada uma conta por conta própria. Como o bloqueio é por conta, quem conhece o e-mail de outra pessoa pode mantê-la bloqueada errando a senha de propósito. Por isso o bloqueio é curto.

## 11. Testes e verificação

### 11.1 Testes automatizados

| Projeto | Resultado |
|---|---|
| API e Web (`dotnet test Tech4Hr.slnx`) | 84 testes aprovados, 0 falhas |
| Aplicativo (`npm test`, vitest) | 50 testes aprovados, 0 falhas |

Cobertura principal dos testes da API e do Web:

| Arquivo | O que verifica |
|---|---|
| `PontoHorarioTests` | Fuso de Brasília com relógio fixo, virada de dia, ordem das batidas, endpoint `hoje` |
| `DesativacaoTests` | Somente ADMIN desativa, conta desativada é recusada, 401 leva ao login com aviso |
| `OperacionalTests` | Login do operacional, permissões, bloqueio de criação de OPERACIONAL em `Usuario`, payload com nível e admissão |
| `LimiteDeLoginTests` | Bloqueio por tentativas, expiração do bloqueio, janela de tempo, zeragem no sucesso, contas independentes, e a mensagem do 429 no Web |
| `GraficosDoPainelTests` | Contas dos gráficos do painel (situação de hoje, equipe, expedientes por dia, topo do eixo) e o dia de Brasília na virada de dia |
| `AuthAuthorizationTests` e `UsuariosControllerTests` | Regras de autorização existentes |

### 11.2 Integração contínua

O repositório tem um workflow do GitHub Actions (`.github/workflows/ci.yml`) que compila a solução e roda os testes em Linux e em Windows a cada push na `main`, na `dev` e nas branches `feature/*`, e em todo Pull Request para a `main` e a `dev`. O aplicativo tem o seu (`.github/workflows/mobile.yml`, na branch `feature/app`), que confere tipos, lint e testes quando algo da pasta `mobile` muda.

No aplicativo, além dos testes, foram executados `typecheck` e `lint` sem erros.

### 11.3 Verificação manual e ponta a ponta

Além da navegação com a API simulada, o sistema foi exercitado contra a API real e o Azure SQL por um roteiro de 77 verificações (48 na API e 29 no Web), todas aprovadas. As contas de teste foram criadas com nome iniciado em "ZZ Teste", com senhas geradas na hora e guardadas só em arquivo local, e removidas no fim, com conferência do banco contra uma foto tirada antes.

Navegação com a API simulada:

O Web foi executado e navegado com uma API simulada que reproduz o contrato real. Foram verificados: login dos três perfis, registro de ponto, desativação com diálogo de confirmação, conta desativada voltando ao login, e ausência de rolagem horizontal em 375, 768, 1100, 1280 e 1440 px, além dos modos escuro e para daltonismo. O aplicativo foi testado com o código real contra a mesma API simulada (login, histórico, hora oficial, registro, mensagem 409 e 401), com o computador em UTC já no dia seguinte, e exibiu o dia e a hora corretos de Brasília.

### 11.4 O que os testes não provam

- Os testes automatizados usam banco em memória, que não executa a procedure `sp_RegistrarPonto`. A procedure real foi confirmada pelo roteiro de ponta a ponta contra o Azure SQL, que hoje é manual.
- O aplicativo ainda não foi exercitado em um celular ou emulador.

## 12. Limitações conhecidas e trabalhos futuros

- Turno que cruza a meia-noite: a procedure exige ENTRADA como primeira batida do dia, então uma saída de madrugada do turno anterior é recusada.
- `GET /api/pontos/meus-pontos` devolve o histórico inteiro. Convém paginar.
- A API não guarda a localização da batida nem oferece recuperação de senha, e o aplicativo foi preparado para ambos.
- O limite de tentativas de login guarda os contadores na memória da API. Para várias instâncias com contagem compartilhada, seria preciso um armazenamento comum, como um cache distribuído.
- A `Tech4Hr.PublicApi` usa .NET 8, que sai de suporte em novembro de 2026, e existem duas APIs com contratos diferentes. Vale unificá-las.
- O roteiro de ponta a ponta contra o banco real foi feito com scripts guardados fora do repositório. Transformá-lo em testes de integração automáticos, contra um SQL Server de teste, evitaria depender de uma pessoa para repeti-lo.

## 13. Como executar

Os passos de instalação, a configuração dos segredos e a ordem dos scripts do banco estão no `README.md` da raiz do repositório. Para o aplicativo, o passo a passo está em `mobile/README.md` na branch `feature/app`.

```bash
dotnet test Tech4Hr.slnx
dotnet run --project Tech4Hr.API
dotnet run --project Tech4Hr.Web
```
