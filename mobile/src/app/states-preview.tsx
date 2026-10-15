import { StyleSheet, Text } from "react-native";
import { AppHeader } from "@/components/common/app-header";
import { Screen } from "@/components/common/screen";
import { EmptyState, LoadingState } from "@/components/common/states";
import { SystemState, SystemStateKind } from "@/components/common/system-state";
import { useAppTheme } from "@/theme/theme-provider";

const states: SystemStateKind[] = [
  "OFFLINE",
  "SESSION_EXPIRED",
  "SERVER_ERROR",
  "MAINTENANCE",
  "UPDATE_REQUIRED",
];

export default function StatesPreviewScreen() {
  const { colors } = useAppTheme();
  return (
    <Screen contentStyle={styles.content}>
      <AppHeader title="Estados da interface" back />
      <Text style={[styles.heading, { color: colors.text }]}>Carregamento</Text>
      <LoadingState />
      <Text style={[styles.heading, { color: colors.text }]}>Dados vazios</Text>
      <EmptyState message="Nenhum dado disponível." />
      {states.map((state) => (
        <SystemState key={state} kind={state} />
      ))}
    </Screen>
  );
}

const styles = StyleSheet.create({
  content: { paddingBottom: 32 },
  heading: { fontSize: 17, fontWeight: "800", marginTop: 22 },
});
