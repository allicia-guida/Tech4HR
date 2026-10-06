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

O trabalho foi feito na branch `feature/ajustes-leandro` (a partir da `dev`) para a API, o Web e o banco, e na branch `feature/app-leandro` (a partir da `feature/app`) para o aplicativo. Cada etapa virou um commit separado, na ordem das seções 4 a 8, para facilitar a revisão. A integração com a `dev` segue o fluxo de Pull Request já adotado pela equipe.

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

O CSS foi reescrito com abordagem mobile first e variáveis de design (cores, espaçamentos, raios, sombras).

| Recurso | Como funciona |
|---|---|
| Navegação | Menu lateral no desktop (a partir de 1024 px). No celular e no tablet, barra de abas fixa na parte de baixo |
| Tabelas | Abaixo de 760 px cada linha vira um cartão, com o rótulo de cada coluna (`data-label`) |
| Rolagem | `scroll-behavior: smooth`, desligada quando o sistema pede movimento reduzido |
| Tema escuro | Segue a preferência do sistema automaticamente |
| Daltonismo | Botão de contraste na barra superior ativa uma paleta segura (Okabe e Ito), lembrada no navegador |
| Ícones | Conjunto de ícones SVG em um único arquivo parcial, sem dependência externa |
| Confirmações | Caixa de diálogo própria (`<dialog>`) no lugar do `confirm()` do navegador |
| Formulários | Botão de mostrar senha, proteção contra clique duplo, avisos que somem sozinhos |
| Instalação | Manifesto web e ícones, para adicionar o site à tela inicial do celular |

Também foram corrigidos três problemas: a validação de formulário no navegador não rodava porque o jQuery não era carregado antes dos scripts de validação, a rota inicial mostrava a página padrão do projeto, e o login voltava sempre para a aba "Funcionário" depois de um erro.

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

## 10. Segurança

- Autenticação por JWT, com validação de emissor, público, assinatura e validade, sem tolerância de relógio.
- Senhas armazenadas com hash (`PasswordHasher` do ASP.NET Core).
- Segredos (connection string e chave JWT) ficam em User Secrets ou variáveis de ambiente, nunca no repositório. A chave JWT precisa estar em Base64 e ter ao menos 32 bytes.
- Sessão do Web com cookie `HttpOnly` e expiração por inatividade de 30 minutos.
- Conta desativada ou com perfil alterado perde o acesso na próxima requisição.
- Mensagens de login não revelam se o e-mail existe.

## 11. Testes e verificação

### 11.1 Testes automatizados

| Projeto | Resultado |
|---|---|
| API e Web (`dotnet test Tech4Hr.slnx`) | 52 testes aprovados, 0 falhas |
| Aplicativo (`npm test`, vitest) | 50 testes aprovados, 0 falhas |

Cobertura principal dos testes da API e do Web:

| Arquivo | O que verifica |
|---|---|
| `PontoHorarioTests` | Fuso de Brasília com relógio fixo, virada de dia, ordem das batidas, endpoint `hoje` |
| `DesativacaoTests` | Somente ADMIN desativa, conta desativada é recusada, 401 leva ao login com aviso |
| `OperacionalTests` | Login do operacional, permissões, bloqueio de criação de OPERACIONAL em `Usuario`, payload com nível e admissão |
| `AuthAuthorizationTests` e `UsuariosControllerTests` | Regras de autorização existentes |

No aplicativo, além dos testes, foram executados `typecheck` e `lint` sem erros.

### 11.2 Verificação manual

O Web foi executado e navegado com uma API simulada que reproduz o contrato real. Foram verificados: login dos três perfis, registro de ponto, desativação com diálogo de confirmação, conta desativada voltando ao login, e ausência de rolagem horizontal em 375, 768, 1100, 1280 e 1440 px, além dos modos escuro e para daltonismo. O aplicativo foi testado com o código real contra a mesma API simulada (login, histórico, hora oficial, registro, mensagem 409 e 401), com o computador em UTC já no dia seguinte, e exibiu o dia e a hora corretos de Brasília.

### 11.3 O que os testes não provam

- Os testes automatizados usam banco em memória, que não executa a procedure `sp_RegistrarPonto`. O comportamento real da procedure só se confirma contra o SQL Server.
- A API real contra o Azure SQL e o aplicativo em celular ou emulador dependem de acessos e aparelhos que não estavam disponíveis ao fim desta etapa.

## 12. Limitações conhecidas e trabalhos futuros

- Turno que cruza a meia-noite: a procedure exige ENTRADA como primeira batida do dia, então uma saída de madrugada do turno anterior é recusada.
- `GET /api/pontos/meus-pontos` devolve o histórico inteiro. Convém paginar.
- A API não guarda a localização da batida nem oferece recuperação de senha, e o aplicativo foi preparado para ambos.
- Não há limite de tentativas de login.
- A solução não tem pipeline de integração contínua.
- A pasta `Tech4Hr.Web/wwwroot/lib/bootstrap` não é mais usada pelo site e pode ser removida por decisão da equipe.
- A `Tech4Hr.PublicApi` usa .NET 8, que sai de suporte em novembro de 2026, e existem duas APIs com contratos diferentes. Vale unificá-las.
- Testes de integração contra um SQL Server de teste dariam cobertura real da procedure.

## 13. Como executar

Os passos de instalação, a configuração dos segredos e a ordem dos scripts do banco estão no `README.md` da raiz do repositório. Para o aplicativo, o passo a passo está em `mobile/README.md` na branch `feature/app`.

```bash
dotnet test Tech4Hr.slnx
dotnet run --project Tech4Hr.API
dotnet run --project Tech4Hr.Web
```
