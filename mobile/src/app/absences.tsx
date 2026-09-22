import * as DocumentPicker from "expo-document-picker";
import { zodResolver } from "@hookform/resolvers/zod";
import { Controller, useForm } from "react-hook-form";
import { useState } from "react";
import { Pressable, StyleSheet, Text, View } from "react-native";
import { AppHeader } from "@/components/common/app-header";
import { AppInput } from "@/components/common/app-input";
import {
  PrimaryButton,
  SecondaryButton,
  TextButton,
} from "@/components/common/buttons";
import { Screen } from "@/components/common/screen";
import {
  useAbsences,
  useCancelAbsence,
  useCreateAbsence,
} from "@/features/absences/hooks/use-absences";
import {
  AbsenceForm,
  absenceSchema,
} from "@/features/absences/schemas/absence-schema";
import { AttachmentReference } from "@/features/corrections/types/correction-request";
import { demoEmployee } from "@/features/profile/fixtures/demo-employee";
import { useSessionStore } from "@/stores/session-store";
import { useAppTheme } from "@/theme/theme-provider";

const types = [
  { value: "VACATION" as const, label: "Férias" },
  { value: "ABSENCE" as const, label: "Falta" },
  { value: "MEDICAL_LEAVE" as const, label: "Atestado" },
];

export default function AbsencesScreen() {
  const { colors } = useAppTheme();
  const employee = useSessionStore((state) => state.user) ?? demoEmployee;
  const requests = useAbsences(employee.id);
  const create = useCreateAbsence(employee.id);
  const cancel = useCancelAbsence(employee.id);
  const [attachments, setAttachments] = useState<AttachmentReference[]>([]);
  const {
    control,
    handleSubmit,
    reset,
    formState: { errors },
  } = useForm<AbsenceForm>({
    resolver: zodResolver(absenceSchema),
    defaultValues: {
      type: "VACATION",
      startDate: "2026-10-01",
      endDate: "2026-10-01",
      reason: "",
    },
  });
  const chooseFile = async () => {
    const result = await DocumentPicker.getDocumentAsync({
      type: ["application/pdf", "image/*"],
      copyToCacheDirectory: true,
    });
    if (result.canceled) return;
    const asset = result.assets[0];
    if ((asset.size ?? 0) > 5 * 1024 * 1024) return;
    setAttachments([
      {
        id: `absence-file-${Date.now()}`,
        name: asset.name,
        mimeType: asset.mimeType ?? "application/octet-stream",
        size: asset.size ?? 0,
        localUri: asset.uri,
      },
    ]);
  };
  const submit = handleSubmit(async (values) => {
    await create.mutateAsync({
      ...values,
      employeeId: employee.id,
      attachments,
    });
    reset({
      type: "VACATION",
      startDate: "2026-10-01",
      endDate: "2026-10-01",
      reason: "",
    });
    setAttachments([]);
  });
  return (
    <Screen contentStyle={styles.content}>
      <AppHeader title="Férias e ausências" back />
      <Text style={[styles.heading, { color: colors.text }]}>
        Nova solicitação
      </Text>
      <Controller
        control={control}
        name="type"
        render={({ field: { value, onChange } }) => (
          <View style={styles.chips}>
            {types.map((type) => (
              <Pressable
                key={type.value}
                onPress={() => onChange(type.value)}
                style={[
                  styles.chip,
                  {
                    backgroundColor:
                      value === type.value ? colors.primary : colors.surface,
                    borderColor:
                      value === type.value ? colors.primary : colors.border,
                  },
                ]}
              >
                <Text
                  style={{
                    color:
                      value === type.value ? colors.onPrimary : colors.text,
                    fontWeight: "600",
                  }}
                >
                  {type.label}
                </Text>
              </Pressable>
            ))}
          </View>
        )}
      />
      <View style={styles.form}>
        <Controller
          control={control}
          name="startDate"
          render={({ field: { value, onChange, onBlur } }) => (
            <AppInput
              label="Data inicial"
              value={value}
              onChangeText={onChange}
              onBlur={onBlur}
              error={errors.startDate?.message}
              placeholder="AAAA-MM-DD"
              maxLength={10}
            />
          )}
        />
        <Controller
          control={control}
          name="endDate"
          render={({ field: { value, onChange, onBlur } }) => (
            <AppInput
              label="Data final"
              value={value}
              onChangeText={onChange}
              onBlur={onBlur}
              error={errors.endDate?.message}
              placeholder="AAAA-MM-DD"
              maxLength={10}
            />
          )}
        />
        <Controller
          control={control}
          name="reason"
          render={({ field: { value, onChange, onBlur } }) => (
            <AppInput
              label="Justificativa"
              value={value}
              onChangeText={onChange}
              onBlur={onBlur}
              error={errors.reason?.message}
              multiline
              maxLength={500}
            />
          )}
        />
        <SecondaryButton
          label="Selecionar atestado ou comprovante"
          onPress={() => void chooseFile()}
        />
        {attachments.map((file) => (
          <Text
            key={file.id}
            style={[styles.file, { color: colors.textSecondary }]}
          >
            {file.name} · {Math.ceil(file.size / 1024)} KB
          </Text>
        ))}
        <PrimaryButton
          label="Enviar solicitação"
          loading={create.isPending}
          onPress={() => void submit()}
        />
      </View>
      <Text style={[styles.heading, { color: colors.text }]}>Solicitações</Text>
      {!requests.data?.length ? (
        <Text style={[styles.empty, { color: colors.textSecondary }]}>
          Nenhuma solicitação registrada.
        </Text>
      ) : (
        requests.data.map((request) => (
          <View
            key={request.id}
            style={[
              styles.card,
              { backgroundColor: colors.surface, borderColor: colors.border },
            ]}
          >
            <View style={styles.cardHeader}>
              <Text style={[styles.cardTitle, { color: colors.text }]}>
                {request.type} · {request.startDate} a {request.endDate}
              </Text>
              <Text style={[styles.status, { color: colors.primary }]}>
                {request.status}
              </Text>
            </View>
            <Text style={[styles.cardText, { color: colors.textSecondary }]}>
              {request.reason}
            </Text>
            {request.status === "PENDING" ? (
              <TextButton
                label="Cancelar"
                onPress={() => cancel.mutate(request.id)}
              />
            ) : null}
          </View>
        ))
      )}
    </Screen>
  );
}

const styles = StyleSheet.create({
  content: { paddingBottom: 32 },
  heading: { fontSize: 18, fontWeight: "800", marginTop: 24, marginBottom: 14 },
  chips: { flexDirection: "row", flexWrap: "wrap", gap: 8 },
  chip: {
    minHeight: 42,
    borderWidth: 1,
    borderRadius: 21,
    paddingHorizontal: 16,
    justifyContent: "center",
  },
  form: { gap: 16, marginTop: 18 },
  file: { fontSize: 13 },
  empty: { fontSize: 14, paddingVertical: 16 },
  card: { borderWidth: 1, borderRadius: 9, padding: 14, marginBottom: 10 },
  cardHeader: {
    flexDirection: "row",
    justifyContent: "space-between",
    gap: 10,
  },
  cardTitle: { flex: 1, fontSize: 13, fontWeight: "700" },
  status: { fontSize: 11, fontWeight: "800" },
  cardText: { fontSize: 13, lineHeight: 19, marginTop: 8 },
});
