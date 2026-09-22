import * as Location from "expo-location";
import { AttendanceLocation } from "../types/time-entry";

const MAXIMUM_ACCURACY_METERS = 100;

export const locationService = {
  async capture(): Promise<AttendanceLocation> {
    if (!(await Location.hasServicesEnabledAsync()))
      throw new Error("LOCATION_DISABLED");

    const permission = await Location.requestForegroundPermissionsAsync();
    if (!permission.granted) throw new Error("LOCATION_PERMISSION_DENIED");

    const position = await Location.getCurrentPositionAsync({
      accuracy: Location.Accuracy.High,
    });
    if (position.mocked) throw new Error("LOCATION_MOCKED");
    if (
      position.coords.accuracy == null ||
      position.coords.accuracy > MAXIMUM_ACCURACY_METERS
    )
      throw new Error("LOCATION_INACCURATE");

    return {
      latitude: position.coords.latitude,
      longitude: position.coords.longitude,
      accuracyMeters: position.coords.accuracy,
      capturedAt: new Date(position.timestamp).toISOString(),
      mocked: Boolean(position.mocked),
    };
  },
};
