interface PageHeroProps {
    title: string;
    subtitle?: string;
}

export function PageHero({ title, subtitle }: PageHeroProps) {
    return (
        <section className="bg-muted/30 border-b">
            <div className="mx-auto max-w-3xl px-4 py-16 text-center md:px-6">
                <h1 className="text-3xl font-bold tracking-tight sm:text-4xl">{title}</h1>
                {subtitle ? <p className="text-muted-foreground mt-4 text-lg">{subtitle}</p> : null}
            </div>
        </section>
    );
}
