# Tech4HR Mobile

Aplicativo corporativo de registro de ponto desenvolvido com React Native, Expo 57, TypeScript e Expo Router.

## Funcionalidades

- splash, login e recuperação de senha validados com React Hook Form + Zod;
- home com relógio local, status e registros do dia;
- confirmação de ponto com biometria e geolocalização em primeiro plano;
- histórico real da API, detalhe de cada ponto e comprovantes;
- geração e compartilhamento de comprovante em PDF;
- perfil e configurações;
- jornada semanal, tolerâncias, intervalos, feriados e ausências configuráveis;
- cálculo provisório de atrasos, horas extras e banco de horas;
- solicitações de correção com justificativa e fluxo de aprovação preparado;
- histórico diário, semanal e mensal com filtros e saldo acumulado;
- calendário mensal com indicadores e detalhe de cada jornada;
- anexos locais em correções, férias, faltas e atestados;
- lembretes locais, biometria e detecção de conexão;
- fila local que nunca apresenta um ponto offline como confirmado;
- documentos preparatórios para publicação nas lojas;
- temas Sistema, Claro e Escuro com preferência persistida;
- estados de carregamento, vazio e erro;
- login e ponto sem fallback demonstrativo: sem API segura, a operação é bloqueada.

## Integração com o backend

O aplicativo fala com uma de duas APIs, escolhida em `EXPO_PUBLIC_API_TARGET`:

| Valor | API | Para que serve |
|---|---|---|
| `tech4hr` | `Tech4Hr.API`, a mesma do site e do banco que já existe | Login do funcionário e do operacional, registro de ponto e histórico |
| `public` (padrão) | `Tech4Hr.PublicApi`, independente, com banco próprio | Correções, ausências, jornadas, resumos e anexos |

Para ativar a integração, copie `.env.example` para `.env`, escolha o alvo e informe em `EXPO_PUBLIC_API_URL` o endereço da API. Em staging e produção o endereço precisa ser HTTPS. Em desenvolvimento também vale `http`, por exemplo `http://192.168.0.10:5287` (o IP do computador na rede, não `localhost`). Não coloque conexão de banco, chave JWT ou qualquer outro segredo no aplicativo.

Com `EXPO_PUBLIC_API_TARGET=tech4hr` o app usa:

- `POST /api/auth/login-funcionario`, com o e-mail corporativo e a senha;
- `GET /api/pontos/hoje`, que devolve o dia e a hora oficiais de Brasília;
- `GET /api/pontos/meus-pontos`, o histórico;
- `POST /api/pontos/registrar`, uma batida por vez, na ordem entrada, saída para o almoço, retorno e saída.

O dia e a hora mostrados no app, e o dia em que cada batida cai, são os de Brasília e vêm da API (`src/utils/server-clock.ts`). A hora ou o fuso do aparelho não mudam isso.

Limites do alvo `tech4hr`: a API ainda não guarda a localização da batida, não tem recuperação de senha por e-mail e não tem correções, ausências, jornadas nem anexos. Essas telas continuam disponíveis no alvo `public`. A lista do que falta criar na API está em `docs/api/BACKEND-GAPS.md`.

Sem URL configurada, o aplicativo bloqueia o login e o registro de ponto para não apresentar dados locais como oficiais.

## Executar

```bash
npm install
npm run android
```

Para executar no iOS:

```bash
npm run ios
```

## Qualidade

```bash
npm run typecheck
npm run lint
npm test
```

O contrato compatível com o backend está em `docs/api/openapi.yaml`. Os textos preparatórios das lojas estão em `docs/store`.

O relógio e os cálculos da interface são locais. Na integração de produção, o servidor deve continuar sendo a autoridade do timestamp, das regras oficiais, dos saldos e das aprovações.

## Segurança

Consulte [RELATORIO-SEGURANCA.md](./RELATORIO-SEGURANCA.md) antes de integrar autenticação, API ou publicar o aplicativo.
