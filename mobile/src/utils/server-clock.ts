// Relógio oficial do ponto. A API devolve a hora de Brasília junto com o
// deslocamento do fuso (por exemplo "2026-10-05T21:55:09-03:00"). O app mede a
// diferença para o relógio do aparelho uma vez e passa a mostrar sempre a hora
// da API. Assim um aparelho com a hora errada, ou em outro fuso, não muda o
// horário que o funcionário vê nem o dia em que o ponto cai.

const DEFAULT_OFFSET_MINUTES = -180;

let differenceMs = 0;
let offsetMinutes = DEFAULT_OFFSET_MINUTES;

// "-03:00" vira -180. "Z" vira 0. Sem deslocamento no texto, devolve null.
export const parseOffsetMinutes = (iso: string): number | null => {
  if (/Z$/i.test(iso)) return 0;
  const match = /([+-])(\d{2}):?(\d{2})$/.exec(iso);
  if (!match) return null;
  const minutes = Number(match[2]) * 60 + Number(match[3]);
  return match[1] === "-" ? -minutes : minutes;
};

export const serverClock = {
  /** Guarda a diferença entre a hora da API e a do aparelho. */
  sync(serverNowIso: string, deviceNowMs: number = Date.now()) {
    const serverMs = Date.parse(serverNowIso);
    if (Number.isNaN(serverMs)) return;
    differenceMs = serverMs - deviceNowMs;
    const offset = parseOffsetMinutes(serverNowIso);
    if (offset !== null) offsetMinutes = offset;
  },
  /** Agora, pela hora oficial. Antes da primeira sincronização usa o aparelho. */
  now(): Date {
    return new Date(Date.now() + differenceMs);
  },
  /** Deslocamento do fuso de Brasília em minutos, como a API informou. */
  offsetMinutes(): number {
    return offsetMinutes;
  },
  reset() {
    differenceMs = 0;
    offsetMinutes = DEFAULT_OFFSET_MINUTES;
  },
};
