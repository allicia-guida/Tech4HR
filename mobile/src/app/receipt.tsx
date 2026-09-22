import * as Print from "expo-print";
import * as Sharing from "expo-sharing";
import { Alert, StyleSheet, View } from "react-native";
import { useLocalSearchParams } from "expo-router";
import { usePreventScreenCapture } from "expo-screen-capture";
import { AppHeader } from "@/components/common/app-header";
import { SecondaryButton } from "@/components/common/buttons";
import { Screen } from "@/components/common/screen";
import { Receipt } from "@/components/attendance/receipt";
import {
  TimeEntryType,
  timeEntryLabel,
} from "@/features/attendance/types/time-entry";
import {
  formatReceiptDate,
  formatTime,
  normalizeTimestamp,
} from "@/utils/date";
import { useAppTheme } from "@/theme/theme-provider";
import { useSessionStore } from "@/stores/session-store";
import { escapeHtml } from "@/utils/html";
import { useTimeEntries } from "@/features/attendance/hooks/use-time-entries";

export default function ReceiptScreen() {
  const { colors } = useAppTheme();
  const params = useLocalSearchParams<{
    id?: string;
    timestamp?: string;
    type?: TimeEntryType;
  }>();
  const timestamp = normalizeTimestamp(params.timestamp);
  const employee = useSessionStore((state) => state.user);
  const entries = useTimeEntries();
  const type = Object.values(TimeEntryType).includes(params.type as TimeEntryType)
    ? (params.type as TimeEntryType)
    : TimeEntryType.CLOCK_IN;
  const entry = entries.data?.find((item) => item.id === params.id) ?? {
    id: params.id ?? "registro",
    employeeId: employee?.id ?? "current",
    type,
    timestamp,
    source: "APP_SMARTPHONE" as const,
    deviceId: "server",
    createdAt: timestamp,
  };
  usePreventScreenCapture("receipt");
  const share = async () => {
    const location =
      entry.latitude != null && entry.longitude != null
        ? `${entry.latitude.toFixed(6)}, ${entry.longitude.toFixed(6)} (±${Math.round(entry.accuracyMeters ?? 0)} m)`
        : "Não disponível";
    const html = `<html><body style="font-family:Arial;padding:40px;color:${colors.text};background:${colors.surface}"><h1 style="color:${colors.primary}">Tech4HR</h1><h2>Comprovante de Ponto</h2><hr/><p><small>Funcionário</small><br/><b>${escapeHtml(employee?.name ?? "Não informado")}</b></p><p><small>Matrícula</small><br/><b>${escapeHtml(employee?.registration ?? "Não informada")}</b></p><p><small>Data</small><br/><b>${escapeHtml(formatReceiptDate(entry.timestamp))}</b></p><p><small>Hora</small><br/><b>${escapeHtml(formatTime(entry.timestamp))}</b></p><p><small>Tipo</small><br/><b>${escapeHtml(timeEntryLabel[entry.type])}</b></p><p><small>Origem</small><br/><b>App - Smartphone</b></p><p><small>Localização</small><br/><b>${escapeHtml(location)}</b></p><hr/><small>Este é um comprovante oficial de registro de ponto.</small></body></html>`;
    try {
      const { uri } = await Print.printToFileAsync({ html });
      if (await Sharing.isAvailableAsync())
        await Sharing.shareAsync(uri, {
          mimeType: "application/pdf",
          dialogTitle: "Compartilhar comprovante",
        });
      else Alert.alert("Comprovante salvo", uri);
    } catch {
      Alert.alert(
        "Não foi possível compartilhar",
        "Tente novamente em instantes.",
      );
    }
  };
  return (
    <Screen contentStyle={styles.content}>
      <AppHeader back title="Comprovante" share={share} />
      <View style={styles.body}>
        <Receipt entry={entry} />
        <SecondaryButton
          label="Salvar ou compartilhar PDF"
          onPress={share}
          style={styles.button}
        />
      </View>
    </Screen>
  );
}
const styles = StyleSheet.create({
  content: { paddingBottom: 24 },
  body: { paddingTop: 24 },
  button: { marginTop: 16 },
});
