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
        route("forgot-password", "features/auth/forgot-password/forgot-password.page.tsx"),
        route("reset-password", "features/auth/reset-password/reset-password.page.tsx"),
    ]),

    // Role dashboards — each layout guards by role and renders the shared shell.
    layout("layouts/admin.layout.tsx", [
        ...prefix("admin", [
            index("features/admin/dashboard/dashboard.page.tsx"),
            route("subjects", "features/admin/subjects/subjects.page.tsx"),
            route("users", "features/admin/users/list/users.page.tsx"),
            route("users/new", "features/admin/users/create/user-create.page.tsx"),
            route("users/:publicId", "features/admin/users/edit/user-edit.page.tsx"),
            route("classrooms", "features/admin/classrooms/classrooms.page.tsx"),
            route("questions", "features/admin/questions/questions.page.tsx"),
            route("exams", "features/admin/exams/exams.page.tsx"),
            route("assignments", "features/admin/assignments/assignments.page.tsx"),
            route("audit-logs", "features/admin/audit-logs/audit-logs.page.tsx"),
            route("reports", "features/admin/reports/reports.page.tsx"),
            route("settings", "features/admin/settings/settings.page.tsx"),
        ]),
    ]),
    layout("layouts/teacher.layout.tsx", [
        ...prefix("teacher", [
            index("features/teacher/dashboard/dashboard.page.tsx"),
            route("classrooms", "features/teacher/classrooms/list/classrooms.page.tsx"),
            route("classrooms/new", "features/teacher/classrooms/create/classroom-create.page.tsx"),
            route(
                "classrooms/:publicId",
                "features/teacher/classrooms/edit/classroom-edit.page.tsx",
            ),
            route(
                "classrooms/:publicId/members",
                "features/teacher/classrooms/members/class-members.page.tsx",
            ),
            route("question-bank", "features/teacher/questions/list/questions.page.tsx"),
            route(
                "question-bank/new",
                "features/teacher/questions/create/question-create.page.tsx",
            ),
            route(
                "question-bank/public",
                "features/teacher/questions/public/public-questions.page.tsx",
            ),
            route(
                "question-bank/:publicId",
                "features/teacher/questions/edit/question-edit.page.tsx",
            ),
            route("exams", "features/teacher/exams/list/exams.page.tsx"),
            route("exams/new", "features/teacher/exams/create/exam-create.page.tsx"),
            route("exams/public", "features/teacher/exams/public/public-exams.page.tsx"),
            route("exams/:publicId", "features/teacher/exams/edit/exam-edit.page.tsx"),
            route("assignments", "features/teacher/assignments/list/assignments.page.tsx"),
            route(
                "assignments/new",
                "features/teacher/assignments/create/assignment-create.page.tsx",
            ),
            route(
                "assignments/:publicId",
                "features/teacher/assignments/edit/assignment-edit.page.tsx",
            ),
            route(
                "assignments/:publicId/report",
                "features/teacher/assignments/report/report.page.tsx",
            ),
            route(
                "assignments/:publicId/attempts/:attemptId/grade",
                "features/teacher/assignments/grade/grade-attempt.page.tsx",
            ),
        ]),
    ]),
    layout("layouts/student.layout.tsx", [
        ...prefix("student", [
            index("features/student/dashboard/dashboard.page.tsx"),
            route("classes", "features/student/classes/list/my-classes.page.tsx"),
            route("classes/join", "features/student/classes/join/join-class.page.tsx"),
            route("classes/requests", "features/student/classes/requests/requests.page.tsx"),
            route(
                "classes/:publicId/members",
                "features/student/classes/members/class-members.page.tsx",
            ),
            route("assignments", "features/student/assignments/list/assignments.page.tsx"),
            route(
                "assignments/:publicId",
                "features/student/assignments/detail/assignment-detail.page.tsx",
            ),
            route(
                "assignments/attempts/:attemptId",
                "features/student/assignments/attempt/attempt.page.tsx",
            ),
        ]),
    ]),
] satisfies RouteConfig;
