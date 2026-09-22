import Constants from "expo-constants";
import { StyleSheet, Text } from "react-native";
import { AppHeader } from "@/components/common/app-header";
import { Screen } from "@/components/common/screen";
import { useAppTheme } from "@/theme/theme-provider";

export default function AboutScreen() {
  const { colors } = useAppTheme();
  return (
    <Screen contentStyle={styles.content}>
      <AppHeader title="Sobre o app" back />
      <Text style={[styles.name, { color: colors.text }]}>Tech4HR</Text>
      <Text style={[styles.version, { color: colors.textSecondary }]}>
        Versão {Constants.expoConfig?.version ?? "1.0.0"}
      </Text>
      <Text style={[styles.body, { color: colors.textSecondary }]}>
        Aplicativo móvel de acompanhamento e registro de jornada integrado à
        API corporativa do Tech4HR. Login, registros de ponto, geolocalização,
        histórico e comprovantes dependem de confirmação segura do servidor.
      </Text>
    </Screen>
  );
}

const styles = StyleSheet.create({
  content: { paddingBottom: 32 },
  name: { fontSize: 28, fontWeight: "900", marginTop: 36 },
  version: { fontSize: 14, marginTop: 8 },
  body: { fontSize: 15, lineHeight: 23, marginTop: 24 },
});
