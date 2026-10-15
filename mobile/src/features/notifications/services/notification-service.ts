import * as Notifications from "expo-notifications";
import { secureJson } from "@/services/storage/secure-json";

const KEY = "tech4hr.notification-identifiers";

const reminders = [
  {
    title: "Hora da entrada",
    body: "Lembre-se de registrar sua entrada.",
    hour: 8,
    minute: 0,
  },
  {
    title: "Fim do intervalo",
    body: "Lembre-se de registrar o retorno do intervalo.",
    hour: 13,
    minute: 0,
  },
  {
    title: "Fim da jornada",
    body: "Confira se sua saída foi registrada.",
    hour: 17,
    minute: 0,
  },
  {
    title: "Jornada incompleta",
    body: "Revise seus registros de hoje.",
    hour: 18,
    minute: 30,
  },
];

export const notificationService = {
  async enable() {
    const permission = await Notifications.requestPermissionsAsync();
    if (!permission.granted) throw new Error("NOTIFICATION_PERMISSION_DENIED");
    const previous = await secureJson.read<string[]>(KEY, []);
    await Promise.all(
      previous.map((id) =>
        Notifications.cancelScheduledNotificationAsync(id).catch(
          () => undefined,
        ),
      ),
    );
    const identifiers: string[] = [];
    for (const reminder of reminders) {
      for (let weekday = 2; weekday <= 6; weekday += 1) {
        const id = await Notifications.scheduleNotificationAsync({
          content: { title: reminder.title, body: reminder.body },
          trigger: {
            type: Notifications.SchedulableTriggerInputTypes.WEEKLY,
            weekday,
            hour: reminder.hour,
            minute: reminder.minute,
          },
        });
        identifiers.push(id);
      }
    }
    await secureJson.write(KEY, identifiers);
  },
  async disable() {
    const identifiers = await secureJson.read<string[]>(KEY, []);
    await Promise.all(
      identifiers.map((id) =>
        Notifications.cancelScheduledNotificationAsync(id).catch(
          () => undefined,
        ),
      ),
    );
    await secureJson.remove(KEY);
  },
};
