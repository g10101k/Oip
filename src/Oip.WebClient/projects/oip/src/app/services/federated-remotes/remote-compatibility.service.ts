import { Injectable } from '@angular/core';
import { FrontendRemoteManifestDto } from 'oip-common';

const SHELL_VERSION = '1.0.1';
const OIP_COMMON_VERSION = '0.4.0';

export type RemoteCompatibilityResult = {
  compatible: boolean;
  reason?: string;
};

@Injectable({ providedIn: 'root' })
export class RemoteCompatibilityService {
  validate(manifest: FrontendRemoteManifestDto): RemoteCompatibilityResult {
    const shell = this.validateRange(manifest.requiredShellVersion, SHELL_VERSION);
    if (!shell.compatible) {
      return {
        compatible: false,
        reason: `Module ${manifest.title ?? manifest.code} requires OIP Shell ${manifest.requiredShellVersion}, current version is ${SHELL_VERSION}.`
      };
    }

    const common = this.validateRange(manifest.requiredOipCommonVersion, OIP_COMMON_VERSION);
    if (!common.compatible) {
      return {
        compatible: false,
        reason: `Module ${manifest.title ?? manifest.code} requires oip-common ${manifest.requiredOipCommonVersion}, current version is ${OIP_COMMON_VERSION}.`
      };
    }

    return { compatible: true };
  }

  private validateRange(range: string | null | undefined, version: string): RemoteCompatibilityResult {
    const normalizedRange = range?.trim();
    if (!normalizedRange) {
      return { compatible: true };
    }

    if (normalizedRange.startsWith('>=')) {
      return { compatible: this.compareVersions(version, normalizedRange.slice(2).trim()) >= 0 };
    }

    if (normalizedRange.startsWith('^')) {
      const expected = this.parseVersion(normalizedRange.slice(1).trim());
      const actual = this.parseVersion(version);

      return {
        compatible: actual.major === expected.major && this.compareVersions(version, normalizedRange.slice(1).trim()) >= 0
      };
    }

    return { compatible: this.compareVersions(version, normalizedRange) === 0 };
  }

  private compareVersions(left: string, right: string): number {
    const a = this.parseVersion(left);
    const b = this.parseVersion(right);

    return a.major - b.major || a.minor - b.minor || a.patch - b.patch;
  }

  private parseVersion(version: string): { major: number; minor: number; patch: number } {
    const [major = 0, minor = 0, patch = 0] = version
      .replace(/^[^\d]*/, '')
      .split('.')
      .map((part) => Number.parseInt(part, 10) || 0);

    return { major, minor, patch };
  }
}
