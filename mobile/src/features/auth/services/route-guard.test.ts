import { describe, expect, it } from "vitest";
import { resolveRouteRedirect } from "./route-guard";

describe("resolveRouteRedirect", () => {
  it("protege uma rota privada", () => {
    expect(resolveRouteRedirect("/(tabs)/home", false)).toBe("/login");
  });

  it("permite recuperação de senha sem sessão", () => {
    expect(resolveRouteRedirect("/forgot-password", false)).toBeNull();
  });

  it("envia usuário autenticado para o início", () => {
    expect(resolveRouteRedirect("/login", true)).toBe("/(tabs)/home");
  });
});
