export enum DemoCustomerStatus {
  Prospect = "Prospect",
  Active = "Active",
  Suspended = "Suspended",
}

export interface AccessObjectTypeDto {
  code?: string | null;
  rights?: AccessRightDto[] | null;
}

export interface AccessRightDto {
  code?: string | null;
  name?: string | null;
}

export interface AclEntryDto {
  subjectKey?: string | null;
  subjectType?: string | null;
  displayName?: string | null;
  isKnown?: boolean;
  allow?: string[] | null;
  deny?: string[] | null;
}

export interface AclEntryUpdateDto {
  subjectKey: string;
  allow?: string[] | null;
  deny?: string[] | null;
}

export interface ApiExceptionResponse {
  title?: string | null;
  message?: string | null;
  statusCode?: number;
  stackTrace?: string | null;
}

export interface ApplyMigrationRequest {
  name?: string | null;
}

export type CustomerModuleSettings = object;

export interface DashboardSettings {
  nothing?: string | null;
}

export interface DemoCustomerCategoryDto {
  id?: number;
  name?: string | null;
  rights?: string[] | null;
}

export interface DemoCustomerTableRowDto {
  id?: number;
  fullName?: string | null;
  email?: string | null;
  category?: string | null;
  country?: string | null;
  status?: DemoCustomerStatus;
  isActive?: boolean;
  creditScore?: number;
  lifetimeValue?: number;
  createdAt?: Date;
  ordersCount?: number;
  rights?: string[] | null;
}

export interface DemoCustomerTableRowDtoTablePageResult {
  data?: DemoCustomerTableRowDto[] | null;
  total?: number;
  first?: number;
  rows?: number;
}

export interface MigrationDto {
  name?: string | null;
  applied?: boolean;
  pending?: boolean;
  exist?: boolean;
}

export interface ObjectAclDto {
  objectType?: string | null;
  objectId?: number;
  parentType?: string | null;
  parentId?: number | null;
  inheritanceBroken?: boolean;
  rights?: AccessRightDto[] | null;
  entries?: AclEntryDto[] | null;
  inheritedEntries?: AclEntryDto[] | null;
}

export interface SaveDemoCustomerRequest {
  fullName: string;
  email: string;
  category: string;
  country: string;
  status: DemoCustomerStatus;
  creditScore?: number;
  lifetimeValue?: number;
}

export interface SubjectDto {
  subjectKey?: string | null;
  subjectType?: string | null;
  displayName?: string | null;
  isKnown?: boolean;
}

export interface TableQueryRequest {
  first?: number;
  rows?: number;
  sortField?: string | null;
  sortOrder?: number;
  globalFilter?: string | null;
  filters?: Record<string, any>;
}

export interface UpdateObjectAclRequest {
  objectType: string;
  objectId?: number;
  inheritanceBroken?: boolean;
  entries?: AclEntryUpdateDto[] | null;
}

export interface WeatherForecastResponse {
  date?: Date;
  temperatureC?: number;
  temperatureF?: number;
  summary?: string | null;
}

export interface WeatherModuleSettings {
  dayCount?: number;
}

export interface GetObjectAclByObjectParams {
  objectType?: string;
  objectId?: number;
}

export interface GetEffectiveRightsByObjectParams {
  objectType?: string;
  objectId?: number;
}

export interface GetSubjectsByFilterParams {
  objectType?: string;
  objectId?: number;
  filter?: string;
}

export interface UpdateParams {
  id: number;
}

export interface DeleteParams {
  id: number;
}

export interface GetWeatherForecastParams {
  dayCount?: number;
}
