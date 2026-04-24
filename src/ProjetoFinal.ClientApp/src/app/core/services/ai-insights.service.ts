import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { catchError, throwError } from 'rxjs';

import { environment } from '../../../environments/environment';
import { AiContentSummaryDto, AiInstructorFrequentQuestionsDto } from '../api/ai.api';

@Injectable({ providedIn: 'root' })
export class AiInsightsService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = environment.baseUrl;

  getContentSummary(contentId: string) {
    return this.http
      .get<AiContentSummaryDto>(`${this.baseUrl}/ai-insights/contents/${contentId}/summary`)
      .pipe(catchError(error => throwError(() => error)));
  }

  getInstructorFrequentQuestions() {
    return this.http
      .get<AiInstructorFrequentQuestionsDto>(`${this.baseUrl}/ai-insights/instructor/frequent-questions`)
      .pipe(catchError(error => throwError(() => error)));
  }
}
