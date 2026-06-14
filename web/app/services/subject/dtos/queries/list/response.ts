/** Subject list row (mirrors backend SubjectDto). */
export interface Subject {
    publicId: string;
    name: string;
    description: string | null;
    isActive: boolean;
    displayOrder: number;
    createdAt: string;
    updatedAt: string;
}
