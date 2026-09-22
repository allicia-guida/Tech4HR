import { Check } from "lucide-react-native";
import { Pressable, StyleSheet, Text, View } from "react-native";
import { ThemePreference, useThemeStore } from "@/stores/theme-store";
import { useAppTheme } from "@/theme/theme-provider";

const options: {
  value: ThemePreference;
  label: string;
  description: string;
}[] = [
  {
    value: "system",
    label: "Sistema",
    description: "Acompanha o tema do dispositivo",
  },
  { value: "light", label: "Claro", description: "Sempre usar tema claro" },
  { value: "dark", label: "Escuro", description: "Sempre usar tema escuro" },
];
export function ThemeSelector() {
  const { colors } = useAppTheme();
  const { preference, setPreference } = useThemeStore();
  return (
    <View>
      {options.map((option) => {
        const selected = option.value === preference;
        return (
          <Pressable
            key={option.value}
            accessibilityRole="radio"
            accessibilityState={{ checked: selected }}
            onPress={() => void setPreference(option.value)}
            style={[styles.row, { borderBottomColor: colors.border }]}
          >
            <View style={styles.copy}>
              <Text style={[styles.label, { color: colors.text }]}>
                {option.label}
              </Text>
              <Text
                style={[styles.description, { color: colors.textSecondary }]}
              >
                {option.description}
              </Text>
            </View>
            <View
              style={[
                styles.radio,
                {
                  borderColor: selected ? colors.primary : colors.border,
                  backgroundColor: selected ? colors.primary : colors.surface,
                },
              ]}
            >
              {selected ? (
                <Check size={14} strokeWidth={3} color={colors.onPrimary} />
              ) : null}
            </View>
          </Pressable>
        );
      })}
    </View>
  );
}
const styles = StyleSheet.create({
  row: {
    minHeight: 68,
    flexDirection: "row",
    alignItems: "center",
    borderBottomWidth: StyleSheet.hairlineWidth,
  },
  copy: { flex: 1, gap: 4 },
  label: { fontSize: 15, fontWeight: "600" },
  description: { fontSize: 12 },
  radio: {
    width: 22,
    height: 22,
    borderRadius: 11,
    borderWidth: 1.5,
    alignItems: "center",
    justifyContent: "center",
  },
});
