import { Fingerprint } from "lucide-react-native";
import { useEffect, useMemo, useState } from "react";
import { StyleSheet, Text, View } from "react-native";
import { useRouter } from "expo-router";
import { AppHeader } from "@/components/common/app-header";
import { PrimaryButton, TextButton } from "@/components/common/buttons";
import { Screen } from "@/components/common/screen";
import {
  EmptyState,
  ErrorState,
  LoadingState,
} from "@/components/common/states";
import { TimeEntryRow } from "@/components/attendance/time-entry-row";
import { useTimeEntries } from "@/features/attendance/hooks/use-time-entries";
import { useAppTheme } from "@/theme/theme-provider";
import { dateKey, formatLongDate, formatTime } from "@/utils/date";
import { serverClock } from "@/utils/server-clock";
import { useSessionStore } from "@/stores/session-store";
import { useNetworkStore } from "@/stores/network-store";

export default function HomeScreen() {
  const { colors } = useAppTheme();
  const router = useRouter();
  const entries = useTimeEntries();
  const user = useSessionStore((state) => state.user);
  const connected = useNetworkStore(
    (state) => state.connected && state.reachable,
  );
  // O intervalo só força uma nova renderização a cada 30 segundos. A hora é
  // calculada na renderização, pela hora oficial da API (Brasília), não pela
  // do aparelho. Quando o histórico chega o relógio acabou de ser sincronizado
  // e a tela também renderiza de novo.
  const [, setTick] = useState(0);
  useEffect(() => {
    const id = setInterval(() => setTick((value) => value + 1), 30_000);
    return () => clearInterval(id);
  }, []);
  const now = serverClock.now();
  const today = useMemo(() => {
    const date = dateKey();
    return (entries.data ?? [])
      .filter(
        (entry) =>
          dateKey(entry.timestamp) === date,
      )
      .slice(0, 4);
  }, [entries.data]);
  return (
    <Screen contentStyle={styles.content}>
      <AppHeader menu avatar={user?.initials ?? "UD"} />
      <View style={styles.hero}>
        <Text style={[styles.date, { color: colors.textSecondary }]}>
          {formatLongDate(now)}
        </Text>
        <Text
          accessibilityLabel={`Horário atual ${formatTime(now)}`}
          style={[styles.clock, { color: colors.text }]}
        >
          {formatTime(now)}
        </Text>
        <View style={styles.online}>
          <View
            style={[
              styles.dot,
              { backgroundColor: connected ? colors.success : colors.danger },
            ]}
          />
          <Text style={[styles.onlineText, { color: colors.textSecondary }]}>
            {connected ? "Online" : "Offline"}
          </Text>
        </View>
      </View>
      <PrimaryButton
        label="Registrar ponto"
        onPress={() => router.push("/clock")}
        icon={<Fingerprint size={22} color={colors.onPrimary} />}
      />
      <View style={styles.section}>
        <View style={styles.sectionHeader}>
          <Text style={[styles.sectionTitle, { color: colors.text }]}>
            Registros de hoje
          </Text>
          <TextButton
            label="Ver todos"
            onPress={() => router.push("/(tabs)/history")}
          />
        </View>
        {entries.isLoading ? (
          <LoadingState />
        ) : entries.isError ? (
          <ErrorState retry={() => void entries.refetch()} />
        ) : today.length === 0 ? (
          <EmptyState />
        ) : (
          today.map((entry) => <TimeEntryRow key={entry.id} entry={entry} />)
        )}
      </View>
    </Screen>
  );
}
const styles = StyleSheet.create({
  content: { paddingBottom: 24 },
  hero: { alignItems: "center", paddingVertical: 40 },
  date: { fontSize: 15, textAlign: "center" },
  clock: {
    fontSize: 44,
    lineHeight: 54,
    fontWeight: "800",
    letterSpacing: -1.5,
    marginTop: 8,
  },
  online: { flexDirection: "row", alignItems: "center", gap: 8, marginTop: 6 },
  dot: { width: 8, height: 8, borderRadius: 4 },
  onlineText: { fontSize: 14 },
  section: { marginTop: 28 },
  sectionHeader: {
    minHeight: 52,
    flexDirection: "row",
    alignItems: "center",
    justifyContent: "space-between",
  },
  sectionTitle: { fontSize: 16, fontWeight: "700" },
});
