export enum ReportSummaryOperation {
  Count = "Count",
  Sum = "Sum",
  Average = "Average",
  Minimum = "Minimum",
  Maximum = "Maximum",
}

export enum ReportSortDirection {
  Ascending = "Ascending",
  Descending = "Descending",
}

export enum ReportParameterType {
  String = "String",
  Number = "Number",
  Date = "Date",
  Boolean = "Boolean",
}

export enum ReportPaperFormat {
  A3 = "A3",
  A4 = "A4",
  A5 = "A5",
  Letter = "Letter",
  Legal = "Legal",
  Custom = "Custom",
}

export enum ReportPageOrientation {
  Portrait = "Portrait",
  Landscape = "Landscape",
}

export enum ReportPageBreak {
  None = "None",
  Before = "Before",
  After = "After",
  BeforeAndAfter = "BeforeAndAfter",
}

export enum ReportMeasurementUnit {
  Millimeter = "Millimeter",
  Inch = "Inch",
  Point = "Point",
  Pixel = "Pixel",
}

export enum ReportJobStatus {
  Pending = "Pending",
  Running = "Running",
  Completed = "Completed",
  Failed = "Failed",
}

export enum ReportExportFormat {
  Html = "Html",
  Pdf = "Pdf",
  Xlsx = "Xlsx",
  Docx = "Docx",
  Csv = "Csv",
}

export enum ReportElementType {
  Text = "Text",
  Value = "Value",
  Line = "Line",
  Rectangle = "Rectangle",
  Image = "Image",
}

export enum ReportDataFieldType {
  String = "String",
  Number = "Number",
  Date = "Date",
  Boolean = "Boolean",
}

export enum ReportBandType {
  ReportHeader = "ReportHeader",
  PageHeader = "PageHeader",
  GroupHeader = "GroupHeader",
  Detail = "Detail",
  GroupFooter = "GroupFooter",
  PageFooter = "PageFooter",
  ReportFooter = "ReportFooter",
  SubReport = "SubReport",
}

export interface ApiExceptionResponse {
  title?: string | null;
  message?: string | null;
  statusCode?: number;
  stackTrace?: string | null;
}

export interface CustomUserNotify {
  username?: string | null;
}

export interface ReportBand {
  id?: string | null;
  type?: ReportBandType;
  title?: string | null;
  styleId?: string | null;
  visible?: boolean;
  displayCondition?: string | null;
  height?: number | null;
  pageBreak?: ReportPageBreak;
  repeatOnEachPage?: boolean;
  grouping?: ReportGroupSettings;
  elements?: ReportElement[] | null;
}

export interface ReportDataFieldDefinition {
  path?: string | null;
  label?: string | null;
  type?: ReportDataFieldType;
}

export interface ReportDataSource {
  key?: string | null;
  providerKey?: string | null;
  query?: Record<string, string>;
}

export interface ReportDataSourceSchema {
  dataSourceKey?: string | null;
  fields?: ReportDataFieldDefinition[] | null;
}

export interface ReportDefinition {
  schemaVersion: number;
  id?: string | null;
  name?: string | null;
  description?: string | null;
  currentVersionLabel?: string | null;
  currentVersion?: number;
  dataSourceKey?: string | null;
  parameters?: ReportParameterDefinition[] | null;
  dataSources?: ReportDataSource[] | null;
  styles?: ReportStyle[] | null;
  bands?: ReportBand[] | null;
  page: ReportPageSettings;
  exports: ReportExportDefinition[] | null;
  localization: ReportLocalizationSettings;
  metadata?: Record<string, string>;
}

export interface ReportDefinitionSummary {
  id?: string | null;
  name?: string | null;
  description?: string | null;
  currentVersion?: number;
  dataSourceKey?: string | null;
}

export interface ReportDocument {
  fileName?: string | null;
  contentType?: string | null;
  html?: string | null;
  generatedAtUtc?: Date;
  cacheKey?: string | null;
}

export interface ReportElement {
  id?: string | null;
  type?: ReportElementType;
  label?: string | null;
  textTemplate?: string | null;
  valuePath?: string | null;
  format?: string | null;
  styleId?: string | null;
  align?: string | null;
  sourceUrl?: string | null;
  layout: ReportElementLayout;
  allowHtml?: boolean;
}

export interface ReportElementLayout {
  x: number;
  y: number;
  width: number;
  height: number;
  zIndex?: number;
}

export interface ReportExecutionLog {
  startedAtUtc?: Date;
  finishedAtUtc?: Date;
  definitionSource?: string | null;
  cacheKey?: string | null;
  rowCount?: number;
  steps?: string[] | null;
  warnings?: string[] | null;
}

export interface ReportExportDefinition {
  format: ReportExportFormat;
  fileNameTemplate: string | null;
  settings: Record<string, string>;
}

export interface ReportExportResult {
  jobId?: string | null;
  reportId?: string | null;
  version?: number;
  status?: ReportJobStatus;
  isCached?: boolean;
  fileName?: string | null;
  contentType?: string | null;
  contentBase64?: string | null;
  executionLog?: ReportExecutionLog;
}

export interface ReportGroupSettings {
  expression?: string | null;
  sortDirection?: ReportSortDirection;
  summaries?: ReportSummaryDefinition[] | null;
}

export interface ReportLocalizationSettings {
  defaultCulture: string | null;
  supportedCultures: string[] | null;
  resources: Record<string, Record<string, string>>;
}

export interface ReportModuleSettings {
  defaultReportId?: string | null;
}

export interface ReportPageMargins {
  top: number;
  right: number;
  bottom: number;
  left: number;
}

export interface ReportPageSettings {
  paperFormat: ReportPaperFormat;
  orientation: ReportPageOrientation;
  width: number;
  height: number;
  unit: ReportMeasurementUnit;
  margins: ReportPageMargins;
}

export interface ReportParameterDefinition {
  name?: string | null;
  label?: string | null;
  type?: ReportParameterType;
  required?: boolean;
  defaultValue?: string | null;
  description?: string | null;
}

export interface ReportRequest {
  reportId?: string | null;
  version?: number | null;
  format?: ReportExportFormat;
  parameters?: Record<string, string | null>;
  userContext?: Record<string, string | null>;
}

export interface ReportResult {
  jobId?: string | null;
  reportId?: string | null;
  version?: number;
  status?: ReportJobStatus;
  isCached?: boolean;
  document?: ReportDocument;
  executionLog?: ReportExecutionLog;
}

export interface ReportStyle {
  id?: string | null;
  cssClass?: string | null;
  properties?: Record<string, string>;
}

export interface ReportSummaryDefinition {
  name?: string | null;
  operation?: ReportSummaryOperation;
  valueExpression?: string | null;
  format?: string | null;
}

export interface SyncUserRequest {
  keycloakUserId?: string | null;
}

export interface UserEntity {
  userId?: number;
  keycloakId?: string | null;
  email: string | null;
  firstName?: string | null;
  lastName?: string | null;
  isActive?: boolean;
  createdAt?: Date;
  updatedAt?: Date;
  lastSyncedAt?: Date;
  photoObjectName?: string | null;
  photoContentType?: string | null;
  settings?: string | null;
}

export interface ReportGetReportByIdParams {
  id?: string;
}

export interface ReportGetReportDataSourceSchemaByReportIdParams {
  id?: string;
}

export interface ReportUpdateReportParams {
  id: string;
}

export interface ReportDeleteReportParams {
  id: string;
}

export interface ReportGetModuleInstanceSettingsParams {
  id?: number;
}

export interface GetAllUsersParams {
  skip?: number;
  take?: number;
}

export interface GetUserParams {
  id?: number;
}

export interface GetUserByKeycloakIdParams {
  keycloakId?: string;
}

export interface SearchUserParams {
  term?: string;
}
