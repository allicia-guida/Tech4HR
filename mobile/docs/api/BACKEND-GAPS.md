# Recursos ainda ausentes no backend

Análise baseada no repositório `allicia-guida/Tech4HR`, commit `505737c`.

## Já integrado ao aplicativo

- login do funcionário por JWT;
- armazenamento protegido do token;
- expiração da sessão;
- histórico de ponto do funcionário autenticado;
- registro de entrada, início do intervalo, retorno do intervalo e saída;
- uso do horário devolvido pelo servidor;
- geolocalização obrigatória com precisão, rejeição de posição simulada e geofence opcional;
- histórico com coordenadas associadas a cada marcação;
- tratamento de respostas 400, 401 e 403.

## Endpoints necessários para concluir a operação real

1. recuperação e redefinição de senha com token de uso único;
2. consulta e edição do perfil do próprio funcionário;
3. jornadas, escalas, tolerâncias e feriados por empresa e funcionário;
4. saldo oficial de horas e fechamento mensal calculado no servidor;
5. solicitação, cancelamento e aprovação de correções de ponto;
6. solicitação e aprovação de férias, faltas, atestados e afastamentos;
7. upload privado de anexos com validação de conteúdo e expiração;
8. notificações remotas e registro de dispositivos;
9. sincronização offline com chave de idempotência e resolução de conflitos;
10. comprovante oficial de ponto verificável;
11. renovação e revogação de sessão;
12. endpoint de saúde e versão mínima obrigatória do aplicativo.

## Ajustes de segurança recomendados no backend

- adicionar limitação de tentativas no login e no registro de ponto;
- configurar CORS somente para clientes administrativos autorizados;
- não devolver mensagens internas do SQL Server;
- adicionar chave de idempotência ao registro de ponto;
- definir explicitamente o fuso da empresa ao calcular `DataPonto`;
- adicionar trilha de auditoria para alterações e aprovações;
- usar segredos de implantação para conexão SQL e chave JWT;
- adicionar rotação de tokens e revogação de sessões;
- revisar retenção e acesso a CPF, atestados e anexos conforme a LGPD.

Enquanto esses endpoints não existirem, o aplicativo mantém correções, ausências e perfil complementar somente como operações locais pendentes e não apresenta esses dados como confirmados pelo servidor. A área demonstrativa do gestor foi retirada da navegação de produção.
