import { apiRequest } from "@/services/api/http-client";
import { serverClock } from "@/utils/server-clock";
import { AttendanceRepository } from "./attendance-repository";
import {
  Tech4hrDayRecord,
  Tech4hrRegistration,
  Tech4hrRegistrationType,
  appToTech4hrType,
  dayRecordToEntries,
  registrationToEntry,
  sortNewestFirst,
} from "./tech4hr-mappers";

// Resposta de GET /api/pontos/hoje.
type Tech4hrToday = {
  dataReferencia: string;
  agora: string;
  fusoHorario: string;
  ponto: Tech4hrDayRecord | null;
  proximoTipoRegistro: Tech4hrRegistrationType | null;
};

// Repositório que fala com a API do Tech4Hr (a mesma do site e do banco que
// já existe). Quem decide a ordem das batidas, o dia e a hora é a API.
export const tech4hrAttendanceRepository: AttendanceRepository = {
  async list() {
    const [days, today] = await Promise.all([
      apiRequest<Tech4hrDayRecord[]>("/api/pontos/meus-pontos"),
      apiRequest<Tech4hrToday>("/api/pontos/hoje"),
    ]);

    // A hora oficial vem junto: o app passa a mostrar a hora de Brasília da
    // API, não a do aparelho.
    serverClock.sync(today.agora);

    return sortNewestFirst(
      days.flatMap((day) =>
        dayRecordToEntries(day, serverClock.offsetMinutes()),
      ),
    );
  },

  async create(type, location) {
    const response = await apiRequest<Tech4hrRegistration>(
      "/api/pontos/registrar",
      {
        method: "POST",
        body: JSON.stringify({ tipoRegistro: appToTech4hrType[type] }),
      },
    );
    return registrationToEntry(response, location);
  },
};
