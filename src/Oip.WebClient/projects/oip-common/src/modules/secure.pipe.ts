import { inject, Pipe, PipeTransform } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { DomSanitizer, SafeUrl } from '@angular/platform-browser';
import { map, Observable } from 'rxjs';

@Pipe({
  standalone: true,
  name: 'secure'
})
export class SecurePipe implements PipeTransform {
  private readonly http = inject(HttpClient);
  private readonly sanitizer = inject(DomSanitizer);

  transform(url: any): Observable<SafeUrl> {
    return this.http
      .get(url, { responseType: 'blob' })
      .pipe(map((val) => this.sanitizer.bypassSecurityTrustUrl(URL.createObjectURL(val))));
  }
}
