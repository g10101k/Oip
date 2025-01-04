import { inject, Injectable } from '@angular/core';
import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import { lastValueFrom, Observable } from 'rxjs';
import { SecurityService } from "./security.service";

@Injectable()
export class BaseDataService {
  private readonly oipSecurityService: SecurityService = inject(SecurityService);
  private readonly http = inject(HttpClient);

  get baseUrl(): string {
    return document.getElementsByTagName('base')[0].href;
  }

  sendRequest<TResponse>(url: string, method: 'GET' | 'PUT' | 'POST' | 'DELETE' = 'GET', data: any = {}): Promise<TResponse> {
    const httpParams = new HttpParams({ fromObject: data });
    let token = this.oipSecurityService.loginResponse.getValue()?.accessToken;

    let headers: HttpHeaders;
    if (token) {
      headers = new HttpHeaders({ Authorization: `Bearer ${token}` });
    }

    const httpOptions = { headers: headers, withCredentials: true, body: httpParams };
    let result: Observable<TResponse>;
    switch (method) {
      case 'GET':
        result = this.http.get<TResponse>(url, { ...httpOptions, params: data });
        break;
      case 'PUT':
        result = this.http.put<TResponse>(url, data, httpOptions);
        break;
      case 'POST':
        result = this.http.post<TResponse>(url, data, httpOptions);
        break;
      case 'DELETE':
        result = this.http.delete<TResponse>(url, httpOptions);
        break;
    }
    return lastValueFrom(result);
  }
}
