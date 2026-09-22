export const publicRoutes = ["/", "/login", "/forgot-password"];

export const resolveRouteRedirect = (
  pathname: string,
  authenticated: boolean,
) => {
  const isPublic = publicRoutes.includes(pathname);
  if (!authenticated && !isPublic) return "/login";
  if (authenticated && (pathname === "/login" || pathname === "/"))
    return "/(tabs)/home";
  return null;
};
