import { ArrowLeft, Menu, Share2 } from "lucide-react-native";
import { Pressable, StyleSheet, Text, View } from "react-native";
import { useRouter } from "expo-router";
import { useAppTheme } from "@/theme/theme-provider";
import { Brand } from "./brand";

type Props = {
  title?: string;
  back?: boolean;
  menu?: boolean;
  avatar?: string;
  share?: () => void;
};
export function AppHeader({ title, back, menu, avatar, share }: Props) {
  const { colors } = useAppTheme();
  const router = useRouter();
  return (
    <View style={[styles.header, { borderBottomColor: colors.border }]}>
      <View style={styles.side}>
        {back ? (
          <Pressable
            onPress={() => router.back()}
            accessibilityLabel="Voltar"
            style={styles.icon}
          >
            <ArrowLeft color={colors.text} size={24} />
          </Pressable>
        ) : menu ? (
          <Pressable accessibilityLabel="Abrir menu" style={styles.icon}>
            <Menu color={colors.text} size={24} />
          </Pressable>
        ) : null}
      </View>
      <View style={styles.center}>
        {title ? (
          <Text
            numberOfLines={1}
            style={[styles.title, { color: colors.text }]}
          >
            {title}
          </Text>
        ) : (
          <Brand size={22} />
        )}
      </View>
      <View style={[styles.side, styles.right]}>
        {share ? (
          <Pressable
            onPress={share}
            accessibilityLabel="Compartilhar comprovante"
            style={styles.icon}
          >
            <Share2 color={colors.text} size={22} />
          </Pressable>
        ) : avatar ? (
          <View
            style={[
              styles.avatar,
              { backgroundColor: colors.surfaceSecondary },
            ]}
          >
            <Text style={[styles.avatarText, { color: colors.text }]}>
              {avatar}
            </Text>
          </View>
        ) : null}
      </View>
    </View>
  );
}
const styles = StyleSheet.create({
  header: {
    height: 64,
    flexDirection: "row",
    alignItems: "center",
    borderBottomWidth: StyleSheet.hairlineWidth,
  },
  side: { width: 56, alignItems: "flex-start" },
  right: { alignItems: "flex-end" },
  center: { flex: 1, alignItems: "flex-start" },
  icon: {
    width: 48,
    height: 48,
    alignItems: "center",
    justifyContent: "center",
  },
  title: { fontSize: 20, fontWeight: "700" },
  avatar: {
    width: 40,
    height: 40,
    borderRadius: 20,
    alignItems: "center",
    justifyContent: "center",
    marginRight: 8,
  },
  avatarText: { fontSize: 13, fontWeight: "700" },
});
