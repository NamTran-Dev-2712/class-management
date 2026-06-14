import {
    Award,
    BookOpen,
    FileQuestion,
    FileText,
    GraduationCap,
    School,
    ShieldCheck,
} from "lucide-react";
import { useTranslation } from "react-i18next";
import { Link } from "react-router";

import { FeatureCard } from "@/components/shared/marketing/feature-card";
import { Button } from "@/components/ui/button";
import type { Route } from "./+types/home.page";

export function meta(_: Route.MetaArgs) {
    return [{ title: "Class Management" }];
}

const features = [
    { icon: School, key: "classrooms" },
    { icon: FileQuestion, key: "questionBank" },
    { icon: FileText, key: "exams" },
    { icon: Award, key: "grading" },
] as const;

const roles = [
    { icon: ShieldCheck, key: "admin" },
    { icon: GraduationCap, key: "teacher" },
    { icon: BookOpen, key: "student" },
] as const;

export default function HomePage() {
    const { t } = useTranslation("public");

    return (
        <>
            {/* Hero */}
            <section className="mx-auto flex max-w-3xl flex-col items-center gap-6 px-4 py-20 text-center md:py-28">
                <span className="bg-primary/10 text-primary rounded-full px-3 py-1 text-sm font-medium">
                    {t("home.badge")}
                </span>
                <h1 className="text-4xl font-bold tracking-tight sm:text-5xl">{t("home.title")}</h1>
                <p className="text-muted-foreground text-lg">{t("home.tagline")}</p>
                <div className="flex flex-wrap items-center justify-center gap-3">
                    <Button asChild size="lg">
                        <Link to="/register">{t("home.cta")}</Link>
                    </Button>
                    <Button asChild size="lg" variant="outline">
                        <Link to="/login">{t("home.signIn")}</Link>
                    </Button>
                </div>
            </section>

            {/* Features */}
            <section className="bg-muted/30 border-y">
                <div className="mx-auto max-w-6xl px-4 py-16 md:px-6">
                    <div className="mx-auto max-w-2xl text-center">
                        <h2 className="text-2xl font-bold tracking-tight sm:text-3xl">
                            {t("home.features.title")}
                        </h2>
                        <p className="text-muted-foreground mt-3">{t("home.features.subtitle")}</p>
                    </div>
                    <div className="mt-10 grid gap-4 sm:grid-cols-2 lg:grid-cols-4">
                        {features.map((f) => (
                            <FeatureCard
                                key={f.key}
                                icon={f.icon}
                                title={t(`home.features.${f.key}.title`)}
                                description={t(`home.features.${f.key}.desc`)}
                            />
                        ))}
                    </div>
                </div>
            </section>

            {/* Roles */}
            <section className="mx-auto max-w-6xl px-4 py-16 md:px-6">
                <div className="mx-auto max-w-2xl text-center">
                    <h2 className="text-2xl font-bold tracking-tight sm:text-3xl">
                        {t("home.roles.title")}
                    </h2>
                    <p className="text-muted-foreground mt-3">{t("home.roles.subtitle")}</p>
                </div>
                <div className="mt-10 grid gap-6 sm:grid-cols-3">
                    {roles.map((r) => (
                        <div key={r.key} className="flex flex-col items-center gap-3 text-center">
                            <div className="bg-primary/10 text-primary flex size-12 items-center justify-center rounded-full">
                                <r.icon className="size-6" />
                            </div>
                            <h3 className="font-semibold">{t(`home.roles.${r.key}.title`)}</h3>
                            <p className="text-muted-foreground text-sm">
                                {t(`home.roles.${r.key}.desc`)}
                            </p>
                        </div>
                    ))}
                </div>
            </section>

            {/* CTA band */}
            <section className="border-t">
                <div className="bg-primary text-primary-foreground mx-auto flex max-w-6xl flex-col items-center gap-4 rounded-none px-4 py-14 text-center md:px-6">
                    <h2 className="text-2xl font-bold tracking-tight sm:text-3xl">
                        {t("home.ctaBand.title")}
                    </h2>
                    <p className="opacity-90">{t("home.ctaBand.subtitle")}</p>
                    <Button asChild size="lg" variant="secondary">
                        <Link to="/register">{t("home.cta")}</Link>
                    </Button>
                </div>
            </section>
        </>
    );
}
