import { Github, Linkedin, Twitter } from "lucide-react";
import { useTranslation } from "react-i18next";
import { Link } from "react-router";

import { brandIcon as BrandIcon } from "@/config/nav";
import { useAppName } from "@/hooks/use-app-name";

const socials = [
    { icon: Github, href: "#", key: "github" },
    { icon: Twitter, href: "#", key: "twitter" },
    { icon: Linkedin, href: "#", key: "linkedin" },
];

const productLinks = [
    { labelKey: "nav.home", to: "/" },
    { labelKey: "nav.pricing", to: "/pricing" },
];

const companyLinks = [
    { labelKey: "nav.about", to: "/about" },
    { labelKey: "nav.contact", to: "/contact" },
];

export function PublicFooter() {
    const { t } = useTranslation(["public", "common"]);
    const appName = useAppName();

    return (
        <footer className="bg-muted/30 border-t">
            <div className="mx-auto grid max-w-6xl gap-8 px-4 py-10 md:grid-cols-4 md:px-6">
                <div className="md:col-span-2">
                    <Link to="/" className="flex items-center gap-2 font-semibold">
                        <BrandIcon className="text-primary size-6" />
                        <span>{appName}</span>
                    </Link>
                    <p className="text-muted-foreground mt-3 max-w-sm text-sm">
                        {t("tagline", { ns: "common" })}
                    </p>
                    <div className="mt-4 flex gap-3">
                        {socials.map((s) => (
                            <a
                                key={s.key}
                                href={s.href}
                                aria-label={t(`footer.social.${s.key}`)}
                                className="text-muted-foreground hover:text-foreground transition-colors"
                            >
                                <s.icon className="size-5" />
                            </a>
                        ))}
                    </div>
                </div>

                <div>
                    <h3 className="text-sm font-semibold">{t("footer.product")}</h3>
                    <ul className="mt-3 space-y-2 text-sm">
                        {productLinks.map((l) => (
                            <li key={l.to}>
                                <Link
                                    to={l.to}
                                    className="text-muted-foreground hover:text-foreground transition-colors"
                                >
                                    {t(l.labelKey)}
                                </Link>
                            </li>
                        ))}
                    </ul>
                </div>

                <div>
                    <h3 className="text-sm font-semibold">{t("footer.company")}</h3>
                    <ul className="mt-3 space-y-2 text-sm">
                        {companyLinks.map((l) => (
                            <li key={l.to}>
                                <Link
                                    to={l.to}
                                    className="text-muted-foreground hover:text-foreground transition-colors"
                                >
                                    {t(l.labelKey)}
                                </Link>
                            </li>
                        ))}
                    </ul>
                </div>
            </div>

            <div className="border-t">
                <div className="text-muted-foreground mx-auto max-w-6xl px-4 py-4 text-center text-sm md:px-6">
                    © {new Date().getFullYear()} {appName}. {t("footer.rights")}
                </div>
            </div>
        </footer>
    );
}
