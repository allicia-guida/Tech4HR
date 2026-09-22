import { AlertCircle, Inbox } from "lucide-react-native";
import { ActivityIndicator, StyleSheet, Text, View } from "react-native";
import { useAppTheme } from "@/theme/theme-provider";
import { SecondaryButton } from "./buttons";

export function LoadingState() {
  const { colors } = useAppTheme();
  return (
    <View style={styles.state}>
      <ActivityIndicator color={colors.primary} />
      <Text style={{ color: colors.textSecondary }}>Carregando...</Text>
    </View>
  );
}
export function EmptyState({
  message = "Nenhum registro hoje.",
}: {
  message?: string;
}) {
  const { colors } = useAppTheme();
  return (
    <View style={styles.state}>
      <Inbox color={colors.textSecondary} size={28} />
      <Text style={{ color: colors.textSecondary }}>{message}</Text>
    </View>
  );
}
export function ErrorState({ retry }: { retry: () => void }) {
  const { colors } = useAppTheme();
  return (
    <View style={styles.state}>
      <AlertCircle color={colors.danger} size={28} />
      <Text style={{ color: colors.text }}>
        Não foi possível carregar os registros.
      </Text>
      <SecondaryButton label="Tentar novamente" onPress={retry} />
    </View>
  );
}
const styles = StyleSheet.create({
  state: {
    minHeight: 140,
    alignItems: "center",
    justifyContent: "center",
    gap: 12,
    padding: 16,
  },
});
