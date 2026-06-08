import { type RouteConfig, index, route } from "@react-router/dev/routes";

export default [
    index("features/public/home/home.page.tsx"),
    route("login", "features/auth/login/login.page.tsx"),
] satisfies RouteConfig;
