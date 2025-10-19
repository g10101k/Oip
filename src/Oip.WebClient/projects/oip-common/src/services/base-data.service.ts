import { inject, Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { lastValueFrom, Observable } from 'rxjs';

/**
 * BaseDataService provides a unified interface for sending HTTP requests
 * using Angular's HttpClient. It supports standard HTTP methods and automatic
 * credential handling.
 */
@Injectable()
export class BaseDataService {
  private readonly http = inject(HttpClient);

  /**
   * Gets the base URL of the application from the HTML <base> tag.
   */
  get baseUrl(): string {
    return document.getElementsByTagName('base')[0].href;
  }

  /**
   * Sends an HTTP request with the specified method and data.
   *
   * @template TResponse - Expected response type.
   * @param url - The target URL for the HTTP request.
   * @param method - The HTTP method to use (GET, PUT, POST, DELETE). Default is 'GET'.
   * @param data - An object containing request parameters or payload.
   * @returns A promise that resolves to the response of type TResponse.
   */
  sendRequest<TResponse>(
    url: string,
    method: 'GET' | 'PUT' | 'POST' | 'DELETE' = 'GET',
    data: any = {}
  ): Promise<TResponse> {
    const httpOptions = { withCredentials: true };
    let result: Observable<TResponse>;

    switch (method) {
      case 'GET':
        result = this.http.get<TResponse>(url, { params: data });
        break;
      case 'PUT':
        result = this.http.put<TResponse>(url, data, httpOptions);
        break;
      case 'POST':
        result = this.http.post<TResponse>(url, data, httpOptions);
        break;
      case 'DELETE':
        result = this.http.request<TResponse>('DELETE', url, {
          body: data,
          headers: new HttpHeaders({ 'Content-Type': 'application/json' }),
          withCredentials: true
        });
        break;
    }

    return lastValueFrom(result);
  }

  /**
   * Sends a GET request and retrieves a response as a Blob.
   *
   * @param url - The target URL for the GET request.
   * @returns A promise that resolves to a Object response.
   */
  getBlob(url: string): Promise<object> {
    const httpOptions = {
      responseType: 'blob' as 'json',
      withCredentials: true
    };
    const result = this.http.get(url, httpOptions);
    return lastValueFrom(result);
  }
}
