# Tech4HR Public API

API independente para o aplicativo móvel Tech4HR. Este projeto não utiliza nem altera o banco de dados da API legada.

## Variáveis obrigatórias

- `ConnectionStrings__PublicApi`
- `Jwt__Issuer`
- `Jwt__Audience`
- `Jwt__Key`
- `Jwt__ExpirationMinutes`

`Jwt__Key` deve ser uma chave aleatória em Base64 com pelo menos 32 bytes.

## Primeiro administrador

Na primeira inicialização de um banco vazio, configure `BootstrapAdmin__Name`, `BootstrapAdmin__Email` e `BootstrapAdmin__Password`. Remova a senha do ambiente depois que o administrador for criado.

## Recuperação de senha

Configure `Smtp__Host`, `Smtp__Port`, `Smtp__EnableSsl`, `Smtp__Username`, `Smtp__Password`, `Smtp__From` e `PasswordReset__AppUrl`.

## Execução

```text
dotnet restore
dotnet run
```

Documentação interativa: `/swagger`.
Saúde da API: `/health`.

## Recursos

- `/api/v1/auth`: login e recuperação de senha.
- `/api/v1/users`: usuários e perfis de acesso.
- `/api/v1/points`: registro geolocalizado e histórico de ponto.
- `/api/v1/corrections`: solicitações e aprovação de correções.
- `/api/v1/schedules`: jornadas semanais e fuso horário.
- `/api/v1/absences`: férias, faltas, atestados e afastamentos.
- `/api/v1/holidays`: calendário de feriados.
- `/api/v1/summaries`: atrasos, horas trabalhadas, horas extras e banco de horas.
- `/api/v1/audit`: trilha administrativa sem senhas ou dados sensíveis.
- `/api/v1/attachments`: anexos protegidos em PDF, JPEG ou PNG.

O campo `idempotencyKey` do registro de ponto permite repetir uma operação offline sem criar registros duplicados.
