import { ChevronRight } from "lucide-react-native";
import { Pressable, StyleSheet, Text, View } from "react-native";
import { useAppTheme } from "@/theme/theme-provider";

export function MenuRow({
  label,
  value,
  onPress,
}: {
  label: string;
  value?: string;
  onPress: () => void;
}) {
  const { colors } = useAppTheme();
  return (
    <Pressable
      onPress={onPress}
      accessibilityRole="button"
      style={({ pressed }) => [
        styles.row,
        { borderBottomColor: colors.border, opacity: pressed ? 0.65 : 1 },
      ]}
    >
      <Text style={[styles.label, { color: colors.text }]}>{label}</Text>
      <View style={styles.trailing}>
        {value ? (
          <Text style={[styles.value, { color: colors.textSecondary }]}>
            {value}
          </Text>
        ) : null}
        <ChevronRight size={20} color={colors.textSecondary} />
      </View>
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
  label: { flex: 1, fontSize: 15 },
  trailing: { flexDirection: "row", alignItems: "center", gap: 8 },
  value: { fontSize: 14 },
});
