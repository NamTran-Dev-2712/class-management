import {
    Award,
    BarChart3,
    BookOpen,
    ClipboardCheck,
    ClipboardList,
    CreditCard,
    FileQuestion,
    FileText,
    GraduationCap,
    LayoutDashboard,
    Library,
    School,
    Users,
    type LucideIcon,
} from "lucide-react";

import { Roles, type Role } from "@/config/roles";

export interface NavItem {
    /** Translation key in the `dashboard` namespace. */
    labelKey: string;
    to: string;
    icon: LucideIcon;
    /** Not yet routable — shown muted, not clickable. */
    disabled?: boolean;
}

/**
 * Sidebar navigation per role. Only the dashboard route exists today; the rest are
 * placeholders (disabled) that mark where each module will plug in.
 */
export const dashboardNav: Record<Role, NavItem[]> = {
    [Roles.Admin]: [
        { labelKey: "nav.dashboard", to: "/admin", icon: LayoutDashboard },
        { labelKey: "nav.subjects", to: "/admin/subjects", icon: Library },
        { labelKey: "nav.users", to: "/admin/users", icon: Users },
        { labelKey: "nav.classrooms", to: "/admin/classrooms", icon: School },
        { labelKey: "nav.payments", to: "/admin/payments", icon: CreditCard, disabled: true },
        { labelKey: "nav.reports", to: "/admin/reports", icon: BarChart3, disabled: true },
    ],
    [Roles.Teacher]: [
        { labelKey: "nav.dashboard", to: "/teacher", icon: LayoutDashboard },
        { labelKey: "nav.classrooms", to: "/teacher/classrooms", icon: School },
        {
            labelKey: "nav.questionBank",
            to: "/teacher/question-bank",
            icon: FileQuestion,
            disabled: true,
        },
        { labelKey: "nav.exams", to: "/teacher/exams", icon: FileText, disabled: true },
        {
            labelKey: "nav.assignments",
            to: "/teacher/assignments",
            icon: ClipboardList,
            disabled: true,
        },
    ],
    [Roles.Student]: [
        { labelKey: "nav.dashboard", to: "/student", icon: LayoutDashboard },
        { labelKey: "nav.myClasses", to: "/student/classes", icon: BookOpen },
        { labelKey: "nav.requests", to: "/student/classes/requests", icon: ClipboardCheck },
        { labelKey: "nav.exams", to: "/student/exams", icon: FileText, disabled: true },
        {
            labelKey: "nav.assignments",
            to: "/student/assignments",
            icon: ClipboardList,
            disabled: true,
        },
        { labelKey: "nav.grades", to: "/student/grades", icon: Award, disabled: true },
    ],
};

/** Brand icon used in the sidebar/header. */
export const brandIcon: LucideIcon = GraduationCap;

export interface MarketingNavItem {
    /** Translation key in the `public` namespace. */
    labelKey: string;
    to: string;
}

/** Top-level navigation for the public marketing site. */
export const marketingNav: MarketingNavItem[] = [
    { labelKey: "nav.home", to: "/" },
    { labelKey: "nav.about", to: "/about" },
    { labelKey: "nav.pricing", to: "/pricing" },
    { labelKey: "nav.contact", to: "/contact" },
];
