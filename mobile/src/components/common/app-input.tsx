import { Eye, EyeOff } from "lucide-react-native";
import { forwardRef, useState } from "react";
import {
  Pressable,
  StyleSheet,
  Text,
  TextInput,
  TextInputProps,
  View,
} from "react-native";
import { useAppTheme } from "@/theme/theme-provider";

type Props = TextInputProps & {
  label: string;
  error?: string;
  password?: boolean;
};
export const AppInput = forwardRef<TextInput, Props>(function AppInput(
  { label, error, password, ...props },
  ref,
) {
  const { colors } = useAppTheme();
  const [visible, setVisible] = useState(false);
  return (
    <View style={styles.wrapper}>
      <Text style={[styles.label, { color: colors.text }]}>{label}</Text>
      <View
        style={[
          styles.inputRow,
          {
            backgroundColor: colors.surface,
            borderColor: error ? colors.danger : colors.border,
          },
        ]}
      >
        <TextInput
          ref={ref}
          placeholderTextColor={colors.textSecondary}
          style={[styles.input, { color: colors.text }]}
          secureTextEntry={password && !visible}
          accessibilityLabel={label}
          {...props}
        />
        {password && (
          <Pressable
            accessibilityRole="button"
            accessibilityLabel={visible ? "Ocultar senha" : "Exibir senha"}
            onPress={() => setVisible((v) => !v)}
            style={styles.icon}
          >
            {visible ? (
              <EyeOff size={20} color={colors.textSecondary} />
            ) : (
              <Eye size={20} color={colors.textSecondary} />
            )}
          </Pressable>
        )}
      </View>
      {error ? (
        <Text style={[styles.error, { color: colors.danger }]}>{error}</Text>
      ) : null}
    </View>
  );
});
const styles = StyleSheet.create({
  wrapper: { gap: 8 },
  label: { fontSize: 14, fontWeight: "600" },
  inputRow: {
    minHeight: 52,
    borderWidth: 1,
    borderRadius: 8,
    flexDirection: "row",
    alignItems: "center",
  },
  input: { flex: 1, minHeight: 52, fontSize: 16, paddingHorizontal: 14 },
  icon: {
    width: 48,
    height: 48,
    alignItems: "center",
    justifyContent: "center",
  },
  error: { fontSize: 12 },
});
