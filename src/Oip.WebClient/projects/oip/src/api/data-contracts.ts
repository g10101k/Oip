export enum DemoCustomerStatus {
  Prospect = "Prospect",
  Active = "Active",
  Suspended = "Suspended",
}

export interface ApiExceptionResponse {
  title?: string | null;
  message?: string | null;
  statusCode?: number;
  stackTrace?: string | null;
}

export type CustomerModuleSettings = object;

export interface DashboardSettings {
  nothing?: string | null;
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
}

export interface DemoCustomerTableRowDtoTablePageResult {
  data?: DemoCustomerTableRowDto[] | null;
  total?: number;
  first?: number;
  rows?: number;
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

export interface TableQueryRequest {
  first?: number;
  rows?: number;
  sortField?: string | null;
  sortOrder?: number;
  globalFilter?: string | null;
  filters?: Record<string, any> | null;
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

export interface UpdateParams {
  id: number;
}

export interface DeleteParams {
  id: number;
}

export interface GetModuleInstanceSettingsParams {
  id?: number;
}

export interface DashboardGetModuleInstanceSettingsParams {
  id?: number;
}

export interface GetWeatherForecastParams {
  dayCount?: number;
}

export interface GetModuleInstanceSettingsParams2 {
  id?: number;
}
