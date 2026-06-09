import { type RouteConfig, index, layout, prefix, route } from "@react-router/dev/routes";

export default [
    // Public marketing pages (header + footer shell).
    layout("layouts/public.layout.tsx", [
        index("features/public/home/home.page.tsx"),
        route("about", "features/public/about/about.page.tsx"),
        route("pricing", "features/public/pricing/pricing.page.tsx"),
        route("contact", "features/public/contact/contact.page.tsx"),
        route("unauthorized", "features/errors/unauthorized/unauthorized.page.tsx"),
    ]),

    // Auth pages — minimal layout; redirects signed-in users to their dashboard.
    layout("layouts/auth.layout.tsx", [
        route("login", "features/auth/login/login.page.tsx"),
        route("register", "features/auth/register/register.page.tsx"),
    ]),

    // Role dashboards — each layout guards by role and renders the shared shell.
    layout("layouts/admin.layout.tsx", [
        ...prefix("admin", [index("features/admin/dashboard/dashboard.page.tsx")]),
    ]),
    layout("layouts/teacher.layout.tsx", [
        ...prefix("teacher", [index("features/teacher/dashboard/dashboard.page.tsx")]),
    ]),
    layout("layouts/student.layout.tsx", [
        ...prefix("student", [index("features/student/dashboard/dashboard.page.tsx")]),
    ]),
] satisfies RouteConfig;
