/** Right of a securable object type. */
export interface AccessRightDto {
  code: string;
  /** Display name, null for the standard rights translated by the client. */
  name?: string | null;
  /** Hint shown next to the name. */
  description?: string | null;
}

/** Subject that rights are given to: role, user, client, group, everyone or owner. */
export interface SubjectDto {
  subjectKey: string;
  subjectType: string;
  displayName: string;
  isKnown: boolean;
}

/** Rights of one subject in an access control list. */
export interface AclEntryDto extends SubjectDto {
  allow: string[];
  deny: string[];
}

/** Explicit and inherited rights of an object. */
export interface ObjectAclDto {
  objectType: string;
  objectId: number;
  parentType?: string | null;
  parentId?: number | null;
  inheritanceBroken: boolean;
  rights: AccessRightDto[];
  entries: AclEntryDto[];
  inheritedEntries: AclEntryDto[];
}

/** Request that replaces the explicit rights of an object. */
export interface UpdateObjectAclRequest {
  objectType: string;
  objectId: number;
  inheritanceBroken: boolean;
  entries: { subjectKey: string; allow: string[]; deny: string[] }[];
}
