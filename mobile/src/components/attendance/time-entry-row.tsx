import { ChevronRight, MapPin } from "lucide-react-native";
import { Pressable, StyleSheet, Text, View } from "react-native";
import { useAppTheme } from "@/theme/theme-provider";
import {
  TimeEntry,
  TimeEntryType,
  timeEntryLabel,
} from "@/features/attendance/types/time-entry";
import { formatTime } from "@/utils/date";

export function TimeEntryRow({
  entry,
  showSource = false,
  onPress,
}: {
  entry: TimeEntry;
  showSource?: boolean;
  onPress?: () => void;
}) {
  const { colors } = useAppTheme();
  const isIn = [TimeEntryType.CLOCK_IN, TimeEntryType.BREAK_END].includes(
    entry.type,
  );
  return (
    <Pressable
      accessibilityRole={onPress ? "button" : undefined}
      accessibilityLabel={onPress ? `Visualizar ${timeEntryLabel[entry.type]}` : undefined}
      disabled={!onPress}
      onPress={onPress}
      style={({ pressed }) => [
        styles.row,
        {
          borderBottomColor: colors.border,
          opacity: pressed ? 0.7 : 1,
        },
      ]}
    >
      <Text style={[styles.time, { color: colors.text }]}>
        {formatTime(entry.timestamp)}
      </Text>
      <View style={styles.status}>
        <View
          style={[
            styles.dot,
            { backgroundColor: isIn ? colors.success : colors.danger },
          ]}
        />
        <Text style={[styles.type, { color: colors.text }]}>
          {timeEntryLabel[entry.type]}
        </Text>
      </View>
      {showSource ? (
        <View style={styles.sourceGroup}>
          {entry.latitude != null ? (
            <MapPin size={15} color={colors.success} />
          ) : null}
          <Text style={[styles.source, { color: colors.textSecondary }]}>
            {entry.latitude != null ? "GPS" : "App"}
          </Text>
        </View>
      ) : null}
      {onPress ? <ChevronRight color={colors.textSecondary} size={18} /> : null}
    </Pressable>
  );
}
const styles = StyleSheet.create({
  row: {
    minHeight: 60,
    flexDirection: "row",
    alignItems: "center",
    borderBottomWidth: StyleSheet.hairlineWidth,
  },
  time: { width: 72, fontSize: 16, fontWeight: "600" },
  status: { flex: 1, flexDirection: "row", alignItems: "center", gap: 10 },
  dot: { width: 8, height: 8, borderRadius: 4 },
  type: { fontSize: 15 },
  source: { fontSize: 14 },
  sourceGroup: { flexDirection: "row", alignItems: "center", gap: 4 },
});
