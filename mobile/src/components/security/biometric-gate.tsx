import * as LocalAuthentication from "expo-local-authentication";
import { PropsWithChildren, useCallback, useEffect, useState } from "react";
import { AppState, StyleSheet, Text, View } from "react-native";
import { PrimaryButton } from "@/components/common/buttons";
import { useSecurityStore } from "@/stores/security-store";
import { useSessionStore } from "@/stores/session-store";
import { useAppTheme } from "@/theme/theme-provider";

export function BiometricGate({ children }: PropsWithChildren) {
  const { colors } = useAppTheme();
  const user = useSessionStore((state) => state.user);
  const { biometricEnabled, hydrated, hydrate } = useSecurityStore();
  const [unlocked, setUnlocked] = useState(false);
  const authenticate = useCallback(async () => {
    const result = await LocalAuthentication.authenticateAsync({
      promptMessage: "Desbloquear Tech4HR",
      cancelLabel: "Cancelar",
      disableDeviceFallback: false,
    });
    return result.success;
  }, []);
  useEffect(() => {
    void hydrate();
  }, [hydrate]);
  useEffect(() => {
    if (hydrated && user && biometricEnabled)
      void authenticate().then(setUnlocked);
  }, [authenticate, biometricEnabled, hydrated, user]);
  useEffect(() => {
    const subscription = AppState.addEventListener("change", (state) => {
      if (state === "background" && biometricEnabled) setUnlocked(false);
      if (state === "active" && biometricEnabled)
        void authenticate().then(setUnlocked);
    });
    return () => subscription.remove();
  }, [authenticate, biometricEnabled]);
  if (!hydrated || unlocked || !user || !biometricEnabled) return children;
  return (
    <View style={[styles.container, { backgroundColor: colors.background }]}>
      <Text style={[styles.title, { color: colors.text }]}>
        Aplicativo bloqueado
      </Text>
      <Text style={[styles.text, { color: colors.textSecondary }]}>
        Confirme sua identidade no aparelho para continuar.
      </Text>
      <PrimaryButton
        label="Desbloquear"
        onPress={() => void authenticate().then(setUnlocked)}
      />
    </View>
  );
}

const styles = StyleSheet.create({
  container: { flex: 1, justifyContent: "center", padding: 24, gap: 18 },
  title: { fontSize: 24, fontWeight: "800", textAlign: "center" },
  text: { fontSize: 15, lineHeight: 22, textAlign: "center" },
});
