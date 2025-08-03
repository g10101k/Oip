export interface EditModuleInstanceDto {
  moduleInstanceId: number,
  moduleId: number;
  parentId: number;
  label: string;
  icon: string;
  viewRoles: string[];
}
