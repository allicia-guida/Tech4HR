import * as LocalAuthentication from "expo-local-authentication";
import { useState } from "react";
import { StyleSheet, Switch, Text, View } from "react-native";
import { AppHeader } from "@/components/common/app-header";
import { Screen } from "@/components/common/screen";
import { useSecurityStore } from "@/stores/security-store";
import { useAppTheme } from "@/theme/theme-provider";

export default function SecurityScreen() {
  const { colors } = useAppTheme();
  const { biometricEnabled, setBiometricEnabled } = useSecurityStore();
  const [message, setMessage] = useState("");
  const toggle = async (enabled: boolean) => {
    if (!enabled) {
      await setBiometricEnabled(false);
      setMessage("Proteção biométrica desativada.");
      return;
    }
    const [hardware, enrolled] = await Promise.all([
      LocalAuthentication.hasHardwareAsync(),
      LocalAuthentication.isEnrolledAsync(),
    ]);
    if (!hardware || !enrolled) {
      setMessage(
        "Configure biometria ou reconhecimento facial no aparelho primeiro.",
      );
      return;
    }
    const result = await LocalAuthentication.authenticateAsync({
      promptMessage: "Ativar proteção biométrica",
    });
    if (result.success) {
      await setBiometricEnabled(true);
      setMessage("Proteção biométrica ativada.");
    }
  };
  return (
    <Screen contentStyle={styles.content}>
      <AppHeader title="Segurança" back />
      <View style={[styles.row, { borderBottomColor: colors.border }]}>
        <View style={styles.copy}>
          <Text style={[styles.title, { color: colors.text }]}>
            Desbloqueio biométrico
          </Text>
          <Text style={[styles.description, { color: colors.textSecondary }]}>
            Solicita a autenticação do aparelho ao abrir o aplicativo.
          </Text>
        </View>
        <Switch
          value={biometricEnabled}
          onValueChange={(value) => void toggle(value)}
        />
      </View>
      {message ? (
        <Text style={[styles.message, { color: colors.textSecondary }]}>
          {message}
        </Text>
      ) : null}
      <Text
        style={[
          styles.notice,
          {
            color: colors.textSecondary,
            backgroundColor: colors.surfaceSecondary,
          },
        ]}
      >
        A biometria protege o acesso local, mas não substitui a autenticação e
        autorização do backend.
      </Text>
    </Screen>
  );
}

const styles = StyleSheet.create({
  content: { paddingBottom: 32 },
  row: {
    minHeight: 88,
    flexDirection: "row",
    alignItems: "center",
    borderBottomWidth: StyleSheet.hairlineWidth,
    marginTop: 16,
  },
  copy: { flex: 1, paddingRight: 16 },
  title: { fontSize: 16, fontWeight: "700" },
  description: { fontSize: 13, lineHeight: 19, marginTop: 5 },
  message: { fontSize: 13, marginTop: 16 },
  notice: {
    marginTop: 24,
    padding: 14,
    borderRadius: 8,
    fontSize: 13,
    lineHeight: 20,
  },
});
