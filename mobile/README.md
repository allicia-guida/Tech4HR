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

O aplicativo está compatível com o backend público em [allicia-guida/Tech4HR](https://github.com/allicia-guida/Tech4HR), analisado no commit `505737c`.

Para ativar a integração, copie `.env.example` para `.env` e informe em `EXPO_PUBLIC_API_URL` a URL HTTPS onde a API estiver publicada. Não coloque conexão de banco, chave JWT ou qualquer outro segredo no aplicativo.

Com uma URL configurada, o app usa:

- `POST /api/auth/login-funcionario`;
- `GET /api/pontos/meus-pontos`;
- `POST /api/pontos/registrar`.

Sem URL configurada, o aplicativo bloqueia o login e o registro de ponto para não apresentar dados locais como oficiais. A lista objetiva do que ainda precisa ser criado no backend está em `docs/api/BACKEND-GAPS.md`.

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
