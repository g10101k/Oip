export interface AddModuleInstanceDto {
  moduleId: number;
  label: string;
  icon: string;
  parentId: number | null | undefined;
}
