import { useQuery, useQueryClient } from "@tanstack/react-query";
import { StyleSheet, Text, View } from "react-native";
import { AppHeader } from "@/components/common/app-header";
import { SecondaryButton } from "@/components/common/buttons";
import { Screen } from "@/components/common/screen";
import { offlineQueue } from "@/features/offline/services/offline-queue";
import { useAppTheme } from "@/theme/theme-provider";

export default function PendingOperationsScreen() {
  const { colors } = useAppTheme();
  const client = useQueryClient();
  const operations = useQuery({
    queryKey: ["pending-operations"],
    queryFn: offlineQueue.list,
  });
  const clear = async () => {
    await offlineQueue.clear();
    await client.invalidateQueries({ queryKey: ["pending-operations"] });
  };
  return (
    <Screen contentStyle={styles.content}>
      <AppHeader title="Operações pendentes" back />
      <Text
        style={[
          styles.notice,
          {
            color: colors.textSecondary,
            backgroundColor: colors.surfaceSecondary,
          },
        ]}
      >
        Itens desta lista não foram confirmados pelo servidor. Eles deverão ser
        revisados quando a API estiver disponível.
      </Text>
      {!operations.data?.length ? (
        <Text style={[styles.empty, { color: colors.textSecondary }]}>
          Nenhuma operação pendente.
        </Text>
      ) : (
        operations.data.map((operation) => (
          <View
            key={operation.id}
            style={[
              styles.card,
              { backgroundColor: colors.surface, borderColor: colors.border },
            ]}
          >
            <Text style={[styles.title, { color: colors.text }]}>
              {operation.kind}
            </Text>
            <Text style={[styles.meta, { color: colors.textSecondary }]}>
              {operation.status} ·{" "}
              {new Date(operation.createdAt).toLocaleString("pt-BR")}
            </Text>
          </View>
        ))
      )}
      {operations.data?.length ? (
        <SecondaryButton
          label="Descartar fila local"
          onPress={() => void clear()}
        />
      ) : null}
    </Screen>
  );
}

const styles = StyleSheet.create({
  content: { paddingBottom: 32, gap: 12 },
  notice: {
    marginTop: 20,
    borderRadius: 8,
    padding: 14,
    fontSize: 13,
    lineHeight: 20,
  },
  empty: { fontSize: 14, textAlign: "center", paddingVertical: 40 },
  card: { borderWidth: 1, borderRadius: 8, padding: 14 },
  title: { fontSize: 14, fontWeight: "800" },
  meta: { fontSize: 12, marginTop: 5 },
});
