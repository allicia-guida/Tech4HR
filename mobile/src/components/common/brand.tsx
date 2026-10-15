import { StyleSheet, Text, View } from "react-native";
import { useAppTheme } from "@/theme/theme-provider";

export function Brand({ size = 26 }: { size?: number }) {
  const { colors } = useAppTheme();
  return (
    <View style={styles.row} accessibilityRole="header">
      <Text style={[styles.word, { color: colors.text, fontSize: size }]}>
        Tech
      </Text>
      <Text style={[styles.word, { color: colors.primary, fontSize: size }]}>
        4HR
      </Text>
    </View>
  );
}
const styles = StyleSheet.create({
  row: { flexDirection: "row", alignItems: "center" },
  word: { fontWeight: "800", letterSpacing: -0.8 },
});
