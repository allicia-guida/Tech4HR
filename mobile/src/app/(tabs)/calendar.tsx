import { ChevronLeft, ChevronRight } from "lucide-react-native";
import { useMemo, useState } from "react";
import { Pressable, StyleSheet, Text, View } from "react-native";
import { useRouter } from "expo-router";
import { AppHeader } from "@/components/common/app-header";
import { Screen } from "@/components/common/screen";
import { useTimeEntries } from "@/features/attendance/hooks/use-time-entries";
import { demoAbsences } from "@/features/workforce/fixtures/absences";
import { defaultWorkPolicy } from "@/features/workforce/fixtures/default-work-policy";
import {
  dateRange,
  shiftPeriod,
  summarizePeriod,
} from "@/features/workforce/services/period-summary";
import { DailyWorkStatus } from "@/features/workforce/types/work-policy";
import { useAppTheme } from "@/theme/theme-provider";
import { dateKey } from "@/utils/date";

const week = ["D", "S", "T", "Q", "Q", "S", "S"];
const statusLabel: Record<DailyWorkStatus, string> = {
  COMPLETE: "Trabalhado",
  INCOMPLETE: "Incompleto",
  ABSENT: "Falta",
  HOLIDAY: "Feriado",
  LEAVE: "Férias/Afastamento",
  DAY_OFF: "Folga",
};

export default function CalendarScreen() {
  const { colors } = useAppTheme();
  const router = useRouter();
  const entries = useTimeEntries();
  const [anchor, setAnchor] = useState(dateKey());
  const days = useMemo(
    () =>
      summarizePeriod(
        anchor,
        "MONTH",
        entries.data ?? [],
        defaultWorkPolicy,
        demoAbsences,
      ).days,
    [anchor, entries.data],
  );
  const firstWeekday = new Date(
    `${dateRange(anchor, "MONTH")[0]}T12:00:00Z`,
  ).getUTCDay();
  const month = new Intl.DateTimeFormat("pt-BR", {
    month: "long",
    year: "numeric",
    timeZone: "UTC",
  }).format(new Date(`${anchor}T12:00:00Z`));
  const colorFor = (status: DailyWorkStatus) => {
    if (status === "COMPLETE") return colors.success;
    if (status === "INCOMPLETE" || status === "ABSENT") return colors.danger;
    if (status === "HOLIDAY" || status === "LEAVE") return colors.primary;
    return colors.border;
  };
  return (
    <Screen contentStyle={styles.content}>
      <AppHeader title="Calendário" />
      <View style={styles.monthPicker}>
        <Pressable
          accessibilityLabel="Mês anterior"
          style={styles.icon}
          onPress={() => setAnchor(shiftPeriod(anchor, "MONTH", -1))}
        >
          <ChevronLeft size={22} color={colors.text} />
        </Pressable>
        <Text style={[styles.month, { color: colors.text }]}>{month}</Text>
        <Pressable
          accessibilityLabel="Próximo mês"
          style={styles.icon}
          onPress={() => setAnchor(shiftPeriod(anchor, "MONTH", 1))}
        >
          <ChevronRight size={22} color={colors.text} />
        </Pressable>
      </View>
      <View style={styles.weekRow}>
        {week.map((name, index) => (
          <Text
            key={`${name}-${index}`}
            style={[styles.weekday, { color: colors.textSecondary }]}
          >
            {name}
          </Text>
        ))}
      </View>
      <View style={styles.grid}>
        {Array.from({ length: firstWeekday }).map((_, index) => (
          <View key={`empty-${index}`} style={styles.day} />
        ))}
        {days.map((day) => (
          <Pressable
            key={day.date}
            accessibilityLabel={`${day.date}, ${statusLabel[day.status]}`}
            onPress={() =>
              router.push({
                pathname: "/day-detail",
                params: { date: day.date },
              })
            }
            style={[
              styles.day,
              { borderColor: colors.border, backgroundColor: colors.surface },
            ]}
          >
            <Text style={[styles.dayNumber, { color: colors.text }]}>
              {Number(day.date.slice(-2))}
            </Text>
            <View
              style={[styles.dot, { backgroundColor: colorFor(day.status) }]}
            />
          </Pressable>
        ))}
      </View>
      <View style={styles.legend}>
        {(Object.keys(statusLabel) as DailyWorkStatus[]).map((status) => (
          <View key={status} style={styles.legendItem}>
            <View
              style={[styles.legendDot, { backgroundColor: colorFor(status) }]}
            />
            <Text style={[styles.legendText, { color: colors.textSecondary }]}>
              {statusLabel[status]}
            </Text>
          </View>
        ))}
      </View>
    </Screen>
  );
}

const styles = StyleSheet.create({
  content: { paddingBottom: 28 },
  monthPicker: {
    height: 68,
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
  month: { fontSize: 17, fontWeight: "800", textTransform: "capitalize" },
  weekRow: { flexDirection: "row", marginBottom: 6 },
  weekday: {
    width: "14.2857%",
    textAlign: "center",
    fontSize: 12,
    fontWeight: "700",
  },
  grid: { flexDirection: "row", flexWrap: "wrap" },
  day: {
    width: "14.2857%",
    aspectRatio: 0.86,
    borderWidth: StyleSheet.hairlineWidth,
    alignItems: "center",
    justifyContent: "center",
    gap: 5,
  },
  dayNumber: { fontSize: 14, fontWeight: "700" },
  dot: { width: 7, height: 7, borderRadius: 4 },
  legend: { flexDirection: "row", flexWrap: "wrap", gap: 12, marginTop: 22 },
  legendItem: { flexDirection: "row", alignItems: "center", gap: 6 },
  legendDot: { width: 8, height: 8, borderRadius: 4 },
  legendText: { fontSize: 11 },
});
