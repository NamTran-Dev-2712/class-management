import type { TFunction } from "i18next";

import { Badge } from "@/components/ui/badge";
import type { AssignmentStatus } from "@/services/assignment/dtos/queries/assignment-list";

const VARIANT: Record<AssignmentStatus, "default" | "secondary" | "outline" | "destructive"> = {
    Draft: "outline",
    Scheduled: "secondary",
    Open: "default",
    Closed: "secondary",
    Archived: "outline",
};

export function AssignmentStatusBadge({ status, t }: { status: AssignmentStatus; t: TFunction }) {
    return <Badge variant={VARIANT[status]}>{t(`status.${status}`)}</Badge>;
}
