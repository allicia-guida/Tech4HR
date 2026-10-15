import * as DocumentPicker from "expo-document-picker";
import { useLocalSearchParams, useRouter } from "expo-router";
import { zodResolver } from "@hookform/resolvers/zod";
import { Controller, useForm, useWatch } from "react-hook-form";
import { useState } from "react";
import { Pressable, StyleSheet, Text, View } from "react-native";
import { AppHeader } from "@/components/common/app-header";
import { AppInput } from "@/components/common/app-input";
import { PrimaryButton, SecondaryButton } from "@/components/common/buttons";
import { Screen } from "@/components/common/screen";
import { useTimeEntries } from "@/features/attendance/hooks/use-time-entries";
import {
  useCreateCorrection,
  useCorrections,
} from "@/features/corrections/hooks/use-corrections";
import {
  CorrectionForm,
  correctionSchema,
} from "@/features/corrections/schemas/correction-schema";
import {
  AttachmentReference,
  CorrectionReason,
} from "@/features/corrections/types/correction-request";
import { demoEmployee } from "@/features/profile/fixtures/demo-employee";
import { useSessionStore } from "@/stores/session-store";
import { useAppTheme } from "@/theme/theme-provider";
import { formatTime } from "@/utils/date";

const reasons: { value: CorrectionReason; label: string }[] = [
  { value: "FORGOT_ENTRY", label: "Esqueci a entrada" },
  { value: "FORGOT_EXIT", label: "Esqueci a saída" },
  { value: "WRONG_TIME", label: "Horário incorreto" },
  { value: "MEDICAL", label: "Justificativa médica" },
  { value: "OTHER", label: "Outro motivo" },
];

const statusLabels = {
  PENDING: "Aguardando aprovação",
  APPROVED: "Aprovada",
  REJECTED: "Rejeitada",
  CANCELED: "Cancelada",
};

export default function CorrectionsScreen() {
  const { colors } = useAppTheme();
  const router = useRouter();
  const params = useLocalSearchParams<{ date?: string }>();
  const initialDate = /^\d{4}-\d{2}-\d{2}$/.test(params.date ?? "")
    ? params.date!
    : new Date().toISOString().slice(0, 10);
  const employee = useSessionStore((state) => state.user) ?? demoEmployee;
  const entries = useTimeEntries();
  const requests = useCorrections(employee.id);
  const createRequest = useCreateCorrection(employee.id);
  const [attachments, setAttachments] = useState<AttachmentReference[]>([]);
  const {
    control,
    handleSubmit,
    reset,
    formState: { errors },
  } = useForm<CorrectionForm>({
    resolver: zodResolver(correctionSchema),
    defaultValues: {
      date: initialDate,
      reason: "FORGOT_ENTRY",
      justification: "",
      selectedEntryId: "",
      proposedType: "CLOCK_IN",
      proposedTime: "08:00",
    },
  });
  const selectedDate = useWatch({ control, name: "date" });
  const dateEntries = (entries.data ?? []).filter((entry) =>
    entry.timestamp.startsWith(selectedDate),
  );
  const chooseAttachment = async () => {
    const result = await DocumentPicker.getDocumentAsync({
      copyToCacheDirectory: true,
      multiple: true,
      type: ["application/pdf", "image/*"],
    });
    if (result.canceled) return;
    const valid = result.assets
      .filter((asset) => (asset.size ?? 0) <= 5 * 1024 * 1024)
      .slice(0, 3)
      .map((asset) => ({
        id: `attachment-${Date.now()}-${asset.name}`,
        name: asset.name,
        mimeType: asset.mimeType ?? "application/octet-stream",
        size: asset.size ?? 0,
        localUri: asset.uri,
      }));
    setAttachments(valid);
  };
  const submit = handleSubmit(async (values) => {
    const timestamp = new Date(
      `${values.date}T${values.proposedTime}:00-03:00`,
    ).toISOString();
    await createRequest.mutateAsync({
      employeeId: employee.id,
      date: values.date,
      reason: values.reason,
      justification: values.justification,
      proposedEntries: [{ type: values.proposedType, timestamp }],
      attachments,
    });
    reset({
      date: initialDate,
      reason: "FORGOT_ENTRY",
      justification: "",
      selectedEntryId: "",
      proposedType: "CLOCK_IN",
      proposedTime: "08:00",
    });
    setAttachments([]);
  });
  return (
    <Screen contentStyle={styles.content}>
      <AppHeader title="Correções de ponto" back />
      <Text style={[styles.heading, { color: colors.text }]}>
        Nova solicitação
      </Text>
      <View style={styles.form}>
        <Controller
          control={control}
          name="date"
          render={({ field: { onChange, onBlur, value } }) => (
            <AppInput
              label="Data"
              value={value}
              onChangeText={onChange}
              onBlur={onBlur}
              error={errors.date?.message}
              placeholder="AAAA-MM-DD"
              maxLength={10}
            />
          )}
        />
        <Controller
          control={control}
          name="selectedEntryId"
          render={({ field: { onChange, value } }) => (
            <ChoiceGroup
              label="Registro relacionado"
              value={value}
              onChange={onChange}
              choices={[
                { value: "", label: "Novo registro" },
                ...dateEntries.map((entry) => ({
                  value: entry.id,
                  label: formatTime(new Date(entry.timestamp)),
                })),
              ]}
            />
          )}
        />
        <Controller
          control={control}
          name="reason"
          render={({ field: { onChange, value } }) => (
            <ChoiceGroup
              label="Motivo"
              value={value}
              onChange={onChange}
              choices={reasons}
            />
          )}
        />
        <Controller
          control={control}
          name="proposedType"
          render={({ field: { onChange, value } }) => (
            <ChoiceGroup
              label="Tipo proposto"
              value={value}
              onChange={onChange}
              choices={[
                { value: "CLOCK_IN", label: "Entrada" },
                { value: "BREAK_START", label: "Início do intervalo" },
                { value: "BREAK_END", label: "Retorno do intervalo" },
                { value: "CLOCK_OUT", label: "Saída" },
              ]}
            />
          )}
        />
        <Controller
          control={control}
          name="proposedTime"
          render={({ field: { onChange, onBlur, value } }) => (
            <AppInput
              label="Horário correto"
              value={value}
              onChangeText={onChange}
              onBlur={onBlur}
              error={errors.proposedTime?.message}
              placeholder="HH:mm"
              maxLength={5}
            />
          )}
        />
        <Controller
          control={control}
          name="justification"
          render={({ field: { onChange, onBlur, value } }) => (
            <AppInput
              label="Justificativa"
              value={value}
              onChangeText={onChange}
              onBlur={onBlur}
              error={errors.justification?.message}
              placeholder="Descreva o que precisa ser corrigido"
              multiline
              maxLength={500}
            />
          )}
        />
        <SecondaryButton
          label="Selecionar anexos"
          onPress={() => void chooseAttachment()}
        />
        {attachments.map((attachment) => (
          <View
            key={attachment.id}
            style={[
              styles.attachment,
              { backgroundColor: colors.surfaceSecondary },
            ]}
          >
            <Text
              numberOfLines={1}
              style={[styles.attachmentName, { color: colors.text }]}
            >
              {attachment.name}
            </Text>
            <Text
              style={[styles.attachmentSize, { color: colors.textSecondary }]}
            >
              {Math.ceil(attachment.size / 1024)} KB
            </Text>
          </View>
        ))}
        <Text style={[styles.helper, { color: colors.textSecondary }]}>
          Até 3 imagens ou PDFs de 5 MB. Os arquivos permanecem locais nesta
          versão.
        </Text>
        <PrimaryButton
          label="Enviar solicitação"
          loading={createRequest.isPending}
          onPress={() => void submit()}
        />
      </View>
      <Text style={[styles.heading, { color: colors.text }]}>
        Minhas solicitações
      </Text>
      {!requests.data?.length ? (
        <Text style={[styles.empty, { color: colors.textSecondary }]}>
          Nenhuma solicitação enviada.
        </Text>
      ) : (
        requests.data.map((request) => (
          <Pressable
            key={request.id}
            onPress={() =>
              router.push({
                pathname: "/correction-detail",
                params: { id: request.id },
              })
            }
            style={[
              styles.card,
              { backgroundColor: colors.surface, borderColor: colors.border },
            ]}
          >
            <View style={styles.cardHeader}>
              <Text style={[styles.cardDate, { color: colors.text }]}>
                {request.date}
              </Text>
              <Text style={[styles.cardStatus, { color: colors.primary }]}>
                {statusLabels[request.status]}
              </Text>
            </View>
            <Text style={[styles.cardText, { color: colors.textSecondary }]}>
              {request.justification}
            </Text>
          </Pressable>
        ))
      )}
    </Screen>
  );
}

function ChoiceGroup<T extends string>({
  label,
  value,
  onChange,
  choices,
}: {
  label: string;
  value: T;
  onChange: (value: T) => void;
  choices: { value: T; label: string }[];
}) {
  const { colors } = useAppTheme();
  return (
    <View style={styles.reasonGroup}>
      <Text style={[styles.label, { color: colors.text }]}>{label}</Text>
      <View style={styles.chips}>
        {choices.map((choice) => {
          const selected = value === choice.value;
          return (
            <Pressable
              key={`${label}-${choice.value || "empty"}`}
              accessibilityRole="radio"
              accessibilityState={{ selected }}
              onPress={() => onChange(choice.value)}
              style={[
                styles.chip,
                {
                  backgroundColor: selected ? colors.primary : colors.surface,
                  borderColor: selected ? colors.primary : colors.border,
                },
              ]}
            >
              <Text
                style={{
                  color: selected ? colors.onPrimary : colors.text,
                  fontWeight: "600",
                }}
              >
                {choice.label}
              </Text>
            </Pressable>
          );
        })}
      </View>
    </View>
  );
}

const styles = StyleSheet.create({
  content: { paddingBottom: 32 },
  heading: { fontSize: 18, fontWeight: "800", marginTop: 24, marginBottom: 14 },
  form: { gap: 18 },
  reasonGroup: { gap: 8 },
  label: { fontSize: 14, fontWeight: "600" },
  chips: { flexDirection: "row", flexWrap: "wrap", gap: 8 },
  chip: {
    minHeight: 42,
    borderWidth: 1,
    borderRadius: 21,
    paddingHorizontal: 14,
    alignItems: "center",
    justifyContent: "center",
  },
  helper: { fontSize: 12, lineHeight: 18 },
  attachment: {
    borderRadius: 8,
    padding: 12,
    flexDirection: "row",
    justifyContent: "space-between",
    gap: 12,
  },
  attachmentName: { flex: 1, fontSize: 13, fontWeight: "600" },
  attachmentSize: { fontSize: 12 },
  empty: { fontSize: 14, paddingVertical: 16 },
  card: { borderWidth: 1, borderRadius: 8, padding: 14, marginBottom: 10 },
  cardHeader: {
    flexDirection: "row",
    justifyContent: "space-between",
    gap: 12,
  },
  cardDate: { fontSize: 14, fontWeight: "700" },
  cardStatus: { fontSize: 12, fontWeight: "700" },
  cardText: { fontSize: 13, lineHeight: 19, marginTop: 8 },
});
