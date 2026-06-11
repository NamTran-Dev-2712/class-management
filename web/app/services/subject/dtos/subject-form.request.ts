export interface SubjectFormRequest {
    name: string;
    description?: string | null;
    isActive: boolean;
    displayOrder: number;
}
