export interface UpdateSubjectRequest {
    name: string;
    description: string | null;
    isActive: boolean;
    displayOrder: number;
}
