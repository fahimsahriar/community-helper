import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';

import { environment } from '../../../../environments/environment';
import { Opportunity } from '../models/opportunity.model';

@Injectable({ providedIn: 'root' })
export class OpportunitiesService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}/opportunities`;

  getAll(): Observable<readonly Opportunity[]> {
    return this.http.get<readonly Opportunity[]>(this.baseUrl);
  }
}
