import { StyleSheet, Text } from "react-native";
import { AppHeader } from "@/components/common/app-header";
import { Screen } from "@/components/common/screen";
import { useAppTheme } from "@/theme/theme-provider";

const sections = [
  [
    "Privacidade",
    "O Tech4HR utiliza dados de identificação profissional, registros de jornada e geolocalização no momento da marcação. Os dados confirmados são enviados por conexão HTTPS à API corporativa.",
  ],
  [
    "Dados sensíveis",
    "Biometria é processada pelo sistema operacional. O aplicativo recebe somente o resultado da autenticação e não acessa impressões digitais ou imagens faciais.",
  ],
  [
    "Anexos",
    "Arquivos selecionados para justificativas permanecem protegidos no armazenamento do aplicativo enquanto a operação estiver pendente de sincronização.",
  ],
  [
    "Termos de uso",
    "Somente registros confirmados pela API corporativa aparecem como pontos e comprovantes oficiais. Tentativas offline permanecem pendentes e não alteram a jornada.",
  ],
  [
    "Direitos do titular",
    "A versão de produção deverá informar controlador, encarregado, canais de atendimento, retenção, compartilhamento e procedimentos para exercício dos direitos previstos na política corporativa.",
  ],
] as const;

export default function PrivacyScreen() {
  const { colors } = useAppTheme();
  return (
    <Screen contentStyle={styles.content}>
      <AppHeader title="Privacidade e termos" back />
      {sections.map(([title, body]) => (
        <Text key={title} style={[styles.section, { color: colors.text }]}>
          <Text style={styles.title}>
            {title}
            {"\n"}
          </Text>
          <Text style={[styles.body, { color: colors.textSecondary }]}>
            {body}
          </Text>
        </Text>
      ))}
      <Text style={[styles.version, { color: colors.textSecondary }]}>
        A política corporativa completa deve informar controlador, encarregado,
        retenção e canais para exercício dos direitos do titular.
      </Text>
    </Screen>
  );
}

const styles = StyleSheet.create({
  content: { paddingBottom: 32 },
  section: { marginTop: 24 },
  title: { fontSize: 17, fontWeight: "800", lineHeight: 28 },
  body: { fontSize: 14, lineHeight: 22 },
  version: { fontSize: 12, lineHeight: 18, marginTop: 28 },
});
