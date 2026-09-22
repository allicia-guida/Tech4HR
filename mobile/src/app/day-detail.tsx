import { useLocalSearchParams, useRouter } from "expo-router";
import { useMemo } from "react";
import { StyleSheet, Text } from "react-native";
import { AppHeader } from "@/components/common/app-header";
import { PrimaryButton } from "@/components/common/buttons";
import { Screen } from "@/components/common/screen";
import { TimeEntryRow } from "@/components/attendance/time-entry-row";
import { WorkSummaryCard } from "@/components/attendance/work-summary-card";
import { useTimeEntries } from "@/features/attendance/hooks/use-time-entries";
import { demoAbsences } from "@/features/workforce/fixtures/absences";
import { defaultWorkPolicy } from "@/features/workforce/fixtures/default-work-policy";
import { calculateDailyWork } from "@/features/workforce/services/work-calculator";
import { useAppTheme } from "@/theme/theme-provider";

export default function DayDetailScreen() {
  const { colors } = useAppTheme();
  const router = useRouter();
  const params = useLocalSearchParams<{ date?: string }>();
  const date = /^\d{4}-\d{2}-\d{2}$/.test(params.date ?? "")
    ? params.date!
    : "2026-09-22";
  const entries = useTimeEntries();
  const dayEntries = (entries.data ?? []).filter((entry) =>
    entry.timestamp.startsWith(date),
  );
  const summary = useMemo(
    () =>
      calculateDailyWork(
        date,
        entries.data ?? [],
        defaultWorkPolicy,
        demoAbsences,
      ),
    [date, entries.data],
  );
  return (
    <Screen contentStyle={styles.content}>
      <AppHeader title="Detalhe do dia" back />
      <Text style={[styles.date, { color: colors.text }]}>{date}</Text>
      <WorkSummaryCard summary={summary} />
      <Text style={[styles.heading, { color: colors.text }]}>Registros</Text>
      {dayEntries.map((entry) => (
        <TimeEntryRow
          key={entry.id}
          entry={entry}
          showSource
          onPress={() =>
            router.push({
              pathname: "/point-detail",
              params: { id: entry.id },
            })
          }
        />
      ))}
      {!dayEntries.length ? (
        <Text style={[styles.empty, { color: colors.textSecondary }]}>
          Nenhum registro neste dia.
        </Text>
      ) : null}
      <PrimaryButton
        label="Solicitar correção"
        onPress={() =>
          router.push({ pathname: "/corrections", params: { date } })
        }
      />
    </Screen>
  );
}

const styles = StyleSheet.create({
  content: { paddingBottom: 32, gap: 14 },
  date: { fontSize: 18, fontWeight: "800", marginTop: 20 },
  heading: { fontSize: 16, fontWeight: "800", marginTop: 8 },
  empty: { fontSize: 13, paddingVertical: 12 },
});
