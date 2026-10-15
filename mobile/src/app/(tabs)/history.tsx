import { ChevronLeft, ChevronRight } from "lucide-react-native";
import { useMemo, useState } from "react";
import { Pressable, StyleSheet, Text, View } from "react-native";
import { AppHeader } from "@/components/common/app-header";
import { Screen } from "@/components/common/screen";
import { ErrorState, LoadingState } from "@/components/common/states";
import { TimeEntryRow } from "@/components/attendance/time-entry-row";
import { WorkSummaryCard } from "@/components/attendance/work-summary-card";
import { useTimeEntries } from "@/features/attendance/hooks/use-time-entries";
import { TimeEntryType } from "@/features/attendance/types/time-entry";
import { demoAbsences } from "@/features/workforce/fixtures/absences";
import { defaultWorkPolicy } from "@/features/workforce/fixtures/default-work-policy";
import {
  accumulatedTimeBank,
  HistoryPeriod,
  shiftPeriod,
  summarizePeriod,
} from "@/features/workforce/services/period-summary";
import { useAppTheme } from "@/theme/theme-provider";
import { useRouter } from "expo-router";
import { formatMinutes } from "@/utils/duration";
import { dateKey } from "@/utils/date";

type EntryFilter = "ALL" | TimeEntryType;

const periodLabels: { value: HistoryPeriod; label: string }[] = [
  { value: "DAY", label: "Dia" },
  { value: "WEEK", label: "Semana" },
  { value: "MONTH", label: "Mês" },
];

const filterLabels: { value: EntryFilter; label: string }[] = [
  { value: "ALL", label: "Todos" },
  { value: TimeEntryType.CLOCK_IN, label: "Entradas" },
  { value: TimeEntryType.BREAK_START, label: "Intervalos" },
  { value: TimeEntryType.BREAK_END, label: "Retornos" },
  { value: TimeEntryType.CLOCK_OUT, label: "Saídas" },
];

const dateLabel = (date: string, period: HistoryPeriod) => {
  const value = new Date(`${date}T12:00:00Z`);
  if (period === "MONTH")
    return new Intl.DateTimeFormat("pt-BR", {
      month: "long",
      year: "numeric",
      timeZone: "UTC",
    }).format(value);
  return new Intl.DateTimeFormat("pt-BR", {
    day: "2-digit",
    month: "long",
    year: "numeric",
    timeZone: "UTC",
  }).format(value);
};

export default function HistoryScreen() {
  const { colors } = useAppTheme();
  const router = useRouter();
  const entries = useTimeEntries();
  const [period, setPeriod] = useState<HistoryPeriod>("DAY");
  const [anchor, setAnchor] = useState(dateKey());
  const [filter, setFilter] = useState<EntryFilter>("ALL");
  const summary = useMemo(
    () =>
      summarizePeriod(
        anchor,
        period,
        entries.data ?? [],
        defaultWorkPolicy,
        demoAbsences,
      ),
    [anchor, entries.data, period],
  );
  const bank = useMemo(
    () =>
      accumulatedTimeBank(
        anchor,
        entries.data ?? [],
        defaultWorkPolicy,
        demoAbsences,
      ),
    [anchor, entries.data],
  );
  const visibleEntries = (entries.data ?? []).filter(
    (entry) =>
      summary.days.some((day) => entry.timestamp.startsWith(day.date)) &&
      (filter === "ALL" || entry.type === filter),
  );
  return (
    <Screen contentStyle={styles.content}>
      <AppHeader title="Meu Histórico" />
      <View style={[styles.tabs, { borderBottomColor: colors.border }]}>
        {periodLabels.map((item) => (
          <Pressable
            key={item.value}
            accessibilityRole="tab"
            accessibilityState={{ selected: period === item.value }}
            onPress={() => setPeriod(item.value)}
            style={[
              styles.tab,
              period === item.value && {
                borderBottomColor: colors.primary,
                borderBottomWidth: 2,
              },
            ]}
          >
            <Text
              style={[
                styles.tabText,
                {
                  color:
                    period === item.value
                      ? colors.primary
                      : colors.textSecondary,
                },
              ]}
            >
              {item.label}
            </Text>
          </Pressable>
        ))}
      </View>
      <View style={styles.datePicker}>
        <Pressable
          accessibilityLabel="Período anterior"
          onPress={() => setAnchor(shiftPeriod(anchor, period, -1))}
          style={styles.icon}
        >
          <ChevronLeft size={22} color={colors.text} />
        </Pressable>
        <Text style={[styles.date, { color: colors.text }]}>
          {dateLabel(anchor, period)}
        </Text>
        <Pressable
          accessibilityLabel="Próximo período"
          onPress={() => setAnchor(shiftPeriod(anchor, period, 1))}
          style={styles.icon}
        >
          <ChevronRight size={22} color={colors.text} />
        </Pressable>
      </View>
      <View style={styles.filters}>
        {filterLabels.map((item) => (
          <Pressable
            key={item.value}
            onPress={() => setFilter(item.value)}
            style={[
              styles.filter,
              {
                backgroundColor:
                  filter === item.value ? colors.primary : colors.surface,
                borderColor:
                  filter === item.value ? colors.primary : colors.border,
              },
            ]}
          >
            <Text
              style={{
                color: filter === item.value ? colors.onPrimary : colors.text,
                fontSize: 12,
                fontWeight: "600",
              }}
            >
              {item.label}
            </Text>
          </Pressable>
        ))}
      </View>
      {entries.isLoading ? (
        <LoadingState />
      ) : entries.isError ? (
        <ErrorState retry={() => void entries.refetch()} />
      ) : (
        <>
          {period === "DAY" ? (
            <WorkSummaryCard summary={summary.days[0]} />
          ) : (
            <View
              style={[
                styles.periodSummary,
                { backgroundColor: colors.surface, borderColor: colors.border },
              ]}
            >
              <Text style={[styles.summaryTitle, { color: colors.text }]}>
                Resumo do período
              </Text>
              <View style={styles.metrics}>
                <Metric
                  label="Trabalhado"
                  value={formatMinutes(summary.workedMinutes)}
                />
                <Metric
                  label="Hora extra"
                  value={formatMinutes(summary.overtimeMinutes)}
                />
                <Metric
                  label="Atrasos"
                  value={formatMinutes(summary.lateMinutes)}
                />
                <Metric
                  label="Saldo"
                  value={formatMinutes(summary.timeBankMinutes)}
                />
              </View>
            </View>
          )}
          <View
            style={[styles.bank, { backgroundColor: colors.surfaceSecondary }]}
          >
            <Text style={[styles.bankLabel, { color: colors.textSecondary }]}>
              Banco acumulado no ano
            </Text>
            <Text
              style={[
                styles.bankValue,
                { color: bank >= 0 ? colors.success : colors.danger },
              ]}
            >
              {formatMinutes(bank)}
            </Text>
          </View>
          {period !== "DAY" ? (
            <View style={styles.days}>
              {summary.days
                .filter((day) => day.workedMinutes || day.status !== "DAY_OFF")
                .map((day) => (
                  <Pressable
                    key={day.date}
                    onPress={() =>
                      router.push({
                        pathname: "/day-detail",
                        params: { date: day.date },
                      })
                    }
                    style={[
                      styles.dayRow,
                      { borderBottomColor: colors.border },
                    ]}
                  >
                    <View>
                      <Text style={[styles.dayDate, { color: colors.text }]}>
                        {day.date}
                      </Text>
                      <Text
                        style={[
                          styles.dayStatus,
                          { color: colors.textSecondary },
                        ]}
                      >
                        {day.status}
                      </Text>
                    </View>
                    <Text style={[styles.dayHours, { color: colors.text }]}>
                      {formatMinutes(day.workedMinutes)}
                    </Text>
                  </Pressable>
                ))}
            </View>
          ) : null}
          <Text style={[styles.listTitle, { color: colors.text }]}>
            Registros
          </Text>
          {visibleEntries.map((entry) => (
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
          {!visibleEntries.length ? (
            <Text style={[styles.empty, { color: colors.textSecondary }]}>
              Nenhum registro para o filtro selecionado.
            </Text>
          ) : null}
        </>
      )}
    </Screen>
  );
}

function Metric({ label, value }: { label: string; value: string }) {
  const { colors } = useAppTheme();
  return (
    <View style={styles.metric}>
      <Text style={[styles.metricLabel, { color: colors.textSecondary }]}>
        {label}
      </Text>
      <Text style={[styles.metricValue, { color: colors.text }]}>{value}</Text>
    </View>
  );
}

const styles = StyleSheet.create({
  content: { paddingBottom: 28 },
  tabs: { flexDirection: "row", borderBottomWidth: StyleSheet.hairlineWidth },
  tab: {
    flex: 1,
    minHeight: 52,
    alignItems: "center",
    justifyContent: "center",
  },
  tabText: { fontSize: 14, fontWeight: "600" },
  datePicker: {
    minHeight: 68,
    flexDirection: "row",
    alignItems: "center",
    justifyContent: "space-between",
  },
  icon: {
    width: 48,
    height: 48,
    alignItems: "center",
    justifyContent: "center",
  },
  date: { fontSize: 15, fontWeight: "700", textTransform: "capitalize" },
  filters: { flexDirection: "row", flexWrap: "wrap", gap: 8, marginBottom: 14 },
  filter: {
    minHeight: 36,
    borderWidth: 1,
    borderRadius: 18,
    alignItems: "center",
    justifyContent: "center",
    paddingHorizontal: 14,
  },
  periodSummary: { borderWidth: 1, borderRadius: 10, padding: 16 },
  summaryTitle: { fontSize: 16, fontWeight: "700" },
  metrics: { flexDirection: "row", flexWrap: "wrap", marginTop: 8 },
  metric: { width: "50%", paddingVertical: 8 },
  metricLabel: { fontSize: 12 },
  metricValue: { fontSize: 15, fontWeight: "700", marginTop: 3 },
  bank: {
    borderRadius: 8,
    padding: 14,
    flexDirection: "row",
    justifyContent: "space-between",
    alignItems: "center",
    marginTop: 12,
  },
  bankLabel: { fontSize: 13, fontWeight: "600" },
  bankValue: { fontSize: 15, fontWeight: "800" },
  days: { marginTop: 16 },
  dayRow: {
    minHeight: 54,
    borderBottomWidth: StyleSheet.hairlineWidth,
    flexDirection: "row",
    alignItems: "center",
    justifyContent: "space-between",
  },
  dayDate: { fontSize: 14, fontWeight: "700" },
  dayStatus: { fontSize: 11, marginTop: 2 },
  dayHours: { fontSize: 14, fontWeight: "700" },
  listTitle: {
    fontSize: 16,
    fontWeight: "800",
    marginTop: 22,
    marginBottom: 6,
  },
  empty: { fontSize: 13, paddingVertical: 18, textAlign: "center" },
});
