# Relatório de Segurança — Tech4HR Mobile

Data da análise: 22 de setembro de 2026

## Resumo executivo

Nenhuma chave de API, chave privada, segredo de cliente ou credencial de serviço foi encontrada nos arquivos atuais nem no histórico Git analisado. A senha de demonstração e os dados pessoais reais que estavam incorporados no código foram removidos.

O aplicativo recebeu proteções adicionais para desenvolvimento e demonstração e agora está integrado aos endpoints de autenticação e ponto existentes no repositório `allicia-guida/Tech4HR`, commit `505737c`. Ele ainda não deve ser usado como sistema de ponto em produção até a API ser publicada em HTTPS e receber os controles pendentes descritos neste relatório.

## Resultado da busca por segredos

| Item analisado                    | Resultado                           |
| --------------------------------- | ----------------------------------- |
| Chaves de API                     | Nenhuma encontrada                  |
| Chaves privadas e certificados    | Nenhum encontrado                   |
| Segredos OAuth                    | Nenhum encontrado                   |
| Tokens Bearer ou tokens de sessão | Nenhum encontrado                   |
| Senhas incorporadas no código     | Removidas                           |
| Dados pessoais reais incorporados | Substituídos por dados genéricos    |
| Histórico Git                     | Nenhum padrão de segredo encontrado |

Variáveis com prefixo `EXPO_PUBLIC_` são públicas e entram no aplicativo compilado. Elas podem conter somente configurações não confidenciais, como a URL pública da API. Chaves administrativas, segredos OAuth, chaves privadas e credenciais de banco devem existir exclusivamente no backend ou no gerenciador seguro do ambiente de implantação.

## Riscos corrigidos

| Risco                                          | Severidade anterior | Correção aplicada                                                                                      |
| ---------------------------------------------- | ------------------: | ------------------------------------------------------------------------------------------------------ |
| Senha de demonstração preenchida no login      |                Alta | Campos agora iniciam vazios e a senha não é persistida                                                 |
| Nome, e-mail e matrícula reais no código       |                Alta | Dados substituídos por uma identidade genérica de demonstração                                         |
| Rotas internas abertas sem sessão              |                Alta | Adicionada proteção central de rotas e redirecionamento para login                                     |
| Sessão persistida de forma inadequada          |                Alta | Sessão nativa usa SecureStore e proteção vinculada ao dispositivo                                      |
| Captura de telas sensíveis                     |               Média | Capturas bloqueadas em login, confirmação e comprovante nas plataformas suportadas                     |
| Parâmetro de data do comprovante sem validação |               Média | Timestamp é normalizado antes de uso                                                                   |
| Conteúdo inserido diretamente no HTML do PDF   |               Média | Valores dinâmicos são escapados antes da geração do documento                                          |
| Segredos futuros incluídos por engano no Git   |                Alta | `.gitignore` ampliado para arquivos de ambiente, certificados, credenciais e configurações de serviços |
| Comunicação futura sem política explícita      |                Alta | Cliente HTTP preparado para aceitar somente HTTPS e aplicar timeout                                    |
| Permissões móveis desnecessárias               |               Média | Câmera e microfone permanecem bloqueados; localização é solicitada somente na tela de ponto             |
| Dados em cache após logout                     |               Média | Logout limpa a sessão, o cache do TanStack Query e mantém dados locais separados da autenticação futura |
| Entrada excessivamente grande no login         |               Baixa | Limites de tamanho adicionados ao e-mail e à senha                                                     |
| Ponto offline apresentado como confirmado      |                Alta | Tentativas offline entram em fila local com aviso explícito de que ainda não foram confirmadas         |
| Abertura e registro sem confirmação local      |               Média | Biometria ou credencial do aparelho podem proteger o app e cada registro de ponto                      |
| Logs contendo informações pessoais             |                Alta | Logger central remove e mascara campos pessoais, tokens, senhas e segredos                             |

## Riscos que ainda permanecem

### 1. API pública ainda não configurada no Expo — Crítico

Sem `EXPO_PUBLIC_API_URL`, o aplicativo bloqueia o login e não cria registros locais que possam ser confundidos com pontos oficiais. Com a URL HTTPS configurada, o login usa `POST /api/auth/login-funcionario`, armazena o JWT no SecureStore e respeita a expiração informada pelo servidor.

Correção necessária para produção:

- publicar e monitorar a API em HTTPS;
- validar senha, bloqueios e tentativas no servidor;
- emitir tokens de curta duração;
- usar renovação de sessão com rotação e revogação;
- aplicar MFA quando compatível com a política da empresa;
- nunca enviar ou armazenar senha em texto simples.

### 2. Implantação da tabela de geolocalização — Crítico

O aplicativo envia o tipo de marcação, as coordenadas, a precisão e a hora de captura. A API valida esses dados, usa seu próprio timestamp para o ponto e grava a localização na mesma transação. O script `database/03_geolocalizacao_ponto.sql` precisa ser executado antes de liberar a nova API.

Correção necessária para produção:

- manter o registro exclusivamente por `POST /api/pontos/registrar` autenticado;
- gerar o timestamp oficial no servidor;
- validar a sequência Entrada/Saída no backend;
- aplicar chave de idempotência por tentativa;
- manter trilha de auditoria imutável;
- rejeitar registros duplicados e requisições reproduzidas.

### 3. Comprovante sem autoridade do servidor — Alta

O PDF é montado no dispositivo e não possui assinatura digital ou identificador verificável. Um aplicativo modificado pode produzir outro conteúdo.

Correção necessária para produção:

- gerar o comprovante no backend;
- associar o documento ao ID imutável do registro;
- incluir hash ou assinatura verificável;
- permitir consulta do comprovante original na API;
- usar URL temporária e autenticada para download.

### 4. Arquivo PDF temporário — Média

O PDF é criado no cache do aplicativo antes do compartilhamento. O sistema operacional normalmente gerencia esse cache, mas o arquivo pode permanecer por algum tempo no dispositivo.

Correção recomendada:

- excluir o arquivo após o compartilhamento;
- aplicar política de expiração no cache;
- não incluir mais dados pessoais que o necessário;
- avaliar criptografia adicional conforme a política corporativa.

### 5. Dependências transitivas — Média

O `npm audit` encontrou 15 alertas moderados, todos relacionados à cadeia de ferramentas e dependências transitivas do Expo. Não há alerta alto ou crítico. As correções automáticas propostas exigem versões antigas ou incompatíveis e não devem ser aplicadas com `npm audit fix --force`.

Tratamento recomendado:

- acompanhar atualizações compatíveis do Expo SDK;
- executar `npm audit` e `npx expo install --check` no CI;
- atualizar somente em conjunto com os testes do aplicativo;
- bloquear publicação quando surgir vulnerabilidade alta ou crítica aplicável ao runtime.

### 6. Anexos e dados trabalhistas locais — Alta

Correções, férias, faltas, atestados e decisões demonstrativas são mantidos no aparelho. O aplicativo guarda somente referências locais dos anexos, mas esses arquivos podem continuar acessíveis no armazenamento administrado pelo sistema operacional.

Correção necessária para produção:

- enviar anexos somente para armazenamento privado autenticado;
- validar tipo, tamanho e conteúdo no servidor;
- aplicar antivírus e remover metadados desnecessários;
- usar URLs temporárias e controle de acesso por funcionário e gestor;
- definir retenção, exclusão e trilha de auditoria compatíveis com a LGPD.

### 7. Biometria é proteção local — Média

A biometria confirma que alguém autorizado desbloqueou o aparelho, mas não prova identidade corporativa e não substitui autenticação, autorização nem antifraude no servidor.

Correção necessária para produção:

- vincular cada ação a uma sessão validada pelo backend;
- exigir reautenticação de acordo com o risco da operação;
- registrar somente o resultado da autenticação local, nunca dados biométricos;
- aplicar autorização por perfil no servidor para aprovações do gestor.

### 8. Fila offline ainda não sincroniza — Alta

A fila atual preserva tentativas como pendentes e evita confirmação falsa. Sem API, ela não pode resolver conflitos, validar horários, garantir idempotência nem concluir a sincronização.

Correção necessária para produção:

- assinar cada operação com identificador único;
- sincronizar somente por HTTPS com sessão válida;
- aplicar idempotência e resolução de conflito no servidor;
- manter tentativas rejeitadas visíveis e auditáveis;
- nunca substituir o horário oficial produzido pelo backend.

## Controles implementados

- armazenamento seguro de sessão no Android e iOS;
- autenticação de funcionário integrada ao JWT do backend fornecido;
- token separado dos dados do perfil e protegido pelo SecureStore;
- histórico e registro de ponto compatíveis com os quatro tipos aceitos pela API;
- localização solicitada somente durante o registro, com precisão máxima de 100 metros;
- rejeição de localização simulada no aplicativo e repetição da validação no servidor;
- geofence configurável e armazenamento transacional do ponto com sua localização;
- timestamp confirmado pelo servidor quando a API está ativa;
- nenhuma persistência de senha;
- proteção central de rotas;
- limpeza de sessão e cache no logout;
- bloqueio de captura nas telas sensíveis;
- validação e limites de entrada;
- normalização de timestamp recebido por rota;
- escape de HTML na geração de comprovante;
- configuração pública separada de segredos;
- transporte futuro restrito a HTTPS;
- timeout em chamadas futuras de API;
- lista mínima de permissões móveis;
- arquivos sensíveis ignorados pelo Git;
- mensagens de erro sem detalhes internos.
- recuperação de senha com resposta genérica contra descoberta de contas;
- biometria ou credencial do aparelho antes do ponto e, opcionalmente, ao abrir o app;
- fila offline com estado pendente e sem confirmação enganosa;
- armazenamento local protegido para solicitações e preferências;
- logs com remoção de campos pessoais e credenciais;
- ambientes separados para desenvolvimento, homologação e produção;
- contrato de API preparado sem incluir segredo no aplicativo.

## Validações executadas

- busca de padrões de segredos nos arquivos do projeto;
- busca de padrões de segredos no histórico Git;
- TypeScript sem erros;
- lint sem erros;
- dependências compatíveis com o Expo;
- bundle Android gerado com sucesso;
- auditoria de dependências concluída.
- 16 testes automatizados do aplicativo concluídos com sucesso;
- 4 testes automatizados do backend concluídos com sucesso;
- 21 verificações do Expo concluídas sem erro.

## Critério para produção

O aplicativo só deve ser considerado apto para produção depois que a API estiver publicada em HTTPS, a migração de geolocalização estiver aplicada, a URL for configurada no Expo e os fluxos forem testados ponta a ponta com o banco de produção ou homologação. Idempotência, comprovantes assinados e revisão independente continuam recomendados.
