import { Mail, MapPin, Phone } from "lucide-react";
import { useTranslation } from "react-i18next";

import { PageHero } from "@/components/shared/marketing/page-hero";
import { Card, CardContent } from "@/components/ui/card";
import { ContactForm } from "./contact.form";
import type { Route } from "./+types/contact.page";

export function meta(_: Route.MetaArgs) {
    return [{ title: "Contact · Class Management" }];
}

const details = [
    { icon: Mail, key: "email" },
    { icon: Phone, key: "phone" },
    { icon: MapPin, key: "address" },
] as const;

export default function ContactPage() {
    const { t } = useTranslation("public");

    return (
        <>
            <PageHero title={t("contact.title")} subtitle={t("contact.subtitle")} />
            <section className="mx-auto grid max-w-6xl gap-8 px-4 py-16 md:grid-cols-2 md:px-6">
                <div className="space-y-4">
                    {details.map((d) => (
                        <Card key={d.key}>
                            <CardContent className="flex items-start gap-4">
                                <div className="bg-primary/10 text-primary flex size-10 shrink-0 items-center justify-center rounded-lg">
                                    <d.icon className="size-5" />
                                </div>
                                <div>
                                    <h3 className="font-medium">
                                        {t(`contact.info.${d.key}.label`)}
                                    </h3>
                                    <p className="text-muted-foreground text-sm">
                                        {t(`contact.info.${d.key}.value`)}
                                    </p>
                                </div>
                            </CardContent>
                        </Card>
                    ))}
                </div>
                <Card>
                    <CardContent>
                        <ContactForm />
                    </CardContent>
                </Card>
            </section>
        </>
    );
}
