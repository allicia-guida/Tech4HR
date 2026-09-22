import { Fingerprint, MapPin } from "lucide-react-native";
import * as LocalAuthentication from "expo-local-authentication";
import { useState } from "react";
import { StyleSheet, Text, View } from "react-native";
import { useRouter } from "expo-router";
import { AppHeader } from "@/components/common/app-header";
import {
  PrimaryButton,
  SecondaryButton,
  TextButton,
} from "@/components/common/buttons";
import { Screen } from "@/components/common/screen";
import {
  useCreateTimeEntry,
  useTimeEntries,
} from "@/features/attendance/hooks/use-time-entries";
import {
  AttendanceLocation,
  nextTimeEntryType,
  timeEntryLabel,
} from "@/features/attendance/types/time-entry";
import { locationService } from "@/features/attendance/services/location-service";
import { useAppTheme } from "@/theme/theme-provider";
import { formatReceiptDate, formatTime } from "@/utils/date";

function Detail({ label, value }: { label: string; value: string }) {
  const { colors } = useAppTheme();
  return (
    <View style={styles.detail}>
      <Text style={[styles.label, { color: colors.textSecondary }]}>
        {label}
      </Text>
      <Text style={[styles.value, { color: colors.text }]}>{value}</Text>
    </View>
  );
}
export default function ClockScreen() {
  const { colors } = useAppTheme();
  const router = useRouter();
  const mutation = useCreateTimeEntry();
  const entries = useTimeEntries();
  const now = new Date();
  const type = nextTimeEntryType(entries.data ?? []);
  const [location, setLocation] = useState<AttendanceLocation | null>(null);
  const [locationLoading, setLocationLoading] = useState(false);
  const [locationError, setLocationError] = useState<string | null>(null);
  const captureLocation = async () => {
    setLocationLoading(true);
    setLocationError(null);
    try {
      const current = await locationService.capture();
      setLocation(current);
      return current;
    } catch (error) {
      setLocation(null);
      const code = error instanceof Error ? error.message : "";
      const messages: Record<string, string> = {
        LOCATION_DISABLED: "Ative a localização do aparelho para continuar.",
        LOCATION_PERMISSION_DENIED:
          "Permita o acesso à localização durante o uso do aplicativo.",
        LOCATION_MOCKED: "Localizações simuladas não são aceitas.",
        LOCATION_INACCURATE:
          "O sinal de localização está impreciso. Vá para uma área aberta e tente novamente.",
      };
      setLocationError(
        messages[code] ?? "Não foi possível obter uma localização válida.",
      );
      return null;
    } finally {
      setLocationLoading(false);
    }
  };
  const confirm = async () => {
    if (!type) return;
    const currentLocation =
      location &&
      Date.now() - new Date(location.capturedAt).getTime() < 60_000
        ? location
        : await captureLocation();
    if (!currentLocation) return;
    const [hardware, enrolled] = await Promise.all([
      LocalAuthentication.hasHardwareAsync(),
      LocalAuthentication.isEnrolledAsync(),
    ]);
    if (hardware && enrolled) {
      const authentication = await LocalAuthentication.authenticateAsync({
        promptMessage: "Confirmar registro de ponto",
        cancelLabel: "Cancelar",
      });
      if (!authentication.success) return;
    }
    mutation.mutate({ type, location: currentLocation }, {
      onSuccess: (entry) =>
        router.replace({
          pathname: "/success",
          params: {
            id: entry.id,
            timestamp: entry.timestamp,
            type: entry.type,
          },
        }),
    });
  };
  return (
    <Screen contentStyle={styles.content}>
      <AppHeader back title="Registrar ponto" />
      <View
        style={[
          styles.card,
          { backgroundColor: colors.surface, borderColor: colors.border },
        ]}
      >
        <View
          style={[
            styles.iconCircle,
            { backgroundColor: colors.surfaceSecondary },
          ]}
        >
          <Fingerprint size={34} color={colors.primary} />
        </View>
        <Text style={[styles.title, { color: colors.text }]}>
          Confirmar registro
        </Text>
        <View style={styles.details}>
          <Detail
            label="Tipo"
            value={type ? timeEntryLabel[type] : "Jornada concluída"}
          />
          <Detail label="Data" value={formatReceiptDate(now)} />
          <Detail label="Hora" value={formatTime(now)} />
          <Detail label="Origem" value="App - Smartphone" />
          <Detail
            label="Localização"
            value={
              location
                ? `${location.latitude.toFixed(6)}, ${location.longitude.toFixed(6)} (±${Math.round(location.accuracyMeters)} m)`
                : locationLoading
                  ? "Obtendo localização segura..."
                  : "Localização não validada"
            }
          />
        </View>
        <SecondaryButton
          label={location ? "Atualizar localização" : "Tentar localização novamente"}
          icon={<MapPin size={19} color={colors.primary} />}
          onPress={() => void captureLocation()}
          disabled={locationLoading || mutation.isPending}
          style={styles.locationButton}
        />
        <PrimaryButton
          label="Confirmar"
          onPress={() => void confirm()}
          loading={mutation.isPending}
          disabled={!type || !location || locationLoading}
        />
        <TextButton
          label="Cancelar"
          onPress={() => router.back()}
          disabled={mutation.isPending}
        />
        {mutation.isError ? (
          <Text style={[styles.error, { color: colors.danger }]}>
            {mutation.error instanceof Error &&
            mutation.error.message === "OFFLINE_NOT_CONFIRMED"
              ? "Sem internet. A tentativa foi guardada como pendente e não foi confirmada."
              : mutation.error instanceof Error &&
                  mutation.error.message === "INVALID_TIME_ENTRY_SEQUENCE"
                ? "A sequência de registros mudou. Atualize o histórico e tente novamente."
                : mutation.error instanceof Error &&
                    mutation.error.message === "API_NOT_CONFIGURED"
                  ? "A API segura do Tech4HR ainda não foi configurada neste aplicativo."
                : "Não foi possível registrar. Tente novamente."}
          </Text>
        ) : null}
        {locationError ? (
          <Text style={[styles.error, { color: colors.danger }]}>
            {locationError}
          </Text>
        ) : null}
      </View>
    </Screen>
  );
}
const styles = StyleSheet.create({
  content: { paddingBottom: 24 },
  card: {
    marginTop: 24,
    padding: 24,
    borderRadius: 8,
    borderWidth: StyleSheet.hairlineWidth,
  },
  iconCircle: {
    width: 64,
    height: 64,
    borderRadius: 32,
    alignItems: "center",
    justifyContent: "center",
    alignSelf: "center",
  },
  title: {
    fontSize: 20,
    fontWeight: "700",
    textAlign: "center",
    marginTop: 16,
  },
  details: { marginVertical: 28, gap: 20 },
  locationButton: { marginBottom: 12 },
  detail: { gap: 5 },
  label: { fontSize: 12 },
  value: { fontSize: 16, fontWeight: "500" },
  error: { textAlign: "center", fontSize: 13, marginTop: 8 },
});
