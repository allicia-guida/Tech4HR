# Tech4Hr

Sistema de gerenciamento de Recursos Humanos e controle de ponto desenvolvido como projeto acadêmico do curso de Análise e Desenvolvimento de Sistemas.

O **Tech4Hr** disponibiliza uma aplicação Web para gerenciamento de funcionários, usuários administrativos e registros de ponto, além de uma API responsável pelas regras de negócio, autenticação e comunicação com o banco de dados.

## Funcionalidades

### Área Administrativa

- Autenticação com JWT
- Perfis de acesso `ADMIN` e `OPERACIONAL`
- Dashboard administrativo
- Cadastro, consulta e edição de funcionários
- Ativação e desativação de funcionários conforme permissões
- Gerenciamento de usuários administrativos
- Consulta de registros de ponto
- Filtros por funcionário e período
- Exportação de registros em XML e XLSX
- Visualização do histórico de ponto por funcionário

### Área do Funcionário

- Login exclusivo para funcionários
- Registro de ponto pelo navegador
- Sequência automática de registros:
  - Entrada
  - Saída para almoço
  - Retorno do almoço
  - Saída
- Visualização dos registros do dia
- Histórico individual de pontos
- Interface responsiva para dispositivos móveis

## Acessibilidade

O projeto possui recursos de acessibilidade visual, incluindo opção de alteração das cores da interface para melhorar a utilização por pessoas com daltonismo.

A aplicação também foi desenvolvida com layouts responsivos para desktop, tablet e dispositivos móveis.

## Tecnologias

### Backend

- C#
- .NET 10
- ASP.NET Core Web API
- Entity Framework Core
- JWT Authentication

### Frontend Web

- ASP.NET Core MVC
- Razor Views
- HTML
- CSS
- JavaScript

### Banco de Dados

- Microsoft SQL Server
- Azure SQL Database
- Stored Procedures

### Testes e versionamento

- xUnit
- Git
- GitHub

## Arquitetura

O projeto está dividido em três componentes principais:

```text
Tech4HR
├── Tech4Hr.API
│   └── API, autenticação e regras de negócio
│
├── Tech4Hr.Web
│   └── Aplicação Web ASP.NET Core MVC
│
├── Tech4Hr.Tests
│   └── Testes automatizados
│
└── database
    └── Scripts SQL e stored procedures
```

A aplicação Web consome a API através de requisições HTTP.

```text
Usuário
   │
   ▼
Tech4Hr.Web
   │
   │ HTTP / JWT
   ▼
Tech4Hr.API
   │
   ▼
Azure SQL Database
```

## Controle de Ponto

O registro de ponto segue a sequência:

```text
ENTRADA
   ↓
SAIDA_ALMOCO
   ↓
ENTRADA_ALMOCO
   ↓
SAIDA
```

A sequência é validada pelo backend e pela stored procedure responsável pelo registro.

Os eventos são mantidos para auditoria e o sistema também mantém um espelho diário contendo os horários de entrada, almoço, retorno e saída.

Os horários de negócio são tratados considerando o fuso horário de Brasília/São Paulo.

## Segurança

O sistema utiliza autenticação baseada em JWT.

Existem diferentes níveis de acesso:

| Perfil | Permissões |
|---|---|
| ADMIN | Acesso completo às funcionalidades administrativas |
| OPERACIONAL | Gerenciamento de funcionários e consulta de pontos |
| FUNCIONÁRIO | Registro e consulta dos próprios pontos |

As senhas são armazenadas utilizando hash e não são mantidas em texto puro.

Também existem validações para impedir operações que comprometam o controle administrativo, como a desativação do último usuário `ADMIN` ativo.

## Exportação

Os registros de ponto podem ser exportados nos formatos:

- XLSX
- XML

Os filtros aplicados na consulta podem ser utilizados para geração dos relatórios.

## Responsividade

A interface foi desenvolvida para diferentes tamanhos de tela, incluindo:

- Desktop
- Tablet
- Smartphones

A área de registro de ponto do funcionário possui uma interface especialmente adaptada para utilização em dispositivos móveis.

## Como executar

### Pré-requisitos

- .NET 10 SDK
- SQL Server ou acesso ao Azure SQL Database
- Git

Clone o repositório:

```bash
git clone <URL-DO-REPOSITORIO>
cd Tech4HR
```

Configure a connection string e demais informações sensíveis utilizando **User Secrets** ou variáveis de ambiente.

> Não armazene senhas, connection strings ou chaves JWT diretamente no repositório.

Execute a API:

```bash
dotnet run --project Tech4Hr.API
```

Em outro terminal, execute a aplicação Web:

```bash
dotnet run --project Tech4Hr.Web
```

## Testes

Para executar os testes automatizados:

```bash
dotnet test Tech4Hr.slnx
```

Para validar a compilação completa da solução:

```bash
dotnet build Tech4Hr.slnx
```

## Fluxo de desenvolvimento

O projeto utiliza branches para separar o desenvolvimento das funcionalidades.

```text
main
  └── dev
       ├── feature/web
       └── feature/app
```

As funcionalidades são desenvolvidas nas branches específicas e posteriormente integradas à `dev` por meio de Pull Requests.

Após os testes de integração, a versão estável pode ser incorporada à `main`.

## Status do Projeto

### Web

- [x] Autenticação administrativa
- [x] Autenticação de funcionários
- [x] Dashboard
- [x] Gerenciamento de funcionários
- [x] Gerenciamento de usuários
- [x] Registro de ponto
- [x] Histórico de pontos
- [x] Consulta administrativa
- [x] Filtros
- [x] Exportação XML
- [x] Exportação XLSX
- [x] Responsividade
- [x] Recursos de acessibilidade

### API

- [x] Autenticação JWT
- [x] Gerenciamento de funcionários
- [x] Gerenciamento de usuários
- [x] Registro de ponto
- [x] Consulta de pontos
- [x] Controle de permissões
- [x] Testes automatizados

## Projeto Acadêmico

Projeto desenvolvido para fins acadêmicos como parte do **PIM IV**, no curso de **Análise e Desenvolvimento de Sistemas**.

O Tech4Hr faz parte da evolução do projeto **Tech4Life**, adicionando funcionalidades voltadas ao gerenciamento de funcionários e controle de jornada.

## Autores

Desenvolvido pela equipe responsável pelo projeto **Tech4Hr**.
