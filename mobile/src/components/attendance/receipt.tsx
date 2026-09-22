import { StyleSheet, Text, View } from "react-native";
import { Brand } from "@/components/common/brand";
import { useAppTheme } from "@/theme/theme-provider";
import { formatReceiptDate, formatTime } from "@/utils/date";
import { useSessionStore } from "@/stores/session-store";
import {
  TimeEntry,
  timeEntryLabel,
} from "@/features/attendance/types/time-entry";

function Field({ label, value }: { label: string; value: string }) {
  const { colors } = useAppTheme();
  return (
    <View style={styles.field}>
      <Text style={[styles.label, { color: colors.textSecondary }]}>
        {label}
      </Text>
      <Text style={[styles.value, { color: colors.text }]}>{value}</Text>
    </View>
  );
}
export function Receipt({ entry }: { entry: TimeEntry }) {
  const { colors } = useAppTheme();
  const employee = useSessionStore((state) => state.user);
  return (
    <View
      style={[
        styles.receipt,
        { backgroundColor: colors.surface, borderColor: colors.border },
      ]}
    >
      <View style={styles.heading}>
        <Brand size={24} />
        <Text style={[styles.title, { color: colors.text }]}>
          Comprovante de Ponto
        </Text>
      </View>
      <View style={[styles.rule, { backgroundColor: colors.border }]} />
      <View style={styles.fields}>
        <Field label="Funcionário" value={employee?.name ?? "Não informado"} />
        <Field label="Matrícula" value={employee?.registration ?? "Não informada"} />
        <Field label="Data" value={formatReceiptDate(entry.timestamp)} />
        <Field label="Hora" value={formatTime(entry.timestamp)} />
        <Field label="Tipo" value={timeEntryLabel[entry.type]} />
        <Field label="Origem" value="App - Smartphone" />
        <Field
          label="Localização"
          value={
            entry.latitude != null && entry.longitude != null
              ? `${entry.latitude.toFixed(6)}, ${entry.longitude.toFixed(6)}`
              : "Não disponível"
          }
        />
        <Field
          label="Precisão"
          value={
            entry.accuracyMeters != null
              ? `±${Math.round(entry.accuracyMeters)} metros`
              : "Não disponível"
          }
        />
      </View>
      <View style={[styles.rule, { backgroundColor: colors.border }]} />
      <Text style={[styles.footer, { color: colors.textSecondary }]}>
        Este é um comprovante oficial de registro de ponto.
      </Text>
    </View>
  );
}
const styles = StyleSheet.create({
  receipt: {
    borderWidth: StyleSheet.hairlineWidth,
    borderRadius: 8,
    padding: 24,
  },
  heading: { alignItems: "center", gap: 10 },
  title: { fontSize: 18, fontWeight: "700" },
  rule: { height: StyleSheet.hairlineWidth, marginVertical: 24 },
  fields: { gap: 18 },
  field: { gap: 4 },
  label: { fontSize: 12 },
  value: { fontSize: 15, fontWeight: "500" },
  footer: { fontSize: 12, lineHeight: 18, textAlign: "center" },
});
