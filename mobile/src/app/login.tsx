import { zodResolver } from "@hookform/resolvers/zod";
import { Controller, useForm } from "react-hook-form";
import { useState } from "react";
import { Pressable, StyleSheet, Text, View } from "react-native";
import { useRouter } from "expo-router";
import { usePreventScreenCapture } from "expo-screen-capture";
import { Check } from "lucide-react-native";
import { AppInput } from "@/components/common/app-input";
import { Brand } from "@/components/common/brand";
import { PrimaryButton, TextButton } from "@/components/common/buttons";
import { Screen } from "@/components/common/screen";
import { LoginForm, loginSchema } from "@/features/auth/schemas/login-schema";
import { useAppTheme } from "@/theme/theme-provider";
import { authService } from "@/features/auth/services/auth-service";
import { useSessionStore } from "@/stores/session-store";
import { ApiError } from "@/services/api/http-client";

export default function LoginScreen() {
  const { colors } = useAppTheme();
  const router = useRouter();
  const signIn = useSessionStore((state) => state.signIn);
  const [remember, setRemember] = useState(false);
  const [loading, setLoading] = useState(false);
  const [submitError, setSubmitError] = useState<string | null>(null);
  usePreventScreenCapture("login");
  const {
    control,
    handleSubmit,
    formState: { errors },
  } = useForm<LoginForm>({
    resolver: zodResolver(loginSchema),
    defaultValues: { email: "", password: "" },
  });
  const submit = handleSubmit(async (values) => {
    setLoading(true);
    setSubmitError(null);
    try {
      const session = await authService.signIn(values);
      await signIn(session, remember);
      router.replace("/(tabs)/home");
    } catch (error) {
      setSubmitError(
        error instanceof ApiError && error.status === 0
          ? error.message
          : "Não foi possível entrar. Verifique os dados e tente novamente.",
      );
    } finally {
      setLoading(false);
    }
  });
  return (
    <Screen contentStyle={styles.content}>
      <View style={styles.brand}>
        <Brand size={30} />
        <Text style={[styles.subtitle, { color: colors.textSecondary }]}>
          Acesse sua conta
        </Text>
      </View>
      <View style={styles.form}>
        <Controller
          control={control}
          name="email"
          render={({ field: { value, onChange, onBlur } }) => (
            <AppInput
              label="E-mail"
              value={value}
              onChangeText={onChange}
              onBlur={onBlur}
              autoCapitalize="none"
              keyboardType="email-address"
              autoComplete="email"
              textContentType="emailAddress"
              error={errors.email?.message}
            />
          )}
        />
        <Controller
          control={control}
          name="password"
          render={({ field: { value, onChange, onBlur } }) => (
            <AppInput
              label="Senha"
              value={value}
              onChangeText={onChange}
              onBlur={onBlur}
              password
              autoComplete="current-password"
              textContentType="password"
              error={errors.password?.message}
            />
          )}
        />
        <View style={styles.options}>
          <Pressable
            accessibilityRole="checkbox"
            accessibilityState={{ checked: remember }}
            onPress={() => setRemember((v) => !v)}
            style={styles.remember}
          >
            <View
              style={[
                styles.checkbox,
                {
                  borderColor: remember ? colors.primary : colors.border,
                  backgroundColor: remember ? colors.primary : colors.surface,
                },
              ]}
            >
              {remember ? (
                <Check size={14} strokeWidth={3} color={colors.onPrimary} />
              ) : null}
            </View>
            <Text style={[styles.optionText, { color: colors.text }]}>
              Lembrar de mim
            </Text>
          </Pressable>
          <TextButton
            label="Esqueceu a senha?"
            onPress={() => router.push("/forgot-password")}
          />
        </View>
        <PrimaryButton label="ENTRAR" onPress={submit} loading={loading} />
        {submitError ? (
          <Text style={[styles.error, { color: colors.danger }]}>
            {submitError}
          </Text>
        ) : null}
      </View>
      <Text style={[styles.version, { color: colors.textSecondary }]}>
        v1.0.0
      </Text>
    </Screen>
  );
}
const styles = StyleSheet.create({
  content: { justifyContent: "center", paddingVertical: 24 },
  brand: { alignItems: "center", gap: 10, marginBottom: 40 },
  subtitle: { fontSize: 16 },
  form: { width: "100%", maxWidth: 440, alignSelf: "center", gap: 20 },
  options: {
    flexDirection: "row",
    justifyContent: "space-between",
    alignItems: "center",
    marginTop: -4,
  },
  remember: {
    minHeight: 48,
    flexDirection: "row",
    alignItems: "center",
    gap: 10,
  },
  checkbox: {
    width: 20,
    height: 20,
    borderRadius: 4,
    borderWidth: 1.5,
    alignItems: "center",
    justifyContent: "center",
  },
  optionText: { fontSize: 14 },
  version: { textAlign: "center", marginTop: 36, fontSize: 12 },
  error: { fontSize: 13, textAlign: "center" },
});
