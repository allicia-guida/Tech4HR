import { ChevronRight, FileCheck2 } from "lucide-react-native";
import { Pressable, StyleSheet, Text, View } from "react-native";
import { useRouter } from "expo-router";
import { AppHeader } from "@/components/common/app-header";
import { Screen } from "@/components/common/screen";
import { ErrorState, LoadingState } from "@/components/common/states";
import { useTimeEntries } from "@/features/attendance/hooks/use-time-entries";
import { timeEntryLabel } from "@/features/attendance/types/time-entry";
import { useAppTheme } from "@/theme/theme-provider";
import { formatReceiptDate, formatTime } from "@/utils/date";

export default function ReceiptsScreen() {
  const { colors } = useAppTheme();
  const router = useRouter();
  const entries = useTimeEntries();
  return (
    <Screen contentStyle={styles.content}>
      <AppHeader title="Comprovantes" />
      <Text style={[styles.section, { color: colors.textSecondary }]}>
        REGISTROS CONFIRMADOS PELA API
      </Text>
      {entries.isLoading ? <LoadingState /> : null}
      {entries.isError ? (
        <ErrorState retry={() => void entries.refetch()} />
      ) : null}
      {(entries.data ?? []).map((entry) => (
        <Pressable
          key={entry.id}
          onPress={() =>
            router.push({
              pathname: "/receipt",
              params: {
                id: entry.id,
                timestamp: entry.timestamp,
                type: entry.type,
              },
            })
          }
          style={[styles.row, { borderBottomColor: colors.border }]}
        >
          <View
            style={[styles.icon, { backgroundColor: colors.surfaceSecondary }]}
          >
            <FileCheck2 size={22} color={colors.primary} />
          </View>
          <View style={styles.info}>
            <Text style={[styles.title, { color: colors.text }]}>
              {timeEntryLabel[entry.type]} • {formatTime(entry.timestamp)}
            </Text>
            <Text style={[styles.subtitle, { color: colors.textSecondary }]}>
              {formatReceiptDate(entry.timestamp)}
            </Text>
          </View>
          <ChevronRight color={colors.textSecondary} size={20} />
        </Pressable>
      ))}
      {!entries.isLoading && !entries.isError && !entries.data?.length ? (
        <Text style={[styles.empty, { color: colors.textSecondary }]}>
          Nenhum comprovante disponível.
        </Text>
      ) : null}
    </Screen>
  );
}
const styles = StyleSheet.create({
  content: { paddingBottom: 24 },
  section: {
    fontSize: 12,
    fontWeight: "700",
    letterSpacing: 0.6,
    marginTop: 28,
    marginBottom: 8,
  },
  row: {
    minHeight: 76,
    flexDirection: "row",
    alignItems: "center",
    borderBottomWidth: StyleSheet.hairlineWidth,
    gap: 12,
  },
  icon: {
    width: 44,
    height: 44,
    borderRadius: 22,
    alignItems: "center",
    justifyContent: "center",
  },
  info: { flex: 1, gap: 5 },
  title: { fontSize: 15, fontWeight: "600" },
  subtitle: { fontSize: 13 },
  empty: { textAlign: "center", paddingVertical: 32, fontSize: 14 },
});
