import { Check } from "lucide-react-native";
import { StyleSheet, Text, View } from "react-native";
import { useLocalSearchParams, useRouter } from "expo-router";
import { usePreventScreenCapture } from "expo-screen-capture";
import { PrimaryButton, SecondaryButton } from "@/components/common/buttons";
import { Screen } from "@/components/common/screen";
import { useAppTheme } from "@/theme/theme-provider";
import {
  formatReceiptDate,
  formatTime,
  normalizeTimestamp,
} from "@/utils/date";
import {
  TimeEntryType,
  timeEntryLabel,
} from "@/features/attendance/types/time-entry";

export default function SuccessScreen() {
  usePreventScreenCapture("success");
  const { colors } = useAppTheme();
  const router = useRouter();
  const params = useLocalSearchParams<{
    timestamp?: string;
    type?: TimeEntryType;
    id?: string;
  }>();
  const timestamp = normalizeTimestamp(params.timestamp);
  const type = Object.values(TimeEntryType).includes(params.type as TimeEntryType)
    ? (params.type as TimeEntryType)
    : TimeEntryType.CLOCK_IN;
  return (
    <Screen
      scroll={false}
      contentStyle={[styles.overlay, { backgroundColor: colors.overlay }]}
    >
      <View style={[styles.modal, { backgroundColor: colors.surface }]}>
        <View
          style={[styles.check, { backgroundColor: colors.surfaceSecondary }]}
        >
          <Check size={38} strokeWidth={3} color={colors.success} />
        </View>
        <Text style={[styles.title, { color: colors.text }]}>
          Ponto registrado!
        </Text>
        <Text style={[styles.description, { color: colors.textSecondary }]}>
          Seu ponto foi registrado com sucesso.
        </Text>
        <View
          style={[styles.summary, { backgroundColor: colors.surfaceSecondary }]}
        >
          <Text style={[styles.summaryTitle, { color: colors.text }]}>
            {timeEntryLabel[type]}
          </Text>
          <Text style={[styles.summaryText, { color: colors.textSecondary }]}>
            {formatReceiptDate(timestamp)} às {formatTime(timestamp)}
          </Text>
          <Text style={[styles.summaryText, { color: colors.textSecondary }]}>
            App - Smartphone
          </Text>
        </View>
        <View style={styles.actions}>
          <SecondaryButton
            label="Ver comprovante"
            onPress={() =>
              router.replace({
                pathname: "/receipt",
                params: { id: params.id, timestamp, type },
              })
            }
          />
          <PrimaryButton
            label="OK"
            onPress={() => router.replace("/(tabs)/home")}
          />
        </View>
      </View>
    </Screen>
  );
}
const styles = StyleSheet.create({
  overlay: { justifyContent: "center", paddingVertical: 24 },
  modal: {
    width: "100%",
    maxWidth: 420,
    alignSelf: "center",
    borderRadius: 12,
    padding: 24,
  },
  check: {
    width: 72,
    height: 72,
    borderRadius: 36,
    alignSelf: "center",
    alignItems: "center",
    justifyContent: "center",
  },
  title: {
    textAlign: "center",
    fontSize: 22,
    fontWeight: "700",
    marginTop: 20,
  },
  description: { textAlign: "center", fontSize: 14, marginTop: 8 },
  summary: { borderRadius: 8, padding: 16, marginVertical: 24, gap: 6 },
  summaryTitle: { fontSize: 16, fontWeight: "700" },
  summaryText: { fontSize: 14 },
  actions: { gap: 12 },
});
