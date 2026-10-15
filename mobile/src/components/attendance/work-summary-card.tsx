import { StyleSheet, Text, View } from "react-native";
import { DailyWorkSummary } from "@/features/workforce/types/work-policy";
import { useAppTheme } from "@/theme/theme-provider";
import { formatMinutes } from "@/utils/duration";

const statusLabels: Record<DailyWorkSummary["status"], string> = {
  COMPLETE: "Jornada concluída",
  INCOMPLETE: "Jornada em andamento",
  ABSENT: "Sem registros",
  HOLIDAY: "Feriado",
  LEAVE: "Afastamento ou férias",
  DAY_OFF: "Dia sem jornada",
};

export function WorkSummaryCard({ summary }: { summary: DailyWorkSummary }) {
  const { colors } = useAppTheme();
  const values = [
    ["Previsto", formatMinutes(summary.scheduledMinutes)],
    ["Trabalhado", formatMinutes(summary.workedMinutes)],
    ["Intervalo", formatMinutes(summary.breakMinutes)],
    ["Atraso", formatMinutes(summary.lateMinutes)],
    ["Hora extra", formatMinutes(summary.overtimeMinutes)],
    ["Banco do dia", formatMinutes(summary.timeBankMinutes)],
  ];
  return (
    <View
      style={[
        styles.card,
        { backgroundColor: colors.surface, borderColor: colors.border },
      ]}
    >
      <Text style={[styles.title, { color: colors.text }]}>
        Resumo da jornada
      </Text>
      <Text style={[styles.status, { color: colors.primary }]}>
        {statusLabels[summary.status]}
      </Text>
      <View style={styles.grid}>
        {values.map(([label, value]) => (
          <View key={label} style={styles.metric}>
            <Text style={[styles.label, { color: colors.textSecondary }]}>
              {label}
            </Text>
            <Text style={[styles.value, { color: colors.text }]}>{value}</Text>
          </View>
        ))}
      </View>
    </View>
  );
}

const styles = StyleSheet.create({
  card: { borderWidth: 1, borderRadius: 10, padding: 16, gap: 4 },
  title: { fontSize: 16, fontWeight: "700" },
  status: { fontSize: 13, fontWeight: "600" },
  grid: { flexDirection: "row", flexWrap: "wrap", marginTop: 10 },
  metric: { width: "50%", paddingVertical: 8 },
  label: { fontSize: 12 },
  value: { fontSize: 15, fontWeight: "700", marginTop: 3 },
});
