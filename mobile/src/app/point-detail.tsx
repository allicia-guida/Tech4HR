import { MapPin, ShieldCheck } from "lucide-react-native";
import { useLocalSearchParams, useRouter } from "expo-router";
import { StyleSheet, Text, View } from "react-native";
import { AppHeader } from "@/components/common/app-header";
import { PrimaryButton } from "@/components/common/buttons";
import { Screen } from "@/components/common/screen";
import { ErrorState, LoadingState } from "@/components/common/states";
import { useTimeEntries } from "@/features/attendance/hooks/use-time-entries";
import { timeEntryLabel } from "@/features/attendance/types/time-entry";
import { useAppTheme } from "@/theme/theme-provider";
import { formatReceiptDate, formatTime } from "@/utils/date";

function Field({ label, value }: { label: string; value: string }) {
  const { colors } = useAppTheme();
  return (
    <View style={styles.field}>
      <Text style={[styles.label, { color: colors.textSecondary }]}>{label}</Text>
      <Text style={[styles.value, { color: colors.text }]}>{value}</Text>
    </View>
  );
}

export default function PointDetailScreen() {
  const { colors } = useAppTheme();
  const router = useRouter();
  const { id } = useLocalSearchParams<{ id?: string }>();
  const entries = useTimeEntries();
  const entry = entries.data?.find((item) => item.id === id);
  return (
    <Screen contentStyle={styles.content}>
      <AppHeader title="Detalhe do ponto" back />
      {entries.isLoading ? <LoadingState /> : null}
      {entries.isError ? (
        <ErrorState retry={() => void entries.refetch()} />
      ) : null}
      {!entries.isLoading && !entries.isError && !entry ? (
        <Text style={[styles.empty, { color: colors.textSecondary }]}>
          Registro não encontrado no histórico.
        </Text>
      ) : null}
      {entry ? (
        <View
          style={[
            styles.card,
            { backgroundColor: colors.surface, borderColor: colors.border },
          ]}
        >
          <View
            style={[styles.icon, { backgroundColor: colors.surfaceSecondary }]}
          >
            <ShieldCheck size={30} color={colors.success} />
          </View>
          <Field label="Tipo" value={timeEntryLabel[entry.type]} />
          <Field label="Data" value={formatReceiptDate(entry.timestamp)} />
          <Field label="Hora" value={formatTime(entry.timestamp)} />
          <Field label="Origem" value="App - Smartphone" />
          <View style={styles.locationTitle}>
            <MapPin size={18} color={colors.primary} />
            <Text style={[styles.locationText, { color: colors.text }]}>
              Geolocalização
            </Text>
          </View>
          <Field
            label="Coordenadas"
            value={
              entry.latitude != null && entry.longitude != null
                ? `${entry.latitude.toFixed(6)}, ${entry.longitude.toFixed(6)}`
                : "Não disponível para este registro"
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
          <PrimaryButton
            label="Ver comprovante"
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
          />
        </View>
      ) : null}
    </Screen>
  );
}

const styles = StyleSheet.create({
  content: { paddingBottom: 32 },
  card: {
    marginTop: 24,
    borderWidth: 1,
    borderRadius: 10,
    padding: 20,
    gap: 18,
  },
  icon: {
    width: 58,
    height: 58,
    borderRadius: 29,
    alignItems: "center",
    justifyContent: "center",
    alignSelf: "center",
  },
  field: { gap: 4 },
  label: { fontSize: 12 },
  value: { fontSize: 15, fontWeight: "600" },
  locationTitle: {
    flexDirection: "row",
    gap: 8,
    alignItems: "center",
    marginTop: 4,
  },
  locationText: { fontSize: 16, fontWeight: "800" },
  empty: { textAlign: "center", paddingVertical: 32 },
});
