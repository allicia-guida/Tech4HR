import { zodResolver } from "@hookform/resolvers/zod";
import { Controller, useForm } from "react-hook-form";
import { useState } from "react";
import { StyleSheet, Text, View } from "react-native";
import { useRouter } from "expo-router";
import { usePreventScreenCapture } from "expo-screen-capture";
import { CheckCircle2, Mail } from "lucide-react-native";
import { AppHeader } from "@/components/common/app-header";
import { AppInput } from "@/components/common/app-input";
import { PrimaryButton, TextButton } from "@/components/common/buttons";
import { Screen } from "@/components/common/screen";
import {
  PasswordResetForm,
  passwordResetSchema,
} from "@/features/auth/schemas/password-reset-schema";
import { authService } from "@/features/auth/services/auth-service";
import { useAppTheme } from "@/theme/theme-provider";

export default function ForgotPasswordScreen() {
  const { colors } = useAppTheme();
  const router = useRouter();
  const [delivery, setDelivery] = useState<"email" | null>(null);
  const [submitError, setSubmitError] = useState<string | null>(null);
  usePreventScreenCapture("forgot-password");
  const {
    control,
    handleSubmit,
    formState: { errors, isSubmitting },
  } = useForm<PasswordResetForm>({
    resolver: zodResolver(passwordResetSchema),
    defaultValues: { email: "" },
  });
  const submit = handleSubmit(async ({ email }) => {
    setSubmitError(null);
    try {
      const result = await authService.requestPasswordReset(email);
      setDelivery(result.delivery);
    } catch {
      setSubmitError(
        "Não foi possível processar a solicitação. Tente novamente.",
      );
    }
  });
  return (
    <Screen contentStyle={styles.content}>
      <AppHeader back title="Recuperar senha" />
      <View style={styles.body}>
        {delivery ? (
          <View style={styles.success}>
            <CheckCircle2 size={56} color={colors.success} />
            <Text style={[styles.title, { color: colors.text }]}>
              Solicitação recebida
            </Text>
            <Text style={[styles.description, { color: colors.textSecondary }]}>
              Se o e-mail estiver cadastrado, você receberá as instruções de recuperação.
            </Text>
            {delivery ? (
              <View
                style={[
                  styles.notice,
                  { backgroundColor: colors.surfaceSecondary },
                ]}
              >
                <Text
                  style={[styles.noticeText, { color: colors.textSecondary }]}
                >
                  Por segurança, não informamos se o endereço está cadastrado.
                  Verifique também a caixa de spam.
                </Text>
              </View>
            ) : null}
            <PrimaryButton
              label="Voltar ao login"
              onPress={() => router.replace("/login")}
              style={styles.fullWidth}
            />
          </View>
        ) : (
          <View style={styles.form}>
            <View
              style={[
                styles.icon,
                { backgroundColor: colors.surfaceSecondary },
              ]}
            >
              <Mail size={28} color={colors.primary} />
            </View>
            <Text style={[styles.title, { color: colors.text }]}>
              Esqueceu sua senha?
            </Text>
            <Text style={[styles.description, { color: colors.textSecondary }]}>
              Informe o e-mail da sua conta para receber as instruções de
              recuperação.
            </Text>
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
            {submitError ? (
              <Text style={[styles.error, { color: colors.danger }]}>
                {submitError}
              </Text>
            ) : null}
            <PrimaryButton
              label="Enviar instruções"
              onPress={submit}
              loading={isSubmitting}
            />
            <TextButton
              label="Voltar ao login"
              onPress={() => router.replace("/login")}
            />
          </View>
        )}
      </View>
    </Screen>
  );
}

const styles = StyleSheet.create({
  content: { paddingBottom: 24 },
  body: { flex: 1, justifyContent: "center", paddingVertical: 32 },
  form: { width: "100%", maxWidth: 440, alignSelf: "center", gap: 20 },
  success: {
    width: "100%",
    maxWidth: 440,
    alignSelf: "center",
    alignItems: "center",
    gap: 18,
  },
  icon: {
    width: 60,
    height: 60,
    borderRadius: 30,
    alignItems: "center",
    justifyContent: "center",
    alignSelf: "center",
  },
  title: { fontSize: 22, fontWeight: "700", textAlign: "center" },
  description: { fontSize: 15, lineHeight: 22, textAlign: "center" },
  notice: { borderRadius: 8, padding: 16, width: "100%" },
  noticeText: { fontSize: 13, lineHeight: 19, textAlign: "center" },
  error: { fontSize: 13, textAlign: "center" },
  fullWidth: { width: "100%" },
});
