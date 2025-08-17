import { inject, Injectable } from '@angular/core';
import { from, Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { CryptoService } from '../utils/crypto/crypto.service';

@Injectable({ providedIn: 'root' })
export class JwtWindowCryptoService {
  private readonly cryptoService = inject(CryptoService);

  async generateCodeChallenge(codeVerifier: string): Promise<string> {
    let challengeRaw = await this.calcHash(codeVerifier);
    return this.base64UrlEncode(challengeRaw);
  }

  async generateAtHash(accessToken: string, algorithm: string): Promise<string> {
    let tokenHash = await this.calcHash(accessToken, algorithm);

    const substr: string = tokenHash.substr(0, tokenHash.length / 2);
    const tokenHashBase64: string = btoa(substr);

    return tokenHashBase64
      .replace(/\+/g, '-')
      .replace(/\//g, '_')
      .replace(/=/g, '');
  }

  private async calcHash(valueToHash: string, algorithm = 'SHA-256'): Promise<string> {
    const msgBuffer: Uint8Array = new TextEncoder().encode(valueToHash);
    let hashBuffer = await this.cryptoService.getCrypto().subtle.digest(algorithm, msgBuffer)

    const buffer = hashBuffer as ArrayBuffer;
    const hashArray: number[] = Array.from(new Uint8Array(buffer));
    return this.toHashString(hashArray);
  }

  private toHashString(byteArray: number[]): string {
    let result = '';

    for (const e of byteArray) {
      result += String.fromCharCode(e);
    }

    return result;
  }

  private base64UrlEncode(str: string): string {
    const base64: string = btoa(str);

    return base64.replace(/\+/g, '-').replace(/\//g, '_').replace(/=/g, '');
  }
}
