import { EnvironmentProviders, makeEnvironmentProviders } from '@angular/core';
import {
  APP_THEME_PRESETS,
  APP_THEME_PRESETS_MERGE_MODE,
  AppThemePreset,
  AppThemePresetMergeMode
} from './theme-presets.token';

export interface ProvideAppThemesOptions {
  mode?: AppThemePresetMergeMode;
}

export function provideAppThemes(themes: AppThemePreset[], options?: ProvideAppThemesOptions): EnvironmentProviders {
  const mode = options?.mode ?? 'mergeWithDefaults';
  return makeEnvironmentProviders([
    { provide: APP_THEME_PRESETS, useValue: themes },
    { provide: APP_THEME_PRESETS_MERGE_MODE, useValue: mode }
  ]);
}

export function mergeWithDefaults(themes: AppThemePreset[]): EnvironmentProviders {
  return provideAppThemes(themes, { mode: 'mergeWithDefaults' });
}

export function replaceDefaults(themes: AppThemePreset[]): EnvironmentProviders {
  return provideAppThemes(themes, { mode: 'replaceDefaults' });
}
