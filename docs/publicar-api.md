# Publicar a Tech4Hr.API

Este guia descreve como colocar a API em um servidor com HTTPS, para o Web e o aplicativo usarem fora da rede local. A imagem foi construída e testada com Docker contra o Azure SQL real: sobe, protege as rotas, faz login e responde no horário de Brasília.

## A imagem

O `Tech4Hr.API/Dockerfile` gera a imagem. O contexto de build é a pasta `Tech4Hr.API`.

```bash
docker build -t tech4hr-api Tech4Hr.API
```

- Escuta na porta 8080 dentro do container.
- Roda sem privilégios de administrador.
- Espera que o HTTPS seja terminado pelo servidor na frente dela (Render, Azure App Service e semelhantes) e confia nos cabeçalhos `X-Forwarded-*`. O aviso "Failed to determine the https port" no log é esperado: não há redirecionamento a fazer dentro do container.
- A imagem base do .NET traz o banco de fusos horários do sistema, que o relógio de Brasília (`America/Sao_Paulo`) exige. Se trocar a imagem base por uma enxuta (por exemplo Alpine), instale o pacote `tzdata`.

## Variáveis de ambiente

Os segredos nunca vão para o repositório nem para a imagem. Cadastre-os no painel do servidor:

| Variável | Conteúdo |
|---|---|
| `ConnectionStrings__Tech4HrDB` | Connection string do Azure SQL |
| `Jwt__Key` | Chave em Base64 com 32 bytes ou mais (gere uma nova para o servidor) |
| `Jwt__Issuer` | Por exemplo `Tech4Hr` |
| `Jwt__Audience` | Por exemplo `Tech4HrClientes` |
| `Jwt__ExpirationMinutes` | `60` |

Os limites do bloqueio de login podem ser ajustados com `Seguranca__Login__MaxFalhas`, `Seguranca__Login__JanelaMinutos` e `Seguranca__Login__BloqueioMinutos`.

## Firewall do Azure SQL

O banco só aceita conexões de endereços liberados. Quem se conecta ao banco é o servidor onde a API roda, e não o celular nem o navegador de quem usa o sistema. Portanto:

- O endereço de saída do servidor da API precisa ser liberado no firewall do servidor SQL (portal do Azure, servidor SQL, Rede, regras de firewall).
- Se a API ficar hospedada no próprio Azure, também há a opção de permitir serviços e recursos do Azure.
- O endereço dos aparelhos que usam o app não precisa de liberação.

Quem faz essa alteração é quem tiver permissão no portal do Azure sobre o servidor SQL.

## Render

1. Criar um Web Service a partir do repositório, com o diretório raiz `Tech4Hr.API` e o ambiente Docker.
2. Cadastrar as variáveis da tabela acima.
3. Se o Render não encontrar a porta sozinho, cadastrar também `PORT` com o valor `8080`.
4. Liberar no firewall do Azure SQL os endereços de saída que o Render informa para a região do serviço.
5. No plano gratuito o serviço dorme quando fica parado e a primeira chamada demora.

## Azure App Service

1. Criar um App Service para contêineres com a imagem da API.
2. Cadastrar as variáveis como configurações do aplicativo e `WEBSITES_PORT` com o valor `8080`.
3. Permitir serviços do Azure no firewall do servidor SQL, ou liberar os endereços de saída do App Service.

## Conferir depois de publicar

Troque `URL` pelo endereço publicado:

```bash
curl -i https://URL/api/pontos/hoje
```

O esperado é `401`: a API respondeu e recusou por falta de token. Depois, entre com uma conta de funcionário de teste pelo Web ou pelo app.

## Pontos de atenção

- Os contadores do limite de tentativas de login ficam na memória da API. Use uma única instância, senão cada uma conta por conta própria.
- O aplicativo exige HTTPS fora do ambiente de desenvolvimento. Ao gerar o APK, o `eas.json` precisa de `EXPO_PUBLIC_API_TARGET=tech4hr` e de `EXPO_PUBLIC_API_URL` com o endereço HTTPS da API publicada.
