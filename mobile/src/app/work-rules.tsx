import { StyleSheet, Text, View } from "react-native";
import { AppHeader } from "@/components/common/app-header";
import { Screen } from "@/components/common/screen";
import { defaultWorkPolicy } from "@/features/workforce/fixtures/default-work-policy";
import { useAppTheme } from "@/theme/theme-provider";
import { formatMinuteOfDay, formatMinutes } from "@/utils/duration";

const weekdayNames = [
  "Domingo",
  "Segunda-feira",
  "Terça-feira",
  "Quarta-feira",
  "Quinta-feira",
  "Sexta-feira",
  "Sábado",
];

export default function WorkRulesScreen() {
  const { colors } = useAppTheme();
  const policy = defaultWorkPolicy;
  return (
    <Screen contentStyle={styles.content}>
      <AppHeader title="Minha jornada" back />
      <Text style={[styles.heading, { color: colors.text }]}>
        {policy.name}
      </Text>
      <Text style={[styles.caption, { color: colors.textSecondary }]}>
        Fuso horário: {policy.timeZone}
      </Text>
      <View style={styles.section}>
        <Text style={[styles.sectionTitle, { color: colors.text }]}>
          Escala semanal
        </Text>
        {policy.schedule.map((day) => (
          <View
            key={day.weekday}
            style={[styles.row, { borderBottomColor: colors.border }]}
          >
            <Text style={[styles.rowLabel, { color: colors.text }]}>
              {weekdayNames[day.weekday]}
            </Text>
            <Text style={[styles.rowValue, { color: colors.textSecondary }]}>
              {day.enabled
                ? `${formatMinuteOfDay(day.startMinute)}–${formatMinuteOfDay(day.endMinute)} · ${formatMinutes(day.breakMinutes)}`
                : "Folga"}
            </Text>
          </View>
        ))}
      </View>
      <View style={styles.section}>
        <Text style={[styles.sectionTitle, { color: colors.text }]}>
          Regras atuais
        </Text>
        <Text style={[styles.rule, { color: colors.textSecondary }]}>
          Tolerância de atraso: {policy.lateToleranceMinutes} minutos
        </Text>
        <Text style={[styles.rule, { color: colors.textSecondary }]}>
          Tolerância de hora extra: {policy.overtimeToleranceMinutes} minutos
        </Text>
        <Text style={[styles.rule, { color: colors.textSecondary }]}>
          Intervalo mínimo: {policy.minimumBreakMinutes} minutos
        </Text>
        <Text style={[styles.rule, { color: colors.textSecondary }]}>
          Banco negativo:{" "}
          {policy.allowNegativeTimeBank ? "permitido" : "não permitido"}
        </Text>
      </View>
      <View style={styles.section}>
        <Text style={[styles.sectionTitle, { color: colors.text }]}>
          Feriados configurados
        </Text>
        {policy.holidays.map((holiday) => (
          <View
            key={holiday.id}
            style={[styles.row, { borderBottomColor: colors.border }]}
          >
            <Text style={[styles.rowLabel, { color: colors.text }]}>
              {holiday.name}
            </Text>
            <Text style={[styles.rowValue, { color: colors.textSecondary }]}>
              {holiday.date}
            </Text>
          </View>
        ))}
      </View>
      <Text
        style={[
          styles.notice,
          {
            color: colors.textSecondary,
            backgroundColor: colors.surfaceSecondary,
          },
        ]}
      >
        Configuração provisória para desenvolvimento. O backend deverá validar e
        fornecer a política oficial da empresa.
      </Text>
    </Screen>
  );
}

const styles = StyleSheet.create({
  content: { paddingBottom: 32 },
  heading: { fontSize: 20, fontWeight: "800", marginTop: 24 },
  caption: { fontSize: 14, marginTop: 6 },
  section: { marginTop: 26 },
  sectionTitle: { fontSize: 16, fontWeight: "700", marginBottom: 8 },
  row: {
    minHeight: 52,
    borderBottomWidth: StyleSheet.hairlineWidth,
    flexDirection: "row",
    alignItems: "center",
    justifyContent: "space-between",
    gap: 12,
  },
  rowLabel: { flex: 1, fontSize: 14, fontWeight: "600" },
  rowValue: { fontSize: 13, textAlign: "right" },
  rule: { fontSize: 14, lineHeight: 24 },
  notice: {
    marginTop: 28,
    padding: 14,
    borderRadius: 8,
    fontSize: 13,
    lineHeight: 20,
  },
});
