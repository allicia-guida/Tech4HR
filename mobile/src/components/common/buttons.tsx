import {
  ActivityIndicator,
  Pressable,
  StyleSheet,
  Text,
  ViewStyle,
} from "react-native";
import { ReactNode } from "react";
import { useAppTheme } from "@/theme/theme-provider";

type Props = {
  label: string;
  onPress: () => void;
  disabled?: boolean;
  loading?: boolean;
  icon?: ReactNode;
  style?: ViewStyle;
  accessibilityLabel?: string;
};
export function PrimaryButton({
  label,
  onPress,
  disabled,
  loading,
  icon,
  style,
  accessibilityLabel,
}: Props) {
  const { colors } = useAppTheme();
  return (
    <Pressable
      accessibilityRole="button"
      accessibilityLabel={accessibilityLabel ?? label}
      disabled={disabled || loading}
      onPress={onPress}
      style={({ pressed }) => [
        styles.base,
        {
          backgroundColor: pressed ? colors.primaryPressed : colors.primary,
          opacity: disabled ? 0.55 : 1,
        },
        style,
      ]}
    >
      {loading ? (
        <ActivityIndicator color={colors.onPrimary} />
      ) : (
        <>
          {icon}
          <Text style={[styles.primaryText, { color: colors.onPrimary }]}>
            {label}
          </Text>
        </>
      )}
    </Pressable>
  );
}
export function SecondaryButton({
  label,
  onPress,
  disabled,
  icon,
  style,
}: Props) {
  const { colors } = useAppTheme();
  return (
    <Pressable
      accessibilityRole="button"
      disabled={disabled}
      onPress={onPress}
      style={({ pressed }) => [
        styles.base,
        {
          backgroundColor: pressed ? colors.surfaceSecondary : colors.surface,
          borderColor: colors.border,
          borderWidth: 1,
        },
        style,
      ]}
    >
      {icon}
      <Text style={[styles.secondaryText, { color: colors.text }]}>
        {label}
      </Text>
    </Pressable>
  );
}
export function TextButton({ label, onPress, disabled, style }: Props) {
  const { colors } = useAppTheme();
  return (
    <Pressable
      accessibilityRole="button"
      disabled={disabled}
      onPress={onPress}
      style={[styles.textButton, style]}
    >
      <Text
        style={[
          styles.textLink,
          { color: colors.primary, opacity: disabled ? 0.55 : 1 },
        ]}
      >
        {label}
      </Text>
    </Pressable>
  );
}
const styles = StyleSheet.create({
  base: {
    minHeight: 52,
    borderRadius: 8,
    paddingHorizontal: 20,
    alignItems: "center",
    justifyContent: "center",
    flexDirection: "row",
    gap: 10,
  },
  primaryText: { fontSize: 15, fontWeight: "700", letterSpacing: 0.4 },
  secondaryText: { fontSize: 15, fontWeight: "600" },
  textButton: {
    minHeight: 48,
    justifyContent: "center",
    alignItems: "center",
    paddingHorizontal: 12,
  },
  textLink: { fontSize: 14, fontWeight: "600" },
});
