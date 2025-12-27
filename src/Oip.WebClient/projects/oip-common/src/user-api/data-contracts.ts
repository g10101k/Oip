/* eslint-disable */
/* tslint:disable */
// @ts-nocheck
/*
 * ---------------------------------------------------------------
 * ## THIS FILE WAS GENERATED VIA SWAGGER-TYPESCRIPT-API        ##
 * ##                                                           ##
 * ## AUTHOR: acacode                                           ##
 * ## SOURCE: https://github.com/acacode/swagger-typescript-api ##
 * ---------------------------------------------------------------
 */

/** Represents user interface and localization settings. */
export interface UserSettingsDto {
  /** Gets or sets the selected visual preset name. */
  preset?: string | null;
  /** Gets or sets the primary color theme. */
  primary?: string | null;
  /** Gets or sets the surface color. Can be null. */
  surface?: string | null;
  /** Gets or sets a value indicating whether the dark theme is enabled. */
  darkTheme?: boolean;
  /** Gets or sets the layout mode for the menu (e.g., static, overlay). */
  menuMode?: string | null;
  /** Gets or sets the selected language code. */
  language?: string | null;
  /** Gets or sets the date format pattern. */
  dateFormat?: string | null;
  /** Gets or sets the time format pattern. */
  timeFormat?: string | null;
  /** Gets or sets the user's time zone. */
  timeZone?: string | null;
}

export interface UserProfileGetUserPhotoParams {
  /** User's email address */
  email?: string;
}

export interface UserProfilePostUserPhotoPayload {
  files?: File;
}
