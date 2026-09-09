import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from  '@angular/common/http';
import { environment } from 'src/environments/environment';


@Injectable({
  providedIn: 'root'
})
export class FileDownloadService {

  constructor(private httpClient: HttpClient) { }

  downloadFile(fileName, fileType, containerName) {

    const queryParamBase = {
      fileName: fileName,
      fileType: fileType,
      containerName: containerName
    };

    let queryParams = new HttpParams();
    Object.entries(queryParamBase).forEach(([key, value]: [string, any]) => {
      if (value !== undefined) {
        if (typeof value === 'string') queryParams = queryParams.set(key, value);
        else if (Array.isArray(value)) value.forEach(v => queryParams = queryParams.append(key, v));
        else queryParams = queryParams.set(key, JSON.stringify(value));
      }
    });

    return this.httpClient.post<any>(environment.rootUrl + '/api/File/download' , {}, {
      params: queryParams,
      observe: 'response', responseType: 'blob' as 'json'
    })

  }

}
