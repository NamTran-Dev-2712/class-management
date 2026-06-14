export interface CreateSubjectRequest {
    name: string;
    description: string | null;
    isActive: boolean;
    displayOrder: number;
}
